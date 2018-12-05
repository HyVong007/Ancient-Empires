using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Util;
using System.IO;


namespace AncientEmpires.Units.Kings
{
	public abstract class King : Unit, IOccupier
	{
		public static readonly IReadOnlyList<Name> kingNames = new List<Name>()
		{
			Name.SAETH, Name.GALAMAR, Name.VALADORN, Name.DEMONLORD
		};

		public static readonly IReadOnlyDictionary<Army.Color, Name> colorKings = new Dictionary<Army.Color, Name>()
		{
			[Army.Color.BLACK] = Name.SAETH,
			[Army.Color.BLUE] = Name.GALAMAR,
			[Army.Color.GREEN] = Name.VALADORN,
			[Army.Color.RED] = Name.DEMONLORD
		};

		public static IReadOnlyDictionary<Army.Color, King> dict { get; private set; }


		static King()
		{
			Battle.onReset += () =>
			  {
				  var tmp = new Dictionary<Army.Color, King>();
				  foreach (Army.Color color in Enum.GetValues(typeof(Army.Color))) tmp[color] = null;
				  foreach (var info in Config.instance.armyInfos)
				  {
					  var king = (King)New(colorKings[info.color], null);
					  tmp[info.color] = king;
				  }

				  dict = tmp;
			  };

			Battle.onStart += () =>
			  {
				  foreach (var kvp in dict)
				  {
					  var king = kvp.Value;
					  if (!king) continue;

					  var army = Army.dict[kvp.Key];
					  king.army = army;
					  king.transform.parent = army.unitAnchor;
				  }
			  };
		}


		public static new King DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			var color = Army.Color.BLACK;
			switch (ID)
			{
				case 62:
					color = Army.Color.BLACK;
					break;

				case 74:
					color = Army.Color.BLUE;
					break;

				case 86:
					color = Army.Color.GREEN;
					break;

				case 98:
					color = Army.Color.RED;
					break;
			}

			var king = dict[color];
			if (king?.gameObject.activeSelf != false) return null;

			king.transform.position = wPos;
			king.army = Army.dict[color];
			king.gameObject.SetActive(true);
			if (use) king.Use();
			return king;
		}


		// ========================================================================


		protected void OnEnable()
		{
			if (GameData.dataToLoad == null)
			{
				health = 100f;
				Wisp.Heal(this, false);
				DireWolf.Poisoning(this, false);
			}
		}


		public override async Task<Tombstone> Die()
		{
			gameObject.SetActive(false);
			if (isUsed)
			{
				var pos = transform.position.WorldToArray();
				array[pos.x][pos.y] = null;
				army.units.Remove(this);
				R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, transform.position);
				R.pool.Show(ObjectPool.POOL_KEY.EXPLOSION_SMOKE, transform.position);
				await Task.Delay(600);
				R.pool.Hide(ObjectPool.POOL_KEY.BLUE_FIRE);
				R.pool.Hide(ObjectPool.POOL_KEY.EXPLOSION_SMOKE);

				R.onDie?.Invoke(this);
			}

			return null;
		}


		public bool CanOccupy(AETerrain terrain) =>
			terrain is IOccupyable && (terrain as IOccupyable).army?.group != army.group;


		public void Occupy()
		{
			var pos = transform.position.WorldToArray();
			(AETerrain.array[pos.x][pos.y] as IOccupyable).Occupy(this);
			isSleep = true;
		}


		// ======================  SAVED DATA  ===================================


		public new abstract class Data : Unit.Data
		{
			protected static new byte[] mSerialize(object obj)
			{
				var unit = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					byte[] data = Unit.Data.mSerialize(obj);
					w.Write(data.Length);
					w.Write(data);


					return m.ToArray();
				}
			}


			protected static new Data mDeSerialize(byte[] data)
			{
				Data unit = null;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					unit = (Data)Unit.Data.mDeSerialize(r.ReadBytes(r.ReadInt32()));

					return unit;
				}
			}


			public static new byte[] Serialize(object obj)
			{
				switch ((obj as King).name)
				{
					case Name.DEMONLORD: return DemonLord.Data.Serialize(obj);
					case Name.GALAMAR: return Galamar.Data.Serialize(obj);
					case Name.SAETH: return Saeth.Data.Serialize(obj);
					case Name.VALADORN: return Valadorn.Data.Serialize(obj);

					default: throw new Exception();
				}
			}


			public static new Data DeSerialize(byte[] data)
			{
				Name name;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					name = (Name)r.ReadByte();
				}

				switch (name)
				{
					case Name.DEMONLORD: return DemonLord.Data.DeSerialize(data);
					case Name.GALAMAR: return Galamar.Data.DeSerialize(data);
					case Name.SAETH: return Saeth.Data.DeSerialize(data);
					case Name.VALADORN: return Valadorn.Data.DeSerialize(data);

					default: throw new Exception();
				}
			}


			protected Data(King king) : base(king)
			{

			}


			protected Data() { }


			protected override void Set(Unit unit)
			{
				base.Set(unit);
			}


			public override object Load(bool use = true)
			{
				var king = dict[armyColor];
				if (king?.gameObject.activeSelf != false) return null;
				king.transform.position = pos.ArrayToWorld();
				king.army = Army.dict[armyColor];
				Set(king);
				king.gameObject.SetActive(true);
				if (use) king.Use();
				return king;
			}
		}
	}
}
