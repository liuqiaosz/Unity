using System;
using System.Collections.Generic;

using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ToolKits
{
    /**
     * 处理UI素材导入的一些公共设置
     * 
     **/
    public class UIAssetPostProcessor
    {
        private static UIAssetPostProcessor Instance = null;
        public static UIAssetPostProcessor GetInstance()
        {
            if (null == Instance)
            {
                Instance = new UIAssetPostProcessor();
            }
            return Instance;
        }

        private UIAssetPostProcessor()
        {

        }

        /**
         * 检查导入素材是否UI资源
         **/
        public bool IsImportUIAsset(string ImportPath)
        {
            //return (ImportPath.IndexOf(EditorSetting.GetInstance().SettingUI.AssetPathRoot) == 0);
            return false;
        }

        /**
         * 
         * 设置导入Texture2D纹理用做UI的一些设置
         * 
         **/
        public void SetUIAssetSetting(TextureImporter Importer, string ImportPath)
        {
            /*
            //资源导入的目标文件夹是UI路径
            if (ImportPath.IndexOf(EditorSetting.GetInstance().SettingUI.AssetPathRoot) == 0)
            {
                //类型
                Importer.textureType = (TextureImporterType)Enum.ToObject(typeof(TextureImporterType), EditorSetting.GetInstance().SettingUI.AssetImportType);
                //mipmap
                Importer.mipmapEnabled = EditorSetting.GetInstance().SettingUI.Mipmap;
                Importer.npotScale = TextureImporterNPOTScale.None;
                //纹理格式
                Importer.textureFormat = (TextureImporterFormat)Enum.ToObject(typeof(TextureImporterFormat), EditorSetting.GetInstance().SettingUI.Format);
                string PackType = EditorSetting.GetInstance().SettingUI.PackTag;
                if (!string.IsNullOrEmpty(PackType))
                {
                    switch (PackType)
                    {
                        case "${Folder}":
                            string FolderName = new DirectoryInfo(Path.GetDirectoryName(ImportPath)).Name;
                            Importer.spritePackingTag = FolderName;
                            break;
                    }
                }
            }
             * */
        }

        /**
         * 
         * 创建UI素材AssetBundle
         * 
         * @param       SavePath        资源包保存路径
         * @param       CommonPackTag   公共资源包Tag
         * 
         **/
        public void CreateUIAtlasAssetBundle(string SavePath,string CommonPackTag = "")
        {

        }
    }
}
