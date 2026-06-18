using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
    public static PickupController Instance { get; private set; }

    [SerializeField] private Transform holdPos;
    [SerializeField] private float throwForce = 500f;
    [SerializeField] private float rotationSensitivity = 1f;

    private FPController_CC _fpController;
    private Interactor _interactor;
    private Collider _playerCollider;
    private GameObject _heldObj;
    private Rigidbody _heldObjRb;
    private bool _canDrop = true;
    private int _holdLayer;
    private int _pickupFrame = -1;

    public bool IsHolding => _heldObj != null;

    /// <summary>The <see cref="InventoryItem"/> currently held, or null. Lets placement slots
    /// check what the player is carrying without reflecting into private state.</summary>
    public InventoryItem HeldItem => _heldObj != null ? _heldObj.GetComponent<InventoryItem>() : null;

    private void Awake()
    {
        Instance = this;
        _holdLayer = LayerMask.NameToLayer("holdLayer");
        _playerCollider = GetComponentInParent<CharacterController>();
        _fpController = GetComponentInParent<FPController_CC>();
        _fpController ??= FindFirstObjectByType<FPController_CC>();
        _interactor = GetComponent<Interactor>();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInteract += HandleInteractOrDrop;
            InputManager.Instance.OnAttack += HandleThrow;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInteract -= HandleInteractOrDrop;
            InputManager.Instance.OnAttack -= HandleThrow;
        }
    }

    private void Update()
    {
        if (_heldObj == null) return;
        MoveObject();
        RotateObject();
    }

    public bool TryPickUp(GameObject obj)
    {
        if (_heldObj != null) return false;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return false;

        _heldObj = obj;
        _heldObjRb = rb;
        _pickupFrame = Time.frameCount;
        _heldObjRb.isKinematic = true;
        _heldObj.transform.SetParent(holdPos);
        _heldObj.layer = _holdLayer;
        var interactable = _heldObj.GetComponent<Interactable>();
        interactable?.DisableInteraction();
        interactable?.OnPickedUp();

        if (_playerCollider != null)
            Physics.IgnoreCollision(_heldObj.GetComponent<Collider>(), _playerCollider, true);

        return true;
    }

    private void HandleInteractOrDrop()
    {
        // Pickup and drop both listen to OnInteract. When the Interactor picks an item up,
        // this handler fires on the same input event — ignore that frame so we don't
        // immediately drop what we just grabbed.
        if (_heldObj == null || !_canDrop || Time.frameCount == _pickupFrame) return;

        // If the player is looking at something interactable (e.g. a placement slot), let that
        // interaction handle E instead of dropping the held item.
        if (_interactor != null && _interactor.FocusedInteractable != null &&
            _interactor.FocusedInteractable.CanInteract()) return;

        StopClipping();
        DropObject();
    }

    /// <summary>Detaches and returns the currently held object without throwing it (used by
    /// placement slots that snap the item into place). Returns null if nothing is held.</summary>
    public GameObject ReleaseHeld()
    {
        if (_heldObj == null) return null;
        GameObject obj = _heldObj;
        DropObject();
        return obj;
    }

    private void HandleThrow()
    {
        if (_heldObj == null || !_canDrop) return;
        StopClipping();
        ThrowObject();
    }

    private void MoveObject()
    {
        _heldObj.transform.position = holdPos.position;
    }

    private void RotateObject()
    {
        bool rotateHeld = Keyboard.current != null && Keyboard.current[Key.R].isPressed;

        if (rotateHeld)
        {
            _canDrop = false;
            _fpController?.SetLookLocked(true);
            Vector2 look = InputManager.Instance != null ? InputManager.Instance.Look : Vector2.zero;
            _heldObj.transform.Rotate(Vector3.down, look.x * rotationSensitivity);
            _heldObj.transform.Rotate(Vector3.right, look.y * rotationSensitivity);
        }
        else
        {
            _canDrop = true;
            _fpController?.SetLookLocked(false);
        }
    }

    private void DropObject()
    {
        if (_playerCollider != null)
            Physics.IgnoreCollision(_heldObj.GetComponent<Collider>(), _playerCollider, false);
        _heldObj.layer = 0;
        _heldObjRb.isKinematic = false;
        _heldObj.transform.SetParent(null);
        var interactable = _heldObj.GetComponent<Interactable>();
        interactable?.EnableInteraction();
        interactable?.OnDropped();
        _fpController?.SetLookLocked(false);
        _heldObj = null;
        _heldObjRb = null;
    }

    private void ThrowObject()
    {
        if (_playerCollider != null)
            Physics.IgnoreCollision(_heldObj.GetComponent<Collider>(), _playerCollider, false);
        _heldObj.layer = 0;
        _heldObjRb.isKinematic = false;
        _heldObj.transform.SetParent(null);
        var interactable = _heldObj.GetComponent<Interactable>();
        interactable?.EnableInteraction();
        interactable?.OnDropped();
        _fpController?.SetLookLocked(false);
        _heldObjRb.AddForce(holdPos.forward * throwForce);
        _heldObj = null;
        _heldObjRb = null;
    }

    private void StopClipping()
    {
        float clipRange = Vector3.Distance(_heldObj.transform.position, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, clipRange);
        if (hits.Length > 1)
            _heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
    }
}
