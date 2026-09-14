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
    private readonly Dictionary<string, SerializedProperty> _props = new Dictionary<string, SerializedProperty>();

    private enum EAxisSource { Manual = 0, ObjectAlign = 1, Euler = 2 }

    private string[][] _destModeNames, _interactModeNames, _loopModeNames, _movePathNames, _speedModeNames, _motionModeNames, _rotateModeNames, _baseNames, _triggerMoveNames, _roleNames, _axisSourceNames, _axisUpNames;

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
            new[] { "Interact (Click Trigger)", "Drag (Drag Trigger)" },
            new[] { "インタラクト (クリック)", "ドラッグ (ドラッグ)" },
            new[] { "交互触发 (点击)", "拖动控制 (拖拽)" }
        };
        _loopModeNames = new[]
        {
            new[] { "Loop", "PingPong" },
            new[] { "ループ", "往復" },
            new[] { "循环", "往返" }
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
            case "AxisPosOffset":return new[] { "Position Offset (Scene draggable)", "位置オフセット", "位置偏移 (场景可拖)" }[l];
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
        if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label));
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

        var roleProp = Get("role");
        if (roleProp != null)
            roleProp.enumValueIndex = EditorGUILayout.Popup(T("Role"), roleProp.enumValueIndex, _roleNames[Lang]);
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

        DrawAlias(new[] { "trigger", "triggerObject", "triggerTransform" }, T("TriggerObj"));

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

        Draw("onReachDestination", T("OnReach"));

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

        Vector3 start = RefPoint(_drag.target);
        Vector3 end = isRotate ? start : GetDestinationCenter();
        Quaternion endRot = GetDestinationRotation();
        Vector3 armEnd = endRot * Quaternion.Inverse(_drag.target.rotation) 
                         * (byCenter ? GetObjectCenter(_drag.target) - _drag.target.position : Vector3.zero);
        Vector3 endPivot = end - armEnd;

        if (previewDestOn)
        {
            DrawMeshWireframe(endPivot, endRot, _drag.target);
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

            EditorComputeAxis(out Vector3 anchor, out Vector3 axisDir, out float axisLen);
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
                // ObjectAlign / Euler: 黄色中心手柄, 整体拖动轴位置
                var offProp = Get("axisPositionOffset");
                Handles.color = Color.yellow;
                EditorGUI.BeginChangeCheck();
                Vector3 newCenter = Handles.PositionHandle(a, Quaternion.identity);
                if (EditorGUI.EndChangeCheck() && offProp != null)
                {
                    Undo.RecordObject(_drag, "Move Axis Center");
                    offProp.vector3Value = offProp.vector3Value + (newCenter - a);
                    ApplyAxisSceneChange();
                }
            }
        }

        if (isRotate) return;

        if (!previewPathOn) return;

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
        Vector3 start = RefPoint(_drag.target);
        if (d == 0) return start + (Get("offsetVector")?.vector3Value ?? Vector3.zero);
        if (d == 1 && _drag.destinationTransform != null)
            return RefPoint(_drag.destinationTransform) + (Get("relativePositionOffset")?.vector3Value ?? Vector3.zero);
        if (d == 2 && _drag.pathPoints != null && _drag.pathPoints.Length > 0 && _drag.pathPoints[_drag.pathPoints.Length - 1] != null)
            return RefPoint(_drag.pathPoints[_drag.pathPoints.Length - 1]);
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

    private void DrawMeshWireframe(Vector3 pivotPos, Quaternion rot, Transform src)
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

        Handles.color = Color.green;
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
