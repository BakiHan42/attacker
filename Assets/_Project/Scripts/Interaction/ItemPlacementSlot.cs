using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A slot the player can place a held or inventoried item into.
///
/// Two modes:
///   Inventory  — checks <see cref="Inventory"/> for <see cref="requiredItemKey"/> (card-style).
///   Held       — checks <see cref="PickupController.IsHolding"/> and the held object's
///                <see cref="InventoryItem.ItemKey"/> (player physically carries the item to the slot).
///
/// On successful placement the slot fires <see cref="onItemPlaced"/> once and stays filled.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemPlacementSlot : MonoBehaviour
{
    public enum CheckMode { Inventory, Held }

    [Header("Slot")]
    [SerializeField] private string    requiredItemKey;
    [SerializeField] private CheckMode checkMode = CheckMode.Held;

    [Tooltip("Optional transform where the placed item should snap to (visual only).")]
    [SerializeField] private Transform placePoint;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [SerializeField] private UnityEvent onItemPlaced;
    [SerializeField] private UnityEvent onWrongItem;

    public bool IsFilled { get; private set; }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsFilled || !other.CompareTag(playerTag)) return;

        if (checkMode == CheckMode.Inventory)
            TryPlaceFromInventory();
        else
            TryPlaceHeld();
    }

    private void TryPlaceFromInventory()
    {
        if (Inventory.Instance != null && Inventory.Instance.Has(requiredItemKey))
            Fill(null);
        else
            onWrongItem?.Invoke();
    }

    private void TryPlaceHeld()
    {
        var pc = PickupController.Instance;
        if (pc == null || !pc.IsHolding) { onWrongItem?.Invoke(); return; }

        // Find the held object
        var heldField = typeof(PickupController).GetField("_heldObj",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var heldObj = heldField?.GetValue(pc) as GameObject;
        if (heldObj == null) { onWrongItem?.Invoke(); return; }

        var item = heldObj.GetComponent<InventoryItem>();
        if (item == null || item.ItemKey != requiredItemKey) { onWrongItem?.Invoke(); return; }

        Fill(heldObj);
    }

    private void Fill(GameObject heldObj)
    {
        IsFilled = true;

        if (heldObj != null && placePoint != null)
        {
            // Detach from PickupController and snap to place point
            var pc = PickupController.Instance;
            var dropMethod = typeof(PickupController).GetMethod("DropObject",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dropMethod?.Invoke(pc, null);

            var rb = heldObj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            heldObj.transform.SetPositionAndRotation(placePoint.position, placePoint.rotation);
        }

        GetComponent<Collider>().enabled = false; // can't place again
        onItemPlaced?.Invoke();
    }

    public void ForcePlace() => Fill(null);
}
