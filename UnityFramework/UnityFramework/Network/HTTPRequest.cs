using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.IO;
using CommonLibrary;
using UnityFramework.Misc.Pool;

namespace UnityFramework.Network
{
	public class HTTPRequest : IPoolable
	{
		public string Url
		{ set; get; }
		public string Method
		{ set; get; }
		public string Data
		{ set; get; }
		public Action<HttpStatusCode, HTTPRequest> OnResp
		{ set; get; }
		private HttpWebRequest Http;
		private ByteArray Buffer;
		private byte[] ByteBuffer;

		public Action<NetworkStatusEnum, Object> OnCallback
		{ set; get; }
		public HTTPRequest()
		{
		}

		public HTTPRequest(string Url, string Method, string Data, Action<HttpStatusCode,HTTPRequest> OnResp)
		{
			this.Url = Url;
			this.Method = Method;
			this.Data = Data;
			this.OnResp = OnResp;
			
		}

		public void Request()
		{
			if (!string.IsNullOrEmpty(Url))
			{
				Http = (HttpWebRequest)WebRequest.Create(Url);
                Http.KeepAlive = false;
                Http.Timeout = 10000;
				if(string.IsNullOrEmpty(Method))
				{
					Method = "POST";
				}
				Buffer = new ByteArray(null);
				Http.Method = Method;
				Http.BeginGetResponse(new AsyncCallback(GetResponse), Http);
			}
		}

		private void GetResponse(IAsyncResult Ar)
		{
			HttpWebRequest Request = Ar.AsyncState as HttpWebRequest;
			if (null != Request)
			{
                HttpWebResponse Response = null;
                try
                {
                    Response = (HttpWebResponse)Request.EndGetResponse(Ar);
                    if (Response.StatusCode != HttpStatusCode.OK)
                    {
                        Response.Close();
                        if (null != OnResp)
                        {
                            ResponseData = Response.StatusDescription;
                            OnResp(Response.StatusCode, this);
                        }
                    }
                    else
                    {
                        Stream Reader = Response.GetResponseStream();
                        //int Size = Reader.EndRead(Ar);
                        ByteBuffer = new byte[4096];
                        Reader.BeginRead(ByteBuffer, 0, ByteBuffer.Length, new AsyncCallback(ReponseReadCallback), Reader);
                    }
                }
                catch (WebException ex)
                {
                    if (null != Response)
                    {
                        Response.Close();
                    }
                    if (null != OnResp)
                    {
                        ResponseData = ex.Message ;
                        OnResp(HttpStatusCode.RequestTimeout, this);
                    }
                }
				
			}
		}

		private void ReponseReadCallback(IAsyncResult Ar)
		{
			Stream Reader = Ar.AsyncState as Stream;
			int Size = Reader.EndRead(Ar);
			if (Size > 0)
			{
				//继续读取
				Buffer.Write(ByteBuffer, 0, Size);
				Reader.BeginRead(ByteBuffer, 0, ByteBuffer.Length, new AsyncCallback(ReponseReadCallback), Reader);
			}
			else
			{
				Reader.Close();
				//读取结束
				ResponseData = Buffer.GetBuffer();
				
				if (null != OnResp)
				{
					OnResp(HttpStatusCode.OK, this);
				}
			}
		}

		public Object ResponseData
		{
			set;
			get;
		}

		public void Reset()
		{
			Url = null;
			Data = null;
			Method = null;
			OnResp = null;
			Http = null;
			Buffer = null;
			ByteBuffer = null;
			OnCallback = null;
		}

		public void Dispose()
		{
			Reset();
		}
	}
}
