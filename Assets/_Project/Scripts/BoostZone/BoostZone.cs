using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A trigger zone that applies a speed or jump multiplier to the player controller.
///
/// Design rules (from CLAUDE.md):
///   - ONE class; Speed and Jump differ only by parameters.
///   - Starts INACTIVE. <see cref="Activate"/> is wired to the relevant NPC's
///     DialogueInteractable.onDialogueComplete UnityEvent in the Inspector.
///   - Boost applies only while the zone is active AND the player is inside.
///   - When the player exits the trigger the boost is removed and the zone
///     deactivates, so they cannot re-enter and get the boost without re-triggering
///     the dialogue (which is disabled after first play).
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoostZone : MonoBehaviour
{
    public enum BoostType { Speed, Jump }

    [Header("Boost Parameters")]
    [Tooltip("Speed multiplies walk and sprint speeds. Jump multiplies jump height.")]
    [SerializeField] private BoostType boostType = BoostType.Speed;

    [Tooltip("Multiplier applied while the player is inside this zone. 2 = double speed/height.")]
    [SerializeField] private float multiplier = 2f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Fired when the boost starts (player enters while active).")]
    [SerializeField] private UnityEvent onBoostStart;

    [Tooltip("Fired when the boost ends (player exits or zone deactivates).")]
    [SerializeField] private UnityEvent onBoostEnd;

    private bool _isActive;
    private bool _playerInside;
    private FPController_CC _controller;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>
    /// Call this from DialogueInteractable.onDialogueComplete (wired in the Inspector).
    /// Activates the zone; if the player is already inside, the boost starts immediately.
    /// </summary>
    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;

        if (_playerInside && _controller != null)
            ApplyBoost();
    }

    /// <summary>Force-deactivates the boost (e.g. on scene transition).</summary>
    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;

        if (_playerInside && _controller != null)
            RemoveBoost();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInside = true;
        _controller = other.GetComponent<FPController_CC>()
                   ?? other.GetComponentInParent<FPController_CC>();

        if (_isActive && _controller != null)
            ApplyBoost();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (_isActive && _controller != null)
            RemoveBoost();

        _playerInside = false;
        _controller = null;

        // Zone deactivates once the player has passed through — they cannot get the
        // boost again without re-triggering the dialogue (which is one-shot).
        _isActive = false;
    }

    private void ApplyBoost()
    {
        if (boostType == BoostType.Speed)
            _controller.SetSpeedBoost(multiplier);
        else
            _controller.SetJumpBoost(multiplier);

        onBoostStart?.Invoke();
    }

    private void RemoveBoost()
    {
        if (boostType == BoostType.Speed)
            _controller.SetSpeedBoost(1f);
        else
            _controller.SetJumpBoost(1f);

        onBoostEnd?.Invoke();
    }

    private void OnDisable()
    {
        if (_isActive && _playerInside && _controller != null)
            RemoveBoost();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;

        // Inactive = dark grey, Active = colour-coded
        if (!Application.isPlaying || !_isActive)
            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.25f);
        else if (boostType == BoostType.Speed)
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);   // green = speed
        else
            Gizmos.color = new Color(0.2f, 0.4f, 1.0f, 0.3f);   // blue = jump

        Gizmos.matrix = transform.localToWorldMatrix;
        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
    }
#endif
}
