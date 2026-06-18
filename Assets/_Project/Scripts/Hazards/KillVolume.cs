using UnityEngine;

/// <summary>
/// A single flat trigger plane at a fixed Y. Entering it respawns the player at the last
/// checkpoint. Per the design rule, the killspace is one flat plane at one vertical level —
/// place a single wide trigger below the playable area.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.Respawn();
    }
}
