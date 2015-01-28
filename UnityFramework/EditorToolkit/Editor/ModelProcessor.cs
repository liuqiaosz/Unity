using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLibrary;
using EditorToolkit.Editor.Domain;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace EditorToolkit.Editor
{
	public class ModelProcessor : Singlton<ModelProcessor>
	{
		/**
		 * 通过给定的配置文件对导入的模型创建导入模型的Prefab
		 **/
		public void CreateModelPrefab(DomainModelSetting Setting,string SavePath)
		{
			if (null != Setting)
			{
				if (!File.Exists(Setting.ModelPath))
				{
					Log("模型文件不存在,查找路径[" + Setting.ModelPath + "]");
					return;
				}

				Log("开始加载模型文件[" + Setting.ModelPath + "]");
				GameObject Model = (GameObject)AssetDatabase.LoadAssetAtPath(Setting.ModelPath, typeof(GameObject));
				/*
				Material ModelMaterial = null;
				if (!string.IsNullOrEmpty(Setting.MaterialPath))
				{
					Log("开始加载模型关联材质[" + Setting.MaterialPath + "]");
					ModelMaterial = (Material)AssetDatabase.LoadAssetAtPath(Setting.MaterialPath, typeof(Material));
				}
				*/

				GameObject Prefab = PrefabUtility.CreatePrefab(SavePath + Setting.PrefabName + ".prefab", Model);

			}
			else
			{
				Log("配置数据为NULL");
			}
		}

		private void Log(string Value)
		{
			Debug.Log(Value);
		}
	}
}
