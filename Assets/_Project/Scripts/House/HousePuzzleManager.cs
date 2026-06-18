using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks the three Phase 9 tasks: train pieces placed, photo assembled + framed, bone bowled.
/// Fires <see cref="onAllComplete"/> when every task is done, regardless of order.
/// Plays <see cref="completionNarration"/> first if assigned, then fires the event.
/// </summary>
public class HousePuzzleManager : MonoBehaviour
{
    [Header("Narration played before the ending fade")]
    [SerializeField] private Dialogue[] completionNarration;

    [Header("Events")]
    [SerializeField] private UnityEvent onAllComplete;

    [Header("Train")]
    [Tooltip("How many train pieces must be placed before the train task counts as done.")]
    [SerializeField] private int trainPieceCount = 3;

    private bool _trainDone;
    private bool _photoDone;
    private bool _boneDone;
    private int _trainPiecesPlaced;

    // Called by each train TrackSlot.onItemPlaced — marks the train task done once all pieces are in.
    public void NotifyTrainPiecePlaced()
    {
        if (_trainDone) return;
        _trainPiecesPlaced++;
        if (_trainPiecesPlaced >= trainPieceCount)
            CompleteTrainPuzzle();
    }

    public void CompleteTrainPuzzle()
    {
        if (_trainDone) return;
        _trainDone = true;
        Check();
    }

    // Called by DragAssemblePuzzle.onPuzzleComplete — adds item then marks done
    public void OnPhotoAssembled()
    {
        Inventory.Instance?.Add("assembled_photo");
    }

    public void CompletePhotoPuzzle()
    {
        if (_photoDone) return;
        _photoDone = true;
        Check();
    }

    public void CompleteBonePuzzle()
    {
        if (_boneDone) return;
        _boneDone = true;
        Check();
    }

    private void Check()
    {
        if (!(_trainDone && _photoDone && _boneDone)) return;

        var dm = FindFirstObjectByType<DialogueManager>();
        if (dm != null && completionNarration != null && completionNarration.Length > 0)
            dm.StartDialogue(completionNarration, () => onAllComplete?.Invoke());
        else
            onAllComplete?.Invoke();
    }

    // Inspector helpers to check state
    public bool TrainDone  => _trainDone;
    public bool PhotoDone  => _photoDone;
    public bool BoneDone   => _boneDone;
    public bool AllDone    => _trainDone && _photoDone && _boneDone;
}
