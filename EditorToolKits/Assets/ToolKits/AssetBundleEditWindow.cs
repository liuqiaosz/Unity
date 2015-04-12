using UnityEditor;
using UnityEngine;

namespace ToolKits
{
    public class AssetBundleEditWindow : EditorWindowGeneric
    {
		/*
        [MenuItem("Tools/AssetBundleTool")]
        static void OpenWindow()
        {
            //EditorWindow.GetWindow(typeof (AssetBundleEditWindow));
            //EditorWindow.GetWindowWithRect (typeof(AssetBundleEditWindow), new UnityEngine.Rect (0, 0, 600, 400));
            ShowWindow<AssetBundleEditWindow>("Test", new UnityEngine.Rect(0, 0, 600, 400));
        }

        [MenuItem("Tools/PackSelected")]
        static void PackSelectFiles()
        {
            string Nav = EditorUtility.SaveFilePanel("选择要输出保存的路径", "", "assets", "assetbundle");
            if (!string.IsNullOrEmpty(Nav))
            {
                //打包在项目里面选择的文件
                Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
                Debug.Log("[" + selection.Length);
                BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, Nav,
                                               BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
                                               BuildTarget.StandaloneWindows);
            }

        }

        [MenuItem("Tools/PackScene")]
        static void PackScene()
        {
            Object[] Objects = GameObject.FindSceneObjectsOfType(typeof(Transform));
            foreach (Transform Obj in Objects)
            {
                Debug.Log(Obj.GetType().ToString());
            }
        }

        public AssetBundleEditWindow()
        {
			AnimationClip Clip = new AnimationClip();
			
        }
		*/

        private string[] AssetType = { "unity3d", "assetbundle" };
        private int SelectAssetTypeIndex = 1;
        private string AssetOutputNav = "";//资源包输出保存目录
        private string AssetSrcNav = "";//提取资源目录

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            //选择保存路径
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出路径", GUILayout.Width(60));
            EditorGUILayout.TextField(AssetOutputNav, GUILayout.Width(250), GUILayout.Height(20));
            if (GUILayout.Button("选择", GUILayout.Width(80)))
            {
                AssetOutputNav = EditorUtility.OpenFolderPanel("选择要输出保存的路径", "", "");
            }
            EditorGUILayout.EndHorizontal();

            //选择提取资源目录
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("提取资源", GUILayout.Width(60));
            EditorGUILayout.TextField(AssetSrcNav, GUILayout.Width(250), GUILayout.Height(20));
            if (GUILayout.Button("选择", GUILayout.Width(80)))
            {
                AssetSrcNav = EditorUtility.OpenFolderPanel("选择提取资源的文件夹", "", "");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("资源类型", GUILayout.Width(60));
            SelectAssetTypeIndex = EditorGUILayout.Popup(SelectAssetTypeIndex, AssetType, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
