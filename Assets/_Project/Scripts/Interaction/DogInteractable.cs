using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Two-state dog NPC for Phase 8.
///
/// First interaction  → plays bark dialogue, starts looping bark SFX, transitions to waiting state.
/// Second interaction → plays hint dialogue, activates the alternate parkour path.
///
/// Design rule: the alternate path ONLY activates after the second interaction.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DogInteractable : MonoBehaviour, IInteractable, IHasInteractionPromptData
{
    public enum DogState { WaitingFirst, WaitingSecond, Done }

    [Header("Dialogue")]
    [SerializeField] private Dialogue[] firstDialogue;
    [SerializeField] private Dialogue[] secondDialogue;

    [Header("Alternate Path")]
    [Tooltip("GameObjects that form the alternate (passable) parkour. Disabled by default, enabled on 2nd interaction.")]
    [SerializeField] private GameObject[] alternatePath;

    [Header("Bark Loop")]
    [SerializeField] private AudioSource barkSource;
    [SerializeField] private AudioClip   barkClip;
    [SerializeField] private float       barkInterval = 2.5f;

    [Header("Prompt")]
    [SerializeField] private string promptVerb = "Pet";

    [Header("Events")]
    [SerializeField] private UnityEvent onFirstInteraction;
    [SerializeField] private UnityEvent onSecondInteraction;

    public DogState State { get; private set; } = DogState.WaitingFirst;

    private DialogueManager _dm;
    private Coroutine       _barkLoop;

    private void Awake()
    {
        _dm = FindAnyObjectByType<DialogueManager>();
        // Alternate path starts hidden
        foreach (var go in alternatePath)
            if (go != null) go.SetActive(false);
    }

    public bool CanInteract()
    {
        if (!gameObject.activeInHierarchy) return false;
        if (State == DogState.Done) return false;
        if (_dm != null && _dm.IsDialogueActive) return false;
        return true;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        if (State == DogState.WaitingFirst)
        {
            State = DogState.WaitingSecond;
            _dm?.StartDialogue(firstDialogue, OnFirstDialogueDone);
            onFirstInteraction?.Invoke();
        }
        else if (State == DogState.WaitingSecond)
        {
            State = DogState.Done;
            StopBarkLoop();
            _dm?.StartDialogue(secondDialogue, ActivateAlternatePath);
            onSecondInteraction?.Invoke();
        }
    }

    public void OnFocusGained() { }
    public void OnFocusLost()   { }
    public void OnPickedUp()    { }
    public void OnDropped()     { }

    public InteractionPromptData GetInteractionPromptData() =>
        new InteractionPromptData(gameObject.name, promptVerb, null,
            InteractionPromptDisplayMode.ScreenSpace, transform, Vector3.up * 0.8f);

    // ── Internal ──────────────────────────────────────────────────────────────

    private void OnFirstDialogueDone()
    {
        _barkLoop = StartCoroutine(BarkLoop());
    }

    private IEnumerator BarkLoop()
    {
        while (true)
        {
            if (barkSource != null && barkClip != null)
                barkSource.PlayOneShot(barkClip);
            yield return new WaitForSeconds(barkInterval);
        }
    }

    private void StopBarkLoop()
    {
        if (_barkLoop != null) { StopCoroutine(_barkLoop); _barkLoop = null; }
        if (barkSource != null) barkSource.Stop();
    }

    private void ActivateAlternatePath()
    {
        foreach (var go in alternatePath)
            if (go != null) go.SetActive(true);
    }
}
