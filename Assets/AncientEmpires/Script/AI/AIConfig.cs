using System;
using System.IO;


namespace AncientEmpires.AI
{
	[Serializable]
	public sealed class AIConfig
	{
		public Army.Color[] armyColors;


		public static byte[] Serialize(object obj)
		{
			var cfg = (AIConfig)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				w.Write(cfg.armyColors.Length);
				foreach (var color in cfg.armyColors) w.Write((byte)color);

				return m.ToArray();
			}
		}


		public static AIConfig DeSerialize(byte[] data)
		{
			var cfg = new AIConfig();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				cfg.armyColors = new Army.Color[r.ReadInt32()];
				for (int i = 0; i < cfg.armyColors.Length; ++i) cfg.armyColors[i] = (Army.Color)r.ReadByte();

				return cfg;
			}
		}
	}
}
