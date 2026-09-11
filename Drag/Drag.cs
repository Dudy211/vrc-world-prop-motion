using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum Language
{
    English,
    Japanese,
    Chinese
}

public enum DestinationMode
{
    OffsetVector,
    DestinationTransform,
    PathPoints
}

public enum MotionMode
{
    Move,
    Rotate
}

public enum RotatePathMode
{
    Spherical,
    Axis
}

public enum MovePathMode
{
    Linear,
    Curve
}

public enum SpeedMode
{
    Fixed,
    Curve
}

public enum InteractionMode
{
    Interact,
    Drag
}

public enum LoopMode
{
    Loop,
    PingPong
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Drag : UdonSharpBehaviour
{
    [Tooltip("Language for logs / UI")]
    public Language language = Language.English;

    public Transform target;
    public DestinationMode destinationMode;
    public Transform destinationTransform;
    public Vector3 relativePositionOffset;
    public Vector3 relativeRotationOffset;
    public Vector3 offsetVector;
    public Vector3 rotationVector;

    [Tooltip("How to trigger movement")]
    public InteractionMode interactionMode;

    [Tooltip("Trigger object: clicked in Interact mode, grabbed & dragged in Drag mode")]
    public Transform trigger;

    [Tooltip("Path points (used in PathPoints mode)")]
    public Transform[] pathPoints;

    [Tooltip("Loop = repeat path freely; PingPong = stay on the start<->end track")]
    public LoopMode loopMode;

    [Tooltip("UdonBehaviour to call OnReachDestination() on when reaching the end")]
    public UdonBehaviour onReachDestination;

    [Tooltip("Move = translate along path; Rotate = only orientation changes")]
    public MotionMode motionMode = MotionMode.Move;

    [Tooltip("Spherical = slerp start->end orientation; Axis = spin around the axis line")]
    public RotatePathMode rotatePathMode = RotatePathMode.Spherical;

    [Tooltip("Axis rotation: start point of the axis line (world space, draggable in Scene)")]
    public Vector3 axisStart = Vector3.zero;

    [Tooltip("Axis rotation: end point of the axis line (world space, draggable in Scene)")]
    public Vector3 axisEnd = new Vector3(0f, 1f, 0f);

    [Tooltip("Axis rotation: total angle in degrees over the full trip (multi-turn allowed, e.g. 3600)")]
    public float axisAngle = 360f;

    public MovePathMode movePathMode = MovePathMode.Linear;

    [Tooltip("In Curve mode: arch height (1 = full path length), projected perpendicular to the baseline")]
    public AnimationCurve moveCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    [Tooltip("Auto place origin P at the midpoint of start and end")]
    public bool autoCurveOrigin = true;

    [Tooltip("Origin P of the affine coordinate system (world space)")]
    public Vector3 curveOrigin = Vector3.zero;

    public SpeedMode speedMode;
    public float moveSpeed = 1.0f;
    public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Tooltip("Rail friction: damps the extra velocity produced by gravity (m/s^2)")]
    public float drag = 0.0f;

    [Tooltip("Cart on a rigid rail: gravity accelerates downhill / decelerates uphill along the path")]
    public float gravityScale = 1.0f;

    public bool previewDestination;
    public bool previewPath;
    [Tooltip("Offset mode only: drag the destination directly in the Scene view (requires Preview Destination)")]
    public bool editEndPosition;

    [UdonSynced]
    private float _t = 0f;

    [UdonSynced]
    private bool _isMoving = false;

    [UdonSynced]
    private int _direction = 1;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _endPosition;
    private Vector3 _triggerStartPosition;
    private bool _isOwner = false;
    private VRC_Pickup _triggerPickup;
    private bool _triggerIsPickup = false;
    private VRCPlayerApi _lastHolder = null;
    private float _curveStartValue = 0f;
    private float _curveEndValue = 0f;
    private float _v = 0f;
    private float _pathLength = 1f;
    private Vector3 _centerArm;
    private Vector3 _dragDirection = Vector3.forward;
    private float _dragLength = 1f;

    void Start()
    {
        CachePositions();
        if (target != null)
        {
            Debug.Log("[Drag] " + target.name + " 轨道中心: " + GetObjectCenter(target) + "（若此点在碰撞箱上而网格不动，说明网格不在 target 的子层级中）");
        }
        _isOwner = Networking.IsOwner(gameObject);
        if (trigger != null)
        {
            _triggerPickup = trigger.GetComponent<VRC_Pickup>();
            _triggerIsPickup = _triggerPickup != null;
        }
        if (interactionMode == InteractionMode.Drag && !_triggerIsPickup)
        {
            Debug.LogWarning("[Drag] 拖动模式下，触发物体需要挂 VRC_Pickup（含 Collider + Rigidbody）");
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        _isOwner = Networking.IsOwner(gameObject);
    }

    public override void OnDeserialization()
    {
        if (!_isOwner && target != null)
        {
            ApplyPositionAndRotation(_t);
        }
    }

    public override void Interact()
    {
        if (interactionMode == InteractionMode.Interact)
        {
            TryStartDrag();
        }
    }

    public void TryStartDrag()
    {
        if (target == null) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer != null && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(localPlayer, gameObject);
        }
        _isOwner = true;
        _v = 0f;

        if (loopMode == LoopMode.PingPong)
        {
            if (!_isMoving)
            {
                _direction = (_t >= 1f) ? -1 : 1;
                _isMoving = true;
            }
            else
            {
                _direction = -_direction;
            }
            _t = Mathf.Clamp01(_t);
        }
        else
        {
            CachePositions();
            _t = 0f;
            _isMoving = true;
            _direction = 1;
        }
        RequestSerialization();
    }

    private void CachePositions()
    {
        _startPosition = GetObjectCenter(target);
        _startRotation = target.rotation;
        _endPosition = GetDestinationPosition();
        if (moveCurve != null)
        {
            _curveStartValue = moveCurve.Evaluate(0f);
            _curveEndValue = moveCurve.Evaluate(1f);
        }
        _centerArm = GetObjectCenter(target) - target.position;
        _pathLength = ComputePathLength();

        if (motionMode == MotionMode.Rotate)
        {
            if (rotatePathMode == RotatePathMode.Axis)
            {
                Vector3 axis = axisEnd - axisStart;
                _dragLength = axis.magnitude;
                _dragDirection = (_dragLength > 1e-6f) ? axis / _dragLength : Vector3.up;
            }
            else
            {
                _dragLength = rotationVector.magnitude;
                _dragDirection = (_dragLength > 1e-6f) ? rotationVector / _dragLength : Vector3.up;
            }
        }
        else
        {
            Vector3 path = _endPosition - _startPosition;
            _dragLength = path.magnitude;
            _dragDirection = (_dragLength > 1e-6f) ? path / _dragLength : Vector3.forward;
        }
        if (_dragLength < 0.001f) _dragLength = 0.001f;

        if (interactionMode == InteractionMode.Drag && trigger != null)
        {
            _triggerStartPosition = trigger.position - _dragDirection * (_t * _dragLength);
        }
    }

    private Vector3 GetObjectCenter(Transform obj)
    {
        if (obj == null) return Vector3.zero;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        bool found = false;

        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(false);
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer r = rends[i];
            if (r == null) continue;
            Bounds b = r.bounds;
            min = Vector3.Min(min, b.min);
            max = Vector3.Max(max, b.max);
            found = true;
        }
        if (!found)
        {
            Collider[] cols = obj.GetComponentsInChildren<Collider>(false);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] == null) continue;
                Bounds b = cols[i].bounds;
                min = Vector3.Min(min, b.min);
                max = Vector3.Max(max, b.max);
                found = true;
            }
        }
        if (!found) return obj.position;
        return (min + max) * 0.5f;
    }

    private float ComputePathLength()
    {
        if (motionMode == MotionMode.Rotate) return 1f;
        if (destinationMode == DestinationMode.PathPoints && pathPoints != null && pathPoints.Length >= 2)
        {
            float len = 0f;
            for (int i = 0; i + 1 < pathPoints.Length; i++)
            {
                if (pathPoints[i] != null && pathPoints[i + 1] != null)
                {
                    len += Vector3.Distance(GetObjectCenter(pathPoints[i]), GetObjectCenter(pathPoints[i + 1]));
                }
            }
            return Mathf.Max(len, 0.001f);
        }
        return Mathf.Max(Vector3.Distance(_startPosition, _endPosition), 0.001f);
    }

    private void Update()
    {
        if (target == null) return;

        if (interactionMode == InteractionMode.Drag)
        {
            if (!_triggerIsPickup) return;

            VRCPlayerApi holder = _triggerPickup.currentPlayer;
            if (holder != null && _lastHolder == null)
            {
                _triggerStartPosition = trigger.position - _dragDirection * (_t * _dragLength);
            }
            if (holder == Networking.LocalPlayer && !Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            _lastHolder = holder;
        }

        if (!_isOwner) return;

        if (interactionMode == InteractionMode.Interact)
        {
            if (!_isMoving) return;
            UpdateAutoMovement();
        }
        else
        {
            UpdateDragMovement();
        }
    }

    private void UpdateAutoMovement()
    {
        float baseSpeed = (speedMode == SpeedMode.Fixed)
            ? moveSpeed
            : speedCurve.Evaluate(_t) * moveSpeed;

        if (motionMode == MotionMode.Move)
        {
            Vector3 tangent = GetTangent(_t);
            _v += Vector3.Dot(Physics.gravity * gravityScale, tangent) * Time.deltaTime;
        }
        if (drag > 0f)
        {
            _v = Mathf.MoveTowards(_v, 0f, drag * Time.deltaTime);
        }

        _t += (baseSpeed * _direction + _v) * Time.deltaTime / _pathLength;

        bool reachedEnd = false;
        if (_direction > 0 && _t >= 1f)
        {
            _t = 1f;
            reachedEnd = true;
        }
        else if (_direction < 0 && _t <= 0f)
        {
            _t = 0f;
            reachedEnd = true;
        }
        else
        {
            _t = Mathf.Clamp01(_t);
        }

        ApplyPositionAndRotation(_t);

        if (reachedEnd)
        {
            HandleReachedEnd();
        }

        RequestSerialization();
    }

    private void UpdateDragMovement()
    {
        if (trigger == null) return;

        Vector3 triggerDelta = trigger.position - _triggerStartPosition;
        if (_dragLength < 0.001f) return;

        float progress = Vector3.Dot(triggerDelta, _dragDirection) / _dragLength;

        if (loopMode == LoopMode.Loop)
        {
            _t = Mathf.Repeat(progress, 1f);
        }
        else
        {
            _t = Mathf.Clamp01(progress);
        }
        _isMoving = (progress < 1f);
        ApplyPositionAndRotation(_t);
        RequestSerialization();
    }

    private void HandleReachedEnd()
    {
        _isMoving = false;
        _v = 0f;
        ApplyPositionAndRotation(_t);
        InvokeReachEvent();
    }

    private void InvokeReachEvent()
    {
        if (onReachDestination != null)
        {
            onReachDestination.SendCustomEvent("OnReachDestination");
        }

        switch (language)
        {
            case Language.English:
                Debug.Log("[Drag] Reached destination");
                break;
            case Language.Japanese:
                Debug.Log("[Drag] 目的地に到着しました");
                break;
            case Language.Chinese:
                Debug.Log("[Drag] 已到达目的地");
                break;
        }
    }

    private void ApplyPositionAndRotation(float t)
    {
        Quaternion rot = GetRotationAt(t);
        Vector3 arm = rot * Quaternion.Inverse(_startRotation) * _centerArm;
        target.rotation = rot;
        target.position = GetPositionAt(t) - arm;
    }

    private Quaternion GetRotationAt(float t)
    {
        if (motionMode == MotionMode.Rotate)
        {
            if (rotatePathMode == RotatePathMode.Axis)
            {
                Vector3 axis = axisEnd - axisStart;
                if (axis.sqrMagnitude < 1e-8f) return _startRotation;
                return _startRotation * Quaternion.AngleAxis(axisAngle * t, axis.normalized);
            }
            return Quaternion.Slerp(_startRotation, _startRotation * Quaternion.Euler(rotationVector), t);
        }

        if (destinationMode == DestinationMode.DestinationTransform && destinationTransform != null)
        {
            return Quaternion.Slerp(_startRotation, destinationTransform.rotation, t)
                   * Quaternion.Euler(relativeRotationOffset * t);
        }
        return _startRotation * Quaternion.Euler(rotationVector * t);
    }

    private Vector3 GetTangent(float t)
    {
        float e = 0.01f;
        float t0 = Mathf.Clamp01(t);
        float t1 = Mathf.Clamp01(t + e);
        Vector3 d = GetPositionAt(t1) - GetPositionAt(t0);
        if (d.sqrMagnitude < 1e-8f)
        {
            float t2 = Mathf.Clamp01(t - e);
            d = GetPositionAt(t0) - GetPositionAt(t2);
        }
        return (d.sqrMagnitude > 1e-8f) ? d.normalized : Vector3.forward;
    }

    private Vector3 GetPositionAt(float t)
    {
        if (motionMode == MotionMode.Rotate) return _startPosition;

        if (destinationMode == DestinationMode.PathPoints && pathPoints != null && pathPoints.Length >= 2)
        {
            return GetPathPointsPosition(t);
        }

        Vector3 pos;
        if (movePathMode == MovePathMode.Curve)
        {
            Vector3 P = GetCurveOrigin();
            Vector3 X = _startPosition - P;
            Vector3 Y = _endPosition - P;

            Vector3 planeNormal = Vector3.Cross(X, Y);
            if (planeNormal.sqrMagnitude < 1e-6f) planeNormal = Vector3.Cross(X.normalized, Vector3.up);
            planeNormal.Normalize();

            Vector3 baseline = Vector3.Lerp(_startPosition, _endPosition, t);
            Vector3 baselineDir = _endPosition - _startPosition;
            float pathLength = baselineDir.magnitude;
            baselineDir = (pathLength > 1e-6f) ? baselineDir / pathLength : Vector3.forward;

            Vector3 perp = Vector3.Cross(baselineDir, planeNormal).normalized;

            float v = moveCurve.Evaluate(t) - Mathf.Lerp(_curveStartValue, _curveEndValue, t);
            pos = baseline + perp * (v * pathLength);
        }
        else
        {
            pos = Vector3.Lerp(_startPosition, _endPosition, t);
        }
        return pos;
    }

    private Vector3 GetPathPointsPosition(float t)
    {
        int segments = pathPoints.Length - 1;
        float scaled = t * segments;
        int index = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, segments - 1);
        float localT = scaled - index;

        Vector3 a = GetObjectCenter(pathPoints[index]);
        Vector3 b = GetObjectCenter(pathPoints[index + 1]);
        return Vector3.Lerp(a, b, localT);
    }

    private Vector3 GetCurveOrigin()
    {
        if (autoCurveOrigin)
        {
            return (_startPosition + _endPosition) * 0.5f;
        }
        return curveOrigin;
    }

    private Vector3 GetDestinationPosition()
    {
        if (motionMode == MotionMode.Rotate) return _startPosition;
        if (destinationMode == DestinationMode.DestinationTransform && destinationTransform != null)
        {
            return GetObjectCenter(destinationTransform) + relativePositionOffset;
        }
        return _startPosition + offsetVector;
    }
}
