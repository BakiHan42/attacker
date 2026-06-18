using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Orchestrates the Phase 2 opening cutscene:
///
///   1. Scene loads fully black (ScreenFader.fadeInOnStart handles the eye-open fade).
///   2. Slow fade-in (2 s) — eye opening.
///   3. Single blink.
///   4. Metro is "moving" — gentle noise on the seated camera.
///   5. Earthquake shake (stronger noise, 3 s after blink).
///   6. Player stands up — teleport + enable FPController.
///   7. CrossFade → swap to player camera.
///   8. Metro decelerates → stops.
///   9. Door opens.
///
/// Setup in Inspector:
///   • Assign SeatedCamera (a CinemachineCamera inside the metro car).
///   • Assign PlayerCamera (the FPCamera).
///   • Assign player, playerSpawnPoint, fader, door.
///   • On ScreenFader, set fadeInOnStart=false — this script drives timing.
/// </summary>
public class OpeningSequencer : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera seatedCamera;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("Shake (Cinemachine Noise)")]
    [SerializeField] private CinemachineBasicMultiChannelPerlin seatedNoise;
    [Tooltip("Amplitude during metro movement (gentle sway).")]
    [SerializeField] private float swayAmplitude     = 0.3f;
    [Tooltip("Amplitude during earthquake.")]
    [SerializeField] private float quakeAmplitude    = 2.5f;
    [SerializeField] private float noiseFrequency    = 1f;

    [Header("Player")]
    [SerializeField] private FPController_CC player;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Scene References")]
    [SerializeField] private ScreenFader fader;
    [SerializeField] private List<MetroDoor> doors;

    [Header("Timing")]
    [SerializeField] private float eyeOpenDuration    = 2.0f;
    [SerializeField] private float holdAfterOpen      = 1.5f;
    [SerializeField] private float blinkOutDuration   = 0.08f;
    [SerializeField] private float blinkInDuration    = 0.15f;
    [SerializeField] private float swayDuration       = 3.0f;  // how long the metro "moves" before quake
    [SerializeField] private float quakeDuration      = 2.0f;
    [SerializeField] private float standUpDelay       = 1.0f;  // after quake starts
    [SerializeField] private float cameraSwapHold     = 0.15f; // black screen during camera swap
    [SerializeField] private float doorOpenDelay      = 1.5f;  // after camera swap

    [Header("Events")]
    [SerializeField] private UnityEvent onCutsceneEnd;

    private void Awake()
    {
        // Cutscene starts: seated cam has priority, player cam is low
        if (seatedCamera != null) seatedCamera.Priority = 20;
        if (playerCamera  != null) playerCamera.Priority  = 10;

        // Lock cursor from the start (matches FPS feel even in cutscene)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Disable player controller so the player can't move during cutscene
        if (player != null)
        {
            player.enabled = false;
            if (playerSpawnPoint != null)
                player.transform.position = playerSpawnPoint.position;
        }

        // Noise starts silent
        if (seatedNoise != null)
        {
            seatedNoise.AmplitudeGain = 0f;
            seatedNoise.FrequencyGain = noiseFrequency;
        }

        // Hand timing control to ScreenFader by turning off its auto-start
        if (fader != null)
        {
            // We drive the first fade ourselves so we can control exact timing
            var so = new System.Reflection.FieldInfo[0]; // no-op; fader already has alpha=1
        }
    }

    private void Start()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // Ensure screen starts black
        if (fader != null && fader.IsFading == false)
        {
            // ScreenFader starts at alpha=1 (black) from its own Awake; don't double-trigger
        }

        // ── 1. Eye opening (slow fade from black) ─────────────────────────────
        bool eyeOpen = false;
        fader?.FadeIn(eyeOpenDuration, onComplete: () => eyeOpen = true);
        yield return new WaitUntil(() => eyeOpen);

        yield return new WaitForSeconds(holdAfterOpen);

        // ── 2. Blink ──────────────────────────────────────────────────────────
        bool blinkDone = false;
        fader?.FadeOut(blinkOutDuration, onComplete: () =>
            fader?.FadeIn(blinkInDuration, onComplete: () => blinkDone = true));
        yield return new WaitUntil(() => blinkDone);

        // ── 3. Metro moving — gentle sway ──────────────────────────────────────
        SetNoise(swayAmplitude);
        yield return new WaitForSeconds(swayDuration);

        // ── 4. Earthquake ─────────────────────────────────────────────────────
        SetNoise(quakeAmplitude);
        yield return new WaitForSeconds(standUpDelay);

        // ── 5. Player stands up ───────────────────────────────────────────────
        if (player != null)
        {
            if (playerSpawnPoint != null)
                player.Teleport(playerSpawnPoint.position);
            player.enabled = true;
        }

        yield return new WaitForSeconds(quakeDuration - standUpDelay);

        // ── 6. Camera handoff (CrossFade hides the swap) ──────────────────────
        bool swapDone = false;
        fader?.CrossFade(
            midpoint: () =>
            {
                if (seatedCamera != null) seatedCamera.Priority = 0;
                if (playerCamera  != null) playerCamera.Priority  = 20;
                SetNoise(0f); // camera swap complete, silence noise
            },
            outDuration:  0.35f,
            inDuration:   0.5f,
            holdDuration: cameraSwapHold,
            onComplete:   () => swapDone = true
        );
        yield return new WaitUntil(() => swapDone);

        // ── 7. Metro decelerates and stops (sway fades out — handled above) ───
        yield return new WaitForSeconds(doorOpenDelay);

        // ── 8. Doors open ────────────────────────────────────────────────────
        foreach (var d in doors) d?.Open();

        onCutsceneEnd?.Invoke();
    }

    private void SetNoise(float amplitude)
    {
        if (seatedNoise == null) return;
        seatedNoise.AmplitudeGain = amplitude;
    }
}
