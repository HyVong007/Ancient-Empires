using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using AncientEmpires.Terrains;
using AncientEmpires.Units;


namespace AncientEmpires
{
	[Serializable]
	public sealed class GameData
	{
		public static GameData dataToLoad;

		[NonSerialized] public Config config;

		public struct LoadingCommand
		{
			public bool isPause;
		}
		public LoadingCommand command;

		[NonSerialized] public Battle.Data battle;
		public IReadOnlyList<Army.Data> armyDatas;
		public Tombstone.Data?[][] tombstones;
		public AETerrain.Data[][] terrains;
		public Unit.Data[][] units;


		// ====================  Util API  ======================================


		public static byte[] Serialize(object obj)
		{
			var saveData = (GameData)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				// write battle
				byte[] data = Battle.Data.Serialize(saveData.battle);
				w.Write(data.Length);
				w.Write(data);

				// write armyDatas
				w.Write(saveData.armyDatas.Count);
				foreach (var army in saveData.armyDatas)
				{
					data = Army.Data.Serialize(army);
					w.Write(data.Length);
					w.Write(data);
				}

				// write tombstone, terrain and unit
				var size = new Vector2Int(saveData.terrains.Length, saveData.terrains[0].Length);
				w.Write(size.x);
				w.Write(size.y);
				for (int x = 0; x < size.x; ++x)
					for (int y = 0; y < size.y; ++y)
					{
						// write tombstone
						var tombstone = saveData.tombstones[x][y];
						w.Write(tombstone != null);
						if (tombstone != null)
						{
							data = Tombstone.Data.Serialize(tombstone);
							w.Write(data.Length); w.Write(data);
						}

						// write terrain
						data = AETerrain.Data.Serialize(saveData.terrains[x][y]);
						w.Write(data.Length); w.Write(data);

						// write unit
						var unit = saveData.units[x][y];
						w.Write(unit != null);
						if (unit != null)
						{
							data = Unit.Data.Serialize(unit);
							w.Write(data.Length); w.Write(data);
						}
					}

				// write config
				data = Config.Serialize(saveData.config);
				w.Write(data.Length);
				w.Write(data);

				return m.ToArray();
			}
		}


		public static GameData DeSerialize(byte[] data)
		{
			var saveData = new GameData();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				// read battle
				saveData.battle = Battle.Data.DeSerialize(r.ReadBytes(r.ReadInt32()));

				// read armyDatas
				int count = r.ReadInt32();
				var armyDatas = new List<Army.Data>();
				for (int i = 0; i < count; ++i) armyDatas.Add(Army.Data.DeSerialize(r.ReadBytes(r.ReadInt32())));
				saveData.armyDatas = armyDatas;

				// read tombstone, terrain and unit
				var size = new Vector2Int(r.ReadInt32(), r.ReadInt32());
				saveData.tombstones = new Tombstone.Data?[size.x][];
				saveData.terrains = new AETerrain.Data[size.x][];
				saveData.units = new Unit.Data[size.x][];
				for (int x = 0; x < size.x; ++x)
				{
					saveData.tombstones[x] = new Tombstone.Data?[size.y];
					saveData.terrains[x] = new AETerrain.Data[size.y];
					saveData.units[x] = new Unit.Data[size.y];
				}

				for (int x = 0; x < size.x; ++x)
					for (int y = 0; y < size.y; ++y)
					{
						// read tombstone
						bool hasTombstone = r.ReadBoolean();
						saveData.tombstones[x][y] = hasTombstone ? Tombstone.Data.DeSerialize(r.ReadBytes(r.ReadInt32())) as Tombstone.Data? : null;

						// read terain
						saveData.terrains[x][y] = AETerrain.Data.DeSerialize(r.ReadBytes(r.ReadInt32()));

						// read unit
						bool hasUnit = r.ReadBoolean();
						saveData.units[x][y] = hasUnit ? Unit.Data.DeSerialize(r.ReadBytes(r.ReadInt32())) : null;
					}

				// read config
				saveData.config = Config.DeSerialize(r.ReadBytes(r.ReadInt32()));

				return saveData;
			}
		}


		/// <summary>
		/// <para>Chỉ nên save khi game đang ở trạng thái rảnh rỗi (không chạy task)</para>
		/// <para>Và giữa thời gian OnTurnBegin ----- OnTurnComplete/OnTimeEnd</para>
		/// <para>Và lượt hiện tại là người chơi (Local).</para>
		/// </summary>
		/// <param name="saveCurrentGame"></param>
		public GameData(bool saveCurrentGame = false)
		{
			if (!saveCurrentGame) return;
			config = Config.DeSerialize(Config.Serialize(Config.instance));
			battle = new Battle.Data(Battle.instance);
			var armyDatas = new List<Army.Data>();
			foreach (var army in Army.dict.Values) if (army) armyDatas.Add(new Army.Data(army));
			this.armyDatas = armyDatas;

			var size = new Vector2Int(AETerrain.array.Length, AETerrain.array[0].Length);
			tombstones = new Tombstone.Data?[size.x][];
			terrains = new AETerrain.Data[size.x][];
			units = new Unit.Data[size.x][];
			for (int x = 0; x < size.x; ++x)
			{
				tombstones[x] = new Tombstone.Data?[size.y];
				terrains[x] = new AETerrain.Data[size.y];
				units[x] = new Unit.Data[size.y];
			}
			for (int x = 0; x < size.x; ++x)
				for (int y = 0; y < size.y; ++y)
				{
					tombstones[x][y] = Tombstone.array[x][y] ? new Tombstone.Data(Tombstone.array[x][y]) as Tombstone.Data? : null;
					terrains[x][y] = AETerrain.Data.Save(AETerrain.array[x][y]);
					units[x][y] = Unit.array[x][y] ? Unit.Data.Save(Unit.array[x][y]) : null;
				}
		}


		public async Task Load()
		{
			UnityEngine.Object.Destroy(Config.instance.map.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(config.map);
			Config.instance = config;
			dataToLoad = this;
			SceneManager.LoadScene(R.SceneBuildIndex.BATTLE);
			while (dataToLoad != null) await Task.Delay(1);
			onLoaded?.Invoke();
			while (!R.input.isAllTaskCompleted) await Task.Delay(1);
			Battle.instance.player.Report(TurnEvent.LOADED);
			R.input.lockInput = Battle.instance.army.type != Army.Type.LOCAL;
		}


		public static event System.Action onLoaded;
	}



	public interface ILoadable
	{
		object Load(bool use = true);
	}
}
