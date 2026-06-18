using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the torn-photo drag-and-assemble minigame.
///
/// Setup (Inspector):
///   • Assign <see cref="puzzleCanvas"/> — the full-screen overlay shown during the puzzle.
///   • Assign <see cref="pieces"/> — each DragPiece in the scene.
///   • Assign matching <see cref="snapTargets"/> in the same order.
///   • Hook <see cref="onPuzzleComplete"/> to add the assembled photo to inventory.
///
/// Usage:
///   Call Open() when the player interacts with the torn photo world object.
///   The player drags all pieces to their targets; once all snap → puzzle auto-completes.
/// </summary>
public class DragAssemblePuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject       puzzleCanvas;
    [SerializeField] private RectTransform    canvasRect;

    [Header("Pieces and Targets")]
    [Tooltip("Draggable pieces — order must match snapTargets.")]
    [SerializeField] private DragPiece[]      pieces;
    [Tooltip("RectTransforms marking where each piece snaps to.")]
    [SerializeField] private RectTransform[]  snapTargets;

    [Header("Feedback")]
    [SerializeField] private AudioClip completionClip;
    [SerializeField] private TMPro.TextMeshProUGUI completionLabel;

    [Header("Events")]
    [SerializeField] private UnityEvent onPuzzleComplete;
    [SerializeField] private UnityEvent onPuzzleOpened;
    [SerializeField] private UnityEvent onPuzzleClosed;

    public bool IsComplete { get; private set; }
    public bool IsOpen     { get; private set; }

    // NOTE: keep the puzzle canvas INACTIVE in the scene. We deliberately do NOT disable it from
    // Awake — this controller lives on the canvas GameObject, so its Awake first runs the instant
    // Open() calls SetActive(true). Disabling here would immediately switch the canvas back off
    // (which made the puzzle appear to "not open"). Piece init is deferred to Open() anyway.

    private void Update()
    {
        if (!IsOpen || IsComplete) return;
        CheckCompletion();

        // Close on Escape
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void Open()
    {
        if (IsComplete || IsOpen) return;

        for (int i = 0; i < pieces.Length; i++)
        {
            pieces[i].Init(canvasRect);
            if (i < snapTargets.Length)
                pieces[i].SnapTarget = snapTargets[i];
        }

        IsOpen = true;
        puzzleCanvas.SetActive(true);

        // Unlock cursor for mouse dragging
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Pause player look (prevent camera spin while dragging)
        var fp = FindAnyObjectByType<FPController_CC>();
        fp?.SetLookLocked(true);

        onPuzzleOpened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        puzzleCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        var fp = FindAnyObjectByType<FPController_CC>();
        fp?.SetLookLocked(false);

        onPuzzleClosed?.Invoke();
    }

    private void CheckCompletion()
    {
        foreach (var p in pieces)
            if (!p.IsSnapped) return;

        IsComplete = true;
        IsOpen     = false;

        if (completionClip != null)
            AudioSource.PlayClipAtPoint(completionClip, UnityEngine.Camera.main.transform.position);

        if (completionLabel != null)
            completionLabel.gameObject.SetActive(true);

        DOTween.Sequence()
            .AppendInterval(1.2f)
            .AppendCallback(() =>
            {
                puzzleCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
                var fp = FindAnyObjectByType<FPController_CC>();
                fp?.SetLookLocked(false);
                onPuzzleComplete?.Invoke();
            });
    }
}
