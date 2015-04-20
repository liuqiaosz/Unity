using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CommonLibrary;

namespace UnityFramework.Engine.Mgr
{
    /**
     * 场景管理器,负责场景的切换，场景前置资源的加载
     **/
    public class SceneManager : Singlton<SceneManager>
	{
		private IScene currentScene;

		private SceneManager()
		{
		}

		/**
		 * 
		 * 场景配置文件
		 * 
		 **/
		public void SetupConfig(string FilePath)
		{

		}

		/// <summary>
		/// 变更场景
		/// </summary>
		/// <param name="scene">Scene.</param>
		public void ChangeScene(IScene scene)
		{
			if (null != scene) 
			{
				if(null != currentScene)
				{
					currentScene.OnDeactive();
				}
				currentScene = scene;
				currentScene.OnActive();
			}
		}
    }
}
