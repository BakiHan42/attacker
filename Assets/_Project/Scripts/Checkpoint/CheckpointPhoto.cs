using UnityEngine;

/// <summary>
/// A photo on the ground. Walking into it (trigger) collects it once: records the respawn
/// point, plays its narrative beat (audio and/or dialogue), and hides the photo. Dual-purpose
/// by design — both a respawn point and a story moment.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointPhoto : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CheckpointData data;

    [Header("Respawn")]
    [Tooltip("Where the player respawns. Defaults to this object's position if unset.")]
    [SerializeField] private Transform respawnPoint;

    [Header("Collection")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Visual to hide on collect. If unset, this object's renderers are disabled instead.")]
    [SerializeField] private GameObject visual;

    [Tooltip("Optional AudioSource for the collect sound. If unset, a one-shot is played at the photo's position.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Optional dialogue on collect")]
    [SerializeField] private DialogueManager dialogueManager;

    private bool _collected;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || !other.CompareTag(playerTag)) return;
        Collect();
    }

    private void Collect()
    {
        _collected = true;

        Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.SetCheckpoint(spawnPos, data);

        PlayNarrativeBeat();

        // Prevent re-collection, then hide the photo. Keep the GameObject alive so a
        // one-shot played on its own AudioSource can finish.
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (visual != null)
            visual.SetActive(false);
        else
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
    }

    private void PlayNarrativeBeat()
    {
        if (data == null) return;

        if (data.audioClip != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(data.audioClip);
            else
                AudioSource.PlayClipAtPoint(data.audioClip, transform.position);
        }

        if (data.dialogue != null && data.dialogue.Length > 0)
        {
            if (dialogueManager == null)
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            dialogueManager?.StartDialogue(data.dialogue);
        }
    }
}
