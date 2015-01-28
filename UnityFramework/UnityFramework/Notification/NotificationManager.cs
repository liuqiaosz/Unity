//------------------------------------------------------------------------------
//
//
// 通知管理器,负责整个框架的消息派发和调用
//
//
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace UnityFramework.Notification
{
	public class NotificationManager
	{
		private static NotificationManager Instance = null;
		public static NotificationManager GetInstance()
		{
			if(null == Instance)
			{
				Instance = new NotificationManager();
			}
			
			return Instance;
		}

		private Dictionary<string,List<Action<NotificationArg>>> NotifyDict = null;

		private NotificationManager ()
		{
			NotifyDict = new Dictionary<string, List<Action<NotificationArg>>>();
		}

		/**
		 * 注册通知响应
		 **/
		public void RegisterNotification(string Id,Action<NotificationArg> Handler)
		{
			List<Action<NotificationArg>> Actions = null;
			if(!NotifyDict.ContainsKey(Id))
			{
				Actions = new List<Action<NotificationArg>>();
				NotifyDict.Add(Id,Actions);
			}
			else
			{

				NotifyDict.TryGetValue(Id,out Actions);
			}

			if(null != Actions)
			{
				Actions.Add(Handler);
			}
		}

		/**
		 * 
		 * 移除通知响应
		 **/
		public void UnRegisterNotification(string Id,Action<NotificationArg> Handler)
		{
			List<Action<NotificationArg>> Actions = null;
			if(NotifyDict.ContainsKey(Id))
			{
				NotifyDict.TryGetValue(Id,out Actions);
			}

			if(null != Actions && Actions.Contains(Handler))
			{
				Actions.Remove(Handler);
			}
		}

		/**
		 * 派发通知
		 * 
		 **/
		public void PostNotify(string Id,NotificationArg Arg = null)
		{
			List<Action<NotificationArg>> Actions = null;
			if(NotifyDict.ContainsKey(Id))
			{
				NotifyDict.TryGetValue(Id,out Actions);
			}

			if(null != Actions)
			{
				foreach(Action<NotificationArg> Handler in Actions)
				{
					Handler(Arg);
				}
			}
		}
	}
}

