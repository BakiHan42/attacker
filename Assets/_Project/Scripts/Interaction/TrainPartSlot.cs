using UnityEngine;

/// <summary>
/// A "look-and-place" slot for the train table. Unlike <see cref="ItemPlacementSlot"/> (which is a
/// trigger you walk into), this slot is an <see cref="Interactable"/>: it only becomes focusable /
/// interactable while the player is *holding the matching <see cref="InventoryItem"/>*. Looking at
/// the glowing slot then shows the "Place ___" prompt; pressing interact snaps the held piece onto
/// the slot, matching the slot's place point position/rotation/scale, and hides the glow ghost.
///
/// Setup:
///   - Put this on a GameObject with a (non-trigger) collider the look-raycast can hit.
///   - <see cref="requiredItemKey"/> must match the carried item's InventoryItem.ItemKey.
///   - <see cref="placePoint"/> is where the piece snaps (defaults to this transform). The glowing
///     ghost mesh should sit at that same transform so the real piece lands exactly on it.
///   - <see cref="ghostVisual"/> is the glowing preview; it is hidden once the piece is placed.
///   - Wire onInteract → HousePuzzleManager.NotifyTrainPiecePlaced (fires only on a successful place).
/// </summary>
public class TrainPartSlot : Interactable
{
    [Header("Train Slot")]
    [Tooltip("Must match the carried InventoryItem.ItemKey (e.g. train_engine).")]
    [SerializeField] private string requiredItemKey;

    [Tooltip("Where the placed piece snaps to (position/rotation/scale). Defaults to this transform.")]
    [SerializeField] private Transform placePoint;

    [Tooltip("Glowing ghost preview shown until the piece is placed, then hidden.")]
    [SerializeField] private GameObject ghostVisual;

    public bool IsFilled { get; private set; }

    public override bool CanInteract()
    {
        if (!base.CanInteract() || IsFilled) return false;
        InventoryItem held = PickupController.Instance != null ? PickupController.Instance.HeldItem : null;
        return held != null && held.ItemKey == requiredItemKey;
    }

    protected override bool HandleInteract()
    {
        PickupController pc = PickupController.Instance;
        if (pc == null) return false;

        InventoryItem held = pc.HeldItem;
        if (held == null || held.ItemKey != requiredItemKey) return false;

        GameObject piece = pc.ReleaseHeld();
        if (piece == null) return false;

        Transform t = placePoint != null ? placePoint : transform;
        if (piece.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
        piece.transform.SetParent(null, true);
        piece.transform.SetPositionAndRotation(t.position, t.rotation);
        piece.transform.localScale = t.lossyScale;

        // Lock the placed piece so it can't be picked back up, and retire the slot.
        if (piece.TryGetComponent(out Interactable placed))
            placed.DisableInteraction();

        IsFilled = true;
        if (ghostVisual != null)
            ghostVisual.SetActive(false);
        DisableInteraction();

        return true; // base.Interact() fires onInteract → puzzle progress
    }
}
