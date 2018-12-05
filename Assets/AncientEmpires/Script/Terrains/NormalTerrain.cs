using UnityEngine;
using System.IO;


namespace AncientEmpires.Terrains
{
	public class NormalTerrain : AETerrain
	{
		public new Name name { get; private set; } = Name.NORMAL;

		public int ID { get; private set; }

		[SerializeField] private RuntimeAnimatorController animWater;

		public enum Name
		{
			NORMAL, WATER, MOUNTAIN, SILVER_TENT, BARREL, PLANE
		}


		public static new NormalTerrain DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			var terrain = Instantiate(R.asset.prefab.normalTerrain, wPos, Quaternion.identity);
			terrain.ID = ID;
			if (ID == 0 || ID == 1 || (40 <= ID && ID <= 52))
			{
				terrain.name = Name.WATER;
				terrain.increaseDefend = 0;
				terrain.decreaseMove = 2;
			}
			else if (ID == 6 || ID == 7 || ID == 38)
			{
				// Bridge and land
				terrain.increaseDefend = 5;
				terrain.decreaseMove = 0;
			}
			else if (ID == 10)
			{
				terrain.name = Name.BARREL;
				terrain.increaseDefend = 15;
				terrain.decreaseMove = 0;
			}
			else if (ID == 11 || ID == 12 || ID == 22)
			{
				// Forest and high land
				terrain.increaseDefend = 10;
				terrain.decreaseMove = 1;
			}
			else if (ID == 23)
			{
				terrain.name = Name.MOUNTAIN;
				terrain.increaseDefend = 15;
				terrain.decreaseMove = 2;
			}
			else if (ID == 24 || ID == 25 || ID == 27)
			{
				// Plane arround
				terrain.increaseDefend = 10;
				terrain.decreaseMove = 0;
			}
			else if (ID == 26)
			{
				terrain.name = Name.PLANE;
				terrain.increaseDefend = 15;
				terrain.decreaseMove = 0;
			}
			else if (30 <= ID && ID <= 36)
			{
				// Road
				terrain.increaseDefend = 0;
				terrain.decreaseMove = 0;
			}
			else if (ID == 37)
			{
				terrain.name = Name.SILVER_TENT;
				terrain.increaseDefend = 15;
				terrain.decreaseMove = 0;
			}

			if (use) terrain.Use();
			return terrain;
		}


		public override void Use()
		{
			base.Use();
			transform.parent = Battle.instance.neutralTerrainAnchor;
		}


		private void Start()
		{
			spriteRenderer.sprite = R.asset.tiles.terrains[ID].sprite;
			if (ID == 0 || ID == 1) gameObject.AddComponent<Animator>().runtimeAnimatorController = animWater;
		}


		public override string ToString() =>
			$"Normal Terrain: name= {name}, ID= {ID}, " + base.ToString();


		// =====================  DATA  =====================


		public new class Data : AETerrain.Data
		{
			protected override Type type => Type.NORMAL_TERRAIN;
			public Name name;
			public int ID;


			public static new byte[] Serialize(object obj)
			{
				var terrain = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					byte[] data = mSerialize(terrain);
					w.Write(data.Length);
					w.Write(data);

					w.Write((byte)terrain.name);
					w.Write(terrain.ID);

					return m.ToArray();
				}
			}


			public static new Data DeSerialize(byte[] data)
			{
				Data terrain = null;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					terrain = (Data)mDeSerialize(r.ReadBytes(r.ReadInt32()));
					terrain.name = (Name)r.ReadByte();
					terrain.ID = r.ReadInt32();

					return terrain;
				}
			}


			public Data(NormalTerrain terrain) : base(terrain)
			{
				name = terrain.name;
				ID = terrain.ID;
			}


			public Data() { }


			public override object Load(bool use = true)
			{
				var t = Instantiate(R.asset.prefab.normalTerrain);
				Set(t);
				t.name = name;
				t.ID = ID;

				if (use) t.Use();
				return t;
			}
		}
	}
}
