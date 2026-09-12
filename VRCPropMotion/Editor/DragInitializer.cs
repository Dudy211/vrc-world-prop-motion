using System.IO;
using UnityEditor;
using UnityEngine;
using UdonSharp;

namespace VRCPropMotion.Editor
{
    [InitializeOnLoad]
    public static class DragInitializer
    {
        static DragInitializer()
        {
            EditorApplication.delayCall += SetupOnce;
        }

        private static void SetupOnce()
        {
            EditorApplication.delayCall -= SetupOnce;

            // 1. 已存在 Drag.asset 就跳过
            string[] assetGuids = AssetDatabase.FindAssets("t:UdonSharpProgramAsset");
            foreach (var g in assetGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g).ToLowerInvariant();
                if (p.EndsWith("/drag.asset"))
                {
                    return;
                }
            }

            // 2. 找 Drag.cs
            string csPath = FindScriptPath("drag.cs");
            if (string.IsNullOrEmpty(csPath))
            {
                Debug.LogWarning("[VRCPropMotion] 未找到 Drag.cs，跳过自动生成 Drag.asset。");
                return;
            }

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(csPath);
            if (monoScript == null)
            {
                Debug.LogWarning("[VRCPropMotion] 无法加载 Drag.cs 的 MonoScript，跳过自动生成 Drag.asset。");
                return;
            }

            // 3. 创建 UdonSharpProgramAsset 并关联源码
            UdonSharpProgramAsset programAsset = ScriptableObject.CreateInstance<UdonSharpProgramAsset>();
            programAsset.sourceCsScript = monoScript;

            string assetDir = Path.GetDirectoryName(csPath).Replace("\\", "/");
            string assetPath = assetDir + "/Drag.asset";
            AssetDatabase.CreateAsset(programAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[VRCPropMotion] 已自动生成 Drag.asset 并关联 Drag.cs：" + assetPath);
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
