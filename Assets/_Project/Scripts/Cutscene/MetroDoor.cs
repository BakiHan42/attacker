using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Animates the metro exit door open/close. Slide or rotate — set by inspector.
/// Call <see cref="Open"/> from <see cref="OpeningSequencer"/> when the metro stops.
/// </summary>
public class MetroDoor : MonoBehaviour
{
    public enum DoorMoveType { Slide, Rotate }

    [Header("Animation")]
    [SerializeField] private DoorMoveType moveType = DoorMoveType.Slide;

    [Tooltip("Local-space offset applied when the door opens (Slide mode).")]
    [SerializeField] private Vector3 slideOffset = new Vector3(0f, 3f, 0f);

    [Tooltip("Local-space euler angle offset applied when the door opens (Rotate mode).")]
    [SerializeField] private Vector3 rotateOffset = new Vector3(0f, 90f, 0f);

    [SerializeField] private float duration = 1.2f;
    [SerializeField] private Ease  ease     = Ease.InOutSine;

    [Header("Events")]
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;

    private Vector3 _closedPosition;
    private Vector3 _closedRotation;
    private bool    _isOpen;

    private void Awake()
    {
        _closedPosition = transform.localPosition;
        _closedRotation = transform.localEulerAngles;
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (moveType == DoorMoveType.Slide)
            transform.DOLocalMove(_closedPosition + slideOffset, duration)
                     .SetEase(ease)
                     .OnComplete(() => onOpened?.Invoke());
        else
            transform.DOLocalRotate(_closedRotation + rotateOffset, duration)
                     .SetEase(ease)
                     .OnComplete(() => onOpened?.Invoke());
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;

        if (moveType == DoorMoveType.Slide)
            transform.DOLocalMove(_closedPosition, duration)
                     .SetEase(ease)
                     .OnComplete(() => onClosed?.Invoke());
        else
            transform.DOLocalRotate(_closedRotation, duration)
                     .SetEase(ease)
                     .OnComplete(() => onClosed?.Invoke());
    }
}
