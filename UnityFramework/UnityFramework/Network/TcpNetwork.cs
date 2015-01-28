//------------------------------------------------------------------------------
// Socket执行进程
//------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
using UnityFramework.Misc;
using System.Text;
using CommonLibrary;

namespace UnityFramework.Network
{
	public class TcpNetwork : Singlton<TcpNetwork>
	{
        public const int BUFFER_SIZE = 4096;
		private Socket NetConnect;

		public string Addr
		{
			set;get;
		}
		public int Port
		{
			set;get;
		}

		public bool IsConnected
		{
			get;set;
		}

		//连接成功回调
		public Action<TcpNetwork> OnConnectedHandler
		{
			set;get;
		}

		//连接异常回调
		public Action<TcpNetwork> OnErrorConnectHandler
		{
			set;get;
		}

		//数据接收回调
		public Action<NetData> OnDataReciveHandler
		{
			set;get;
		}

		public string ErrorMessage
		{
			set;get;
		}
		
        private byte[] ReciveBuffer = new byte[BUFFER_SIZE];
        private IDataParser Parser;
		public TcpNetwork()
		{
			IsConnected = false;
		}

		public void Connect(string Addr, int Port,IDataParser Parser)
		{
			this.Addr = Addr;
			this.Port = Port;
			this.Parser = Parser;
			if(!IsConnected)
			{
				NetConnect = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
				IPAddress NetAddr = IPAddress.Parse(Addr);
				IPEndPoint NetPoint = new IPEndPoint(NetAddr,Port);

				NetConnect.BeginConnect(NetPoint,new AsyncCallback(OnConnectResult),NetConnect);
			}
		}

		/**
		 * 连接服务器返回
		 **/
		private void OnConnectResult(IAsyncResult Result)
		{
			Socket Connector = Result.AsyncState as Socket;
			try
			{
                Connector.EndConnect(Result);
				IsConnected = true;
				//连接成功
				if(null != OnConnectedHandler)
				{
					OnConnectedHandler(this);
				}

                BeginRecive();
                
                //Connector.BeginReceiveFrom(
			}
			catch(Exception Ex)
			{
				ErrorMessage = Ex.Message;
				IsConnected = false;
				//连接异常
				if(null != OnErrorConnectHandler)
				{
					OnErrorConnectHandler(this);
				}
			}
		}

        /**
         * 读取数据
         **/
        private void BeginRecive()
        {
            if (IsConnected)
            {
                NetConnect.BeginReceive(ReciveBuffer, 0, ReciveBuffer.Length, SocketFlags.None,new AsyncCallback(OnReciveProgress), NetConnect);
            }
        }

        private void OnReciveProgress(IAsyncResult Result)
        {
            Socket Connector = Result.AsyncState as Socket;

            int ByteAvailable = Connector.EndReceive(Result);

            byte[] Buffer = new byte[ByteAvailable];
            Array.Copy(ReciveBuffer, Buffer, ByteAvailable);
            if (null != Parser)
            {
                Parser.OnRecive(Buffer);
            }
            //ReciveBuffer = new byte[BUFFER_SIZE];
            BeginRecive();
        }

		/**
		 * 发送数据
		 **/
		public void Send(byte[] Data)
		{
			if (IsConnected)
			{
			}
		}

		public void Send(string Data)
		{
 
		}
		
		/**
		 * 关闭网络连接
		 **/
		public void ShutdownConnect()
		{
			if(IsConnected)
			{
				try
				{
					NetConnect.Close();
				}
				catch(Exception Ex)
				{
					ErrorMessage = Ex.Message;
				}
				IsConnected = false;
			}
		}

		public void Dispose()
		{
			ShutdownConnect();
			NetConnect = null;
		}
	}
}

