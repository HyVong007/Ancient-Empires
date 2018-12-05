using System;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace AncientEmpires
{
	public class Map : MonoBehaviour
	{
		public Tilemap unitMap, terrainMap;
		public byte[][] unitArray, terrainArray;
		public Army.Color[] armyColors;


		public static byte[] Serialize(object obj)
		{
			var map = (Map)obj;
			bool hasMap = map.unitMap;
			if (hasMap)
			{
				map.unitMap.CompressBounds();
				map.terrainMap.CompressBounds();
				map.unitMap.origin = map.terrainMap.origin;
			}

			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				var size = hasMap ? map.terrainMap.size : new Vector3Int(map.terrainArray.Length, map.terrainArray[0].Length, 0);
				int x, y;
				w.Write(size.x);
				w.Write(size.y);

				if (hasMap)
				{
					var origin = map.terrainMap.origin;
					var index = new Vector3Int();
					for (x = 0, index.x = origin.x; x < size.x; ++x, ++index.x)
						for (y = 0, index.y = origin.y; y < size.y; ++y, ++index.y)
						{
							var uTile = map.unitMap.GetTile(index);
							var tTile = map.terrainMap.GetTile(index);
							w.Write(uTile ? Convert.ToByte(uTile.name) : (byte)0);
							w.Write(Convert.ToByte(tTile.name));
						}
				}
				else for (x = 0; x < size.x; ++x)
						for (y = 0; y < size.y; ++y)
						{
							w.Write(map.unitArray[x][y]);
							w.Write(map.terrainArray[x][y]);
						}

				// write colors;
				w.Write(map.armyColors.Length);
				foreach (var color in map.armyColors) w.Write((byte)color);
				return m.ToArray();
			}
		}


		public static Map DeSerialize(byte[] data)
		{
			var map = Instantiate(R.asset.maps.emptyMap);
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				var size = new Vector2Int(r.ReadInt32(), r.ReadInt32());
				map.unitArray = new byte[size.x][];
				map.terrainArray = new byte[size.x][];

				for (int x = 0; x < size.x; ++x)
				{
					map.unitArray[x] = new byte[size.y];
					map.terrainArray[x] = new byte[size.y];

					for (int y = 0; y < size.y; ++y)
					{
						map.unitArray[x][y] = r.ReadByte();
						map.terrainArray[x][y] = r.ReadByte();
					}
				}

				// Read colors
				map.armyColors = new Army.Color[r.ReadInt32()];
				for (int i = 0; i < map.armyColors.Length; ++i) map.armyColors[i] = (Army.Color)r.ReadByte();
				return map;
			}
		}
	}
}
