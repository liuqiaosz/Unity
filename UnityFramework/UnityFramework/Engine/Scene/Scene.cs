using System;
using System.Collections.Generic;

using System.Text;
using UnityEngine;
using UnityFramework.Engine.Mgr;
using UnityFramework.Notification;
namespace UnityFramework.Engine.Scene
{
    public class Scene : MonoBehaviour,IScene
    {
		[SerializeField]
		public string SceneName;

		void Start()
		{
			SceneManager.Instance.ChangeScene (this);
		}

		void Destroy()
		{

		}

		/// <summary>
		/// 场景激活
		/// </summary>
		public virtual void OnActive()
		{

		}

		/// <summary>
		/// 场景卸载
		/// </summary>
		public virtual void OnDeactive()
		{

		}

		/// <summary>
		/// 引导逻辑
		/// </summary>
		public virtual void OnGuide()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">Identifier.</param>
		protected void SendNotify(string id)
		{
			NotificationManager.Instance.PostNotify (id, null);
		}
		protected void SendNotify(string id,NotificationArg arg)
		{
			NotificationManager.Instance.PostNotify (id, arg);
		}

		protected void AddNotifyListener(string id,Action<NotificationArg> handler)
		{
			NotificationManager.Instance.RegisterNotification (id, handler);
		}

		protected void RemoveNotifyListener(string id,Action<NotificationArg> handler)
		{
			NotificationManager.Instance.UnRegisterNotification (id, handler);
		}
    }
}
