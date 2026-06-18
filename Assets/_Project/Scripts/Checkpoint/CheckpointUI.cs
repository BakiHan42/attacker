using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Bottom-right "Checkpoint saved" toast. Listens to <see cref="RespawnManager.OnCheckpointSet"/>
/// and fades a CanvasGroup in, holds, then fades out.
/// </summary>
public class CheckpointUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string message = "Checkpoint saved";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float holdDuration = 1.5f;

    private Sequence _seq;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 0f;
        if (label != null) label.text = message;
    }

    private void OnEnable()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnCheckpointSet += HandleCheckpointSet;
    }

    private void OnDisable()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnCheckpointSet -= HandleCheckpointSet;
    }

    private void HandleCheckpointSet(CheckpointData data)
    {
        if (group == null) return;
        if (label != null) label.text = message;

        _seq?.Kill();
        _seq = DOTween.Sequence();
        _seq.Append(group.DOFade(1f, fadeDuration))
            .AppendInterval(holdDuration)
            .Append(group.DOFade(0f, fadeDuration));
    }
}
