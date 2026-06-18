using UnityEngine;

/// <summary>
/// Moves a platform between waypoints (Patrol) or continuously upward (Elevator).
///
/// <b>Patrol</b>: ping-pongs between a local-space list of waypoints. Pauses briefly at
/// each end. Use for side-to-side or up-down patrol platforms in the parkour sections.
///
/// <b>Elevator</b>: moves upward continuously from its start position to an offset height,
/// then snaps back to the start and repeats. Players must time their jump onto it.
/// The snap-back is instant; the platform is not a carrier during the reset.
///
/// <b>Player carrying</b>: exposes <see cref="Velocity"/> (world-space, units/s).
/// <see cref="FPController_CC"/> reads it via <c>OnControllerColliderHit</c> and adds
/// it to the player's own movement — no parenting needed.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    public enum PlatformMode { Patrol, Elevator }

    [Header("Mode")]
    [SerializeField] private PlatformMode mode = PlatformMode.Patrol;

    [Header("Patrol Settings")]
    [Tooltip("World-space waypoints. The platform visits them in order and ping-pongs back.")]
    [SerializeField] private Vector3[] waypoints = { Vector3.zero, new Vector3(0f, 0f, 5f) };

    [Tooltip("Wait time (seconds) when the platform reaches a waypoint end.")]
    [SerializeField] private float waitAtEnd = 0.5f;

    [Header("Elevator Settings")]
    [Tooltip("How far upward (local Y) the elevator travels before snapping back to start.")]
    [SerializeField] private float elevatorHeight = 10f;

    [Header("Shared")]
    [SerializeField] private float speed = 3f;

    /// <summary>Current world-space velocity. Read by FPController_CC to carry the player.</summary>
    public Vector3 Velocity { get; private set; }

    // Patrol state
    private int   _waypointIndex;
    private int   _direction = 1;      // +1 forward, -1 backward
    private float _waitTimer;

    // Elevator state
    private Vector3 _elevatorStart;
    private Vector3 _elevatorTop;
    private bool    _elevatorResetting;

    // Previous position for velocity calculation
    private Vector3 _prevPosition;

    private void Start()
    {
        _prevPosition   = transform.position;
        _elevatorStart  = transform.position;
        _elevatorTop    = _elevatorStart + Vector3.up * elevatorHeight;

        if (mode == PlatformMode.Patrol && waypoints.Length > 0)
            transform.position = waypoints[0];
    }

    private void Update()
    {
        switch (mode)
        {
            case PlatformMode.Patrol:   UpdatePatrol();   break;
            case PlatformMode.Elevator: UpdateElevator(); break;
        }

        Velocity      = (transform.position - _prevPosition) / Time.deltaTime;
        _prevPosition = transform.position;
    }

    // ── Patrol ────────────────────────────────────────────────────────────────

    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        int  targetIndex = Mathf.Clamp(_waypointIndex + _direction, 0, waypoints.Length - 1);
        Vector3 target   = waypoints[targetIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            _waypointIndex = targetIndex;

            bool atEnd = _waypointIndex == 0 || _waypointIndex == waypoints.Length - 1;
            if (atEnd)
            {
                _direction = -_direction;
                _waitTimer = waitAtEnd;
            }
        }
    }

    // ── Elevator ──────────────────────────────────────────────────────────────

    private void UpdateElevator()
    {
        if (_elevatorResetting)
        {
            // Instant snap back — player cannot be carried during reset.
            transform.position = _elevatorStart;
            _elevatorResetting = false;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _elevatorTop, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _elevatorTop) < 0.01f)
            _elevatorResetting = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (mode == PlatformMode.Patrol && waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.15f);
                if (i > 0) Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
            }
        }
        else if (mode == PlatformMode.Elevator)
        {
            Vector3 start = Application.isPlaying ? _elevatorStart : transform.position;
            Vector3 top   = start + Vector3.up * elevatorHeight;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, top);
            Gizmos.DrawWireCube(top, new Vector3(transform.localScale.x, 0.1f, transform.localScale.z));
        }
    }
#endif
}
