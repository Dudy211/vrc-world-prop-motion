#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DragTriggerRelay))]
public class DragTriggerRelayEditor : Editor
{
    private SerializedProperty _dragProp;

    private int Lang
    {
        get
        {
            var drag = _dragProp != null ? _dragProp.objectReferenceValue as Drag : null;
            return drag != null ? (int)drag.language : 2;
        }
    }

    private string T(string k)
    {
        int l = Lang;
        switch (k)
        {
            case "DragField": return new[] { "Drag (component to trigger)", "Drag（起動するコンポーネント）", "Drag（要触发的拖动组件）" }[l];
            case "NeedDrag":  return new[] { "Assign the Drag component to trigger.", "起動したい Drag コンポーネントを設定してください。", "请把要触发的 Drag 组件拖到这里。" }[l];
            default:          return k;
        }
    }

    private void OnEnable()
    {
        _dragProp = serializedObject.FindProperty("drag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (_dragProp != null)
        {
            EditorGUILayout.PropertyField(_dragProp, new GUIContent(T("DragField")));
            if (_dragProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(T("NeedDrag"), MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
