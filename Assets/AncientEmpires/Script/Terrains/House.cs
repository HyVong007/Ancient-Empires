using AncientEmpires.Units;
using System.IO;
using UnityEngine;


namespace AncientEmpires.Terrains
{
	public class House : AETerrain, IOccupyable
	{
		public Army army { get; private set; }

		public State state { get; private set; }

		public enum State
		{
			OWNED, FREE, BROKEN
		}

		[SerializeField] private Sprite free, broken;
		[SerializeField] private ArmyColor_Sprite sprites;
		[SerializeField] private GameObject smoke;


		public static new House DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			var house = Instantiate(R.asset.prefab.house, wPos, Quaternion.identity);
			switch (ID)
			{
				case 3:
					house.army = Army.dict[Army.Color.BLACK];
					house.state = house.army ? State.OWNED : State.FREE;
					break;

				case 5:
					house.army = Army.dict[Army.Color.BLUE];
					house.state = house.army ? State.OWNED : State.FREE;
					break;

				case 8:
					house.army = null;
					house.state = State.BROKEN;
					break;

				case 14:
					house.army = null;
					house.state = State.FREE;
					break;

				case 21:
					house.army = Army.dict[Army.Color.GREEN];
					house.state = house.army ? State.OWNED : State.FREE;
					break;

				case 29:
					house.army = Army.dict[Army.Color.RED];
					house.state = house.army ? State.OWNED : State.FREE;
					break;
			}
			house.increaseDefend = 15;
			house.decreaseMove = 0;

			if (use) house.Use();
			return house;
		}


		private void Start()
		{
			switch (state)
			{
				case State.OWNED:
					spriteRenderer.sprite = sprites[army.color];
					smoke.SetActive(true);
					break;

				case State.FREE: spriteRenderer.sprite = free; break;
				case State.BROKEN: spriteRenderer.sprite = broken; break;
			}
		}


		public override void Use()
		{
			base.Use();
			transform.parent = army ? army.buildingAnchor : Battle.instance.neutralTerrainAnchor;
			army?.buildings.Add(this);
		}


		// ======================================================================


		public void Occupy<U>(U unit) where U : Unit, IOccupier
		{
			switch (state)
			{
				case State.BROKEN:
					state = State.FREE;
					spriteRenderer.sprite = free;
					return;

				case State.FREE:
					state = State.OWNED;
					smoke.SetActive(true);
					break;

				case State.OWNED:
					army.buildings.Remove(this);
					break;
			}

			army = unit.army;
			army.buildings.Add(this);
			transform.parent = army.buildingAnchor;
			spriteRenderer.sprite = sprites[army.color];
		}


		public bool CanBreak()
		{
			var pos = transform.position.WorldToArray();
			var unit = Unit.array[pos.x][pos.y];
			switch (state)
			{
				case State.BROKEN: return false;
				case State.FREE:
					return !unit || unit.army.group == Battle.instance.army.group;

				case State.OWNED:
					return army.group != Battle.instance.army.group;
			}

			return false;
		}


		public void Break()
		{
			if (state == State.OWNED)
			{
				army.buildings.Remove(this);
				army = null;
				transform.parent = Battle.instance.neutralTerrainAnchor;
				smoke.SetActive(false);
			}

			state = State.BROKEN;
			spriteRenderer.sprite = broken;
		}


		public override string ToString() =>
			$"House: army?.color= {army?.color}, state= {state}, " + base.ToString();


		// ===============  DATA  =====================


		public new class Data : AETerrain.Data
		{
			protected override Type type => Type.HOUSE;
			public Army.Color? armyColor;
			public State state;


			public static new byte[] Serialize(object obj)
			{
				var terrain = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					byte[] data = mSerialize(terrain);
					w.Write(data.Length);
					w.Write(data);

					w.Write(terrain.armyColor != null);
					if (terrain.armyColor != null) w.Write((byte)terrain.armyColor.Value);

					w.Write((byte)terrain.state);

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
					bool hasArmy = r.ReadBoolean();
					terrain.armyColor = hasArmy ? (Army.Color?)r.ReadByte() : null;
					terrain.state = (State)r.ReadByte();

					return terrain;
				}
			}


			public Data(House house) : base(house)
			{
				armyColor = house.army?.color;
				state = house.state;
			}


			public Data() { }


			public override object Load(bool use = true)
			{
				var t = Instantiate(R.asset.prefab.house);
				Set(t);
				t.army = armyColor != null ? Army.dict[armyColor.Value] : null;
				t.state = state;

				if (use) t.Use();
				return t;
			}
		}
	}
}
