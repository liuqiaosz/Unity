using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEditor;
using UnityEngine;
using ToolKits.Domain;
using ToolKits.Common;
namespace ToolKits
{
    /**
     * 
     * 自定义编辑器功能配置数据加载
     * 
     * 配置文件固定放在Assets/Editor文件夹根目录下
     * 
     **/
    public class EditorSetting : Singlton<EditorSetting>
    {
        public const string DEFAULT_PATH = "Assets/Editor/Setting";
        //public const string DEFAULT_PATH = "D:\\UnitySpace\\MyTrain\\Assets\\Editor\\Setting.xml";
        public Setting setting
        {
            set;
            get;
        }
        private EditorSetting()
        {
            
        }

        protected override void Initializer()
        {
            base.Initializer();
            RefreshSetting();
        }

        /**
         * 刷新配置文件
         **/
        public void RefreshSetting()
        {
            Log("refresh config");
            BinaryFormatter formater = new BinaryFormatter();
            if (File.Exists(DEFAULT_PATH))
            {
                Log("file exsist");
                //配置文件找到
                Log("开始加载配置文件[" + DEFAULT_PATH + "]");
                FileStream reader = new FileStream(DEFAULT_PATH, FileMode.Open, FileAccess.Read, FileShare.Read);
                setting = (Setting)formater.Deserialize(reader);
                
                reader.Close();
            }
            else
            {
                Log("file not found");
                //没找到配置文件
                Log("配置文件未找到,创建默认");
                setting = new Setting();
                FileStream writer = new FileStream(DEFAULT_PATH, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
                formater.Serialize(writer, setting);
                writer.Close();
            }
        }

        /// <summary>
        /// 保存当前配置
        /// </summary>
        public void save()
        {
            if (null != setting)
            {
                BinaryFormatter formater = new BinaryFormatter();
                FileStream writer = new FileStream(DEFAULT_PATH, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                formater.Serialize(writer, setting);
                writer.Close();
                Log("save complete");
            }
        }

        private void Log(string Msg)
        {
            Debug.Log(Msg);
        }
    }
}
