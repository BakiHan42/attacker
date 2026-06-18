using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A door/gate the player can only pass while the required item is in their <see cref="Inventory"/>.
/// For "must be holding" items (see <see cref="InventoryItem"/> removeFromInventoryOnDrop), dropping
/// the item removes it from the inventory and the gate re-locks. The gate re-evaluates on every
/// approach and re-locks once the player leaves the sensor — unless <see cref="ForceUnlock"/> was
/// called (e.g. a cutscene), which opens it permanently.
///
/// Setup:
///   - Attach to a trigger collider that covers the doorway (the "sensor"). Make it deep enough to
///     contain the player while they pass the blocking collider, so it doesn't re-lock on top of them.
///   - Assign the solid blocking collider (e.g. the gate mesh's collider) to <see cref="blockingCollider"/>.
///     It is disabled while open and re-enabled when locked.
///   - Set <see cref="requiredItemKey"/> to match the <see cref="InventoryItem.ItemKey"/> on the collectible.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LockedDoor : MonoBehaviour
{
    [Header("Lock")]
    [Tooltip("Must match InventoryItem.itemKey on the required collectible.")]
    [SerializeField] private string requiredItemKey;

    [Header("References")]
    [Tooltip("The solid collider that physically blocks passage. Disabled while open.")]
    [SerializeField] private Collider blockingCollider;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Fired each time the door opens (play open animation, SFX, etc.).")]
    [SerializeField] private UnityEvent onUnlocked;

    [Tooltip("Fired each time the player approaches without the required item.")]
    [SerializeField] private UnityEvent onLocked;

    private bool _open;
    private bool _forcedOpen;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || _forcedOpen) return;

        bool hasItem = Inventory.Instance != null && Inventory.Instance.Has(requiredItemKey);
        if (hasItem)
        {
            SetOpen(true);
        }
        else
        {
            LockedDoorUI.Instance?.Show();
            onLocked?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || _forcedOpen) return;

        // Re-lock once the player leaves the sensor, so the item must be held on each approach.
        SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        if (_open == open) return;
        _open = open;

        if (blockingCollider != null)
            blockingCollider.enabled = !open;

        if (open)
            onUnlocked?.Invoke();
    }

    /// <summary>Force-unlock permanently from script or UnityEvent (e.g. a key cutscene).</summary>
    public void ForceUnlock()
    {
        _forcedOpen = true;
        SetOpen(true);
    }
}
