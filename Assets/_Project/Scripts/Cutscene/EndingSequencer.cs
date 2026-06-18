using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Phase 10 ending sequence.
/// Triggered by HousePuzzleManager.onAllComplete → FadeOut → this.StartEnding().
///
/// Flow:
///   1. Teleport player back to metro opening position (same seat).
///   2. Re-activate SeatedCamera (priority 20), drop FP camera (priority 0).
///   3. FadeIn — player "wakes up" on the metro again.
///   4. Soft ambient sway only — NO earthquake.
///   5. After a short delay the metro stops and the door opens.
///   6. Player control handed back (CrossFade: seated→FP camera).
///   7. Player walks to ExitTrigger inside the metro → fires onPlayerExited.
/// </summary>
public class EndingSequencer : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private CinemachineCamera seatedCamera;
    [SerializeField] private CinemachineCamera fpCamera;
    [SerializeField] private FPController_CC   fpController;
    [SerializeField] private Transform         playerSpawnPoint;
    [SerializeField] private MetroDoor         metroDoor;

    [Header("Timing")]
    [SerializeField] private float fadeInDelay      = 0.5f;  // black-screen hold before fade in
    [SerializeField] private float swayDuration     = 5.0f;  // how long the seated sway lasts
    [SerializeField] private float playerHandoffDelay = 1.5f; // pause after door opens before handing control

    [Header("Camera sway (no earthquake)")]
    [SerializeField] private float swayAmplitude = 0.3f;
    [SerializeField] private float swayFrequency = 0.4f;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceReady;  // fired once player control is returned

    private CinemachineBasicMultiChannelPerlin _seatedNoise;

    private void Awake()
    {
        if (seatedCamera != null)
            _seatedNoise = seatedCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (_seatedNoise == null && seatedCamera != null)
            _seatedNoise = seatedCamera.gameObject.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    /// <summary>
    /// Called by ScreenFader once the fade-to-black finishes (via a DOTween delayed call or
    /// wired through onAllComplete chain).  Safe to call from a UnityEvent.
    /// </summary>
    public void StartEnding()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // ── 1. Teleport & lock player ────────────────────────────────────────
        if (fpController != null)
        {
            fpController.enabled = false;
            if (playerSpawnPoint != null)
                fpController.Teleport(playerSpawnPoint.position);
        }

        // ── 2. Seated camera takes over ──────────────────────────────────────
        if (seatedCamera != null) seatedCamera.Priority = 20;
        if (fpCamera     != null) fpCamera.Priority      = 0;

        // ── 3. Soft sway ─────────────────────────────────────────────────────
        if (_seatedNoise != null)
        {
            _seatedNoise.AmplitudeGain = swayAmplitude;
            _seatedNoise.FrequencyGain = swayFrequency;
        }

        // ── 4. Fade in ────────────────────────────────────────────────────────
        yield return new WaitForSeconds(fadeInDelay);
        var fader = ScreenFader.Instance;
        if (fader != null) fader.FadeIn();

        // ── 5. Wait, then stop sway and open door ────────────────────────────
        yield return new WaitForSeconds(swayDuration);

        if (_seatedNoise != null) _seatedNoise.AmplitudeGain = 0f;

        if (metroDoor != null) metroDoor.Open();

        yield return new WaitForSeconds(playerHandoffDelay);

        // ── 6. Hand control to player (CrossFade) ────────────────────────────
        if (fader != null)
        {
            fader.CrossFade(() =>
            {
                if (seatedCamera != null) seatedCamera.Priority = 0;
                if (fpCamera     != null) fpCamera.Priority      = 20;
                if (fpController != null) fpController.enabled   = true;
            });
        }
        else
        {
            if (seatedCamera != null) seatedCamera.Priority = 0;
            if (fpCamera     != null) fpCamera.Priority      = 20;
            if (fpController != null) fpController.enabled   = true;
        }

        onSequenceReady?.Invoke();
    }
}
