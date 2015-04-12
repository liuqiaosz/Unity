using System;
using System.Collections.Generic;

using System.Text;

namespace UnityFramework.Network
{
	/**
	 * 数据编码基类，实现了数据编码接口的所有函数,但是只是空实现，具体实现逻辑需要各协议解析模块实现
	 **/
	public class ParserGeneric : IDataParser
	{
		public virtual void OnRecive(byte[] Buffer){ }
		public virtual void OnRecive(string Data) { }
	}
}
