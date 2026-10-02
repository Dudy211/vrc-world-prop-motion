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

public enum PathBase
{
    Pivot,
    Center
}

public enum MotionMode
{
    Move,
    Rotate
}

public enum RotateMode
{
    Spherical,
    Axis
}

public enum AxisSourceMode
{
    Manual,
    ObjectAlign,
    Euler
}

public enum AxisUp
{
    X,
    Y,
    Z
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
    Drag,
    Passive
}

public enum RoleMode
{
    Receiver,
    Sender
}

public enum TriggerMoveMode
{
    Free,
    SyncTarget,
    Rail
}

public enum HoverMode
{
    None,
    Native,
    Custom
}

public enum LoopMode
{
    Loop,
    PingPong,
    Repeat,
    Once
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Drag : UdonSharpBehaviour
{
    [Tooltip("Language for logs / UI")]
    public Language language = Language.English;

    [Tooltip("Receiver = this component moves the target; Sender = sits on the clicked/grabbed object and fires the receiver")]
    public RoleMode role = RoleMode.Receiver;

    [Tooltip("Sender mode: the receiver Drag component to trigger")]
    public Drag receiver;

    public Transform target;

    [Tooltip("How to trigger movement")]
    public InteractionMode interactionMode;

    [Tooltip("Trigger object: clicked in Interact mode, grabbed & dragged in Drag mode. Passive mode: the zone below is used instead")]
    public Transform trigger;

    [Tooltip("Passive mode: trigger zone center, local offset from this GameObject (editor auto-syncs a BoxCollider)")]
    public Vector3 triggerZoneCenter = Vector3.zero;

    [Tooltip("Passive mode: trigger zone size in local meters")]
    public Vector3 triggerZoneSize = Vector3.one;

    [Tooltip("Internal: whether the zone has been initialized (auto-wrap once)")]
    public bool triggerZoneInit = false;

    [Tooltip("Drag mode: how the trigger handle itself moves (Free = carried by hand; SyncTarget = rigidly follows target; Rail = slides on the rail line)")]
    public TriggerMoveMode triggerMoveMode = TriggerMoveMode.Rail;

    [Header("Motion")]
    [Tooltip("Move = translate along path; Rotate = only orientation changes")]
    public MotionMode motionMode = MotionMode.Move;

    [Header("Rotation (Rotate mode only)")]
    [Tooltip("Spherical = slerp start->end orientation; Axis = spin around the axis line")]
    public RotateMode rotateMode = RotateMode.Spherical;

    [Tooltip("Spherical rotation: orientation offset from the start orientation to the end (Euler degrees)")]
    public Vector3 rotateVector;

    [Tooltip("Axis rotation: how the axis line is defined")]
    public AxisSourceMode axisSource = AxisSourceMode.Manual;

    [Tooltip("Manual mode: start point of the axis line (world space, draggable in Scene)")]
    public Vector3 axisStart = Vector3.zero;

    [Tooltip("Manual mode: end point of the axis line (world space, draggable in Scene)")]
    public Vector3 axisEnd = new Vector3(0f, 1f, 0f);

    [Tooltip("Axis line length in meters (all axis source modes)")]
    public float axisLength = 1f;

    [Tooltip("ObjectAlign mode: the axis aligns to one of its local axes and follows its position")]
    public Transform axisObject;

    [Tooltip("ObjectAlign mode: which local axis of the object is used as the axis direction (上方向)")]
    public AxisUp axisUp = AxisUp.Y;

    [Tooltip("World-space offset from the auto-computed axis center (draggable in Scene)")]
    public Vector3 axisPositionOffset = Vector3.zero;

    [Tooltip("Euler mode: 3D angle in degrees. Default frame: up = +Y, forward = -Z")]
    public Vector3 axisEuler = Vector3.zero;

    [Tooltip("Axis rotation: total angle in degrees over the full trip (multi-turn allowed, e.g. 3600)")]
    public float axisAngle = 360f;

    [Header("Movement (Move mode only)")]
    public DestinationMode destinationMode;
    public Transform destinationTransform;
    public Vector3 relativePositionOffset;
    public Vector3 relativeRotationOffset;
    public Vector3 offsetVector;

    [Tooltip("Move mode only: extra rotation applied while translating (Euler degrees, multiplied by progress)")]
    public Vector3 rotationVector;

    [Tooltip("Path points (used in PathPoints mode)")]
    public Transform[] pathPoints;

    [Tooltip("PathPoints mode: connect the last point back to the first (closed loop)")]
    public bool closePath = false;

    public MovePathMode movePathMode = MovePathMode.Linear;

    [Header("Common")]
    [Tooltip("Loop = repeat path freely; PingPong = stay on the start<->end track")]
    public LoopMode loopMode;

    [Tooltip("Path reference point: Pivot = transform position; Center = renderer bounds center")]
    public PathBase pathBase = PathBase.Pivot;

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

    [Tooltip("Minimum seconds between triggers, all trigger methods (0 = no cooldown)")]
    public float triggerCooldown = 0f;

    [Tooltip("Seconds to wait after a trigger before moving (0 = immediate; Drag mode ignores this)")]
    public float startDelay = 0f;

    [Tooltip("Master switch for events and Drag chains (off = they never fire)")]
    public bool useChainEvents = false;

    [Tooltip("UdonBehaviour to call OnReachDestination() on when reaching the end")]
    public UdonBehaviour onReachDestination;

    [Tooltip("UdonBehaviour to call OnReachStart() on when returning to the start (PingPong)")]
    public UdonBehaviour onReachStart;

    [Tooltip("Drag components to trigger (TryStartDrag) when reaching the END — domino chains")]
    public Drag[] nextOnReach;

    [Tooltip("Drag components to trigger (TryStartDrag) when returning to the START (PingPong)")]
    public Drag[] nextOnReturn;

    [Tooltip("UdonBehaviours called with OnPathPoint() when passing path point i (PathPoints mode, index matches pathPoints)")]
    public UdonBehaviour[] onPathPointEvents;

    [Tooltip("UdonBehaviour to call OnGrabbed() when the handle is grabbed (all clients)")]
    public UdonBehaviour onGrabbed;
    [Tooltip("UdonBehaviour to call OnReleased() when the handle is released (all clients)")]
    public UdonBehaviour onReleased;

    [Tooltip("Enable sound effects (requires an AudioSource on this GameObject)")]
    public bool useSounds = false;

    [Tooltip("Played locally when reaching the end (put an AudioSource on this GameObject)")]
    public AudioClip reachSound;
    [Tooltip("Played locally when returning to the start")]
    public AudioClip returnSound;
    [Tooltip("Played locally when the handle is grabbed")]
    public AudioClip grabSound;
    [Tooltip("Played locally when the handle is released")]
    public AudioClip releaseSound;

    [Tooltip("Owner sends sync at most every N frames (1 = every frame)")]
    public int syncEveryNFrames = 1;

    [Tooltip("Force a sync when |progress| changed more than this since the last send")]
    public float syncMinDelta = 0.002f;

    [Tooltip("Drag mode: after release, progress returns to 0 at this speed along the path (units/sec; 0 = stay where released)")]
    public float snapBackSpeed = 0f;

    [Tooltip("Drag mode: while someone holds the handle, other players cannot trigger via click / proximity")]
    public bool lockWhileHeld = false;

    [Tooltip("Hover feedback: None / Native (VRChat default prompt, needs Interactive layer) / Custom (raycast + your own visuals)")]
    public HoverMode hoverMode = HoverMode.None;

    [Tooltip("Shown while the local player aims at the trigger (hidden otherwise); build your own text / outline / glow")]
    public GameObject hoverVisual;

    [Tooltip("Max hover distance in meters")]
    public float hoverDistance = 2f;

    [Tooltip("Tint the target's renderers while hovered (per-client, via MaterialPropertyBlock)")]
    public bool hoverHighlight = false;

    public Color highlightColor = new Color(0.3f, 1f, 1f);

    [Tooltip("Pulse the highlight color")]
    public bool highlightBlink = true;

    public float highlightBlinkSpeed = 3f;

    [Tooltip("Show a world-space text prompt above the object while hovered (child 'HoverPrompt', auto-created by the editor)")]
    public bool hoverText = false;

    [Tooltip("Prompt text; empty = USE / 使用 by language")]
    public string hoverTextContent = "";

    public float hoverTextSize = 0.1f;

    public Color hoverTextColor = Color.white;

    [Tooltip("Extra offset from the auto position (bounds top)")]
    public Vector3 hoverTextOffset = Vector3.zero;

    [Tooltip("Optional font; null = built-in")]
    public Font hoverFont;

    [Tooltip("Editor: ghost wireframe slices between start and end (0 = off)")]
    public int previewGhostSteps = 0;

    [Tooltip("Editor: show previews of chained Drags (levels up/downstream, 0 = off)")]
    public int previewRelationDepth = 0;

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

    [UdonSynced]
    private bool _onceDone = false;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _endPosition;
    private Vector3 _triggerStartPosition;
    private bool _isOwner = false;
    private VRC_Pickup _triggerPickup;
    private bool _triggerIsPickup = false;
    private Rigidbody _triggerRb;
    private Vector3 _triggerRestPosition;
    private Quaternion _triggerRestRotation;
    private VRCPlayerApi _lastHolder = null;
    private Vector3 _triggerRestLocalPos;
    private Quaternion _triggerRestLocalRot;
    private float _curveStartValue = 0f;
    private float _curveEndValue = 0f;
    private float _v = 0f;
    private float _pathLength = 1f;
    private Vector3 _centerArm;
    private Vector3 _dragDirection = Vector3.forward;
    private float _dragLength = 1f;
    private Drag _sender;
    private TriggerMoveMode _effectiveTriggerMode = TriggerMoveMode.Rail;
    private Vector3 _railDirection = Vector3.forward;
    private float _railLength = 1f;
    private Vector3 _axisAnchor = Vector3.zero;
    private Vector3 _axisDir = Vector3.up;
    private float _axisLen = 1f;
    private bool _axisDynamic = false;
    private float _remoteT = 0f;
    private float _displayT = 0f;
    private bool _hasRemoteTarget = false;
    private int _framesSinceSync = 0;
    private float _lastSentT = -1f;
    private bool _snapping = false;
    private int _lastPathSeg = -1;
    private bool _dragEndFired = false;
    private bool _dragStartFired = true;
    private float _cooldownUntil = 0f;
    private float _delayRemaining = 0f;
    private bool _remoteEndPlayed = false;
    private bool _remoteStartPlayed = false;
    private AudioSource _audio = null;
    private bool _hoverShown = false;
    private bool _zoneInside = false;
    private Renderer[] _targetRenderers = null;
    private Color[] _baseColors = null;
    private GameObject _hoverTextGo = null; // 仅 SetActive/朝向；文字配置由编辑器在编辑时写入

    void Start()
    {
        if (role == RoleMode.Sender) return;

        _displayT = _t;
        CachePositions();
        _dragStartFired = (_t <= 0f);
        _dragEndFired = (_t >= 1f);
        if (target != null)
        {
            Debug.Log("[Drag] " + target.name + " 基准: " + pathBase + " | 起点: " + _startPosition + " | 终点: " + _endPosition + " | 偏移: " + (destinationMode == DestinationMode.DestinationTransform ? relativePositionOffset : offsetVector));
        }
        _isOwner = Networking.IsOwner(gameObject);
        if (trigger != null)
        {
            _triggerPickup = trigger.GetComponent<VRC_Pickup>();
            _triggerIsPickup = _triggerPickup != null;
            _triggerRb = trigger.GetComponent<Rigidbody>();
            _triggerRestPosition = trigger.position;
            _triggerRestRotation = trigger.rotation;
            if (target != null)
            {
                _triggerRestLocalPos = target.InverseTransformPoint(trigger.position);
                _triggerRestLocalRot = Quaternion.Inverse(target.rotation) * trigger.rotation;
            }
        }
        if (interactionMode == InteractionMode.Drag && !_triggerIsPickup)
        {
            Debug.LogWarning("[Drag] 拖动模式下，触发物体需要挂 VRC_Pickup（含 Collider + Rigidbody）");
        }
        if (hoverVisual != null) hoverVisual.SetActive(false);
        if (interactionMode == InteractionMode.Passive && role == RoleMode.Receiver && target != null)
        {
            _zoneInside = IsLocalPlayerInZone(); // 进入沿才触发：出生/传送在区域内不算
        }
        if (hoverHighlight && target != null)
        {
            _targetRenderers = target.GetComponentsInChildren<Renderer>(false);
            _baseColors = new Color[_targetRenderers.Length];
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                Material m = _targetRenderers[i].sharedMaterial;
                _baseColors[i] = (m != null && m.HasProperty("_Color")) ? m.color : Color.white;
            }
        }
        var htChild = transform.Find("HoverPrompt");
        if (htChild != null) _hoverTextGo = htChild.gameObject;
        if (_hoverTextGo != null) _hoverTextGo.SetActive(false);
        if (useSounds && (reachSound != null || returnSound != null || grabSound != null || releaseSound != null))
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
            {
                Debug.LogWarning("[Drag] 配置了音效但本物体没有 AudioSource，请在同一 GameObject 添加 AudioSource");
            }
        }
        SetupRail();
    }

    private void SetupRail()
    {
        _railDirection = _dragDirection;
        _railLength = _dragLength;

        if (trigger == null) return;
        _sender = trigger.GetComponent<Drag>();
        if (_sender == null || _sender.role != RoleMode.Sender) return;

        _effectiveTriggerMode = _sender.triggerMoveMode;
        Vector3 rail = SenderRailEnd(_sender) - SenderRailStart(_sender);
        _railLength = rail.magnitude;
        _railDirection = (_railLength > 1e-6f) ? rail / _railLength : Vector3.forward;
        if (_railLength < 0.001f) _railLength = 0.001f;
    }

    private Vector3 SenderRailStart(Drag sender)
    {
        return sender.pathBase == PathBase.Center ? GetObjectCenter(sender.transform) : sender.transform.position;
    }

    private Vector3 SenderRailEnd(Drag sender)
    {
        if (sender.destinationMode == DestinationMode.DestinationTransform && sender.destinationTransform != null)
        {
            Transform dt = sender.destinationTransform;
            Vector3 dp = sender.pathBase == PathBase.Center ? GetObjectCenter(dt) : dt.position;
            return dp + sender.relativePositionOffset;
        }
        return SenderRailStart(sender) + sender.offsetVector;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        _isOwner = Networking.IsOwner(gameObject);
        if (_isOwner)
        {
            // 接管：以当前同步值为准，避免新旧主人进度不一致
            _displayT = _t;
        }
    }

    public override void OnDeserialization()
    {
        if (!_isOwner && target != null)
        {
            _remoteT = _t;
            if (!_hasRemoteTarget)
            {
                _hasRemoteTarget = true;
                _displayT = _t;
                ApplyPositionAndRotation(_displayT);
            }
        }
    }

    public override void Interact()
    {
        if (role == RoleMode.Sender)
        {
            if (receiver != null)
            {
                receiver.TryStartDrag();
            }
            return;
        }
        if (interactionMode == InteractionMode.Interact)
        {
            TryStartDrag();
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid() || !player.isLocal) return;
        if (role == RoleMode.Sender)
        {
            // 发送端被动触发：TriggerZone 子物体上的发送端 Drag 收到进入事件后触发本接收端
            if (receiver != null) receiver.TryStartDrag();
            return;
        }
        if (interactionMode != InteractionMode.Passive) return;
        TryStartDrag();
    }

    public void TryStartDrag()
    {
        if (target == null) return;
        if (loopMode == LoopMode.Once && _onceDone) return; // 单次使命已完成，不再响应

        if (lockWhileHeld && _triggerIsPickup && _triggerPickup != null)
        {
            VRCPlayerApi holderNow = _triggerPickup.currentPlayer;
            if (holderNow != null && !holderNow.isLocal) return; // 他人抓住期间禁止远程触发
        }
        if (triggerCooldown > 0f && Time.time < _cooldownUntil) return; // 触发冷却

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer != null && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(localPlayer, gameObject);
        }
        _isOwner = true;
        _v = 0f;
        _snapping = false;
        _cooldownUntil = Time.time + Mathf.Max(0f, triggerCooldown);

        if (startDelay > 0f && interactionMode != InteractionMode.Drag)
        {
            _delayRemaining = Mathf.Max(0f, startDelay);
            RequestSerialization();
            return;
        }
        StartMotion();
    }

    private void StartMotion()
    {
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
        else if (loopMode == LoopMode.Repeat)
        {
            // 重复：基准固定在 Start 时（CachePositions 已在 Start 执行），
            // 重触发回到原始起点重播，绝不累加漂移
            _t = 0f;
            _isMoving = true;
            _direction = 1;
        }
        else
        {
            // 循环：只有静止时才重取基准（从当前位置续走）；
            // 运动中再次触发保留原始起终点、从头重播，避免终点漂移
            if (!_isMoving) CachePositions();
            _t = 0f;
            _isMoving = true;
            _direction = 1;
        }
        RequestSerialization();
    }

    public void SetProgress(float t)
    {
        if (role == RoleMode.Sender) return;
        if (target == null) return;
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer != null && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(localPlayer, gameObject);
        }
        _isOwner = true;
        _isMoving = false;
        _v = 0f;
        _snapping = false;
        _onceDone = false; // 外部 SetProgress 可解除单次锁定（重置谜题用）
        _t = Mathf.Clamp01(t);
        ApplyPositionAndRotation(_t);
        RequestSerialization();
    }

    public void GoToStart()
    {
        SetProgress(0f);
    }

    public void GoToEnd()
    {
        SetProgress(1f);
    }

    private bool ByCenter()
    {
        return pathBase == PathBase.Center;
    }

    private Vector3 RefPoint(Transform obj)
    {
        if (obj == null) return Vector3.zero;
        return ByCenter() ? GetObjectCenter(obj) : obj.position;
    }

    private void ComputeAxisLine()
    {
        _axisLen = Mathf.Max(axisLength, 0.001f);

        if (axisSource == AxisSourceMode.ObjectAlign && axisObject != null)
        {
            Vector3 local = axisUp == AxisUp.X ? Vector3.right : (axisUp == AxisUp.Z ? Vector3.forward : Vector3.up);
            _axisDir = (axisObject.rotation * local).normalized;
            _axisAnchor = RefPoint(axisObject) + axisPositionOffset;
            return;
        }

        if (axisSource == AxisSourceMode.Euler)
        {
            _axisDir = (Quaternion.Euler(axisEuler) * Vector3.up).normalized;
            _axisAnchor = RefPoint(target) + axisPositionOffset;
            return;
        }

        Vector3 d = axisEnd - axisStart;
        _axisDir = (d.sqrMagnitude > 1e-8f) ? d.normalized : Vector3.up;
        _axisAnchor = axisStart;
    }

    private void CachePositions()
    {
        _startPosition = RefPoint(target);
        _startRotation = target.rotation;
        _endPosition = GetDestinationPosition();
        if (moveCurve != null)
        {
            _curveStartValue = moveCurve.Evaluate(0f);
            _curveEndValue = moveCurve.Evaluate(1f);
        }
        _centerArm = ByCenter() ? GetObjectCenter(target) - target.position : Vector3.zero;
        _pathLength = ComputePathLength();
        _lastPathSeg = CurrentPathSeg();

        if (motionMode == MotionMode.Rotate)
        {
            if (rotateMode == RotateMode.Axis)
            {
                ComputeAxisLine();
                // 轴定义在目标自身/子级时，旋转会带动锚点形成反馈（抽搐/位移），
                // 只有轴来自目标之外（如独立的对齐物体且它自己还会动）才每帧跟随
                _axisDynamic = axisObject != null && !axisObject.IsChildOf(target);
                _dragLength = _axisLen;
                _dragDirection = _axisDir;
            }
            else
            {
                _dragLength = rotateVector.magnitude;
                _dragDirection = (_dragLength > 1e-6f) ? rotateVector / _dragLength : Vector3.up;
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

    private int PathSegments()
    {
        if (pathPoints == null || pathPoints.Length < 2) return 0;
        return closePath ? pathPoints.Length : pathPoints.Length - 1;
    }

    private float ComputePathLength()
    {
        if (motionMode == MotionMode.Rotate) return 1f;
        if (destinationMode == DestinationMode.PathPoints && pathPoints != null && pathPoints.Length >= 2)
        {
            int segments = PathSegments();
            float len = 0f;
            for (int i = 0; i < segments; i++)
            {
                Transform a = pathPoints[i];
                Transform b = pathPoints[(i + 1) % pathPoints.Length];
                if (a != null && b != null)
                {
                    len += Vector3.Distance(RefPoint(a), RefPoint(b));
                }
            }
            return Mathf.Max(len, 0.001f);
        }
        return Mathf.Max(Vector3.Distance(_startPosition, _endPosition), 0.001f);
    }

    private int CurrentPathSeg()
    {
        if (destinationMode != DestinationMode.PathPoints || pathPoints == null || pathPoints.Length < 2) return -1;
        int segments = PathSegments();
        return Mathf.Clamp(Mathf.FloorToInt(_t * segments), 0, segments - 1);
    }

    private void FirePathPointEvents(int prevSeg, int newSeg)
    {
        if (!useChainEvents || onPathPointEvents == null || onPathPointEvents.Length == 0) return;
        if (prevSeg < 0 || newSeg <= prevSeg) return;
        for (int k = prevSeg + 1; k <= newSeg && k < onPathPointEvents.Length; k++)
        {
            if (onPathPointEvents[k] != null)
            {
                onPathPointEvents[k].SendCustomEvent("OnPathPoint");
            }
        }
    }

    private void Update()
    {
        if (role == RoleMode.Sender) return;
        if (target == null) return;

        if (hoverMode == HoverMode.Custom) UpdateCustomHover(); // 每个客户端各自检测自己的准星（无同步开销）
        if (interactionMode == InteractionMode.Passive && role == RoleMode.Receiver)
        {
            bool inside = IsLocalPlayerInZone();
            if (inside && !_zoneInside) TryStartDrag(); // 进入沿触发一次
            _zoneInside = inside;
        }

        if (!_isOwner)
        {
            UpdateRemoteSmoothing();
            return;
        }

        if (_delayRemaining > 0f)
        {
            _delayRemaining -= Time.deltaTime;
            if (_delayRemaining > 0f) return;
            _delayRemaining = 0f;
            StartMotion();
            return;
        }

        if (_snapping)
        {
            UpdateSnapBack();
            return;
        }

        if (interactionMode == InteractionMode.Drag) return; // Drag 由把手驱动；Interact/Passive 走自动移动
        if (!_isMoving) return;
        UpdateAutoMovement();
    }

    // 自定义悬停：本地玩家头部射线命中本物体碰撞体时显示 hoverVisual
    private void UpdateCustomHover()
    {
        if (hoverVisual == null) return;
        VRCPlayerApi lp = Networking.LocalPlayer;
        if (lp == null || !lp.IsValid()) return;

        var tracking = lp.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        bool hit = false;
        RaycastHit h;
        if (!Physics.Raycast(tracking.position, tracking.rotation * Vector3.forward, out h, Mathf.Max(0.1f, hoverDistance)))
        {
            if (_hoverShown || hoverVisual != null) { } // 未命中：走下方统一处理
        }
        else
        {
            Collider own = GetComponent<Collider>();
            if (own != null)
            {
                hit = (h.collider == own) || h.collider.transform.IsChildOf(transform);
            }
            else if (target != null)
            {
                // 本物体无碰撞体：回退检测目标物体（无需 BoxCollider）
                hit = (h.collider.transform == target) || h.collider.transform.IsChildOf(target);
            }
        }
        if (hoverVisual != null && hit != _hoverShown)
        {
            _hoverShown = hit;
            hoverVisual.SetActive(hit);
        }
        ApplyHighlight(hit);
        ApplyHoverText(hit, lp);
    }

    private void ApplyHighlight(bool hit)
    {
        if (!hoverHighlight || _targetRenderers == null) return;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        float k = 1f;
        if (highlightBlink)
            k = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Max(0.1f, highlightBlinkSpeed) * 6.283f);
        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            if (_targetRenderers[i] == null) continue;
            block.Clear();
            Color c = hit ? Color.Lerp(_baseColors[i], highlightColor, k) : _baseColors[i];
            block.SetColor("_Color", c);
            _targetRenderers[i].SetPropertyBlock(block);
        }
    }

    private void ApplyHoverText(bool hit, VRCPlayerApi lp)
    {
        if (_hoverTextGo == null) return;
        if (_hoverTextGo.activeSelf != hit) _hoverTextGo.SetActive(hit);
        if (!hit) return;
        var tracking = lp.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        Vector3 dir = _hoverTextGo.transform.position - tracking.position;
        if (dir.sqrMagnitude > 1e-6f)
            _hoverTextGo.transform.rotation = Quaternion.LookRotation(dir);
    }

    // 被动触发：纯位置检测，无需碰撞体。本地玩家头部位置变换到本地空间后与区域箱体做包含测试。
    private bool IsLocalPlayerInZone()
    {
        VRCPlayerApi lp = Networking.LocalPlayer;
        if (lp == null || !lp.IsValid() || !lp.isLocal) return false;
        var tracking = lp.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        Vector3 local = transform.InverseTransformPoint(tracking.position);
        return Mathf.Abs(local.x - triggerZoneCenter.x) <= triggerZoneSize.x * 0.5f
            && Mathf.Abs(local.y - triggerZoneCenter.y) <= triggerZoneSize.y * 0.5f
            && Mathf.Abs(local.z - triggerZoneCenter.z) <= triggerZoneSize.z * 0.5f;
    }

    private void UpdateRemoteSmoothing()
    {
        if (!_hasRemoteTarget) return;
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        _displayT = Mathf.Lerp(_displayT, _remoteT, k);
        if (Mathf.Abs(_displayT - _remoteT) < 0.0005f) _displayT = _remoteT;
        ApplyPositionAndRotation(_displayT);

        // 非主人端到达两端时本地播放音效（到达事件本身仍由主人端经同步/链触发）
        if (_displayT >= 1f) { if (!_remoteEndPlayed) { _remoteEndPlayed = true; PlayClip(reachSound); } }
        else _remoteEndPlayed = false;
        if (_displayT <= 0f) { if (!_remoteStartPlayed) { _remoteStartPlayed = true; PlayClip(returnSound); } }
        else _remoteStartPlayed = false;
    }

    private void UpdateSnapBack()
    {
        if (interactionMode != InteractionMode.Drag) { _snapping = false; return; }
        if (loopMode == LoopMode.Once && _onceDone) { _snapping = false; return; } // 完成使命不回弹
        if (_triggerPickup != null && _triggerPickup.currentPlayer != null) { _snapping = false; return; }

        _t = Mathf.MoveTowards(_t, 0f, snapBackSpeed * Time.deltaTime / Mathf.Max(_pathLength, 0.001f));
        _isMoving = _t > 0f;
        ApplyPositionAndRotation(_t);

        if (trigger != null)
        {
            if (_effectiveTriggerMode == TriggerMoveMode.SyncTarget && target != null)
            {
                trigger.position = target.TransformPoint(_triggerRestLocalPos);
                trigger.rotation = target.rotation * _triggerRestLocalRot;
            }
            else if (_effectiveTriggerMode == TriggerMoveMode.Rail)
            {
                trigger.position = _triggerRestPosition + _railDirection * (_t * _railLength);
                trigger.rotation = _triggerRestRotation;
            }
            else
            {
                trigger.position = _triggerRestPosition + _dragDirection * (_t * _dragLength);
                trigger.rotation = _triggerRestRotation;
            }
            // VRC_Pickup 抓住期间刚体是 kinematic：此时写速度会每帧刷警告，跳过
            if (_triggerRb != null && !_triggerRb.isKinematic)
            {
                _triggerRb.velocity = Vector3.zero;
                _triggerRb.angularVelocity = Vector3.zero;
            }
        }

        bool done = _t <= 0f;
        if (done) _snapping = false;
        SyncIfNeeded(done);
    }

    private void SyncIfNeeded(bool force)
    {
        _framesSinceSync++;
        int interval = syncEveryNFrames < 1 ? 1 : syncEveryNFrames;
        float minDelta = Mathf.Max(0f, syncMinDelta);
        if (force || _framesSinceSync >= interval || Mathf.Abs(_t - _lastSentT) >= minDelta)
        {
            _framesSinceSync = 0;
            _lastSentT = _t;
            RequestSerialization();
        }
    }

    private void LateUpdate()
    {
        if (role == RoleMode.Sender) return;
        if (target == null) return;
        if (interactionMode != InteractionMode.Drag) return;
        if (!_triggerIsPickup) return;

        VRCPlayerApi holder = _triggerPickup.currentPlayer;
        if (holder != null && _lastHolder == null)
        {
            // 抓住瞬间：取消回弹，重新基准
            _snapping = false;
            Vector3 actDir = (_effectiveTriggerMode == TriggerMoveMode.Rail) ? _railDirection : _dragDirection;
            float actLen = (_effectiveTriggerMode == TriggerMoveMode.Rail) ? _railLength : _dragLength;
            _triggerStartPosition = trigger.position - actDir * (_t * actLen);
            InvokeGrabEvent();
        }
        if (holder == Networking.LocalPlayer && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        if (holder == Networking.LocalPlayer && _effectiveTriggerMode == TriggerMoveMode.Rail)
        {
            float along = Vector3.Dot(trigger.position - _triggerRestPosition, _railDirection);
            if (loopMode == LoopMode.PingPong)
            {
                along = Mathf.Clamp(along, 0f, _railLength);
            }
            trigger.position = _triggerRestPosition + _railDirection * along;
            trigger.rotation = _triggerRestRotation;
            // VRC_Pickup 抓住期间刚体是 kinematic：此时写速度会每帧刷警告，跳过
            if (_triggerRb != null && !_triggerRb.isKinematic)
            {
                _triggerRb.velocity = Vector3.zero;
                _triggerRb.angularVelocity = Vector3.zero;
            }
        }
        if (holder == null)
        {
            // 松手：冻结进度（释放瞬间 Pickup 交给物理会乱飞，不能让它影响进度）；
            // 同步移动模式继续把把手贴在门上并清零速度
            if (_isOwner && _effectiveTriggerMode == TriggerMoveMode.SyncTarget && target != null)
            {
                trigger.position = target.TransformPoint(_triggerRestLocalPos);
                trigger.rotation = target.rotation * _triggerRestLocalRot;
                // VRC_Pickup 抓住期间刚体是 kinematic：此时写速度会每帧刷警告，跳过
                if (_triggerRb != null && !_triggerRb.isKinematic)
                {
                    _triggerRb.velocity = Vector3.zero;
                    _triggerRb.angularVelocity = Vector3.zero;
                }
            }
            if (_isOwner && snapBackSpeed > 0f && _t > 0f)
            {
                _snapping = true;
            }
            if (_lastHolder != null) InvokeReleaseEvent();
            _lastHolder = holder;
            return;
        }
        _lastHolder = holder;

        if (!_isOwner) return;
        UpdateDragMovement();
    }

    private void UpdateAutoMovement()
    {
        if (destinationMode == DestinationMode.DestinationTransform && motionMode == MotionMode.Move)
        {
            _endPosition = GetDestinationPosition();
        }

        float baseSpeed = moveSpeed;
        if (speedMode == SpeedMode.Curve && speedCurve != null && speedCurve.length > 0)
        {
            // 进度由速度驱动、速度由进度取值：曲线开头为 0 时进度永远停在 0 速度区（死锁）。
            // 钳到底速 1%：慢速爬过零速区，曲线值上来后自然接管
            baseSpeed = Mathf.Max(speedCurve.Evaluate(_t), 0.01f) * moveSpeed;
        }

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

        int seg = CurrentPathSeg();
        if (seg > _lastPathSeg) FirePathPointEvents(_lastPathSeg, seg);
        _lastPathSeg = seg;

        if (reachedEnd)
        {
            if (_direction > 0) HandleReachedEnd();
            else HandleReachedStart();
            SyncIfNeeded(true);
        }
        else
        {
            SyncIfNeeded(false);
        }
    }

    private void UpdateDragMovement()
    {
        if (trigger == null) return;
        if (loopMode == LoopMode.Once && _onceDone) return; // 拖到位后永久锁定

        if (destinationMode == DestinationMode.DestinationTransform && motionMode == MotionMode.Move)
        {
            _endPosition = GetDestinationPosition();
        }

        // 自由/同步移动：把手被 VRChat 每帧重置到手部位置，LateUpdate 读到的是干净的手世界坐标，
        // 直接用固定基准的世界位移（不能用"相对门的位移"——门跟随手会导致自激振荡）
        float progress;
        if (_effectiveTriggerMode == TriggerMoveMode.Rail)
        {
            if (_railLength < 0.001f) return;
            Vector3 triggerDelta = trigger.position - _triggerStartPosition;
            progress = Vector3.Dot(triggerDelta, _railDirection) / _railLength;
        }
        else
        {
            if (_dragLength < 0.001f) return;
            Vector3 triggerDelta = trigger.position - _triggerStartPosition;
            progress = Vector3.Dot(triggerDelta, _dragDirection) / _dragLength;
        }

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

        int seg = CurrentPathSeg();
        if (seg > _lastPathSeg) FirePathPointEvents(_lastPathSeg, seg);
        _lastPathSeg = seg;

        // 拖过终点/起点：触发 Drag 链（边沿检测——按住终点不重复触发，拉回后再推过才再次触发）
        bool atEnd = progress >= 1f;   // 用 progress：Loop 回绕后 _t 已跳回 [0,1)，检测会漏
        bool atStart = progress <= 0f;
        if (atEnd && !_dragEndFired)
        {
            _dragEndFired = true;
            HandleReachedEnd();
            SyncIfNeeded(true);
            return;
        }
        if (atStart && !_dragStartFired)
        {
            _dragStartFired = true;
            HandleReachedStart();
            SyncIfNeeded(true);
            return;
        }
        if (!atEnd) _dragEndFired = false;
        if (!atStart) _dragStartFired = false;

        if (_effectiveTriggerMode == TriggerMoveMode.SyncTarget && target != null)
        {
            trigger.position = target.TransformPoint(_triggerRestLocalPos);
            trigger.rotation = target.rotation * _triggerRestLocalRot;
            // VRC_Pickup 抓住期间刚体是 kinematic：此时写速度会每帧刷警告，跳过
            if (_triggerRb != null && !_triggerRb.isKinematic)
            {
                _triggerRb.velocity = Vector3.zero;
                _triggerRb.angularVelocity = Vector3.zero;
            }
        }

        SyncIfNeeded(false);
    }

    private void HandleReachedEnd()
    {
        _isMoving = false;
        _v = 0f;
        if (loopMode == LoopMode.Once) _onceDone = true; // 到达终点即完成使命
        ApplyPositionAndRotation(_t);
        InvokeReachEvent();
        if (useChainEvents && nextOnReach != null)
        {
            for (int i = 0; i < nextOnReach.Length; i++)
            {
                if (nextOnReach[i] != null) nextOnReach[i].TryStartDrag();
            }
        }
    }

    private void HandleReachedStart()
    {
        _isMoving = false;
        _v = 0f;
        ApplyPositionAndRotation(_t);
        InvokeReturnEvent();
        if (useChainEvents && nextOnReturn != null)
        {
            for (int i = 0; i < nextOnReturn.Length; i++)
            {
                if (nextOnReturn[i] != null) nextOnReturn[i].TryStartDrag();
            }
        }
    }

    private void InvokeReachEvent()
    {
        if (useChainEvents && onReachDestination != null)
        {
            onReachDestination.SendCustomEvent("OnReachDestination");
        }
        PlayClip(reachSound);

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

    private void InvokeReturnEvent()
    {
        if (useChainEvents && onReachStart != null)
        {
            onReachStart.SendCustomEvent("OnReachStart");
        }
        PlayClip(returnSound);

        switch (language)
        {
            case Language.English:
                Debug.Log("[Drag] Returned to start");
                break;
            case Language.Japanese:
                Debug.Log("[Drag] 始点に戻りました");
                break;
            case Language.Chinese:
                Debug.Log("[Drag] 已返回起点");
                break;
        }
    }

    private void InvokeGrabEvent()
    {
        if (useChainEvents && onGrabbed != null) onGrabbed.SendCustomEvent("OnGrabbed");
        PlayClip(grabSound);
    }

    private void InvokeReleaseEvent()
    {
        if (useChainEvents && onReleased != null) onReleased.SendCustomEvent("OnReleased");
        PlayClip(releaseSound);
    }

    private void PlayClip(AudioClip clip)
    {
        if (!useSounds) return;
        if (clip != null && _audio != null) _audio.PlayOneShot(clip);
    }

    public float GetProgress()
    {
        return _t;
    }

    private void ApplyPositionAndRotation(float t)
    {
        if (motionMode == MotionMode.Rotate && rotateMode == RotateMode.Axis && _axisDynamic) ComputeAxisLine();
        Quaternion rot = GetRotationAt(t);
        Vector3 arm = rot * Quaternion.Inverse(_startRotation) * _centerArm;
        target.rotation = rot;
        target.position = GetPositionAt(t) - arm;
    }

    private Quaternion GetRotationAt(float t)
    {
        if (motionMode == MotionMode.Rotate)
        {
            if (rotateMode == RotateMode.Axis)
            {
                if (_axisDir.sqrMagnitude < 1e-8f) return _startRotation;
                return _startRotation * Quaternion.AngleAxis(axisAngle * t, _axisDir);
            }
            return Quaternion.Slerp(_startRotation, _startRotation * Quaternion.Euler(rotateVector), t);
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
        if (motionMode == MotionMode.Rotate)
        {
            if (rotateMode == RotateMode.Axis)
            {
                Quaternion r = Quaternion.AngleAxis(axisAngle * t, _axisDir);
                return _axisAnchor + r * (_startPosition - _axisAnchor);
            }
            return _startPosition;
        }

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
        int segments = PathSegments();
        float scaled = t * segments;
        int index = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, segments - 1);
        float localT = scaled - index;

        Vector3 a = RefPoint(pathPoints[index]);
        Vector3 b = RefPoint(pathPoints[(index + 1) % pathPoints.Length]);
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
            return RefPoint(destinationTransform) + relativePositionOffset;
        }
        return _startPosition + offsetVector;
    }
}
