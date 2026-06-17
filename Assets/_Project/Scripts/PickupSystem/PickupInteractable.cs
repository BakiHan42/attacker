using UnityEngine;

public class PickupInteractable : Interactable
{
    [SerializeField] private Collider colliderToDisable;

    protected override bool HandleInteract()
    {
        if (PickupController.Instance == null) return false;
        return PickupController.Instance.TryPickUp(gameObject);
    }

    public override void OnPickedUp()
    {
        if (colliderToDisable != null)
            colliderToDisable.enabled = false;
    }

    public override void OnDropped()
    {
        if (colliderToDisable != null)
            colliderToDisable.enabled = true;
    }
}
