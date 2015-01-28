using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLibrary;
namespace UnityFramework.Network
{
	public class TcpDataParserGeneric : ParserGeneric
    {
        override public void OnRecive(byte[] Buffer)
        {
            ByteArray Data = new ByteArray(Buffer);
            string v = Data.ReadUTFString();
            Console.WriteLine("收到消息：{0}", v);
        }
    }
}
