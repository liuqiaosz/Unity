using System;
using System.Collections.Generic;

using System.Text;

using System.IO;
namespace UnityFramework.Utils
{
	//LZMA压缩，解压缩工具
	public class LzmaTools
	{
		public static void CompressFileLZMA(string SrcFile, string CompressFile)
		{
			SevenZip.Compression.LZMA.Encoder Coder = new SevenZip.Compression.LZMA.Encoder();
			FileStream Input = new FileStream(SrcFile, FileMode.Open);
			FileStream Output = new FileStream(CompressFile, FileMode.Create);

			// Write the encoder properties
			Coder.WriteCoderProperties(Output);

			// Write the decompressed file size.
			Output.Write(BitConverter.GetBytes(Input.Length), 0, 8);

			// Encode the file.
			Coder.Code(Input, Output, Input.Length, -1, null);
			Output.Flush();
			Output.Close();
			Input.Close();
		}

		public static void DecompressFileLZMA(string inFile, string outFile)
		{
			SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
			FileStream input = new FileStream(inFile, FileMode.Open);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Read the decoder properties
			byte[] properties = new byte[5];
			input.Read(properties, 0, 5);

			// Read in the decompress file size.
			byte[] fileLengthBytes = new byte[8];
			input.Read(fileLengthBytes, 0, 8);
			long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

			// Decompress the file.
			coder.SetDecoderProperties(properties);
			coder.Code(input, output, input.Length, fileLength, null);
			output.Flush();
			output.Close();
			input.Close();
		}
	}
}
