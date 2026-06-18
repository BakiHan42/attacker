using System;
using UnityEngine;

/// <summary>
/// Holds the player's current respawn point and performs respawns. Loosely coupled:
/// checkpoint photos call <see cref="SetCheckpoint"/>, kill volumes call <see cref="Respawn"/>,
/// and UI listens to the events. Until the first checkpoint is collected, the player respawns
/// at <see cref="defaultSpawn"/> (or their start position).
/// </summary>
[DefaultExecutionOrder(-50)]
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [SerializeField] private FPController_CC player;

    [Tooltip("Respawn point used before the first checkpoint. Defaults to the player's start position.")]
    [SerializeField] private Transform defaultSpawn;

    /// <summary>Fired when a checkpoint is set (UI subscribes for the \"Checkpoint saved\" toast).</summary>
    public event Action<CheckpointData> OnCheckpointSet;

    /// <summary>Fired after the player has been moved back to the last respawn point.</summary>
    public event Action OnRespawn;

    public CheckpointData CurrentCheckpoint { get; private set; }
    public bool HasCheckpoint { get; private set; }

    private Vector3 _respawnPosition;

    private void Awake()
    {
        Instance = this;
        if (player == null)
            player = FindFirstObjectByType<FPController_CC>();
    }

    private void Start()
    {
        _respawnPosition =
            defaultSpawn != null ? defaultSpawn.position :
            player != null ? player.transform.position :
            Vector3.zero;
    }

    /// <summary>Records a new respawn point. <paramref name="data"/> may be null (plain checkpoint).</summary>
    public void SetCheckpoint(Vector3 position, CheckpointData data = null)
    {
        _respawnPosition = position;
        HasCheckpoint = true;
        CurrentCheckpoint = data;
        OnCheckpointSet?.Invoke(data);
    }

    /// <summary>Moves the player back to the last respawn point.</summary>
    public void Respawn()
    {
        if (player == null)
            player = FindFirstObjectByType<FPController_CC>();

        if (player != null)
            player.Teleport(_respawnPosition);

        OnRespawn?.Invoke();
    }
}
