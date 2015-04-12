using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ToolKits;
using ToolKits.Domain;
using ToolKits.Common;

namespace ToolKits.Processor
{
    /// <summary>
    /// 导入资源预处理
    /// </summary>
    public class ImportAssetProcessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string saveDirectory = CommonTools.GetDirectoryPath(assetPath);
            SettingUI setting = EditorSetting.Instance.setting.settingUI;
            if (null != setting)
            {
                Log(setting.ToString());

                if (setting.asset_root.IndexOf(saveDirectory) >= 0)
                {
                    Log("is UI Import");
                }
                else
                {
                    Log("not ui");
                }
            }
        }

        private void Log(string msg)
        {
            Debug.Log(msg);
        }
    }

    
}
