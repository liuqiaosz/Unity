using System;
using CommonLibrary;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework.Notification;
using UnityFramework.Engine.Mgr;
using UnityFramework.Utils;

namespace UnityFramework.Engine.Core
{
	/**
	 * 框架启动器
	 * 
	 **/
	public class Framework : Singlton<Framework>
	{
		private List<IFrameworkUpdator> updator;
		private FrameworkDemon demon;

		public Action<float> OnUpdateHandler 
		{
			set;
			get;
		}

		public IFrameworkUpdator RegisterActor<T>() where T:IFrameworkUpdator
		{
			try
			{
				IFrameworkUpdator ins = (IFrameworkUpdator)Activator.CreateInstance(typeof(T),false);
				ins.Startup();
				updator.Add(ins);
				return ins;
			}
			catch(Exception ex)
			{
				return null;
			}
		}
		
		public void Startup()
		{
			GameObject loader = new GameObject ("FrameworkDemon");
			demon = loader.AddComponent<FrameworkDemon> ();
			demon.OnUpdateHandler = Update;
			GameObject.DontDestroyOnLoad (loader);

			updator = new List<IFrameworkUpdator> ();
			NotificationManager.Instance.Startup ();
			TimerManager.Instance.Startup ();

			Logger.Debug ("Framework is startup");
		}
		
		void Update(float delta)
		{
			int time = (int)(delta * 1000);
			NotificationManager.Instance.FixedUpdate (time);
			TimerManager.Instance.FixedUpdate (time);

			foreach (IFrameworkUpdator actor in updator) 
			{
				actor.FixedUpdate(time);
			}
		}
	}
}
