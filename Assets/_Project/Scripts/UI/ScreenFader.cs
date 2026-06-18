using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Full-screen black overlay that fades in (black→clear) and out (clear→black).
/// Driven by DOTween. Blocks raycasts while the screen is black so the player
/// can't interact during transitions.
///
/// Usage:
///   ScreenFader.Instance.FadeIn();          // black → clear  (opening)
///   ScreenFader.Instance.FadeOut(() => ...); // clear → black  (then do something)
///
/// Opening sequence (Phase 2): set <see cref="fadeInOnStart"/> = true and
/// <see cref="fadeInDelay"/> to a short value so the scene loads before the fade begins.
///
/// Ending (Phase 10): call FadeOut, wait for callback, show title via ShowTitle().
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image       blackPanel;

    [Tooltip("Optional label shown during a black screen (e.g. 'Between Stops' ending card).")]
    [SerializeField] private TextMeshProUGUI titleLabel;

    [Header("Opening Fade")]
    [Tooltip("Automatically fade from black to clear on Start (use for scene open).")]
    [SerializeField] private bool  fadeInOnStart = true;
    [SerializeField] private float fadeInDelay   = 0.5f;

    [Header("Defaults")]
    [SerializeField] private float defaultFadeInDuration  = 1.0f;
    [SerializeField] private float defaultFadeOutDuration = 0.8f;
    [SerializeField] private Ease  fadeInEase  = Ease.OutQuad;
    [SerializeField] private Ease  fadeOutEase = Ease.InQuad;

    public bool IsFading { get; private set; }

    private Sequence _seq;

    private void Awake()
    {
        Instance = this;

        // Start fully black so the opening fade makes sense.
        if (group != null) group.alpha = 1f;
        if (titleLabel != null) titleLabel.alpha = 0f;

        // Block interaction while the screen is black.
        SetBlocking(true);
    }

    private void Start()
    {
        if (fadeInOnStart)
            FadeIn(defaultFadeInDuration, delay: fadeInDelay);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Black → clear. The game becomes visible.</summary>
    public void FadeIn(float duration = -1f, Action onComplete = null, float delay = 0f)
    {
        float dur = duration < 0f ? defaultFadeInDuration : duration;
        Play(1f, 0f, dur, fadeInEase, delay, onComplete);
    }

    /// <summary>Parameterless overload for wiring via UnityEvent (uses default duration).</summary>
    public void FadeOut() => FadeOut(-1f, null, 0f);

    /// <summary>Clear → black. Typically followed by a scene change or camera cut.</summary>
    public void FadeOut(float duration, Action onComplete = null, float delay = 0f)
    {
        float dur = duration < 0f ? defaultFadeOutDuration : duration;
        Play(0f, 1f, dur, fadeOutEase, delay, onComplete);
    }

    /// <summary>
    /// Cross-fade helper: fade out → invoke <paramref name="midpoint"/> → fade in.
    /// Use for camera handoffs, level transitions, etc.
    /// </summary>
    public void CrossFade(Action midpoint,
                          float outDuration = -1f, float inDuration = -1f,
                          float holdDuration = 0.1f,
                          Action onComplete = null)
    {
        FadeOut(outDuration, onComplete: () =>
        {
            midpoint?.Invoke();
            DOVirtual.DelayedCall(holdDuration, () => FadeIn(inDuration, onComplete));
        });
    }

    /// <summary>
    /// Shows a title string on the black screen (e.g. "Between Stops" at the end).
    /// Call after FadeOut's callback to ensure the screen is black first.
    /// </summary>
    public void ShowTitle(string text, float fadeInDuration = 0.8f, Action onComplete = null)
    {
        if (titleLabel == null) { onComplete?.Invoke(); return; }
        titleLabel.text  = text;
        titleLabel.alpha = 0f;
        titleLabel.DOFade(1f, fadeInDuration).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>Hides the title label.</summary>
    public void HideTitle(float fadeOutDuration = 0.5f, Action onComplete = null)
    {
        if (titleLabel == null) { onComplete?.Invoke(); return; }
        titleLabel.DOFade(0f, fadeOutDuration).OnComplete(() => onComplete?.Invoke());
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Play(float from, float to, float duration, Ease ease, float delay, Action onComplete)
    {
        _seq?.Kill();
        IsFading = true;
        SetBlocking(true);

        if (group == null) { IsFading = false; onComplete?.Invoke(); return; }

        group.alpha = from;
        _seq = DOTween.Sequence();

        if (delay > 0f)
            _seq.AppendInterval(delay);

        _seq.Append(group.DOFade(to, duration).SetEase(ease));
        _seq.OnComplete(() =>
        {
            IsFading = false;
            // Only block when the screen is opaque (to == 1 means we faded to black).
            SetBlocking(to >= 1f);
            onComplete?.Invoke();
        });
    }

    private void SetBlocking(bool block)
    {
        if (group == null) return;
        group.blocksRaycasts = block;
        group.interactable   = block;
    }
}
