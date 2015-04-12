using UnityEngine;
using System.Collections;
using ToolKits;
using UnityEngine.UI;
using UnityEditor;
namespace ToolKits.Window
{
    public class UISettingWindow : EditorWindowGeneric
    {
        private bool init = false;
        private string asset_root = "";
        private string pack_id = "";
        private bool mipmap = false;

        void OnGUI()
        {
            if (!init)
            {
                asset_root = EditorSetting.Instance.setting.settingUI.asset_root;
                pack_id = EditorSetting.Instance.setting.settingUI.packTag;
                mipmap = EditorSetting.Instance.setting.settingUI.mipmap;
                init = true;
            }

            EditorGUILayout.BeginVertical();

            //选择保存路径
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI素材根路径", GUILayout.Width(60));
            EditorGUILayout.TextField(asset_root, GUILayout.Width(250), GUILayout.Height(20));
            if (GUILayout.Button("选择", GUILayout.Width(80)))
            {
                asset_root = EditorUtility.OpenFolderPanel("选择根路径", "", "");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("图集ID", GUILayout.Width(60));
            pack_id = EditorGUILayout.TextField(pack_id, GUILayout.Width(250), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            mipmap = EditorGUILayout.Toggle("开启Mipmap", mipmap);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", GUILayout.Width(80)))
            {
                Log("保存");

                EditorSetting.Instance.setting.settingUI.asset_root = asset_root;
                EditorSetting.Instance.setting.settingUI.mipmap = mipmap;
                EditorSetting.Instance.setting.settingUI.packTag = pack_id;
                EditorSetting.Instance.save();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

    }
}