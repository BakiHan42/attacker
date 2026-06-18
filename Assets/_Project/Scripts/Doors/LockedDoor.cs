using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A door that the player can only pass through if a specific item is in their
/// <see cref="Inventory"/>. On approach without the item, shows the "Locked" UI toast.
/// On approach with the item, disables the optional blocking collider and fires
/// <see cref="onUnlocked"/> once.
///
/// Setup:
///   - Attach to a trigger collider that covers the doorway (the "sensor").
///   - Assign the physical blocking collider (e.g. a solid door mesh's collider) to
///     <see cref="blockingCollider"/>. It will be disabled on unlock.
///   - Set <see cref="requiredItemKey"/> to match the <see cref="InventoryItem.ItemKey"/>
///     on the collectible.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LockedDoor : MonoBehaviour
{
    [Header("Lock")]
    [Tooltip("Must match InventoryItem.itemKey on the required collectible.")]
    [SerializeField] private string requiredItemKey;

    [Header("References")]
    [Tooltip("The solid collider that physically blocks passage. Disabled on unlock.")]
    [SerializeField] private Collider blockingCollider;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Fired once when the door is unlocked (play open animation, SFX, etc.).")]
    [SerializeField] private UnityEvent onUnlocked;

    [Tooltip("Fired each time the player approaches without the required item.")]
    [SerializeField] private UnityEvent onLocked;

    private bool _unlocked;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_unlocked) return;

        if (Inventory.Instance != null && Inventory.Instance.Has(requiredItemKey))
        {
            Unlock();
        }
        else
        {
            LockedDoorUI.Instance?.Show();
            onLocked?.Invoke();
        }
    }

    private void Unlock()
    {
        _unlocked = true;

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        onUnlocked?.Invoke();
    }

    /// <summary>Force-unlock from script or UnityEvent (e.g. a key cutscene).</summary>
    public void ForceUnlock() => Unlock();
}
