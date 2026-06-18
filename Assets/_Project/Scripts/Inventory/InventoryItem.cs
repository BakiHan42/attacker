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

    [Tooltip("When set, dropping the item also removes it from the inventory, so anything " +
             "gated on it (e.g. LockedDoor) requires the item to be currently held. Leave off " +
             "for items that should stay collected once picked up.")]
    [SerializeField] private bool removeFromInventoryOnDrop;

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
        // By default the item stays registered once collected — physical drop just means the
        // player is no longer holding it. When removeFromInventoryOnDrop is set (e.g. the metro
        // card), dropping de-registers it so the player must be holding it to pass its gate.
        if (removeFromInventoryOnDrop && !string.IsNullOrEmpty(itemKey) && Inventory.Instance != null)
            Inventory.Instance.Remove(itemKey);
    }
}
