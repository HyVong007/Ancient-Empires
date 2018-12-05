using System;
using System.IO;
using System.Collections.Generic;
using Photon.Pun;
using ExitGames.Client.Photon;


namespace AncientEmpires.GamePlay.Online
{
	[Serializable]
	public sealed class OnlineConfig
	{
		public Dictionary<int, Army.Color> players;


		public static byte[] Serialize(object obj)
		{
			var cfg = (OnlineConfig)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				w.Write(cfg.players.Count);
				foreach (var kvp in cfg.players)
				{
					w.Write(kvp.Key);
					w.Write((byte)kvp.Value);
				}

				return m.ToArray();
			}
		}


		public static OnlineConfig DeSerialize(byte[] data)
		{
			var cfg = new OnlineConfig();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				int count = r.ReadInt32();
				cfg.players = new Dictionary<int, Army.Color>(count);
				for (int i = 0; i < count; ++i)
				{
					int key = r.ReadInt32();
					var value = (Army.Color)r.ReadByte();
					cfg.players[key] = value;
				}

				return cfg;
			}
		}


		public bool IsLocalArmy(Army.Color color)
		{
			int id = PhotonNetwork.LocalPlayer.ActorNumber;
			if (id == -1) return true;
			return players.ContainsKey(id) && players[id] == color;
		}


		static OnlineConfig()
		{
			PhotonPeer.RegisterType(typeof(OnlineConfig), (byte)'o', Serialize, DeSerialize);
			PhotonPeer.RegisterType(typeof(Config), (byte)'c', Config.Serialize, Config.DeSerialize);
		}
	}
}
