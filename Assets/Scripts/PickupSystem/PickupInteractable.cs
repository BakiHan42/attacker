using UnityEngine;

public class PickupInteractable : Interactable
{
    protected override bool HandleInteract()
    {
        if (PickupController.Instance == null) return false;
        return PickupController.Instance.TryPickUp(gameObject);
    }
}
