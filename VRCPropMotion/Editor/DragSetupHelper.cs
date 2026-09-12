using System.IO;
using UnityEditor;
using UnityEngine;
using UdonSharp;

namespace VRCPropMotion.Editor
{
    public static class DragSetupHelper
    {
        [MenuItem("Tools/VRC Prop Motion/Setup Drag Program Asset")]
        public static void SetupDragAsset()
        {
            string csPath = FindScriptPath("drag.cs");
            if (string.IsNullOrEmpty(csPath))
            {
                EditorUtility.DisplayDialog("VRCPropMotion", "没找到 Drag.cs。\n请确认 Assets/VRCPropMotion/Drag.cs 在插件目录里。", "知道了");
                return;
            }

            string assetPath = Path.GetDirectoryName(csPath).Replace("\\", "/") + "/Drag.asset";
            if (AssetDatabase.LoadAssetAtPath<UdonSharpProgramAsset>(assetPath) != null)
            {
                EditorUtility.DisplayDialog("VRCPropMotion", "Drag.asset 已存在：\n" + assetPath, "好的");
                return;
            }

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(csPath);
            UdonSharpProgramAsset programAsset = ScriptableObject.CreateInstance<UdonSharpProgramAsset>();
            programAsset.sourceCsScript = monoScript;

            AssetDatabase.CreateAsset(programAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("VRCPropMotion", "已生成 Drag.asset：\n" + assetPath + "\n\n挂 Drag 组件后，Udon Behaviour 的 Program Source 会自动指向它。", "完成");
        }

        private static string FindScriptPath(string lowerFileName)
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript Drag");
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (Path.GetFileName(p).ToLowerInvariant() == lowerFileName)
                {
                    return p;
                }
            }
            return null;
        }
    }
}