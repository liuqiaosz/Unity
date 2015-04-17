using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using CommonLibrary;
using UnityFramework.Misc.Pool;
using UnityFramework.Engine.Mgr;
namespace UnityFramework.Network
{
	public class HTTPNetwork : Singlton<HTTPNetwork>
	{
		private bool IsWorking = false;
		private NetworkWorkEnum Work = NetworkWorkEnum.AUTO;
		private Queue<HTTPRequest> WaitQueue;
		//private Dictionary<HTTPRequest, Action<NetworkStatusEnum, Object>> Cache;

		protected override void Initializer()
		{
 			 base.Initializer();
			 WaitQueue = new Queue<HTTPRequest>();
			 //Cache = new Dictionary<HTTPRequest, Action<NetworkStatusEnum, Object>>();
			ObjectPool.Instance.CreateObjectPool<HTTPRequest>(ObjectPool.CAPACITY);
		}

		public NetworkStatusEnum Post(string Url, string Data, Action<NetworkStatusEnum, Object> OnResponse)
		{
			HTTPRequest Request  = ObjectPool.Instance.GetObjectInstance<HTTPRequest>();
			if (null == Request)
			{
				//从对象池获取对象失败,异常
				return NetworkStatusEnum.ERROR;
			}

			Request.Url = Url;
			Request.Method = "POST";
			Request.OnResp = ActionResponse;
			Request.OnCallback = OnResponse;
			WaitQueue.Enqueue(Request);
			if (Work == NetworkWorkEnum.AUTO)
			{
				if (!IsWorking)
				{
					BeginWork();
				}
			}
			return NetworkStatusEnum.SUCCESS;
		}

		public NetworkStatusEnum Get(string Url, string Data, Action<NetworkStatusEnum, Object> OnResponse)
		{
			HTTPRequest Request = ObjectPool.Instance.GetObjectInstance<HTTPRequest>();
			if (null == Request)
			{
				//从对象池获取对象失败,异常
				return NetworkStatusEnum.ERROR;
			}

			Request.Url = Url;
			Request.Method = "GET";
			Request.OnResp = ActionResponse;
			Request.OnCallback = OnResponse;
			WaitQueue.Enqueue(Request);
			if (Work == NetworkWorkEnum.AUTO)
			{
				if (!IsWorking)
				{
					BeginWork();
				}
			}
			return NetworkStatusEnum.SUCCESS;
		} 

		private HTTPRequest CurrentRequest;
		private void BeginWork()
		{
			IsWorking = true;
			if (WaitQueue.Count > 0)
			{
				CurrentRequest = WaitQueue.Dequeue();
				CurrentRequest.Request();
			}
			else
			{
				IsWorking = false;
			}
		}

		/**
		 * 响应
		 **/
		private void ActionResponse(HttpStatusCode Status, HTTPRequest Request)
		{
			Action<NetworkStatusEnum, Object> Handler = Request.OnCallback;
			if (Status == HttpStatusCode.OK)
			{
				if (null != Handler)
				{
					Handler(NetworkStatusEnum.SUCCESS, CurrentRequest.ResponseData);
				}
			}
			else
			{
				if (null != Handler)
				{
					//异常
					Handler(NetworkStatusEnum.ERROR, CurrentRequest.ResponseData);
				}
			}
			CurrentRequest = null;
			ObjectPool.Instance.ReturnObject<HTTPRequest>(Request);
			BeginWork();
		}

		private string heartbeatUrl = "";
		private bool isHeartbeatActive = false;
		/// <summary>
		/// 开启心跳
		/// </summary>
		/// <param name="delay">Delay.</param>
		public void ActvieHeartbaet(int delay,string url)
		{
			if (!isHeartbeatActive) 
			{
				heartbeatUrl = url;
				TimerManager.Instance.AddTimer(delay,(int count)=>{
					HTTPRequest Request = ObjectPool.Instance.GetObjectInstance<HTTPRequest>();
					if (null == Request)
					{
						//从对象池获取对象失败,异常
						return NetworkStatusEnum.ERROR;
					}
					
					Request.Url = heartbeatUrl;
					Request.Method = "GET";
					Request.OnResp = ActionResponse;
					Request.OnCallback = ()=>{


					};
				},-1);
			}
		}

		public void Dispose()
		{
		}
	}
}
