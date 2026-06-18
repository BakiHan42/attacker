using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Singleton "Locked" toast shown when the player tries to pass a <see cref="LockedDoor"/>
/// without the required item. Fades in, holds briefly, then fades out.
/// </summary>
public class LockedDoorUI : MonoBehaviour
{
    public static LockedDoorUI Instance { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string message = "Locked";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float holdDuration = 1.2f;

    private Sequence _seq;

    private void Awake()
    {
        Instance = this;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 0f;
        if (label != null) label.text = message;
    }

    public void Show()
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
