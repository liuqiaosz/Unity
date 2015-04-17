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
using CommonLibrary;
using UnityEngine;
using UnityFramework.Misc.Pool;
using UnityFramework.Engine.Core;

namespace UnityFramework.Notification
{
	public class NotificationManager : Singlton<NotificationManager>,IFrameworkUpdator
	{
		private Dictionary<string,List<Action<NotificationArg>>> NotifyDict = null;

		private Queue<Item> addQueue;
		private Queue<Item> removeQueue;
		private Queue<Item> postQueue;
		private NotificationManager ()
		{
			NotifyDict = new Dictionary<string, List<Action<NotificationArg>>>();
			addQueue = new Queue<Item> ();
			removeQueue = new Queue<Item> ();
			postQueue = new Queue<Item> ();
		}

		public void Startup()
		{
			ObjectPool.Instance.CreateObjectPool<Item> (ObjectPool.CAPACITY);
		}

		public void FixedUpdate(int delta)
		{
			Item item;
			while (postQueue.Count > 0) 
			{
				item = postQueue.Dequeue();
				List<Action<NotificationArg>> Actions = null;
				if(NotifyDict.ContainsKey(item.id))
				{
					NotifyDict.TryGetValue(item.id,out Actions);
				}
				
				if(null != Actions)
				{
					foreach(Action<NotificationArg> Handler in Actions)
					{
						Handler(item.arg);
					}
				}
				ObjectPool.Instance.ReturnObject<Item>(item);
			}

			//register notify
			while (addQueue.Count > 0) 
			{
				item = addQueue.Dequeue();
				List<Action<NotificationArg>> Actions = null;
				if(!NotifyDict.ContainsKey(item.id))
				{
					Actions = new List<Action<NotificationArg>>();
					NotifyDict.Add(item.id,Actions);
				}
				else
				{
					NotifyDict.TryGetValue(item.id,out Actions);
				}
				
				if(null != Actions)
				{
					Actions.Add(item.handler);
				}
				ObjectPool.Instance.ReturnObject<Item>(item);
			}

			//unregister notify
			while (removeQueue.Count > 0) 
			{
				item = removeQueue.Dequeue();
				List<Action<NotificationArg>> Actions = null;
				if(NotifyDict.ContainsKey(item.id))
				{
					NotifyDict.TryGetValue(item.id,out Actions);
				}
				
				if(null != Actions && Actions.Contains(item.handler))
				{
					Actions.Remove(item.handler);
				}

				ObjectPool.Instance.ReturnObject<Item>(item);
			}
		}

		/**
		 * 注册通知响应
		 **/
		public void RegisterNotification(string Id,Action<NotificationArg> Handler)
		{
			Item item = ObjectPool.Instance.GetObjectInstance<Item> ();
			item.id = Id;
			item.handler = Handler;
			addQueue.Enqueue (item);
		}

		/**
		 * 
		 * 移除通知响应
		 **/
		public void UnRegisterNotification(string Id,Action<NotificationArg> Handler)
		{
			Item item = ObjectPool.Instance.GetObjectInstance<Item> ();
			item.id = Id;
			item.handler = Handler;
			removeQueue.Enqueue (item);
		}

		/**
		 * 派发通知
		 * 
		 **/
		public void PostNotify(string Id,NotificationArg Arg)
		{
			Item item = ObjectPool.Instance.GetObjectInstance<Item> ();
			item.id = Id;
			item.arg = Arg;
			postQueue.Enqueue (item);

		}

		void Log(string msg)
		{
			UnityEngine.Debug.Log(msg);
		}

	}

	class Item : UnityFramework.Misc.Pool.IPoolable
	{
		public string id;
		public NotificationArg arg;
		public Action<NotificationArg> handler;

		public void Reset()
		{
			id = "";
			arg = null;
			handler = null;

		}

		public void Dispose ()
		{
		}
	}

}

