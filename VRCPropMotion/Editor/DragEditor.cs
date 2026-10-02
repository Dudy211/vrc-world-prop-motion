#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Drag))]
[CanEditMultipleObjects]
public class DragEditor : Editor
{
    private enum EPath  { Linear = 0, Curve = 1 }
    private enum ESpeed { Fixed = 0, Curve = 1 }
    private enum EMotion { Move = 0, Rotate = 1 }
    private enum ERotMode { Spherical = 0, Axis = 1 }
    private enum EBase { Pivot = 0, Center = 1 }
    private enum ERole { Receiver = 0, Sender = 1 }
    private enum ETrig { Free = 0, SyncTarget = 1, Rail = 2 }

    private Drag _drag;
    private bool _showHelp = false;
    private bool _scrubOn = false;
    private float _scrubT = 0.5f;
    private bool _poseBaseValid = false;
    private bool _poseBaseIsAxis = false;
    private Quaternion _poseBaseRot = Quaternion.identity;
    private Vector3 _poseBaseStart = Vector3.zero;
    private Vector3 _poseBaseEnd = Vector3.zero;
    private Vector3 _poseBaseAxisAnchor = Vector3.zero;
    private Vector3 _poseBaseAxisDir = Vector3.up;
    private readonly Dictionary<string, SerializedProperty> _props = new Dictionary<string, SerializedProperty>();

    private enum EAxisSource { Manual = 0, ObjectAlign = 1, Euler = 2 }

    private string[][] _destModeNames, _interactModeNames, _loopModeNames, _movePathNames, _speedModeNames, _motionModeNames, _rotateModeNames, _baseNames, _triggerMoveNames, _roleNames, _axisSourceNames, _axisUpNames, _hoverModeNames;

    private void InitTexts()
    {
        _destModeNames = new[]
        {
            new[] { "Offset Vector", "Destination Transform", "Path Points" },
            new[] { "オフセットベクトル", "目的地トランスフォーム", "パスポイント" },
            new[] { "偏移向量", "目标变换", "路径点" }
        };
        _interactModeNames = new[]
        {
            new[] { "Interact (Click Trigger)", "Drag (Drag Trigger)", "Passive (Enter Zone)" },
            new[] { "インタラクト (クリック)", "ドラッグ (ドラッグ)", "受動トリガー（进入）" },
            new[] { "交互触发 (点击)", "拖动控制 (拖拽)", "被动触发（进入区域）" }
        };
        _hoverModeNames = new[]
        {
            new[] { "None", "Native (built-in)", "Custom" },
            new[] { "なし", "標準（内蔵）", "カスタム" },
            new[] { "无", "原生碰撞箱", "自定义" }
        };
        _loopModeNames = new[]
        {
            new[] { "Loop", "PingPong", "Repeat", "Once" },
            new[] { "ループ", "往復", "繰り返し", "一回きり" },
            new[] { "循环", "往返", "重复", "单次" }
        };
        _movePathNames = new[]
        {
            new[] { "Linear", "Curve" },
            new[] { "直線", "曲線" },
            new[] { "直线", "曲线" }
        };
        _speedModeNames = new[]
        {
            new[] { "Fixed (Number)", "Curve (Graph)" },
            new[] { "固定 (数字)", "曲線 (グラフ)" },
            new[] { "固定 (数字)", "曲线 (函数图)" }
        };
        _motionModeNames = new[]
        {
            new[] { "Move", "Rotate" },
            new[] { "移動", "回転" },
            new[] { "移动", "旋转" }
        };
        _rotateModeNames = new[]
        {
            new[] { "Spherical", "Axis" },
            new[] { "球面", "軸回転" },
            new[] { "球状旋转", "轴旋转" }
        };
        _baseNames = new[]
        {
            new[] { "Pivot", "Center" },
            new[] { "軸心", "中心" },
            new[] { "轴心", "中心" }
        };
        _triggerMoveNames = new[]
        {
            new[] { "Free (carried by hand)", "Sync Target (follows object)", "Rail (slides on track)" },
            new[] { "自由 (手で運ぶ)", "対象と同期", "レール上を滑る" },
            new[] { "自由拖动 (随手走)", "同步移动 (跟随物体)", "固定轨道 (沿轨道滑动)" }
        };
        _roleNames = new[]
        {
            new[] { "Receiver (moves object)", "Sender (on trigger object)" },
            new[] { "受信側 (動かす本体)", "送信側 (トリガー物体)" },
            new[] { "接收端 (驱动本体)", "发送端 (触发物体上)" }
        };
        _axisSourceNames = new[]
        {
            new[] { "Manual (Start / End)", "Object Align", "3D Angle (Euler)" },
            new[] { "手動 (始点/終点)", "オブジェクト一致", "オイラー角" },
            new[] { "手动 (首尾坐标)", "物体对齐", "三维角度" }
        };
        _axisUpNames = new[]
        {
            new[] { "X", "Y", "Z" },
            new[] { "X", "Y", "Z" },
            new[] { "X", "Y", "Z" }
        };
    }

    private int Lang => (Get("language")?.enumValueIndex ?? 2);
    private string T(string k) => GetText(Lang, k);

    private string GetText(int l, string k)
    {
        switch (k)
        {
            case "Core":         return new[] { "Core", "コア", "核心设置" }[l];
            case "Preview":      return new[] { "Preview", "プレビュー", "预览设置" }[l];
            case "Language":     return new[] { "Language", "言語", "语言" }[l];
            case "Role":         return new[] { "Role", "役割", "端类型" }[l];
            case "Receiver":     return new[] { "Receiver", "受信側", "接收端" }[l];
            case "Sender":       return new[] { "Sender", "送信側", "发送端" }[l];
            case "ReceiverRef":  return new[] { "Receiver Drag", "受信側 Drag", "接收端 Drag" }[l];
            case "RailParams":   return new[] { "Rail Params (this sender)", "レール参数（この送信側）", "轨道参数（本端轨道）" }[l];
            case "SelectRecv":   return new[] { "Select Receiver", "受信側を選択", "选中接收端" }[l];
            case "SenderHelp":   return new[] { "Put this on the object to click/grab. It needs a collider (+ VRC_Pickup if the receiver uses drag interaction).", "クリック／掴む物体に付けてください。コライダー必須（受信側がドラッグの場合は VRC_Pickup も）。", "挂在要点击/抓取的物体上，需要 Collider（接收端用拖动模式时还需 VRC_Pickup）。" }[l];
            case "Target":       return new[] { "Target", "ターゲット", "目标物体" }[l];
            case "DestMode":     return new[] { "Destination Mode", "目的地モード", "目的地模式" }[l];
            case "DestTrans":    return new[] { "Destination Object", "目的地オブジェクト", "目的地物体" }[l];
            case "InteractMode": return new[] { "Interaction Mode", "交互モード", "交互模式" }[l];
            case "TriggerMove":  return new[] { "Trigger Move Mode", "トリガー移動方式", "触发移动方式" }[l];
            case "TriggerObj":   return new[] { "Trigger Object", "トリガー", "触发物体" }[l];
            case "OffsetVec":    return new[] { "Offset Vector", "オフセット", "偏移向量" }[l];
            case "RotVec":       return new[] { "Rotation Vector", "回転ベクトル", "旋转向量" }[l];
            case "PathPoints":   return new[] { "Path Points", "パスポイント", "路径点" }[l];
            case "LoopMode":     return new[] { "Loop Mode", "ループ", "循环模式" }[l];
            case "OnReach":      return new[] { "On Reach Destination", "到達イベント", "到达触发事件" }[l];
            case "Param":        return new[] { "Parameter", "パラメータ", "参数设置" }[l];
            case "PathBase":     return new[] { "Path Base", "基準点", "路径基准" }[l];
            case "MotionMode":   return new[] { "Motion Mode", "動作モード", "运动方式" }[l];
            case "RotateMode":   return new[] { "Rotate Mode", "回転方式", "旋转方式" }[l];
            case "RotateParams": return new[] { "Rotation Params", "回転パラメータ", "旋转参数" }[l];
            case "MoveParams":   return new[] { "Move Params", "移動パラメータ", "移动参数" }[l];
            case "AxisStart":    return new[] { "Axis Start", "軸の始点", "轴起点" }[l];
            case "AxisEnd":      return new[] { "Axis End", "軸の終点", "轴终点" }[l];
            case "AxisAngle":    return new[] { "Axis Angle (deg)", "軸角度 (度)", "轴角度 (度)" }[l];
            case "AxisSource":   return new[] { "Axis Source", "軸の定義", "轴定义方式" }[l];
            case "AxisLength":   return new[] { "Axis Length (m)", "軸の長さ (m)", "轴长度 (米)" }[l];
            case "AxisObject":   return new[] { "Align Object", "基準オブジェクト", "对齐物体" }[l];
            case "AxisUp":       return new[] { "Up Axis (local)", "上方向 (ローカル)", "为上轴向 (本地)" }[l];
            case "AxisPosOffset":return new[] { "Position Offset (drag axis midpoint)", "位置オフセット（軸中点をドラッグ）", "位置偏移 (场景拖轴中点)" }[l];
            case "AxisEuler":    return new[] { "3D Angle / Euler (deg)", "三次元角度 (度)", "三维角度 (欧拉角)" }[l];
            case "AlignWorldAxis":return new[] { "Align to World Axis", "世界軸に合わせる", "对齐到世界轴" }[l];
            case "MovePath":     return new[] { "Move Path", "移動パス", "移动路径" }[l];
            case "SpeedMode":    return new[] { "Speed Mode", "速度モード", "速度模式" }[l];
            case "MoveSpeed":    return new[] { "Move Speed", "移動速度", "移动速度" }[l];
            case "SpeedCurve":   return new[] { "Speed Curve", "速度曲線", "速度曲线" }[l];
            case "MoveCurve":    return new[] { "Move Curve", "移動曲線", "移动曲线" }[l];
            case "Drag":         return new[] { "Drag", "阻力", "阻力" }[l];
            case "Gravity":      return new[] { "Gravity Scale", "重力スケール", "重力缩放" }[l];
            case "AutoOrigin":   return new[] { "Auto Origin (Midpoint)", "自動原点(中点)", "自动原点(中点)" }[l];
            case "CurveOrigin":  return new[] { "Curve Origin (P)", "曲線原点(P)", "曲线原点(P)" }[l];
            case "PreviewDest":  return new[] { "Preview Destination", "目的地プレビュー", "预览目的地" }[l];
            case "PreviewPath":  return new[] { "Preview Path", "パスプレビュー", "预览路径" }[l];
            case "PreviewMesh":  return new[] { "Preview Mesh", "プレビューメッシュ", "预览网格" }[l];
            case "EditEnd":      return new[] { "Drag Destination (Offset mode)", "終点をドラッグ（オフセット）", "拖拽终点（位移模式）" }[l];
            case "CurveHelp":    return new[] { "Curve Y = arch height (1 = path length).\nYellow P handle: drag freely or per-axis in the Scene view.", "曲線のY軸＝アーチ高（1＝経路全長）。\nシーンビューで黄色のPハンドルをドラッグ（自由／単軸）。", "曲线 Y 轴 = 拱桥高度 (1格 = 路径全长)。\n原点 P 手柄可在 Scene 视图 XYZ 任意拖 / 单轴拖。" }[l];
            case "HelpBtn":      return new[] { "Help", "ヘルプ", "帮助" }[l];
            case "HelpQuick":    return new[]
            {
                "Quick Start — Receiver: put Drag on a manager object, set Target (moves) and Trigger (clicked/grabbed). Sender: put a second Drag (role = Sender) on the object to click, and set its Receiver. Drag interaction: the Trigger needs VRC_Pickup (Collider + Rigidbody). Trigger move: Free = carried by hand; Sync Target = rigidly follows the target; Rail = slides along the rail line.",
                "クイックスタート — 受信側：管理用オブジェクトに Drag を付け、Target（動かす物体）と Trigger（クリック／掴む物体）を設定。送信側：クリックされる物体にもう一枚 Drag（役割＝送信側）を付け、Receiver に受信側を指定。ドラッグ操作では Trigger に VRC_Pickup（Collider＋Rigidbody）が必須。トリガー移動：自由＝手で運ぶ／対象と同期＝物体に追随／レール＝軌道上を滑る。",
                "快速上手 — 接收端：把 Drag 挂在管理物体上，设置 Target（要动的物体）与 Trigger（被点击/被抓的物体）。发送端：在要点击的物体上再挂一个 Drag（端类型＝发送端），Receiver 指向接收端。拖动交互：Trigger 必须挂 VRC_Pickup（Collider + Rigidbody）。触发移动方式：自由拖动＝随手走；同步移动＝刚性跟随目标；固定轨道＝沿轨道线滑动。"
            }[l];
            case "HelpAsset":    return new[]
            {
                "IMPORTANT — Drag.asset is the compiled UdonSharp program of Drag.cs. If you edit FIELDS in Drag.cs and the Inspector shows missing/wrong fields or behavior does not change: delete Drag.asset (and its .meta), then run Tools → VRC Prop Motion → Setup Drag Program Asset. The auto-initializer only creates the asset when it is missing; it never detects source changes. Editor-only script changes need no rebuild.",
                "重要 — Drag.asset は Drag.cs をコンパイルした UdonSharp プログラムです。Drag.cs のフィールドを編集後、インスペクターの項目が消えた／動作が変わらない場合は Drag.asset（と .meta）を削除し、Tools → VRC Prop Motion → Setup Drag Program Asset を実行してください。自動生成は資産が無い時のみで、ソース変更は検知しません。Editor/ だけの変更は再生成不要です。",
                "重要 — Drag.asset 是 Drag.cs 编译出的 UdonSharp 程序资产。修改 Drag.cs 的字段后，若 Inspector 字段缺失/异常或行为没有变化：删除 Drag.asset（连同 .meta），再执行菜单 Tools → VRC Prop Motion → Setup Drag Program Asset。自动初始化只在资产缺失时创建，不会检测源码变更；只改 Editor/ 下脚本则无需重建。"
            }[l];
            case "HelpSync":     return new[]
            {
                "Sync — Progress is UdonSynced (continuous): the owner sends every frame, non-owners apply position on deserialization. Reaching the end calls OnReachDestination() on the UdonBehaviour assigned in On Reach Destination.",
                "同期 — 進行度は UdonSynced（連続）で、オーナーが毎フレーム送信し、非オーナーは受信時に位置を適用します。終点に到達すると「到達イベント」に設定した UdonBehaviour の OnReachDestination() が呼ばれます。",
                "同步 — 进度通过 UdonSynced（连续模式）同步：主人每帧发送，非主人在反序列化时直接应用位置。到达终点时会调用「到达触发事件」里配置的 UdonBehaviour 的 OnReachDestination()。"
            }[l];
            case "HelpTrouble":  return new[]
            {
                "Troubleshooting — Drag not responding: the Trigger needs VRC_Pickup + Collider + Rigidbody. Scene axis handle hard to grab: it can overlap the selected object's native move gizmo; drag its XYZ arrows instead, or type a small offset in the Inspector first.",
                "トラブル対応 — ドラッグが反応しない：Trigger に VRC_Pickup＋Collider＋Rigidbody が必要。シーンの軸ハンドルが掴みにくい：選択中オブジェクトの標準ギズモと重なることがあります。XYZ の矢印を掴むか、先に小さなオフセットを入力してください。",
                "常见问题 — 拖不动：Trigger 需要 VRC_Pickup + Collider + Rigidbody。场景轴手柄难拖：可能和选中物体的原生移动 Gizmo 重叠，改拖它的 XYZ 彩色箭头，或先在 Inspector 里输一个小偏移。"
            }[l];
            case "ChainTitle":   return new[] { "Chain / Events", "連鎖 / イベント", "事件与连锁" }[l];
            case "PreviewRelDepth": return new[] { "Chain Preview Depth (0 = off)", "連鎖プレビュー深さ (0=無効)", "关联预览深度 (0=关)" }[l];
            case "RelDown":      return new[] { "Downstream (this triggers)", "下流（自分が触发）", "下游（我触发它）" }[l];
            case "RelUp":        return new[] { "Upstream (triggers this)", "上流（自分を触发）", "上游（它触发我）" }[l];
            case "TrigTiming":   return new[] { "Trigger Timing", "触发时机", "触发时机" }[l];
            case "Cooldown":     return new[] { "Trigger Cooldown (s)", "触发クールダウン (秒)", "触发冷却 (秒)" }[l];
            case "StartDelay":   return new[] { "Start Delay (s)", "開始遅延 (秒)", "启动延迟 (秒)" }[l];
            case "OnGrabbed":    return new[] { "On Grabbed", "掴んだイベント", "抓住事件" }[l];
            case "OnReleased":   return new[] { "On Released", "離したイベント", "释放事件" }[l];
            case "Sounds":       return new[] { "Sounds", "音效", "音效" }[l];
            case "UseSounds":    return new[] { "Enable Sounds", "音效を有効化", "启用音效" }[l];
            case "UseChain":     return new[] { "Enable Chain / Events", "連鎖/イベントを有効化", "启用事件与连锁" }[l];
            case "ReachSound":   return new[] { "Reach Sound", "到達音", "到达音效" }[l];
            case "ReturnSound":  return new[] { "Return Sound", "折返音", "返回音效" }[l];
            case "GrabSound":    return new[] { "Grab Sound", "掴んだ音", "抓住音效" }[l];
            case "ReleaseSound": return new[] { "Release Sound", "離した音", "释放音效" }[l];
            case "ClosePath":    return new[] { "Close Path (loop)", "パスを閉じる", "闭合路径" }[l];
            case "ScrubTitle":   return new[] { "Scrub Preview", "進行度プレビュー", "进度预览" }[l];
            case "ScrubBtn":     return new[] { "Scrub", "スクラブ", "姿态预览" }[l];
            case "PoseBase":     return new[] { "Set Pose Base", "基準に設定", "以当前姿态为基准" }[l];
            case "ApplyPose":    return new[] { "Apply Pose", "姿勢を適用", "应用该姿态" }[l];
            case "ResetPose":    return new[] { "Back to Start", "始点に戻す", "回到起点姿态" }[l];
            case "OnReachStart": return new[] { "On Reach Start (Return)", "折返イベント", "返回起点事件" }[l];
            case "NextOnReach":  return new[] { "Next On Reach (Drag[])", "到達で次を起動", "到达后触发 (Drag 链)" }[l];
            case "NextOnReturn": return new[] { "Next On Return (Drag[])", "折返で次を起動", "返回后触发 (Drag 链)" }[l];
            case "PathPointEvents": return new[] { "Path Point Events", "パス点イベント", "路径点事件" }[l];
            case "NetTitle":     return new[] { "Network", "ネットワーク", "网络同步" }[l];
            case "SyncEveryN":   return new[] { "Sync Every N Frames", "同期間隔 (フレーム)", "同步间隔 (帧)" }[l];
            case "SyncMinDelta": return new[] { "Sync Min Delta", "同期しきい値", "同步最小变化" }[l];
            case "DragOptTitle": return new[] { "Drag Options", "ドラッグ設定", "拖动选项" }[l];
            case "SnapBack":     return new[] { "Snap Back Speed (0 = off)", "戻る速度 (0=無効)", "回弹速度 (0=关闭)" }[l];
            case "LockHeld":     return new[] { "Lock While Held", "保持中は他者触发を禁止", "抓住时锁定触发" }[l];
            case "HoverTitle":   return new[] { "Hover Feedback", "悬停反馈", "悬停反馈" }[l];
            case "HoverMode":    return new[] { "Hover Mode", "悬停モード", "悬停方式" }[l];
            case "NativeHelp":   return new[]
            {
                "Native mode uses VRChat's own highlight and USE prompt: put a collider on the Interactive layer.",
                "標準モードは VRChat 内蔵の強調と USE 表示を使います。Interactive 层のコライダーが必要です。",
                "原生模式使用 VRChat 内建高亮和 USE 提示：需要把碰撞体放在 Interactive 层。"
            }[l];
            case "HoverVisual":  return new[] { "Hover Visual (shown while aiming)", "悬停時に表示するオブジェクト", "悬停显示物体" }[l];
            case "HoverDistance":return new[] { "Hover Distance (m)", "悬停距離 (m)", "悬停距离 (米)" }[l];
            case "HoverHighlight":return new[] { "Built-in Highlight (tint renderers)", "内蔵ハイライト", "内置高亮" }[l];
            case "HighlightColor":return new[] { "Highlight Color", "ハイライト色", "高亮颜色" }[l];
            case "HighlightBlink":return new[] { "Blink", "点滅", "闪烁" }[l];
            case "HighlightSpeed":return new[] { "Blink Speed", "点滅速度", "闪烁速度" }[l];
            case "HoverText":  return new[] { "Built-in Text Prompt", "内蔵テキストプロンプト", "内置文字提示" }[l];
            case "HoverTextContent": return new[] { "Prompt Text (empty = USE/使用)", "プロンプト文字（空=使用）", "提示文字（空=使用）" }[l];
            case "HoverTextSize": return new[] { "Text Size", "文字サイズ", "文字大小" }[l];
            case "HoverTextColor": return new[] { "Text Color", "文字色", "文字颜色" }[l];
            case "HoverTextOffset": return new[] { "Position Offset", "位置オフセット", "位置偏移" }[l];
            case "HoverFont":  return new[] { "Font (optional)", "フォント（任意）", "字体（可选）" }[l];
            case "PreviewGhosts":return new[] { "Ghost Slices (0 = off)", "中間分割数 (0=無効)", "中途预览份数 (0=关)" }[l];
            case "HelpChain":    return new[]
            {
                "New — Events: On Reach Destination / On Reach Start, Next On Reach/Return chains other Drags (domino), Path Point Events. Passive mode starts when a player enters the zone (managed in the Inspector). Snap Back returns the handle after release. Non-owners see interpolated motion; sync can be throttled under Network.",
                "新機能 — イベント：到達／折返イベント、Drag チェーン（多米诺）、パス点イベント。受動モードは进入ゾーンで開始（インスペクターで管理）。リリース後にハンドルを戻す設定あり。非オーナーは補間表示、ネットワーク節約も設定可。",
                "新增 — 事件：到达/返回事件、Drag 连锁触发（多米诺）、路径点事件。被动触发模式玩家进入区域即启动（区域在 Inspector 中可视化调整）。回弹让松手后把手自动复位。非主人端插值平滑，网络节可在「网络同步」里调。"
            }[l];
            case "PassiveHelp":  return new[]
            {
                "Passive mode: the trigger zone below (drawn in Scene) starts the motion when a player enters. Pure position detection - no collider needed.",
                "受動トリガー：シーンに表示される触发ゾーンにプレイヤーが进入すると開始。位置判定のみでコライダー不要。",
                "被动触发：玩家进入下方触发区域（场景中可见）时启动。纯位置检测，无需碰撞体。"
            }[l];
            case "ZoneCenter":   return new[] { "Zone Center (local)", "ゾーン中心（ローカル）", "触发区域中心（本地）" }[l];
            case "ZoneSize":     return new[] { "Zone Size (local meters)", "ゾーンサイズ（ローカルm）", "触发区域大小（本地米）" }[l];
            case "ZoneWrap":     return new[] { "Wrap Whole Object", "全体にフィット", "包裹整个物体" }[l];
            default:             return k;
        }
    }

    private SerializedProperty Get(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return _props.TryGetValue(name, out var p) ? p : null;
    }

    private void CacheProperty(string primary, params string[] aliases)
    {
        var list = new List<string> { primary };
        if (aliases != null) list.AddRange(aliases);
        foreach (var n in list)
        {
            if (string.IsNullOrEmpty(n)) continue;
            if (_props.ContainsKey(n)) return;
            var prop = serializedObject.FindProperty(n);
            if (prop != null)
            {
                _props[n] = prop;
                return;
            }
        }
    }

    private void OnEnable()
    {
        _drag = (Drag)target;
        _poseBaseValid = false;
        _poseBaseIsAxis = false;
        InitTexts();
        _props.Clear();

        CacheProperty("language");
        CacheProperty("role");
        CacheProperty("receiver");
        CacheProperty("target");
        CacheProperty("destinationMode");
        CacheProperty("interactionMode");
        CacheProperty("trigger", "triggerObject", "triggerTransform");
        CacheProperty("triggerMoveMode", "triggerMove");
        CacheProperty("offsetVector");
        CacheProperty("rotationVector");
        CacheProperty("rotateVector");
        CacheProperty("destinationTransform");
        CacheProperty("relativePositionOffset");
        CacheProperty("relativeRotationOffset");
        CacheProperty("pathPoints");
        CacheProperty("loopMode");
        CacheProperty("onReachDestination", "arriveEvent");
        CacheProperty("pathBase", "baseMode");
        CacheProperty("motionMode");
        CacheProperty("rotateMode", "rotatePathMode", "rotationPathMode");
        CacheProperty("axisStart", "axisStartPoint");
        CacheProperty("axisEnd", "axisEndPoint");
        CacheProperty("axisAngle", "rotationAngle");
        CacheProperty("axisSource");
        CacheProperty("axisLength");
        CacheProperty("axisObject");
        CacheProperty("axisUp");
        CacheProperty("axisPositionOffset");
        CacheProperty("axisEuler");
        CacheProperty("movePathMode", "movePath");
        CacheProperty("speedMode");
        CacheProperty("moveSpeed", "speed");
        CacheProperty("speedCurve");
        CacheProperty("moveCurve");
        CacheProperty("drag");
        CacheProperty("gravityScale");
        CacheProperty("autoCurveOrigin", "autoOrigin");
        CacheProperty("curveOrigin");
        CacheProperty("previewDestination");
        CacheProperty("previewPath");
        CacheProperty("editEndPosition");
        CacheProperty("previewMesh");
        CacheProperty("onReachStart");
        CacheProperty("nextOnReach");
        CacheProperty("nextOnReturn");
        CacheProperty("onPathPointEvents");
        CacheProperty("syncEveryNFrames");
        CacheProperty("syncMinDelta");
        CacheProperty("snapBackSpeed");
        CacheProperty("lockWhileHeld");
        CacheProperty("previewGhostSteps");
        CacheProperty("previewRelationDepth");
        CacheProperty("closePath");
        CacheProperty("triggerCooldown");
        CacheProperty("startDelay");
        CacheProperty("onGrabbed");
        CacheProperty("onReleased");
        CacheProperty("reachSound");
        CacheProperty("returnSound");
        CacheProperty("grabSound");
        CacheProperty("releaseSound");
        CacheProperty("useSounds");
        CacheProperty("useChainEvents");
        CacheProperty("triggerZoneCenter");
        CacheProperty("triggerZoneSize");
        CacheProperty("triggerZoneInit");
        CacheProperty("hoverMode");
        CacheProperty("hoverVisual");
        CacheProperty("hoverDistance");
        CacheProperty("hoverHighlight");
        CacheProperty("highlightColor");
        CacheProperty("highlightBlink");
        CacheProperty("highlightBlinkSpeed");
        CacheProperty("hoverText");
        CacheProperty("hoverTextContent");
        CacheProperty("hoverTextSize");
        CacheProperty("hoverTextColor");
        CacheProperty("hoverTextOffset");
        CacheProperty("hoverFont");

        // 兼容旧工程: 手动模式下 axisLength 与 |axisEnd - axisStart| 同步一次
        var asP = Get("axisStart");
        var aeP = Get("axisEnd");
        var alP = Get("axisLength");
        var srcP0 = Get("axisSource");
        if (asP != null && aeP != null && alP != null && (srcP0 == null || srcP0.enumValueIndex == (int)EAxisSource.Manual))
        {
            float mag = (aeP.vector3Value - asP.vector3Value).magnitude;
            if (mag > 0.001f && Mathf.Abs(alP.floatValue - mag) > 0.0005f)
                alP.floatValue = mag;
        }

        var required = new[] { "trigger", "movePathMode", "previewDestination" };
        var missing = new List<string>();
        foreach (var r in required)
        {
            bool found;
            if (r == "trigger") found = Get("trigger") != null || Get("triggerObject") != null || Get("triggerTransform") != null;
            else if (r == "movePathMode") found = Get("movePathMode") != null || Get("movePath") != null;
            else found = Get(r) != null;
            if (!found) missing.Add(r);
        }
        if (missing.Count > 0)
            Debug.LogWarning("[DragEditor] Drag.cs 中未找到关键字段（请核对命名）: " + string.Join(", ", missing));
    }

    private void Draw(string name, string label)
    {
        var p = Get(name);
        if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label, TooltipFor(name)));
    }

    // 数值下限钳制：输入越界时立刻弹回
    private void ClampFloat(string name, float min)
    {
        var p = Get(name);
        if (p != null && p.floatValue < min) { p.floatValue = min; serializedObject.ApplyModifiedProperties(); }
    }

    private void ClampInt(string name, int min)
    {
        var p = Get(name);
        if (p != null && p.intValue < min) { p.intValue = min; serializedObject.ApplyModifiedProperties(); }
    }

    // 字段悬浮提示（三语），按字段名查表；无条目则不显示
    private string TooltipFor(string fieldName)
    {
        int l = Lang;
        switch (fieldName)
        {
            case "language": return new[] { "UI and log language", "UIとログの言語", "界面与日志语言" }[l];
            case "role": return new[] { "Receiver moves the target; Sender sits on the clicked object and fires the receiver", "受信側＝動かす側、送信側＝クリックされる側", "接收端驱动目标运动；发送端挂在被点击的物体上" }[l];
            case "receiver": return new[] { "Sender mode: the receiver Drag to trigger", "送信側：触发する受信側 Drag", "发送端：要触发的接收端 Drag" }[l];
            case "target": return new[] { "The object that moves", "動かす対象", "要运动的物体" }[l];
            case "interactionMode": return new[] { "Interact = click; Drag = grab the handle; Passive = player enters the zone", "クリック／ドラッグ／进入ゾーン", "交互点击 / 拖拽把手 / 玩家进入区域" }[l];
            case "trigger": return new[] { "Clicked (Interact) or grabbed (Drag) object; unused in Passive mode", "クリック/掴む対象。受動モードでは未使用", "被点击/被抓的物体；被动模式下不使用" }[l];
            case "triggerMoveMode": return new[] { "Free = carried by hand; SyncTarget = follows target; Rail = slides on the rail line", "自由＝手で運ぶ、同期＝対象に追随、レール＝軌道", "自由＝随手移动；同步＝刚性跟随目标；轨道＝沿轨道线滑动" }[l];
            case "motionMode": return new[] { "Move = translate; Rotate = orientation only", "移動＝平行移動、回転＝向きのみ", "移动＝位移；旋转＝只改变朝向" }[l];
            case "rotateMode": return new[] { "Spherical = slerp start->end; Axis = spin around the axis line", "球面＝補間、軸＝軸回転", "球面＝起止姿态插值；轴＝绕轴线旋转" }[l];
            case "rotateVector": return new[] { "Spherical mode: euler offset from start to end orientation", "球面：開始→終了のオイラー角", "球面模式的起止欧拉角" }[l];
            case "axisSource": return new[] { "How the axis line is defined", "軸の定義方法", "轴线的定义方式" }[l];
            case "axisStart": return new[] { "Manual mode: axis start point (world)", "手動：軸の始点（世界座標）", "手动模式：轴起点（世界坐标）" }[l];
            case "axisEnd": return new[] { "Manual mode: axis end point (world)", "手動：軸の終点（世界座標）", "手动模式：轴终点（世界坐标）" }[l];
            case "axisLength": return new[] { "Axis line length in meters", "軸の長さ(m)", "轴长（米）" }[l];
            case "axisObject": return new[] { "ObjectAlign: the object whose local axis defines the axis direction", "物体对齐：轴朝向取自该物体", "物体对齐：轴朝向取自该物体的本地轴" }[l];
            case "axisUp": return new[] { "ObjectAlign: which local axis to use", "物体对齐：本地轴方向", "物体对齐：使用哪个本地轴" }[l];
            case "axisPositionOffset": return new[] { "World offset from the auto-computed axis center (drag the axis midpoint in Scene)", "自動軸中心からの世界オフセット（シーンで軸中点をドラッグ）", "轴心世界偏移（场景中可拖轴中点）" }[l];
            case "axisEuler": return new[] { "Euler mode: define the axis by euler angles", "オイラー角で軸を定義", "欧拉角模式：用欧拉角定义轴" }[l];
            case "axisAngle": return new[] { "Total rotation angle over the full trip (multi-turn allowed)", "一周の総回転角（多圈可）", "全程总旋转角（允许多圈）" }[l];
            case "destinationMode": return new[] { "Offset vector / destination transform / path points", "オフセット／目標物体／パス点", "偏移向量 / 目标物体 / 路径点" }[l];
            case "destinationTransform": return new[] { "The destination transform", "目的地の Transform", "终点参考物体" }[l];
            case "relativePositionOffset": return new[] { "Offset added on top of the destination position", "目的地に加えるオフセット", "叠加在终点位置上的偏移" }[l];
            case "relativeRotationOffset": return new[] { "Extra rotation applied while moving to the destination", "移動中の追加回転", "移向终点期间的附加旋转" }[l];
            case "offsetVector": return new[] { "World offset from the start position", "起点からの世界オフセット", "起点世界偏移向量" }[l];
            case "rotationVector": return new[] { "Move mode: extra rotation while translating", "移動中の追加回転", "移动模式：位移期间的附加旋转" }[l];
            case "pathPoints": return new[] { "Path waypoints", "経路の通過点", "路径途经点" }[l];
            case "closePath": return new[] { "Connect the last point back to the first (closed loop)", "最後の点を最初の点に戻す", "闭合路径：最后一点连回第一点" }[l];
            case "movePathMode": return new[] { "Linear / curved path", "直線／曲線", "直线 / 曲线" }[l];
            case "loopMode": return new[] { "Loop = continue from current; PingPong = back and forth; Repeat = replay from the original start; Once = play to the end then locked forever (SetProgress revives)", "ループ＝現在位置から続行、往復＝行き来、繰り返し＝最初から再開、一回きり＝終点で永久锁定", "循环＝从当前位置续走；往返＝来回；重复＝回原始起点重播；单次＝到终点后永久锁定（SetProgress 可解除）" }[l];
            case "useChainEvents": return new[] { "Master switch: when off, events and Drag chains never fire", "総开关：オフでイベントと連鎖が動かない", "总开关：关闭后事件与连锁不会触发" }[l];
            case "onReachDestination": return new[] { "UdonBehaviour to call OnReachDestination() at the end", "終点で OnReachDestination() を呼ぶ", "到达终点时调用其 OnReachDestination()" }[l];
            case "onReachStart": return new[] { "UdonBehaviour to call OnReachStart() when returning to start", "始点に戻ったら OnReachStart() を呼ぶ", "返回起点时调用其 OnReachStart()" }[l];
            case "nextOnReach": return new[] { "Drag components to trigger when reaching the end", "終点で起動する Drag", "到达终点时触发的 Drag" }[l];
            case "nextOnReturn": return new[] { "Drag components to trigger when returning to start", "始点に戻ったら起動する Drag", "返回起点时触发的 Drag" }[l];
            case "onPathPointEvents": return new[] { "UdonBehaviours to call OnPathPoint() when passing each path point", "パス点ごとに OnPathPoint() を呼ぶ", "经过每个路径点时调用 OnPathPoint()" }[l];
            case "onGrabbed": return new[] { "UdonBehaviour to call OnGrabbed() when the handle is grabbed", "ハンドルを掴んだら OnGrabbed() を呼ぶ", "抓住把手时调用 OnGrabbed()" }[l];
            case "onReleased": return new[] { "UdonBehaviour to call OnReleased() when the handle is released", "ハンドルを離したら OnReleased() を呼ぶ", "释放把手时调用 OnReleased()" }[l];
            case "useSounds": return new[] { "Master switch for sound effects (needs an AudioSource on this GameObject)", "音效の総开关（AudioSource が必要）", "音效总开关（本物体需挂 AudioSource）" }[l];
            case "reachSound": return new[] { "Clip played when reaching the end", "終点に到達した音", "到达终点时播放" }[l];
            case "returnSound": return new[] { "Clip played when returning to start", "始点に戻った音", "返回起点时播放" }[l];
            case "grabSound": return new[] { "Clip played when grabbing the handle", "ハンドルを掴んだ音", "抓住把手时播放" }[l];
            case "releaseSound": return new[] { "Clip played when releasing the handle", "ハンドルを離した音", "释放把手时播放" }[l];
            case "triggerCooldown": return new[] { "Minimum seconds between triggers, all methods (0 = none)", "触发の最短間隔・秒（0=無効）", "所有触发方式的最短间隔（秒），0=无冷却" }[l];
            case "startDelay": return new[] { "Seconds to wait after a trigger before moving (Drag mode ignores)", "触发後の待機秒数（ドラッグは無視）", "触发后延迟开始运动的秒数（拖动模式忽略）" }[l];
            case "syncEveryNFrames": return new[] { "Owner sends sync at most every N frames", "オーナーが最大 N フレームごとに同期", "主人端最多每 N 帧同步一次" }[l];
            case "syncMinDelta": return new[] { "Force a sync when progress changes more than this", "進行度がこの値以上変わると強制同期", "进度变化超过此值时强制同步" }[l];
            case "snapBackSpeed": return new[] { "After release, return to rest at this speed (0 = stay)", "離した後この速度で戻る（0=そのまま）", "松手后按此速度回弹（0=停在原地）" }[l];
            case "lockWhileHeld": return new[] { "While another player holds the handle, triggers are ignored", "他人が掴んでいる間は触发を無視", "他人抓住把手期间忽略触发" }[l];
            case "previewDestination": return new[] { "Editor: draw the destination wireframe", "エディタ：目的地のワイヤーフレーム", "编辑器：画终点线框" }[l];
            case "previewPath": return new[] { "Editor: draw the path / orbit arc", "エディタ：経路・軌道を表示", "编辑器：画路径/公转弧线" }[l];
            case "previewGhostSteps": return new[] { "Editor: ghost wireframes between start and end (0 = off)", "エディタ：始点と終点の間の分割数（0=無効）", "编辑器：起点与终点之间的幽灵份数（0=关）" }[l];
            case "previewRelationDepth": return new[] { "Editor: show chained Drag previews, N levels up/downstream", "エディタ：連鎖 Drag のプレビュー深さ", "编辑器：连锁预览的上下层级数" }[l];
            case "editEndPosition": return new[] { "Editor: drag the destination directly in the Scene view", "エディタ：シーンで目的地をドラッグ", "编辑器：在场景中直接拖动终点" }[l];
            case "pathBase": return new[] { "Reference point: pivot or renderer bounds center", "参照点：軸心／バウンディング中心", "参考点：轴心 / 渲染包围盒中心" }[l];
            case "moveCurve": return new[] { "Curve mode: arch height curve", "曲線モードの盛り上がりカーブ", "曲线模式的拱高曲线" }[l];
            case "autoCurveOrigin": return new[] { "Place the curve origin at the midpoint automatically", "曲線原点を中点に自動設定", "曲线原点自动取起终点中点" }[l];
            case "curveOrigin": return new[] { "Curve origin (world space)", "曲線原点（世界座標）", "曲线原点（世界坐标）" }[l];
            case "speedMode": return new[] { "Fixed speed / curve-driven speed", "固定速度／カーブ速度", "固定速度 / 曲线速度" }[l];
            case "moveSpeed": return new[] { "Movement speed (m/s)", "移動速度(m/s)", "运动速度（米/秒）" }[l];
            case "speedCurve": return new[] { "Curve mode: speed multiplier over progress (values below 0.01 are floored so motion never stalls)", "カーブ速度の倍率（0.01 未満は底速で進行）", "曲线速度倍率（低于 0.01 会按底速 1% 行进，防止卡死）" }[l];
            case "drag": return new[] { "Rail friction, damps gravity-induced speed", "レール摩擦（重力による加速を減衰）", "轨道摩擦，衰减重力带来的加速" }[l];
            case "gravityScale": return new[] { "Gravity acceleration along the path (rail cart effect)", "経路に沿った重力加速度", "沿路径的重力加速度（轨道小车效果）" }[l];
            case "triggerZoneCenter": return new[] { "Passive mode: zone center, local offset from this GameObject (drag the yellow dot in Scene)", "受動モード：ゾーン中心（シーンで黄点をドラッグ）", "被动模式：区域中心（场景中拖黄点）" }[l];
            case "triggerZoneSize": return new[] { "Passive mode: zone size in local meters (drag the cyan face dots in Scene)", "受動モード：ゾーンサイズ（シーンで面の点をドラッグ）", "被动模式：区域大小（场景中拖面中心点）" }[l];
            case "triggerZoneInit": return new[] { "Internal: zone has been auto-wrapped once", "内部用：初期化済み", "内部标记：已初始化" }[l];
            case "hoverMode": return new[] { "None = no hover feedback; Native = VRChat default prompt; Custom = raycast with your own visuals", "なし／標準／カスタム（カスタムは自前のビジュアルを使用）", "无 / 原生内建提示 / 自定义（射线 + 自制视觉效果）" }[l];
            case "hoverVisual": return new[] { "Any GameObject shown on hover (text, outline, glow...); hidden otherwise", "悬停時に表示する任意のオブジェクト（文字・輪郭・光など）", "悬停时显示的任意物体（文字/描边/光效），平时隐藏" }[l];
            case "hoverDistance": return new[] { "Max raycast distance for hover", "悬停の最大射线距離", "悬停射线最大距离" }[l];
            case "hoverHighlight": return new[] { "Tint the target's _Color while hovered (per-client, restores on exit)", "悬停时给目标 _Color 着色（逐客户端，退出恢复）", "悬停时给目标的 _Color 着色（逐客户端，移出恢复）" }[l];
            case "highlightBlink": return new[] { "Pulse the highlight between base and highlight color", "基本色と高亮色の間で点滅", "在基础色和高亮色之间脉冲" }[l];
            case "hoverText": return new[] { "Show a world-space text above the object while hovered (auto-created child 'HoverPrompt')", "悬停时在物体上方显示世界文字（自动创建 HoverPrompt 子物体）", "悬停时在物体上方显示世界空间文字（自动创建 HoverPrompt 子物体）" }[l];
            case "hoverTextContent": return new[] { "Prompt text; empty = USE / 使用 by language", "提示文字；留空按语言显示 USE/使用", "提示文字；留空按语言显示 使用" }[l];
            default: return null;
        }
    }

    private void DrawAlias(string[] names, string label)
    {
        foreach (var n in names)
        {
            var p = Get(n);
            if (p != null)
            {
                EditorGUILayout.PropertyField(p, new GUIContent(label));
                return;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Draw("language", T("Language"));

        // 帮助按钮：位于角色分支之前，接收端/发送端均可用，内容跟随语言设置
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(T("HelpBtn"), EditorStyles.miniButton, GUILayout.Width(70)))
            _showHelp = !_showHelp;
        if (GUILayout.Button("GitHub", EditorStyles.miniButton, GUILayout.Width(70)))
            Application.OpenURL("https://github.com/Dudy211/vrc-world-prop-motion");
        EditorGUILayout.EndHorizontal();
        if (_showHelp)
        {
            EditorGUILayout.HelpBox(T("HelpQuick"), MessageType.None);
            EditorGUILayout.HelpBox(T("HelpAsset"), MessageType.Warning);
            EditorGUILayout.HelpBox(T("HelpSync"), MessageType.None);
            EditorGUILayout.HelpBox(T("HelpChain"), MessageType.None);
            EditorGUILayout.HelpBox(T("HelpTrouble"), MessageType.None);
        }

        var roleProp = Get("role");
        var imLockP = Get("interactionMode");
        bool passiveLock = imLockP != null && imLockP.enumValueIndex == 2;
        if (passiveLock && roleProp != null && roleProp.enumValueIndex != (int)ERole.Receiver)
        {
            roleProp.enumValueIndex = (int)ERole.Receiver; // 被动触发仅接收端
            serializedObject.ApplyModifiedProperties();
        }
        using (new EditorGUI.DisabledScope(passiveLock))
        {
            if (roleProp != null)
                roleProp.enumValueIndex = EditorGUILayout.Popup(T("Role"), roleProp.enumValueIndex, _roleNames[Lang]);
        }
        EditorGUILayout.Space();

        if (roleProp != null && roleProp.enumValueIndex == (int)ERole.Sender)
        {
            Draw("receiver", T("ReceiverRef"));

            var triggerMove = Get("triggerMoveMode") ?? Get("triggerMove");
            if (triggerMove != null)
                triggerMove.enumValueIndex = EditorGUILayout.Popup(T("TriggerMove"), triggerMove.enumValueIndex, _triggerMoveNames[Lang]);
            int tmMode = triggerMove != null ? triggerMove.enumValueIndex : (int)ETrig.Rail;

            if (tmMode == (int)ETrig.Rail)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(T("RailParams"), EditorStyles.boldLabel);

                var sDest = Get("destinationMode");
                if (sDest != null)
                    sDest.enumValueIndex = EditorGUILayout.Popup(T("DestMode"), sDest.enumValueIndex, _destModeNames[Lang]);
                int sd = sDest != null ? sDest.enumValueIndex : 0;

                var sBase = Get("pathBase") ?? Get("baseMode");
                if (sBase != null)
                    sBase.enumValueIndex = EditorGUILayout.Popup(T("PathBase"), sBase.enumValueIndex, _baseNames[Lang]);

                if (sd == 0)
                {
                    Draw("offsetVector", T("OffsetVec"));
                }
                else if (sd == 1)
                {
                    Draw("destinationTransform", T("DestTrans"));
                    var sDestTrans = Get("destinationTransform");
                    bool sNoDest = sDestTrans != null && sDestTrans.objectReferenceValue == null;
                    using (new EditorGUI.DisabledScope(sNoDest))
                    {
                        Draw("relativePositionOffset", T("OffsetVec"));
                    }
                }

                Draw("previewDestination", T("PreviewDest"));
                Draw("previewPath", T("PreviewPath"));
            }

            EditorGUILayout.HelpBox(T("SenderHelp"), MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.LabelField(T("Core"), EditorStyles.boldLabel);
        Draw("target", T("Target"));

        var interactMode = Get("interactionMode");
        if (interactMode != null)
            interactMode.enumValueIndex = EditorGUILayout.Popup(T("InteractMode"), interactMode.enumValueIndex, _interactModeNames[Lang]);

        var imProp0 = Get("interactionMode");
        bool isPassive = imProp0 != null && imProp0.enumValueIndex == 2;
        if (!isPassive)
        {
            DrawAlias(new[] { "trigger", "triggerObject", "triggerTransform" }, T("TriggerObj"));
        }
        else
        {
            EditorGUILayout.HelpBox(T("PassiveHelp"), MessageType.Info);
            var initP = Get("triggerZoneInit");
            if (initP == null || !initP.boolValue)
            {
                WrapZoneToObject(); // 首次进入被动模式：默认包裹整个物体
                if (initP != null) { initP.boolValue = true; serializedObject.ApplyModifiedProperties(); }
            }
            Draw("triggerZoneCenter", T("ZoneCenter"));
            Draw("triggerZoneSize", T("ZoneSize"));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(T("ZoneWrap"))) WrapZoneToObject();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        var motionMode = Get("motionMode");
        if (motionMode != null)
            motionMode.enumValueIndex = EditorGUILayout.Popup(T("MotionMode"), motionMode.enumValueIndex, _motionModeNames[Lang]);
        bool isRotate = motionMode != null && motionMode.enumValueIndex == (int)EMotion.Rotate;

        var destMode = Get("destinationMode");
        int dMode = destMode != null ? destMode.enumValueIndex : 0;

        if (isRotate)
        {
            EditorGUILayout.LabelField(T("RotateParams"), EditorStyles.boldLabel);

            var rotateMode = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
            if (rotateMode != null)
                rotateMode.enumValueIndex = EditorGUILayout.Popup(T("RotateMode"), rotateMode.enumValueIndex, _rotateModeNames[Lang]);
            bool isAxis = rotateMode != null && rotateMode.enumValueIndex == (int)ERotMode.Axis;

            if (isAxis)
            {
                var axisSource = Get("axisSource");
                if (axisSource != null)
                    axisSource.enumValueIndex = EditorGUILayout.Popup(T("AxisSource"), axisSource.enumValueIndex, _axisSourceNames[Lang]);
                int srcMode = axisSource != null ? axisSource.enumValueIndex : 0;

                if (srcMode == (int)EAxisSource.ObjectAlign)
                {
                    Draw("axisObject", T("AxisObject"));
                    var upProp = Get("axisUp");
                    if (upProp != null)
                        upProp.enumValueIndex = EditorGUILayout.Popup(T("AxisUp"), upProp.enumValueIndex, _axisUpNames[Lang]);
                    Draw("axisPositionOffset", T("AxisPosOffset"));
                    Draw("axisLength", T("AxisLength"));
                }
                else if (srcMode == (int)EAxisSource.Euler)
                {
                    Draw("axisEuler", T("AxisEuler"));
                    Draw("axisPositionOffset", T("AxisPosOffset"));
                    Draw("axisLength", T("AxisLength"));
                    DrawWorldAxisButtons();
                }
                else
                {
                    Draw("axisStart", T("AxisStart"));
                    Draw("axisEnd", T("AxisEnd"));
                    Draw("axisLength", T("AxisLength"));
                    DrawWorldAxisButtons();
                }

                Draw("axisAngle", T("AxisAngle"));
            }
            else
            {
                Draw("rotateVector", T("RotVec"));
            }
        }
        else
        {
            EditorGUILayout.LabelField(T("MoveParams"), EditorStyles.boldLabel);

            if (destMode != null)
                destMode.enumValueIndex = EditorGUILayout.Popup(T("DestMode"), destMode.enumValueIndex, _destModeNames[Lang]);
            dMode = destMode != null ? destMode.enumValueIndex : 0;

            if (dMode == 0)
            {
                Draw("offsetVector", T("OffsetVec"));
                Draw("rotationVector", T("RotVec"));
            }
            else if (dMode == 1)
            {
                Draw("destinationTransform", T("DestTrans"));
                var destTrans = Get("destinationTransform");
                bool noDest = destTrans != null && destTrans.objectReferenceValue == null;
                using (new EditorGUI.DisabledScope(noDest))
                {
                    Draw("relativePositionOffset", T("OffsetVec"));
                    Draw("relativeRotationOffset", T("RotVec"));
                }
            }
            else if (dMode == 2)
            {
                Draw("pathPoints", T("PathPoints"));
                Draw("closePath", T("ClosePath"));
            }

            var movePath = Get("movePathMode") ?? Get("movePath");
            if (movePath != null)
                movePath.enumValueIndex = EditorGUILayout.Popup(T("MovePath"), movePath.enumValueIndex, _movePathNames[Lang]);
            bool isCurve = movePath != null && movePath.enumValueIndex == (int)EPath.Curve;

            if (isCurve)
            {
                Draw("moveCurve", T("MoveCurve"));
                EditorGUILayout.HelpBox(T("CurveHelp"), MessageType.Info);
                DrawAlias(new[] { "autoCurveOrigin", "autoOrigin" }, T("AutoOrigin"));
                var auto = Get("autoCurveOrigin") ?? Get("autoOrigin");
                using (new EditorGUI.DisabledScope(auto != null && auto.boolValue))
                    Draw("curveOrigin", T("CurveOrigin"));
            }

            Draw("gravityScale", T("Gravity"));
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(T("Param"), EditorStyles.boldLabel);

        var pathBase = Get("pathBase") ?? Get("baseMode");
        if (pathBase != null)
            pathBase.enumValueIndex = EditorGUILayout.Popup(T("PathBase"), pathBase.enumValueIndex, _baseNames[Lang]);

        var speedMode = Get("speedMode");
        if (speedMode != null)
            speedMode.enumValueIndex = EditorGUILayout.Popup(T("SpeedMode"), speedMode.enumValueIndex, _speedModeNames[Lang]);
        int sm = speedMode != null ? speedMode.enumValueIndex : 0;

        if (sm == (int)ESpeed.Fixed)
        {
            Draw("moveSpeed", T("MoveSpeed"));
        }
        else
        {
            Draw("speedCurve", T("SpeedCurve"));
        }

        Draw("drag", T("Drag"));

        var loopMode = Get("loopMode");
        if (loopMode != null)
            loopMode.enumValueIndex = EditorGUILayout.Popup(T("LoopMode"), loopMode.enumValueIndex, _loopModeNames[Lang]);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("TrigTiming"), EditorStyles.boldLabel);
        Draw("triggerCooldown", T("Cooldown"));
        ClampFloat("triggerCooldown", 0f);
        Draw("startDelay", T("StartDelay"));
        ClampFloat("startDelay", 0f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("ChainTitle"), EditorStyles.boldLabel);
        Draw("useChainEvents", T("UseChain"));
        var sceP = Get("useChainEvents");
        if (sceP == null || sceP.boolValue)
        {
            Draw("onReachDestination", T("OnReach"));
            Draw("onReachStart", T("OnReachStart"));
            Draw("nextOnReach", T("NextOnReach"));
            Draw("nextOnReturn", T("NextOnReturn"));
            if (dMode == 2) Draw("onPathPointEvents", T("PathPointEvents"));
            Draw("onGrabbed", T("OnGrabbed"));
            Draw("onReleased", T("OnReleased"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("Sounds"), EditorStyles.boldLabel);
        Draw("useSounds", T("UseSounds"));
        var useSoundsP = Get("useSounds");
        if (useSoundsP != null && useSoundsP.boolValue)
        {
            Draw("reachSound", T("ReachSound"));
            Draw("returnSound", T("ReturnSound"));
            Draw("grabSound", T("GrabSound"));
            Draw("releaseSound", T("ReleaseSound"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("NetTitle"), EditorStyles.boldLabel);
        Draw("syncEveryNFrames", T("SyncEveryN"));
        ClampInt("syncEveryNFrames", 1);
        Draw("syncMinDelta", T("SyncMinDelta"));
        ClampFloat("syncMinDelta", 0f);

        var imProp = Get("interactionMode");
        if (imProp != null && imProp.enumValueIndex == 1)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(T("DragOptTitle"), EditorStyles.boldLabel);
            Draw("snapBackSpeed", T("SnapBack"));
            Draw("lockWhileHeld", T("LockHeld"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("HoverTitle"), EditorStyles.boldLabel);
        var hmP = Get("hoverMode");
        if (hmP != null)
            hmP.enumValueIndex = EditorGUILayout.Popup(T("HoverMode"), hmP.enumValueIndex, _hoverModeNames[Lang]);
        if (hmP != null && hmP.enumValueIndex == 1)
            EditorGUILayout.HelpBox(T("NativeHelp"), MessageType.Info);
        if (hmP != null && hmP.enumValueIndex == 2)
        {
            Draw("hoverVisual", T("HoverVisual"));
            Draw("hoverDistance", T("HoverDistance"));
            ClampFloat("hoverDistance", 0.1f);

            Draw("hoverHighlight", T("HoverHighlight"));
            var hhP = Get("hoverHighlight");
            if (hhP != null && hhP.boolValue)
            {
                Draw("highlightColor", T("HighlightColor"));
                Draw("highlightBlink", T("HighlightBlink"));
                var hbP = Get("highlightBlink");
                if (hbP != null && hbP.boolValue)
                {
                    Draw("highlightBlinkSpeed", T("HighlightSpeed"));
                    ClampFloat("highlightBlinkSpeed", 0.1f);
                }
            }

            Draw("hoverText", T("HoverText"));
            var htP = Get("hoverText");
            if (htP != null && htP.boolValue)
            {
                Draw("hoverTextContent", T("HoverTextContent"));
                Draw("hoverTextSize", T("HoverTextSize"));
                ClampFloat("hoverTextSize", 0.01f);
                Draw("hoverTextColor", T("HoverTextColor"));
                Draw("hoverTextOffset", T("HoverTextOffset"));
                Draw("hoverFont", T("HoverFont"));
                HoverTextSync();
            }
        }

        // 清理历史版本遗留：TriggerZone 子物体（旧代理/碰撞体/Sender）、本体上的托管碰撞体
        if (_drag != null)
        {
            var zoneT2 = _drag.transform.Find("TriggerZone");
            if (zoneT2 != null)
            {
                Undo.DestroyObjectImmediate(zoneT2.gameObject);
            }
            var oldCol = _drag.GetComponent<BoxCollider>();
            var czP = Get("triggerZoneCenter");
            var szP = Get("triggerZoneSize");
            if (oldCol != null && oldCol.isTrigger && czP != null && szP != null
                && oldCol.center == czP.vector3Value && oldCol.size == szP.vector3Value)
            {
                Undo.DestroyObjectImmediate(oldCol); // 旧版本迁移
            }
        }

        // 悬停文字关闭时：清理自动创建的 HoverPrompt 子物体
        if (_drag != null)
        {
            var htOffP = Get("hoverText");
            var htChild = _drag.transform.Find("HoverPrompt");
            if (htChild != null && (htOffP == null || !htOffP.boolValue))
            {
                Undo.DestroyObjectImmediate(htChild.gameObject);
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(T("Preview"), EditorStyles.boldLabel);
        Draw("previewDestination", T("PreviewDest"));
        if (!isRotate && dMode == 0)
        {
            var previewDestToggle = Get("previewDestination");
            using (new EditorGUI.DisabledScope(previewDestToggle == null || !previewDestToggle.boolValue))
                Draw("editEndPosition", T("EditEnd"));
        }
        Draw("previewPath", T("PreviewPath"));
        var pdToggle = Get("previewDestination");
        using (new EditorGUI.DisabledScope(pdToggle == null || !pdToggle.boolValue))
            Draw("previewGhostSteps", T("PreviewGhosts"));
        Draw("previewRelationDepth", T("PreviewRelDepth"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(T("ScrubTitle"), EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        bool newScrubOn = GUILayout.Toggle(_scrubOn, T("ScrubBtn"), EditorStyles.miniButton, GUILayout.Width(64));
        using (new EditorGUI.DisabledScope(!newScrubOn))
        {
            float newScrubT = GUILayout.HorizontalSlider(_scrubT, 0f, 1f);
            GUILayout.Label(newScrubT.ToString("0.00"), GUILayout.Width(36));
            if (newScrubT != _scrubT)
            {
                _scrubT = newScrubT;
                if (newScrubOn) ApplyScrubPose(_scrubT, false); // 实时驱动物体（不进 Undo）
            }
        }
        EditorGUILayout.EndHorizontal();
        if (newScrubOn != _scrubOn)
        {
            _scrubOn = newScrubOn;
            if (_scrubOn) ApplyScrubPose(_scrubT, false); // 开启：立即摆到滑条姿态
            else ApplyScrubPose(0f, false);               // 关闭：回到冻结基准的起点姿态
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(T("PoseBase")))
        {
            CapturePoseBase();
            EditorUtility.SetDirty(_drag);
            SceneView.RepaintAll(); // 立即刷新场景预览绘制
        }
        using (new EditorGUI.DisabledScope(!_scrubOn))
        {
            if (GUILayout.Button(T("ResetPose")))
            {
                _scrubT = 0f; // 滑条同步归零
                ApplyScrubPose(0f);
            }
        }
        EditorGUILayout.EndHorizontal();

        Draw("previewMesh", T("PreviewMesh"));

        serializedObject.ApplyModifiedProperties();
    }

    private void ApplyAxisSceneChange()
    {
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(_drag);
    }

    // 与 Drag.ComputeAxisLine 保持一致
    private void EditorComputeAxis(out Vector3 anchor, out Vector3 dir, out float len)
    {
        var srcP = Get("axisSource");
        int src = srcP != null ? srcP.enumValueIndex : 0;
        var lenP = Get("axisLength");
        len = lenP != null ? lenP.floatValue : 1f;
        if (len < 0.001f) len = 0.001f;

        Vector3 s = Get("axisStart") != null ? Get("axisStart").vector3Value : Vector3.zero;
        Vector3 en = Get("axisEnd") != null ? Get("axisEnd").vector3Value : Vector3.up;

        if (src == (int)EAxisSource.ObjectAlign && _drag.axisObject != null)
        {
            var upP = Get("axisUp");
            int up = upP != null ? upP.enumValueIndex : 1;
            Vector3 local = up == 0 ? Vector3.right : (up == 2 ? Vector3.forward : Vector3.up);
            dir = (_drag.axisObject.rotation * local).normalized;
            anchor = RefPoint(_drag.axisObject) + (Get("axisPositionOffset")?.vector3Value ?? Vector3.zero);
        }
        else if (src == (int)EAxisSource.Euler)
        {
            dir = (Quaternion.Euler(Get("axisEuler")?.vector3Value ?? Vector3.zero) * Vector3.up).normalized;
            anchor = RefPoint(_drag.target) + (Get("axisPositionOffset")?.vector3Value ?? Vector3.zero);
        }
        else
        {
            Vector3 d = en - s;
            dir = (d.sqrMagnitude > 1e-8f) ? d.normalized : Vector3.up;
            anchor = s;
        }
    }

    private void DrawWorldAxisButtons()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(T("AlignWorldAxis"));
        if (GUILayout.Button("X")) AlignAxisToWorld(Vector3.right);
        if (GUILayout.Button("Y")) AlignAxisToWorld(Vector3.up);
        if (GUILayout.Button("Z")) AlignAxisToWorld(Vector3.forward);
        EditorGUILayout.EndHorizontal();
    }

    private void AlignAxisToWorld(Vector3 dir)
    {
        var srcP = Get("axisSource");
        int src = srcP != null ? srcP.enumValueIndex : 0;

        if (src == (int)EAxisSource.Euler)
        {
            // 默认框架 up=+Y: 用最短弧得到等价欧拉角
            var ep = Get("axisEuler");
            if (ep != null) ep.vector3Value = Quaternion.FromToRotation(Vector3.up, dir).eulerAngles;
        }
        else
        {
            // Manual: 保留起点, 终点 = 起点 + 世界方向 * 长度
            var sp = Get("axisStart");
            var ep2 = Get("axisEnd");
            var lp = Get("axisLength");
            Vector3 a = sp != null ? sp.vector3Value : Vector3.zero;
            float len = lp != null ? lp.floatValue : 1f;
            if (ep2 != null) ep2.vector3Value = a + dir * len;
        }
        ApplyAxisSceneChange();
    }

    private void OnSceneGUI()
    {
        if (_drag == null) return;
        serializedObject.Update(); // 场景 GUI 读取前刷新，避免用到过期的序列化数据（预览按旧轴位置渲染）

        var senderRoleProp = Get("role");
        if (senderRoleProp != null && senderRoleProp.enumValueIndex == (int)ERole.Sender)
        {
            var sTm = Get("triggerMoveMode") ?? Get("triggerMove");
            int sMode = sTm != null ? sTm.enumValueIndex : (int)ETrig.Rail;
            var sPreviewPath = Get("previewPath");
            bool sPathOn = sPreviewPath != null && sPreviewPath.boolValue;
            if (sMode == (int)ETrig.Rail && sPathOn)
            {
                var sBase = Get("pathBase") ?? Get("baseMode");
                bool sCenter = sBase != null && sBase.enumValueIndex == (int)EBase.Center;
                Vector3 railStart = sCenter ? GetObjectCenter(_drag.transform) : _drag.transform.position;
                Vector3 railEnd = railStart + (Get("offsetVector")?.vector3Value ?? Vector3.zero);
                var sDestMode = Get("destinationMode");
                if (sDestMode != null && sDestMode.enumValueIndex == 1)
                {
                    var dt = _drag.destinationTransform;
                    if (dt != null)
                    {
                        railEnd = (sCenter ? GetObjectCenter(dt) : dt.position) + (Get("relativePositionOffset")?.vector3Value ?? Vector3.zero);
                    }
                }
                Handles.color = new Color(1f, 0.5f, 0f);
                Handles.DrawLine(railStart, railEnd);
                Handles.SphereHandleCap(0, railStart, Quaternion.identity, HandleUtility.GetHandleSize(railStart) * 0.06f, EventType.Repaint);
                Handles.SphereHandleCap(0, railEnd, Quaternion.identity, HandleUtility.GetHandleSize(railEnd) * 0.06f, EventType.Repaint);
            }
            return;
        }

        if (_drag.target == null) return;

        var previewDestProp = Get("previewDestination");
        bool previewDestOn = previewDestProp != null && previewDestProp.boolValue;
        var previewPathProp = Get("previewPath");
        bool previewPathOn = previewPathProp != null && previewPathProp.boolValue;
        var pathBaseProp = Get("pathBase") ?? Get("baseMode");
        bool byCenter = pathBaseProp != null && pathBaseProp.enumValueIndex == (int)EBase.Center;
        var motionModeProp = Get("motionMode");
        bool isRotate = motionModeProp != null && motionModeProp.enumValueIndex == (int)EMotion.Rotate;
        var rotatePathProp = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
        bool isAxis = isRotate && rotatePathProp != null && rotatePathProp.enumValueIndex == (int)ERotMode.Axis;
        var movePath = Get("movePathMode") ?? Get("movePath");
        bool isCurve = !isRotate && movePath != null && movePath.enumValueIndex == (int)EPath.Curve;

        // 冻结基准：应用过姿态预览后，所有预览（终点/幽灵/弧线/轴线）都在冻结坐标系里计算，
        // 不随物体当前姿态漂移
        bool frozenAxis = _poseBaseValid && _poseBaseIsAxis;
        Vector3? fDir = frozenAxis ? _poseBaseAxisDir : (Vector3?)null;
        Vector3? fAnchor = frozenAxis ? _poseBaseAxisAnchor : (Vector3?)null;
        Vector3 start = _poseBaseValid ? _poseBaseStart : RefPoint(_drag.target);
        Vector3 end = _poseBaseValid ? _poseBaseEnd : GetDestinationCenter();
        Quaternion baseRot = _poseBaseValid ? _poseBaseRot : _drag.target.rotation;
        Quaternion endRot = EditorRotationAt(1f, isRotate, isAxis, baseRot, fDir);
        Vector3 armEnd = endRot * Quaternion.Inverse(baseRot)
                         * (byCenter ? GetObjectCenter(_drag.target) - _drag.target.position : Vector3.zero);
        Vector3 endPivot = end - armEnd;

        if (previewDestOn)
        {
            DrawMeshWireframe(endPivot, endRot, _drag.target);

            var ghostsProp = Get("previewGhostSteps");
            int ghostCount = ghostsProp != null ? ghostsProp.intValue : 0;
            if (ghostCount > 0)
            {
                for (int gi = 1; gi <= ghostCount; gi++)
                {
                    float gt = gi / (float)(ghostCount + 1);
                    Vector3 gp = EditorPositionAt(gt, start, end, isRotate, isAxis, fDir, fAnchor);
                    Quaternion gr = EditorRotationAt(gt, isRotate, isAxis, baseRot, fDir);
                    Vector3 gArm = gr * Quaternion.Inverse(baseRot)
                                   * (byCenter ? GetObjectCenter(_drag.target) - _drag.target.position : Vector3.zero);
                    DrawMeshWireframe(gp - gArm, gr, _drag.target);
                }
            }
        }

        if (_scrubOn)
        {
            Quaternion sBaseRot = _poseBaseValid ? _poseBaseRot : _drag.target.rotation;
            Vector3 sStart = _poseBaseValid ? _poseBaseStart : start;
            Vector3 sEnd = _poseBaseValid ? _poseBaseEnd : end;
            Vector3 sp = EditorPositionAt(_scrubT, sStart, sEnd, isRotate, isAxis, fDir, fAnchor);
            Quaternion sr = EditorRotationAt(_scrubT, isRotate, isAxis, sBaseRot, fDir);
            Vector3 sArm = sr * Quaternion.Inverse(sBaseRot)
                           * (byCenter ? GetObjectCenter(_drag.target) - _drag.target.position : Vector3.zero);
            DrawMeshWireframe(sp - sArm, sr, _drag.target, new Color(1f, 0.85f, 0.2f));
        }

        var editEndProp = Get("editEndPosition");
        var destModeProp = Get("destinationMode");
        if (editEndProp != null && editEndProp.boolValue && previewDestOn && !isRotate
            && (destModeProp == null || destModeProp.enumValueIndex == 0))
        {
            Vector3 endHandle = start + (Get("offsetVector")?.vector3Value ?? Vector3.zero);
            Handles.color = Color.magenta;
            EditorGUI.BeginChangeCheck();
            Vector3 newEnd = Handles.PositionHandle(endHandle, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_drag, "Move Destination");
                var ov = Get("offsetVector");
                if (ov != null) ov.vector3Value = newEnd - start;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_drag);
            }
        }

        if (isAxis)
        {
            var srcProp = Get("axisSource");
            int srcMode = srcProp != null ? srcProp.enumValueIndex : 0;

            Vector3 anchor, axisDir;
            float axisLen;
            if (frozenAxis)
            {
                anchor = fAnchor.Value;
                axisDir = fDir.Value;
                var lenP2 = Get("axisLength");
                axisLen = lenP2 != null ? lenP2.floatValue : 1f;
                if (axisLen < 0.001f) axisLen = 0.001f;
            }
            else
            {
                EditorComputeAxis(out anchor, out axisDir, out axisLen);
            }
            Vector3 a = anchor;
            Vector3 b = anchor + axisDir * axisLen;

            Handles.color = Color.cyan;
            Handles.DrawLine(a, b);
            Handles.SphereHandleCap(0, a, Quaternion.identity, HandleUtility.GetHandleSize(a) * 0.06f, EventType.Repaint);
            Handles.SphereHandleCap(0, b, Quaternion.identity, HandleUtility.GetHandleSize(b) * 0.06f, EventType.Repaint);

            var alProp = Get("axisLength");

            if (srcMode == (int)EAxisSource.Manual)
            {
                var asProp = Get("axisStart");
                var aeProp = Get("axisEnd");
                Vector3 sa = asProp != null ? asProp.vector3Value : Vector3.zero;
                Vector3 sb = aeProp != null ? aeProp.vector3Value : Vector3.up;

                Handles.color = Color.red;
                EditorGUI.BeginChangeCheck();
                Vector3 newA = Handles.PositionHandle(sa, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && asProp != null)
                {
                    Undo.RecordObject(_drag, "Move Axis Start");
                    asProp.vector3Value = newA;
                    if (alProp != null) alProp.floatValue = (sb - newA).magnitude;
                    ApplyAxisSceneChange();
                }

                Handles.color = Color.green;
                EditorGUI.BeginChangeCheck();
                Vector3 newB = Handles.PositionHandle(sb, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && aeProp != null)
                {
                    Undo.RecordObject(_drag, "Move Axis End");
                    aeProp.vector3Value = newB;
                    if (alProp != null) alProp.floatValue = (newB - sa).magnitude;
                    ApplyAxisSceneChange();
                }
            }
            else
            {
                // ObjectAlign / Euler: 黄色手柄放在轴中点，整体拖动轴位置
                // 中点避开物体原生 Gizmo 的轴心抢占；回写用「新手柄位置 - 不含偏移的稳定基准」，
                // 基准在拖动过程中不变。
                var offProp = Get("axisPositionOffset");
                if (offProp != null)
                {
                    Vector3 mid = a + axisDir * axisLen * 0.5f;
                    Vector3 baseMid = mid - offProp.vector3Value;
                    Handles.color = Color.yellow;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newMid = Handles.PositionHandle(mid, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_drag, "Move Axis Center");
                        offProp.vector3Value = newMid - baseMid;
                        ApplyAxisSceneChange();
                    }
                    Handles.SphereHandleCap(0, mid, Quaternion.identity, HandleUtility.GetHandleSize(mid) * 0.05f, EventType.Repaint);

                    // 轴长手柄：拖动轴终点调整 axisLength
                    if (srcMode != (int)EAxisSource.Manual)
                    {
                        var alenP = Get("axisLength");
                        if (alenP != null)
                        {
                            Vector3 axisTip = a + axisDir * axisLen;
                            Handles.color = Color.white;
                            EditorGUI.BeginChangeCheck();
                            Vector3 newTip = Handles.FreeMoveHandle(axisTip, HandleUtility.GetHandleSize(axisTip) * 0.07f, Vector3.zero, Handles.SphereHandleCap);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_drag, "Axis Length");
                                alenP.floatValue = Mathf.Max(0.01f, Vector3.Dot(newTip - a, axisDir));
                                ApplyAxisSceneChange();
                            }
                        }
                    }
                }
            }
        }

        DrawChainPreviews(start);

        if (isRotate)
        {
            // 公转弧线：采样参考点绕轴的轨迹
            if (previewPathOn && isAxis)
            {
                Vector3 arcAnchor, arcDir;
                if (frozenAxis) { arcAnchor = fAnchor.Value; arcDir = fDir.Value; }
                else EditorComputeAxis(out arcAnchor, out arcDir, out _);
                float arcAng = Get("axisAngle") != null ? Get("axisAngle").floatValue : 360f;
                Vector3 c0 = _poseBaseValid ? _poseBaseStart : RefPoint(_drag.target);
                Handles.color = new Color(0f, 0.75f, 0.75f, 0.9f);
                const int arcSeg = 48;
                Vector3 prevPt = c0;
                for (int i = 1; i <= arcSeg; i++)
                {
                    float tt = i / (float)arcSeg;
                    Vector3 pt = arcAnchor + Quaternion.AngleAxis(arcAng * tt, arcDir) * (c0 - arcAnchor);
                    Handles.DrawLine(prevPt, pt);
                    prevPt = pt;
                }
            }
            return;
        }

        if (!previewPathOn) return;

        var destMode0 = Get("destinationMode");
        int dMode0 = destMode0 != null ? destMode0.enumValueIndex : 0;
        var pts0 = _drag.pathPoints;
        if (dMode0 == 2 && pts0 != null && pts0.Length >= 2)
        {
            var closeP0 = Get("closePath");
            bool closed0 = closeP0 != null && closeP0.boolValue;
            Handles.color = Color.cyan;
            Vector3 prevPt = start;
            for (int i = 0; i < pts0.Length; i++)
            {
                Vector3 pt = pts0[i] != null ? RefPoint(pts0[i]) : prevPt;
                Handles.DrawLine(prevPt, pt);
                prevPt = pt;
            }
            if (closed0) Handles.DrawLine(prevPt, start);
            return;
        }

        Handles.color = Color.cyan;
        Handles.DrawLine(start, end);

        if (!isCurve) return;

        var auto = Get("autoCurveOrigin") ?? Get("autoOrigin");
        var co = Get("curveOrigin");
        Vector3 P = (auto != null && auto.boolValue) ? (start + end) * 0.5f
            : (co != null ? co.vector3Value : (start + end) * 0.5f);

        Vector3 X = start - P;
        Vector3 Y = end - P;

        Vector3 planeNormal = Vector3.Cross(X, Y);
        if (planeNormal.sqrMagnitude < 1e-6f) planeNormal = Vector3.Cross(X.normalized, Vector3.up);
        planeNormal.Normalize();

        Vector3 baselineDir = end - start;
        float pathLength = baselineDir.magnitude;
        baselineDir = (pathLength > 1e-6f) ? baselineDir / pathLength : Vector3.forward;
        Vector3 perp = Vector3.Cross(baselineDir, planeNormal).normalized;

        Handles.color = Color.red;   Handles.DrawLine(P, start);
        Handles.color = Color.green; Handles.DrawLine(P, end);

        Handles.color = Color.cyan;
        Vector3 prev = start;
        var curveProp = Get("moveCurve");
        AnimationCurve curve = curveProp?.animationCurveValue;
        float v0 = (curve != null) ? curve.Evaluate(0f) : 0f;
        float v1 = (curve != null) ? curve.Evaluate(1f) : 0f;
        const int segments = 30;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float v = ((curve != null) ? curve.Evaluate(t) : 0f) - Mathf.Lerp(v0, v1, t);
            Vector3 baseline = Vector3.Lerp(start, end, t);
            Vector3 pt = baseline + perp * (v * pathLength);
            Handles.DrawLine(prev, pt);
            prev = pt;
        }

        Handles.color = Color.yellow;
        EditorGUI.BeginChangeCheck();
        Vector3 newP = Handles.PositionHandle(P, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_drag, "Move Curve Origin");
            if (co != null) co.vector3Value = newP;
            if (auto != null) auto.boolValue = false;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_drag);
        }
        Handles.SphereHandleCap(0, P, Quaternion.identity, HandleUtility.GetHandleSize(P) * 0.08f, EventType.Repaint);
    }

    private Vector3 RefPoint(Transform obj)
    {
        if (obj == null) return Vector3.zero;
        var pb = Get("pathBase") ?? Get("baseMode");
        bool byCenter = pb != null && pb.enumValueIndex == (int)EBase.Center;
        if (!byCenter) return obj.position;
        return GetObjectCenter(obj);
    }

    private Vector3 GetObjectCenter(Transform obj)
    {
        if (obj == null) return Vector3.zero;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        bool found = false;

        foreach (var r in obj.GetComponentsInChildren<Renderer>(false))
        {
            if (r == null) continue;
            min = Vector3.Min(min, r.bounds.min);
            max = Vector3.Max(max, r.bounds.max);
            found = true;
        }
        if (!found)
        {
            foreach (var c in obj.GetComponentsInChildren<Collider>(false))
            {
                if (c == null) continue;
                min = Vector3.Min(min, c.bounds.min);
                max = Vector3.Max(max, c.bounds.max);
                found = true;
            }
        }
        if (!found) return obj.position;
        return (min + max) * 0.5f;
    }

    private Vector3 GetDestinationCenter()
    {
        var motionModeProp = Get("motionMode");
        if (motionModeProp != null && motionModeProp.enumValueIndex == (int)EMotion.Rotate)
        {
            var rp = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
            if (rp != null && rp.enumValueIndex == (int)ERotMode.Axis)
            {
                EditorComputeAxis(out Vector3 anchor, out Vector3 dir, out _);
                float ang = Get("axisAngle") != null ? Get("axisAngle").floatValue : 360f;
                Vector3 c0 = RefPoint(_drag.target);
                return anchor + Quaternion.AngleAxis(ang, dir) * (c0 - anchor);
            }
            return RefPoint(_drag.target);
        }
        var destMode = Get("destinationMode");
        int d = destMode != null ? destMode.enumValueIndex : 0;
        DrawZoneHandles();

        Vector3 start = RefPoint(_drag.target);
        if (d == 0) return start + (Get("offsetVector")?.vector3Value ?? Vector3.zero);
        if (d == 1 && _drag.destinationTransform != null)
            return RefPoint(_drag.destinationTransform) + (Get("relativePositionOffset")?.vector3Value ?? Vector3.zero);
        if (d == 2 && _drag.pathPoints != null && _drag.pathPoints.Length > 0)
        {
            var closeP = Get("closePath");
            Transform lastPt = (closeP != null && closeP.boolValue)
                ? _drag.pathPoints[0]
                : _drag.pathPoints[_drag.pathPoints.Length - 1];
            if (lastPt != null) return RefPoint(lastPt);
        }
        return start;
    }

    private Quaternion GetDestinationRotation()
    {
        var motionModeProp = Get("motionMode");
        if (motionModeProp != null && motionModeProp.enumValueIndex == (int)EMotion.Rotate)
        {
            var rp = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
            if (rp != null && rp.enumValueIndex == (int)ERotMode.Axis)
            {
                EditorComputeAxis(out _, out Vector3 axisDir, out _);
                if (axisDir.sqrMagnitude < 1e-8f) return _drag.target.rotation;
                float ang = Get("axisAngle") != null ? Get("axisAngle").floatValue : 360f;
                return _drag.target.rotation * Quaternion.AngleAxis(ang, axisDir);
            }
            var rvp = Get("rotateVector") ?? Get("rotationVector");
            Vector3 rv = rvp != null ? rvp.vector3Value : Vector3.zero;
            return _drag.target.rotation * Quaternion.Euler(rv);
        }
        var destMode = Get("destinationMode");
        int d = destMode != null ? destMode.enumValueIndex : 0;
        if (d == 0) return _drag.target.rotation * Quaternion.Euler(Get("rotationVector")?.vector3Value ?? Vector3.zero);
        if (d == 1 && _drag.destinationTransform != null)
            return _drag.destinationTransform.rotation * Quaternion.Euler(Get("relativeRotationOffset")?.vector3Value ?? Vector3.zero);
        return _drag.target.rotation;
    }

    private Vector3 EditorPositionAt(float t, Vector3 startPos, Vector3 endPos, bool rotate, bool axis, Vector3? frozenDir = null, Vector3? frozenAnchor = null)
    {
        if (rotate)
        {
            if (axis)
            {
                Vector3 dir, anchor;
                if (frozenDir.HasValue) { dir = frozenDir.Value; anchor = frozenAnchor.Value; }
                else EditorComputeAxis(out anchor, out dir, out _);
                float ang = Get("axisAngle") != null ? Get("axisAngle").floatValue : 360f;
                return anchor + Quaternion.AngleAxis(ang * t, dir) * (startPos - anchor);
            }
            return startPos;
        }
        var destMode = Get("destinationMode");
        var pts = _drag.pathPoints;
        if (destMode != null && destMode.enumValueIndex == 2 && pts != null && pts.Length >= 2)
        {
            var closeP = Get("closePath");
            int segments = (closeP != null && closeP.boolValue) ? pts.Length : pts.Length - 1;
            float scaled = t * segments;
            int index = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, segments - 1);
            float localT = scaled - index;
            Vector3 pa = pts[index] != null ? RefPoint(pts[index]) : startPos;
            Vector3 pb = pts[(index + 1) % pts.Length] != null ? RefPoint(pts[(index + 1) % pts.Length]) : endPos;
            return Vector3.Lerp(pa, pb, localT);
        }
        var mp = Get("movePathMode") ?? Get("movePath");
        if (mp != null && mp.enumValueIndex == (int)EPath.Curve)
        {
            var curveProp = Get("moveCurve");
            AnimationCurve curve = curveProp != null ? curveProp.animationCurveValue : null;
            var auto = Get("autoCurveOrigin") ?? Get("autoOrigin");
            var co = Get("curveOrigin");
            Vector3 P = (auto != null && auto.boolValue) ? (startPos + endPos) * 0.5f
                : (co != null ? co.vector3Value : (startPos + endPos) * 0.5f);
            Vector3 X = startPos - P;
            Vector3 Y = endPos - P;
            Vector3 planeNormal = Vector3.Cross(X, Y);
            if (planeNormal.sqrMagnitude < 1e-6f) planeNormal = Vector3.Cross(X.normalized, Vector3.up);
            planeNormal.Normalize();
            Vector3 baselineDir = endPos - startPos;
            float pathLength = baselineDir.magnitude;
            baselineDir = (pathLength > 1e-6f) ? baselineDir / pathLength : Vector3.forward;
            Vector3 perp = Vector3.Cross(baselineDir, planeNormal).normalized;
            float v0 = (curve != null) ? curve.Evaluate(0f) : 0f;
            float v1 = (curve != null) ? curve.Evaluate(1f) : 0f;
            float v = ((curve != null) ? curve.Evaluate(t) : 0f) - Mathf.Lerp(v0, v1, t);
            return Vector3.Lerp(startPos, endPos, t) + perp * (v * pathLength);
        }
        return Vector3.Lerp(startPos, endPos, t);
    }

    private Quaternion EditorRotationAt(float t, bool rotate, bool axis, Quaternion? baseRot = null, Vector3? frozenDir = null)
    {
        Quaternion bRot = baseRot.HasValue ? baseRot.Value : _drag.target.rotation;
        if (rotate)
        {
            if (axis)
            {
                Vector3 axisDir = frozenDir.HasValue ? frozenDir.Value : EditorComputeAxisDir();
                if (axisDir.sqrMagnitude < 1e-8f) return bRot;
                float ang = Get("axisAngle") != null ? Get("axisAngle").floatValue : 360f;
                return bRot * Quaternion.AngleAxis(ang * t, axisDir);
            }
            var rvp = Get("rotateVector");
            Vector3 rv = rvp != null ? rvp.vector3Value : Vector3.zero;
            return bRot * Quaternion.Euler(rv * t);
        }
        var destMode = Get("destinationMode");
        if (destMode != null && destMode.enumValueIndex == 1 && _drag.destinationTransform != null)
            return Quaternion.Slerp(bRot, _drag.destinationTransform.rotation, t)
                   * Quaternion.Euler((Get("relativeRotationOffset")?.vector3Value ?? Vector3.zero) * t);
        return bRot * Quaternion.Euler((Get("rotationVector")?.vector3Value ?? Vector3.zero) * t);
    }

    // ============ 连锁预览：显示触发关系图中上下游 Drag 的预览 ============

    private bool ChainContains(Drag a, Drag b)
    {
        if (a == null || b == null) return false;
        if (a.nextOnReach != null)
            for (int i = 0; i < a.nextOnReach.Length; i++)
                if (a.nextOnReach[i] == b) return true;
        if (a.nextOnReturn != null)
            for (int i = 0; i < a.nextOnReturn.Length; i++)
                if (a.nextOnReturn[i] == b) return true;
        return false;
    }

    private void AddChainEdges(Drag from, Drag[] arr, List<Drag> next, List<Drag> visited, List<Drag[]> edges)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
        {
            Drag t = arr[i];
            if (t == null || t == _drag || next.Contains(t) || visited.Contains(t)) continue;
            edges.Add(new[] { from, t });
            next.Add(t);
        }
    }

    // 收集「边」而非「节点」：edges[i] = { 连接线起点, 被渲染的 Drag }，深度按层逐级展开
    private void CollectRelatedEdges(List<Drag[]> edges, List<Drag> seeds, bool upstream, int depth)
    {
        var frontier = new List<Drag>(seeds);
        var visited = new List<Drag>(seeds);
        for (int d = 0; d < depth; d++)
        {
            var next = new List<Drag>();
            if (upstream)
            {
                Drag[] all = FindObjectsOfType<Drag>(false);
                for (int i = 0; i < all.Length; i++)
                {
                    Drag o = all[i];
                    if (o == null || visited.Contains(o)) continue;
                    for (int j = 0; j < frontier.Count; j++)
                    {
                        if (ChainContains(o, frontier[j]))
                        {
                            edges.Add(new[] { frontier[j], o });
                            next.Add(o);
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < frontier.Count; j++)
                {
                    Drag f = frontier[j];
                    if (f == null) continue;
                    AddChainEdges(f, f.nextOnReach, next, visited, edges);
                    AddChainEdges(f, f.nextOnReturn, next, visited, edges);
                }
            }
            for (int i = 0; i < next.Count; i++)
                if (!visited.Contains(next[i])) visited.Add(next[i]);
            frontier = next;
            if (frontier.Count == 0) break;
        }
    }

    private Vector3 RefPointOf(Drag o)
    {
        if (o == null || o.target == null) return Vector3.zero;
        return o.pathBase == PathBase.Center ? GetObjectCenter(o.target) : o.target.position;
    }

    private void DrawChainPreviews(Vector3 myPos)
    {
        var depthProp = Get("previewRelationDepth");
        int depth = depthProp != null ? depthProp.intValue : 0;
        if (depth <= 0) return;

        var seeds = new List<Drag> { _drag };
        var downEdges = new List<Drag[]>();
        CollectRelatedEdges(downEdges, seeds, false, depth);
        var upEdges = new List<Drag[]>();
        CollectRelatedEdges(upEdges, seeds, true, depth);

        // 下游（本 Drag 触发别人）= 橙；上游（别人触发本 Drag）= 紫
        Color downColor = new Color(1f, 0.55f, 0.1f);
        Color upColor = new Color(0.65f, 0.4f, 1f);

        // 连接线从「父节点」画到「子节点」：A→B、B→C 逐级呈现，而不是全部连回 A
        for (int i = 0; i < downEdges.Count; i++)
            DrawOtherPreview(downEdges[i][1], downColor, RefPointOf(downEdges[i][0]));
        for (int i = 0; i < upEdges.Count; i++)
            DrawOtherPreview(upEdges[i][1], upColor, RefPointOf(upEdges[i][0]));
    }

    private void DrawOtherPreview(Drag o, Color color, Vector3 fromPos)
    {
        if (o == null || o.target == null) return;

        Vector3 oStart = OtherRefPoint(o, o.target);
        Vector3 oEnd = oStart;
        Quaternion oEndRot = o.target.rotation;

        bool oRotate = o.motionMode == MotionMode.Rotate;
        bool oAxis = oRotate && o.rotateMode == RotateMode.Axis;
        if (oRotate)
        {
            if (oAxis)
            {
                OtherComputeAxis(o, out Vector3 oAnchor, out Vector3 oDir, oStart);
                if (oDir.sqrMagnitude > 1e-8f)
                {
                    oEnd = oAnchor + Quaternion.AngleAxis(o.axisAngle, oDir) * (oStart - oAnchor);
                    oEndRot = o.target.rotation * Quaternion.AngleAxis(o.axisAngle, oDir);
                }
            }
            else if (o.rotateMode == RotateMode.Spherical)
            {
                oEndRot = o.target.rotation * Quaternion.Euler(o.rotateVector);
            }
        }
        else
        {
            oEnd = OtherDestination(o, oStart);
            if (o.destinationMode == DestinationMode.DestinationTransform && o.destinationTransform != null)
                oEndRot = o.destinationTransform.rotation * Quaternion.Euler(o.relativeRotationOffset);
            else
                oEndRot = o.target.rotation * Quaternion.Euler(o.rotationVector);
        }

        // 关系连接线 + 起点标记
        Handles.color = new Color(color.r, color.g, color.b, 0.6f);
        Handles.DrawDottedLine(fromPos, oStart, 4f);
        Handles.SphereHandleCap(0, oStart, Quaternion.identity, HandleUtility.GetHandleSize(oStart) * 0.05f, EventType.Repaint);

        // 用对方自己的预览设置
        bool byC = o.pathBase == PathBase.Center;
        Vector3 arm = oEndRot * Quaternion.Inverse(o.target.rotation)
                      * (byC ? GetObjectCenter(o.target) - o.target.position : Vector3.zero);
        if (o.previewDestination)
            DrawMeshWireframe(oEnd - arm, oEndRot, o.target, color);
        if (o.previewPath && !oRotate)
        {
            Handles.color = color;
            Handles.DrawLine(oStart, oEnd);
        }
    }

    private Vector3 OtherRefPoint(Drag o, Transform t)
    {
        if (t == null) return Vector3.zero;
        return o.pathBase == PathBase.Center ? GetObjectCenter(t) : t.position;
    }

    private Vector3 OtherDestination(Drag o, Vector3 oStart)
    {
        if (o.destinationMode == DestinationMode.DestinationTransform && o.destinationTransform != null)
            return OtherRefPoint(o, o.destinationTransform) + o.relativePositionOffset;
        if (o.destinationMode == DestinationMode.PathPoints && o.pathPoints != null && o.pathPoints.Length > 0
            && o.pathPoints[o.pathPoints.Length - 1] != null)
            return OtherRefPoint(o, o.pathPoints[o.pathPoints.Length - 1]);
        return oStart + o.offsetVector;
    }

    private void OtherComputeAxis(Drag o, out Vector3 anchor, out Vector3 dir, Vector3 oTargetStart)
    {
        if (o.axisSource == AxisSourceMode.ObjectAlign && o.axisObject != null)
        {
            Vector3 local = o.axisUp == AxisUp.X ? Vector3.right : (o.axisUp == AxisUp.Z ? Vector3.forward : Vector3.up);
            dir = (o.axisObject.rotation * local).normalized;
            anchor = OtherRefPoint(o, o.axisObject) + o.axisPositionOffset;
            return;
        }
        if (o.axisSource == AxisSourceMode.Euler)
        {
            dir = (Quaternion.Euler(o.axisEuler) * Vector3.up).normalized;
            anchor = oTargetStart + o.axisPositionOffset;
            return;
        }
        Vector3 d = o.axisEnd - o.axisStart;
        dir = (d.sqrMagnitude > 1e-8f) ? d.normalized : Vector3.up;
        anchor = o.axisStart;
    }

    private Vector3 EditorComputeAxisDir()
    {
        EditorComputeAxis(out _, out Vector3 d, out _);
        return d;
    }

    private void CapturePoseBase()
    {
        if (_drag == null || _drag.target == null) return;
        _poseBaseRot = _drag.target.rotation;
        _poseBaseStart = RefPoint(_drag.target);
        _poseBaseEnd = GetDestinationCenter();
        var mp = Get("motionMode");
        bool rotMode = mp != null && mp.enumValueIndex == (int)EMotion.Rotate;
        var rp = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
        _poseBaseIsAxis = rotMode && rp != null && rp.enumValueIndex == (int)ERotMode.Axis;
        if (_poseBaseIsAxis)
        {
            EditorComputeAxis(out _poseBaseAxisAnchor, out _poseBaseAxisDir, out _);
        }
        _poseBaseValid = true;
    }

    // 同步字段 -> 自动创建的 TextMesh 子物体（含创建）
    private void HoverTextSync()
    {
        if (_drag == null) return;
        var tt = _drag.transform.Find("HoverPrompt");
        GameObject go = tt != null ? tt.gameObject : null;
        var onP = Get("hoverText");
        if (onP == null || !onP.boolValue)
        {
            if (go != null) Undo.DestroyObjectImmediate(go);
            return;
        }
        if (go == null)
        {
            go = new GameObject("HoverPrompt");
            Undo.RegisterCreatedObjectUndo(go, "Create Hover Prompt");
            go.transform.SetParent(_drag.transform, false);
            var tm = go.AddComponent<TextMesh>();
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 64;
        }
        var mesh = go.GetComponent<TextMesh>();
        if (mesh == null) return;
        var cP = Get("hoverTextContent");
        var sP = Get("hoverTextSize");
        var colP = Get("hoverTextColor");
        var offP = Get("hoverTextOffset");
        var fP = Get("hoverFont");
        string txt = cP != null ? cP.stringValue : "";
        if (string.IsNullOrEmpty(txt)) txt = Lang == 0 ? "USE" : "使用";
        if (mesh.text != txt) mesh.text = txt;
        if (colP != null && mesh.color != colP.colorValue) mesh.color = colP.colorValue;
        if (sP != null && mesh.characterSize != sP.floatValue) mesh.characterSize = sP.floatValue;
        if (fP != null && fP.objectReferenceValue != null && mesh.font != (Font)fP.objectReferenceValue) mesh.font = (Font)fP.objectReferenceValue;
        Bounds wb = ZoneWorldBounds(_drag.gameObject);
        Vector3 pos = wb.center + Vector3.up * (wb.extents.y + 0.1f)
                      + (offP != null ? offP.vector3Value : Vector3.zero);
        go.transform.position = pos;
        EditorUtility.SetDirty(go);
    }

    private Bounds ZoneWorldBounds(GameObject go)
    {
        Vector3 wmin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 wmax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        bool found = false;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(false);
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] == null) continue;
            Bounds b = rends[i].bounds;
            wmin = Vector3.Min(wmin, b.min);
            wmax = Vector3.Max(wmax, b.max);
            found = true;
        }
        if (!found) return new Bounds(go.transform.position, Vector3.one);
        return new Bounds((wmin + wmax) * 0.5f, wmax - wmin);
    }

    // ============ 被动触发区域：箱体可视化与手柄 ============

    private void DrawZoneHandles()
    {
        var imP = Get("interactionMode");
        if (imP == null || imP.enumValueIndex != 2) return;
        var cP = Get("triggerZoneCenter");
        var sP = Get("triggerZoneSize");
        if (cP == null || sP == null) return;

        Transform t = _drag.transform;
        Vector3 half = sP.vector3Value * 0.5f;
        Vector3 cW = t.TransformPoint(cP.vector3Value);
        Quaternion rot = t.rotation;

        DrawZoneBox(cW, rot, sP.vector3Value);

        // 6 个面中心手柄（青色方块）：沿法线拖动调整该面
        Vector3[] dirs = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        for (int di = 0; di < 6; di++)
        {
            Vector3 d = dirs[di];
            Vector3 wp = cW + rot * Vector3.Scale(d, half);
            Handles.color = Color.cyan;
            EditorGUI.BeginChangeCheck();
            Vector3 np = Handles.FreeMoveHandle(wp, HandleUtility.GetHandleSize(wp) * 0.05f, Vector3.zero, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                float delta = Vector3.Dot(np - wp, rot * d);
                int axis = d.x != 0f ? 0 : (d.y != 0f ? 1 : 2);
                Vector3 sizeL = sP.vector3Value;
                Vector3 centerL = cP.vector3Value;
                float old = axis == 0 ? sizeL.x : (axis == 1 ? sizeL.y : sizeL.z);
                float ns = Mathf.Max(0.01f, old + delta);
                float real = ns - old;
                if (axis == 0) sizeL.x = ns; else if (axis == 1) sizeL.y = ns; else sizeL.z = ns;
                centerL += d * (real * 0.5f);
                Undo.RecordObject(_drag, "Adjust Trigger Zone");
                sP.vector3Value = sizeL;
                cP.vector3Value = centerL;
                serializedObject.ApplyModifiedProperties();
                }
        }

        // 中心手柄（黄色球）：拖动整个箱体
        Handles.color = Color.yellow;
        EditorGUI.BeginChangeCheck();
        Vector3 nc = Handles.FreeMoveHandle(cW, HandleUtility.GetHandleSize(cW) * 0.07f, Vector3.zero, Handles.SphereHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_drag, "Move Trigger Zone");
            cP.vector3Value = t.InverseTransformPoint(nc);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawZoneBox(Vector3 centerW, Quaternion rot, Vector3 size)
    {
        Vector3 h = size * 0.5f;
        Vector3[] c = new Vector3[8];
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    c[i++] = centerW + rot * new Vector3(h.x * x, h.y * y, h.z * z);
        int[,] edges = { {0,1},{0,2},{0,4},{1,3},{1,5},{2,3},{2,6},{3,7},{4,5},{4,6},{5,7},{6,7} };
        Handles.color = new Color(0.2f, 0.9f, 0.9f, 0.9f);
        for (int e = 0; e < 12; e++)
            Handles.DrawLine(c[edges[e, 0]], c[edges[e, 1]]);
    }

    // （被动触发已改为纯位置检测，不再需要 TriggerZone 子物体 / 碰撞体 / 发送端代理）

    // 计算本物体渲染器/碰撞体在本地空间的包围盒
    private Bounds ZoneLocalBounds(GameObject go)
    {
        Vector3 wmin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 wmax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        bool found = false;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(false);
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] == null) continue;
            Bounds b = rends[i].bounds;
            wmin = Vector3.Min(wmin, b.min);
            wmax = Vector3.Max(wmax, b.max);
            found = true;
        }
        if (!found)
        {
            Collider[] cols = go.GetComponentsInChildren<Collider>(false);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] == null) continue;
                Bounds b = cols[i].bounds;
                wmin = Vector3.Min(wmin, b.min);
                wmax = Vector3.Max(wmax, b.max);
                found = true;
            }
        }
        if (!found) return new Bounds(Vector3.zero, Vector3.one);
        Transform t = go.transform;
        Vector3 cLocal = t.InverseTransformPoint((wmin + wmax) * 0.5f);
        Vector3 sizeW = wmax - wmin;
        Vector3 ls = t.lossyScale;
        Vector3 sizeL = new Vector3(
            sizeW.x / Mathf.Max(Mathf.Abs(ls.x), 1e-4f),
            sizeW.y / Mathf.Max(Mathf.Abs(ls.y), 1e-4f),
            sizeW.z / Mathf.Max(Mathf.Abs(ls.z), 1e-4f));
        return new Bounds(cLocal, sizeL);
    }

    private void WrapZoneToObject()
    {
        if (_drag == null) return;
        Bounds b = ZoneLocalBounds(_drag.gameObject);
        Undo.RecordObject(_drag, "Wrap Trigger Zone");
        var cP = Get("triggerZoneCenter");
        var sP = Get("triggerZoneSize");
        if (cP != null) cP.vector3Value = b.center;
        if (sP != null) sP.vector3Value = b.size;
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    private void ApplyScrubPose(float t, bool recordUndo = true)
    {
        if (_drag == null || _drag.target == null) return;
        if (!_poseBaseValid) CapturePoseBase(); // 应用时自动冻结基准，避免预览以应用后的姿态为基准叠加
        var mp = Get("motionMode");
        bool rotMode = mp != null && mp.enumValueIndex == (int)EMotion.Rotate;
        var rp = Get("rotateMode") ?? Get("rotatePathMode") ?? Get("rotationPathMode");
        bool axisMode = rotMode && rp != null && rp.enumValueIndex == (int)ERotMode.Axis;
        var pb = Get("pathBase");
        bool byCenter = pb != null && pb.enumValueIndex == (int)PathBase.Center;

        Quaternion baseRot = _poseBaseValid ? _poseBaseRot : _drag.target.rotation;
        Vector3 baseStart = _poseBaseValid ? _poseBaseStart : RefPoint(_drag.target);
        Vector3 baseEnd = _poseBaseValid ? _poseBaseEnd : GetDestinationCenter();

        if (recordUndo) Undo.RecordObject(_drag.target, "Apply Drag Pose");
        Vector3? fDir = _poseBaseIsAxis ? _poseBaseAxisDir : (Vector3?)null;
        Vector3? fAnchor = _poseBaseIsAxis ? _poseBaseAxisAnchor : (Vector3?)null;
        Quaternion newRot = EditorRotationAt(t, rotMode, axisMode, baseRot, fDir);
        Vector3 arm = newRot * Quaternion.Inverse(baseRot)
                      * (byCenter ? GetObjectCenter(_drag.target) - _drag.target.position : Vector3.zero);
        _drag.target.rotation = newRot;
        _drag.target.position = EditorPositionAt(t, baseStart, baseEnd, rotMode, axisMode, fDir, fAnchor) - arm;
        EditorUtility.SetDirty(_drag.target);
    }

    private void DrawMeshWireframe(Vector3 pivotPos, Quaternion rot, Transform src, Color? tint = null)
    {
        if (src == null) return;

        Bounds b;
        var mf = src.GetComponent<MeshFilter>();
        bool hasLocal = mf != null && mf.sharedMesh != null;
        if (hasLocal)
        {
            b = mf.sharedMesh.bounds;
        }
        else
        {
            var rend = src.GetComponent<Renderer>();
            if (rend == null) return;
            b = new Bounds(Vector3.zero, rend.bounds.size);
        }

        Vector3 hs = b.size * 0.5f;
        Vector3 c = b.center;
        Matrix4x4 m = Matrix4x4.TRS(pivotPos, rot, Vector3.one);
        Vector3[] v = new Vector3[8];
        v[0] = m.MultiplyPoint(c + new Vector3(-hs.x, -hs.y, -hs.z));
        v[1] = m.MultiplyPoint(c + new Vector3( hs.x, -hs.y, -hs.z));
        v[2] = m.MultiplyPoint(c + new Vector3( hs.x, -hs.y,  hs.z));
        v[3] = m.MultiplyPoint(c + new Vector3(-hs.x, -hs.y,  hs.z));
        v[4] = m.MultiplyPoint(c + new Vector3(-hs.x,  hs.y, -hs.z));
        v[5] = m.MultiplyPoint(c + new Vector3( hs.x,  hs.y, -hs.z));
        v[6] = m.MultiplyPoint(c + new Vector3( hs.x,  hs.y,  hs.z));
        v[7] = m.MultiplyPoint(c + new Vector3(-hs.x,  hs.y,  hs.z));

        Handles.color = tint.HasValue ? tint.Value : Color.green;
        int[][] edges = new int[][]
        {
            new[]{0,1}, new[]{1,2}, new[]{2,3}, new[]{3,0},
            new[]{4,5}, new[]{5,6}, new[]{6,7}, new[]{7,4},
            new[]{0,4}, new[]{1,5}, new[]{2,6}, new[]{3,7}
        };
        foreach (var ed in edges) Handles.DrawLine(v[ed[0]], v[ed[1]]);
    }
}
#endif
