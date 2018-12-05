using UnityEngine;
using AncientEmpires.Units;
using System.IO;
using System;


namespace AncientEmpires.Terrains
{
	[RequireComponent(typeof(SpriteRenderer))]
	public abstract class AETerrain : MonoBehaviour, IUsable, IClickHandler
	{
		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public int increaseDefend { get; protected set; }

		public int decreaseMove { get; protected set; }

		public static AETerrain[][] array { get; private set; }

		[SerializeField] private SpriteRenderer _spriteRenderer;


		static AETerrain()
		{
			Battle.onReset += () =>
			  {
				  var map = Config.instance.map;
				  var size = new Vector2Int(map.terrainArray.Length, map.terrainArray[0].Length);
				  array = new AETerrain[size.x][];
				  for (int x = 0; x < size.x; ++x) array[x] = new AETerrain[size.y];
			  };
		}


		public static AETerrain DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			AETerrain terrain = null;
			if (ID == 3 || ID == 5 || ID == 8 || ID == 14 || ID == 21 || ID == 29)
				terrain = House.DeSerialize(ID, wPos, use);
			else if (15 <= ID && ID <= 19)
				terrain = Castle.DeSerialize(ID, wPos, use);
			else terrain = NormalTerrain.DeSerialize(ID, wPos, use);

			return terrain;
		}


		public virtual void Use()
		{
			var pos = transform.position.WorldToArray();
			array[pos.x][pos.y] = this;
		}


		public virtual void OnClick(Vector3Int index)
		{

		}


		public override string ToString() =>
			$"aPos= {transform.position.WorldToArray()}, increaseDefend= {increaseDefend}, decreaseMove= {decreaseMove}\n";


		// ========================  DATA  ======================


		[Serializable]
		public abstract class Data : ILoadable
		{
			protected enum Type
			{
				NORMAL_TERRAIN, HOUSE, CASTLE
			}
			protected abstract Type type { get; }

			public Vector3Int pos;
			public int increaseDefend, decreaseMove;


			protected static byte[] mSerialize(object obj)
			{
				var terrain = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					w.Write((byte)terrain.type);
					w.Write(terrain.pos.x); w.Write(terrain.pos.y);
					w.Write(terrain.increaseDefend);
					w.Write(terrain.decreaseMove);

					return m.ToArray();
				}
			}


			protected static Data mDeSerialize(byte[] data)
			{
				Data terrain = null;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					var type = (Type)r.ReadByte();
					switch (type)
					{
						case Type.CASTLE: terrain = new Castle.Data(); break;
						case Type.HOUSE: terrain = new House.Data(); break;
						case Type.NORMAL_TERRAIN: terrain = new NormalTerrain.Data(); break;
					}

					terrain.pos = new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0);
					terrain.increaseDefend = r.ReadInt32();
					terrain.decreaseMove = r.ReadInt32();

					return terrain;
				}
			}


			public static byte[] Serialize(object obj)
			{
				if (obj is NormalTerrain.Data) return NormalTerrain.Data.Serialize(obj);
				else if (obj is House.Data) return House.Data.Serialize(obj);
				else if (obj is Castle.Data) return Castle.Data.Serialize(obj);
				else throw new Exception();
			}


			public static Data DeSerialize(byte[] data)
			{
				Type type;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					type = (Type)r.ReadByte();
				}

				switch (type)
				{
					case Type.CASTLE: return Castle.Data.DeSerialize(data);
					case Type.HOUSE: return House.Data.DeSerialize(data);
					case Type.NORMAL_TERRAIN: return NormalTerrain.Data.DeSerialize(data);

					default: throw new Exception();
				}
			}


			protected Data(AETerrain terrain)
			{
				pos = terrain.transform.position.WorldToArray();
				increaseDefend = terrain.increaseDefend;
				decreaseMove = terrain.decreaseMove;
			}


			protected Data() { }


			public static Data Save(AETerrain terrain)
			{
				if (terrain is NormalTerrain) return new NormalTerrain.Data(terrain as NormalTerrain);
				else if (terrain is House) return new House.Data(terrain as House);
				else if (terrain is Castle) return new Castle.Data(terrain as Castle);

				throw new Exception();
			}


			public abstract object Load(bool use = true);


			protected void Set(AETerrain terrain)
			{
				terrain.transform.position = pos.ArrayToWorld();
				terrain.increaseDefend = increaseDefend;
				terrain.decreaseMove = decreaseMove;
			}
		}
	}



	public interface IOccupyable
	{
		Army army { get; }

		void Occupy<U>(U unit) where U : Unit, IOccupier;
	}
}
