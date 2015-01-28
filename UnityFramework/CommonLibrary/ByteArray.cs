using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace CommonLibrary
{
    /**
     * 字节数组读取封装类
     * 
     **/
    public class ByteArray
    {
        private byte[] Source;

        /**
         * 当前读取位置
         **/
        public int Position
        {
            set;
            get;
        }

        /**
         * 剩余字节数
         **/
        public int ByteAvailable
        {
            get
            {
                if (null != Source)
                {
                    return Source.Length - Position;
                }
                return 0;
            }
        }

        public ByteArray(byte[] Value = null)
        {
            Source = Value;
        }

		public BinaryWriter Writer
		{
			private set;
			get;
		}

		/**
		 * 写模式
		 **/
		public ByteArray()
		{
			Writer = new BinaryWriter(new MemoryStream());
		}

		public byte[] GetBuffer()
		{
			byte[] Buffer = null;
			if (null != Writer && Writer.BaseStream.Length > 0)
			{
				//Buffer = new byte[Writer.BaseStream.Length];
				Buffer = ((MemoryStream)Writer.BaseStream).ToArray();
				//Writer.BaseStream.Read(Buffer, 0, (int)Writer.BaseStream.Length);
			}
			return Buffer;
		}

        public byte ReadByte()
        {
            byte Result = Source[Position];
            Position += 1;
            return Result;
        }
        public uint ReadUnsignedInt()
        {
            //Position += (Position > 0 ? 1 : 0);
            uint Result = BitConverter.ToUInt32(Source, Position);
            Position += 4;
            return Result;
        }

        public ulong ReadUnsignedLong()
        {
            ulong Result = BitConverter.ToUInt64(Source, Position);
            Position += 8;
            return Result;
        }

        public ushort ReadUnsignedShort()
        {
            ushort Result = BitConverter.ToUInt16(Source, Position);
            Position += 2;
            return Result;
        }

        public int ReadInt()
        {
            int Result = BitConverter.ToInt32(Source, Position);
            Position += 4;
            return Result;
        }
        public int ReadShort()
        {
            int Result = BitConverter.ToInt16(Source, Position);
            Position += 2;
            return Result;
        }
        public long ReadLong()
        {
            long Result = BitConverter.ToInt64(Source, Position);
            Position += 8;
            return Result;
        }

        public string ReadUTFString()
        {
            string Result = "";
            int Len = ReadUnsignedShort();
            Result = Encoding.UTF8.GetString(Source, Position, Len);
            //Result = BitConverter.ToString(Source, Position, Len);
            Position += Len;
            return Result;
        }

        /**
         * 读取固定长度的字符串
         **/
        public string ReadUTFString(int Length)
        {
            string Result = "";
            Result = Encoding.UTF8.GetString(Source, Position, Length);
            Position += Length;
            return Result;
        }

		public void Write(int Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}
		public void Write(uint Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}
		public void Write(string Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}
		public void Write(long Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}
		public void Write(byte Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}

		public void Write(float Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}

		public void Write(double Value)
		{
			if (null != Writer)
			{
				Writer.Write(Value);
			}
		}

		public void Write(byte[] Value)
		{
			Writer.Write(Value);
		}

		public void Write(byte[] Value,int Start,int Length)
		{
			Writer.Write(Value, Start, Length);
		}
    }
}
