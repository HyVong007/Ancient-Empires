using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;


namespace AncientEmpires.Terrains
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Tombstone : MonoBehaviour, IUsable
	{
		public const int LIFE_TIME = 2;

		public int turn { get; private set; }

		public static Tombstone[][] array { get; private set; }


		static Tombstone()
		{
			Battle.onReset += () =>
			  {
				  var a = Config.instance.map.terrainArray;
				  var size = new Vector2Int(a.Length, a[0].Length);
				  array = new Tombstone[size.x][];
				  for (int x = 0; x < size.x; ++x) array[x] = new Tombstone[size.y];

				  list.Clear();
			  };

			Battle.onTurnBegin += OnTurnBegin;
		}


		public static Tombstone NewOrUpdate(Vector3 wPos, int turn = 0, bool use = true)
		{
			if (turn == 0) turn = Battle.instance.turn + LIFE_TIME;
			var pos = wPos.WorldToArray();
			var tombstone = array[pos.x][pos.y];
			if (tombstone)
			{
				tombstone.turn = turn;
				return tombstone;
			}

			tombstone = Instantiate(R.asset.prefab.tombstone, wPos, Quaternion.identity);
			tombstone.turn = turn;
			if (use) tombstone.Use();
			return tombstone;
		}


		private bool isUsed;

		public void Use()
		{
			isUsed = true;
			var pos = transform.position.WorldToArray();
			array[pos.x][pos.y] = this;
			list.Add(this);
			transform.parent = Battle.instance.neutralTerrainAnchor;
		}


		public void Destroy()
		{
			if (isUsed)
			{
				var pos = transform.position.WorldToArray();
				array[pos.x][pos.y] = null;
				list.Remove(this);
			}

			Destroy(gameObject);
		}


		// ======================  TURN BASE EVENTS  ============================


		private static readonly List<Tombstone> list = new List<Tombstone>();

		private static void OnTurnBegin()
		{
			var tmp = new List<Tombstone>();
			int currentTurn = Battle.instance.turn;
			foreach (var tombstone in list)
				if (currentTurn >= tombstone.turn) tmp.Add(tombstone);

			foreach (var tombstone in tmp) tombstone.Destroy();
		}


		public override string ToString() => $"Tombstone: aPos= {transform.position.WorldToArray()}, turn= {turn}\n";


		// ==============  DATA  ==========================


		[Serializable]
		public struct Data : ILoadable
		{
			public Vector3Int pos;
			public int turn;


			public static byte[] Serialize(object obj)
			{
				var tombstone = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					w.Write(tombstone.pos.x); w.Write(tombstone.pos.y);
					w.Write(tombstone.turn);

					return m.ToArray();
				}
			}


			public static Data DeSerialize(byte[] data)
			{
				var tombstone = new Data();
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					tombstone.pos = new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0);
					tombstone.turn = r.ReadInt32();

					return tombstone;
				}
			}


			public Data(Tombstone tombstone)
			{
				pos = tombstone.transform.position.WorldToArray();
				turn = tombstone.turn;
			}


			public object Load(bool use = true)
			{
				var tombstone = Instantiate(R.asset.prefab.tombstone, pos.ArrayToWorld(), Quaternion.identity);
				tombstone.turn = turn;

				if (use) tombstone.Use();
				return tombstone;
			}
		}
	}
}
