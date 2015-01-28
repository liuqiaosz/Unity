using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using UnityEditor;
using UnityEngine;

using UnityFramework.Engine.Editor.Domain;
namespace Editor
{
    /**
     * 
     * 自定义编辑器功能配置数据加载
     * 
     * 配置文件固定放在Assets/Editor文件夹根目录下
     * 
     **/
    public class EditorSetting
    {
        
        public const string DEFAULT_PATH = "Assets/Editor/Setting.xml";
        //public const string DEFAULT_PATH = "D:\\UnitySpace\\MyTrain\\Assets\\Editor\\Setting.xml";
        private static EditorSetting Instance = null;
        public static EditorSetting GetInstance()
        {
            if (null == Instance)
            {
                Instance = new EditorSetting();
                Instance.Initializer();
            }
            return Instance;
        }

        private EditorSetting()
        {

        }

        private void Initializer()
        {
            //加载配置文件并且初始化
            RefreshSetting(DEFAULT_PATH);
        }

        /**
         * 刷新配置文件
         **/
        public void RefreshSetting(string Path)
        {
            if (File.Exists(Path) && EditorTools.GetFileSuffix(Path) == "xml")
            {
                //配置文件找到
                Log("开始加载配置文件[" + Path + "]");
                XmlDocument Document = new XmlDocument();
                try
                {
                    Document.Load(Path);
                    ReadUISetting(Document);
                    Log("解析完成");
                }
                catch (Exception Ex)
                {
                    Log("加载配置文件异常,异常信息[" + Ex.Message + "]");
                }
            }
            else
            {
                //没找到配置文件
                Log("配置文件未找到,尝试加载的配置文件路径[" + Path + "]");
            }
        }

        public DomainUISetting SettingUI
        {
            private set;
            get;
        }
        
        /**
         * 加载UI相关配置
         **/
        private void ReadUISetting(XmlDocument Document)
        {
            XmlNode NodeAssetPathRoot = Document.SelectSingleNode("/Setting/UI/UIAssetPathRoot");
            if (null != NodeAssetPathRoot)
            {
                SettingUI = new DomainUISetting();
                SettingUI.AssetPathRoot = NodeAssetPathRoot.InnerText;
                XmlNodeList ChildrenNodes = Document.SelectSingleNode("/Setting/UI/UIAssetImportArg").ChildNodes;

                string NodeValue = "";
                foreach (XmlNode ChildrenNode in ChildrenNodes)
                {
                    NodeValue = ChildrenNode.InnerText;
                    switch (ChildrenNode.Name)
                    {
                        case "Type":
                            SettingUI.AssetImportType = int.Parse(NodeValue);
                            break;
                        case "PackTag":
                            SettingUI.PackTag = NodeValue;
                            break;
                        case "Mipmap":
                            SettingUI.Mipmap = bool.Parse(NodeValue);
                            break;
                        case "Format":
                            SettingUI.Format = int.Parse(NodeValue);
                            break;
                    }
                }
            }
        }

        private void Log(string Msg)
        {
            Debug.Log(Msg);
        }
    }
}
