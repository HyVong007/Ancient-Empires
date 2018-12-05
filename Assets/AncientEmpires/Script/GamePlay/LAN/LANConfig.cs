using System;
using System.IO;


namespace AncientEmpires.GamePlay.LAN
{
	[Serializable]
	public sealed class LANConfig
	{
		public static byte[] Serialize(object obj)
		{
			var cfg = (LANConfig)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				return m.ToArray();
			}
		}


		public static LANConfig DeSerialize(byte[] data)
		{
			var cfg = new LANConfig();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				return cfg;
			}
		}
	}
}
