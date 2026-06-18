using UnityEngine;

/// <summary>
/// Extends <see cref="PickupInteractable"/> so that picking this item up also registers
/// its key in the <see cref="Inventory"/>. The key is a designer-set string (e.g. "Card",
/// "TrainPiece1", "Bone") that <see cref="LockedDoor"/> checks against.
///
/// Usage: Replace PickupInteractable with InventoryItem on items that gate doors or puzzles.
/// Leave key empty for physics props that don't need inventory tracking.
/// </summary>
public class InventoryItem : PickupInteractable
{
    [Header("Inventory")]
    [Tooltip("Unique item identifier checked by LockedDoor. Leave empty to skip inventory tracking.")]
    [SerializeField] private string itemKey;

    public string ItemKey => itemKey;

    public override void OnPickedUp()
    {
        base.OnPickedUp();
        if (!string.IsNullOrEmpty(itemKey) && Inventory.Instance != null)
            Inventory.Instance.Add(itemKey);
    }

    public override void OnDropped()
    {
        base.OnDropped();
        // Keep the item registered even when dropped — once collected, it stays in
        // inventory. Physical drop just means the player is no longer holding it.
    }
}
