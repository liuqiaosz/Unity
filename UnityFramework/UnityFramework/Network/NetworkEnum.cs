using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityFramework.Network
{
	public enum NetworkStatusEnum
	{
		SUCCESS,
		ERROR,
	}

	public enum NetworkWorkEnum
	{
		AUTO,//自动工作模式
		MANUAL,//被动模式，需要激活处理
	}
}
