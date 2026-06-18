using UnityEngine;

/// <summary>
/// Data for a checkpoint photo. Checkpoint photos are dual-purpose: a respawn point AND a
/// narrative beat. This asset carries the narrative half (what to play when collected).
/// </summary>
[CreateAssetMenu(menuName = "Between Stops/Checkpoint Data", fileName = "CheckpointData")]
public class CheckpointData : ScriptableObject
{
    [Tooltip("Identifier for this checkpoint (e.g. \"Father photo\"). Shown in logs, not the HUD.")]
    public string checkpointName = "Checkpoint";

    [Header("Narrative beat (played on collect)")]
    [Tooltip("One-shot audio played when the photo is collected (e.g. a voice line, dog barking).")]
    public AudioClip audioClip;

    [Tooltip("Optional dialogue to start when the photo is collected. Leave empty for audio-only.")]
    public Dialogue[] dialogue;

    [Tooltip("Optional photo image, for future UI use.")]
    public Sprite photoSprite;
}
