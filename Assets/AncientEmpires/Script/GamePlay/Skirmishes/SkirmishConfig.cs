using System;
using System.IO;


namespace AncientEmpires.GamePlay.Skirmishes
{
	[Serializable]
	public sealed class SkirmishConfig
	{
		public static byte[] Serialize(object obj)
		{
			var cfg = (SkirmishConfig)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				return m.ToArray();
			}
		}


		public static SkirmishConfig DeSerialize(byte[] data)
		{
			var cfg = new SkirmishConfig();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				return cfg;
			}
		}
	}
}
