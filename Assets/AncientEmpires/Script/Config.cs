using System;
using System.IO;
using AncientEmpires.GamePlay.Campaigns;
using AncientEmpires.GamePlay.LAN;
using AncientEmpires.GamePlay.Online;
using AncientEmpires.GamePlay.Skirmishes;
using AncientEmpires.AI;


namespace AncientEmpires
{
	/// <summary>
	/// Cẩn thận field "map" ! DontDestroyOnLoad(map) !
	/// </summary>
	[Serializable]
	public sealed class Config
	{
		public static Config instance;

		public Map map;
		public int money;
		public float turnTime;

		[Serializable]
		public struct ArmyInfo
		{
			public Army.Color color;
			public Army.Group group;
			public string name;
		}
		public ArmyInfo[] armyInfos;

		public enum PlayMode
		{
			CAMPAIGN, SKIRMISH, ONLINE, LAN
		}
		public PlayMode playMode;
		public object playModeConfig;  // Skirmish, Online & LAN
		public AIConfig aiConfig;


		public static byte[] Serialize(object obj)
		{
			var cfg = (Config)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				// write map
				byte[] map_data = Map.Serialize(cfg.map);
				w.Write(map_data.Length);
				w.Write(map_data);

				// write money
				w.Write(cfg.money);

				// write turn time
				w.Write(cfg.turnTime);

				// write army infos
				w.Write(cfg.armyInfos.Length);
				foreach (var info in cfg.armyInfos)
				{
					w.Write((byte)info.color);
					//w.Write((byte)info.type);
					w.Write((byte)info.group);
					w.Write(info.name);
				}

				// write playMode
				w.Write((byte)cfg.playMode);

				// write playMode config
				switch (cfg.playMode)
				{
					case PlayMode.CAMPAIGN: break;

					case PlayMode.SKIRMISH:
						{
							byte[] data = SkirmishConfig.Serialize(cfg.playModeConfig);
							w.Write(data.Length);
							w.Write(data);
							break;
						}

					case PlayMode.ONLINE:
						{
							byte[] data = OnlineConfig.Serialize(cfg.playModeConfig);
							w.Write(data.Length);
							w.Write(data);
							break;
						}

					case PlayMode.LAN:
						{
							byte[] data = LANConfig.Serialize(cfg.playModeConfig);
							w.Write(data.Length);
							w.Write(data);
							break;
						}
				}

				// write AI config
				w.Write(cfg.aiConfig != null);
				if (cfg.aiConfig != null)
				{
					byte[] data = AIConfig.Serialize(cfg.aiConfig);
					w.Write(data.Length);
					w.Write(data);
				}

				return m.ToArray();
			}
		}


		public static Config DeSerialize(byte[] data)
		{
			var cfg = new Config();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				// read map
				cfg.map = Map.DeSerialize(r.ReadBytes(r.ReadInt32()));

				// read money
				cfg.money = r.ReadInt32();

				// read turn time
				cfg.turnTime = r.ReadSingle();

				// read army infos
				cfg.armyInfos = new ArmyInfo[r.ReadInt32()];
				for (int i = 0; i < cfg.armyInfos.Length; ++i)
					cfg.armyInfos[i] = new ArmyInfo()
					{
						color = (Army.Color)r.ReadByte(),
						group = (Army.Group)r.ReadByte(),
						name = r.ReadString()
					};

				// read playMode
				cfg.playMode = (PlayMode)r.ReadByte();

				// read playMode config
				switch (cfg.playMode)
				{
					case PlayMode.CAMPAIGN: break;

					case PlayMode.SKIRMISH:
						{
							cfg.playModeConfig = SkirmishConfig.DeSerialize(r.ReadBytes(r.ReadInt32()));
							break;
						}

					case PlayMode.ONLINE:
						{
							cfg.playModeConfig = OnlineConfig.DeSerialize(r.ReadBytes(r.ReadInt32()));
							break;
						}

					case PlayMode.LAN:
						{
							cfg.playModeConfig = LANConfig.DeSerialize(r.ReadBytes(r.ReadInt32()));
							break;
						}
				}

				// read AI config
				if (r.ReadBoolean()) cfg.aiConfig = AIConfig.DeSerialize(r.ReadBytes(r.ReadInt32()));

				return cfg;
			}
		}
	}
}
