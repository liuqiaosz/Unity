//------------------------------------------------------------------------------
// 网络数据解析接口
//------------------------------------------------------------------------------
using System;
namespace UnityFramework.Network
{
	public interface IDataParser
	{
        void OnRecive(byte[] Data);
		void OnRecive(string Data);
	}
}

