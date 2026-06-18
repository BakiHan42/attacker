using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A photo fragment the player can drag toward its snap target.
/// Used by <see cref="DragAssemblePuzzle"/>.
/// </summary>
public class DragPiece : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("World-space distance (in canvas units) within which this piece snaps to its target.")]
    [SerializeField] private float snapRadius = 40f;

    public bool IsSnapped { get; private set; }

    private RectTransform _rect;
    private RectTransform _canvasRect;
    private Vector2       _dragOffset;
    private Vector2       _restPosition;   // original unsnapped position (for reset)

    // Assigned by DragAssemblePuzzle at runtime
    public RectTransform SnapTarget { get; set; }

    private void Awake()
    {
        _rect        = GetComponent<RectTransform>();
        _restPosition = _rect.anchoredPosition;
    }

    public void Init(RectTransform canvasRect)
    {
        _canvasRect = canvasRect;
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (IsSnapped) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, data.position, data.pressEventCamera, out var localPoint);
        _dragOffset = _rect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData data)
    {
        if (IsSnapped) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, data.position, data.pressEventCamera, out var localPoint);
        _rect.anchoredPosition = localPoint + _dragOffset;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (IsSnapped || SnapTarget == null) return;

        float dist = Vector2.Distance(_rect.anchoredPosition, SnapTarget.anchoredPosition);
        if (dist <= snapRadius)
            Snap();
    }

    private void Snap()
    {
        IsSnapped = true;
        _rect.anchoredPosition = SnapTarget.anchoredPosition;
        GetComponent<UnityEngine.UI.Image>().raycastTarget = false; // no more drag
    }

    public void ResetPiece()
    {
        IsSnapped = false;
        _rect.anchoredPosition = _restPosition;
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.raycastTarget = true;
    }
}
