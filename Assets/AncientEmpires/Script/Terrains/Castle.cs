using AncientEmpires.Units;
using UnityEngine;
using System.Threading.Tasks;
using AncientEmpires.Units.Kings;
using System;
using System.Collections.Generic;
using System.IO;

namespace AncientEmpires.Terrains
{
	public class Castle : AETerrain, IOccupyable
	{
		public Army army { get; private set; }

		[SerializeField] private Sprite free;
		[SerializeField] private ArmyColor_Sprite sprites;


		public static new Castle DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			var castle = Instantiate(R.asset.prefab.castle, wPos, Quaternion.identity);
			switch (ID)
			{
				case 15: castle.army = Army.dict[Army.Color.BLACK]; break;
				case 16: castle.army = Army.dict[Army.Color.BLUE]; break;
				case 17: castle.army = Army.dict[Army.Color.GREEN]; break;
				case 18: castle.army = Army.dict[Army.Color.RED]; break;
				case 19: castle.army = null; break;
			}
			castle.increaseDefend = 15;
			castle.decreaseMove = 0;

			if (use) castle.Use();
			return castle;
		}


		private void Start() => spriteRenderer.sprite = army ? sprites[army.color] : free;


		public override void Use()
		{
			base.Use();
			transform.parent = army ? army.buildingAnchor : Battle.instance.neutralTerrainAnchor;
			army?.buildings.Add(this);
		}


		private static IReadOnlyList<Action> verifiedActions;

		static Castle()
		{
			Battle.onPlayerAction += (IReadOnlyList<Action> actions) => { verifiedActions = actions; };
		}


		// ========================================================================


		public void Occupy<U>(U unit) where U : Unit, IOccupier
		{
			army?.buildings.Remove(this);
			army = unit.army;
			army.buildings.Add(this);
			transform.parent = army.buildingAnchor;
			spriteRenderer.sprite = sprites[army.color];
		}


		public bool CanBuy(Unit.Name unitName)
		{
			if (!army) return false;
			var king = King.dict[army.color];
			if (King.kingNames.Contains(unitName))
			{
				if (unitName != king.name) throw new Exception("Cannot buy King of other army but the Castle's Army !");
				else if (king.gameObject.activeSelf) return false;
			}
			else if (unitName == Unit.Name.SKELETON || unitName == Unit.Name.CRYSTAL) return false;

			var unit = unitName == king.name ? king : R.asset.prefab.units[unitName];
			unit.transform.position = transform.position;
			var pos = transform.position.WorldToArray();
			return army.money >= unit.cost && (!Unit.array[pos.x][pos.y] || UnitAlgorithm.FindMove(unit, army).Count > 0);
		}


		public bool CanBuy()
		{
			if (!army) return false;
			if (CanBuy(King.dict[army.color].name)) return true;
			foreach (Unit.Name name in Enum.GetValues(typeof(Unit.Name)))
				if (name != Unit.Name.SKELETON && name != Unit.Name.CRYSTAL
					&& !King.kingNames.Contains(name) && CanBuy(name)) return true;

			return false;
		}


		public Unit Buy(Unit.Name unitName)
		{
			var pos = transform.position.WorldToArray();
			bool hasUnit = Unit.array[pos.x][pos.y];
			var king = King.dict[army.color];
			if (unitName == king.name)
			{
				army.units.Add(king);
				king.transform.position = transform.position;
				if (!hasUnit) Unit.array[pos.x][pos.y] = king;
				king.gameObject.SetActive(true);
				return king;
			}

			var unit = Unit.New(unitName, army, transform.position);
			unit.Use();
			army.money -= unit.cost;
			return unit;
		}


		public static bool isBuying { get; private set; }

		public async Task<Unit> Buy(List<Action> actions)
		{
			var unit = await R.shop.Buy(this);
			if (!unit) return null;

			var thisPos = transform.position.WorldToArray();
			R.info.UpdateUnitUI(thisPos);
			var prevUnit = Unit.array[thisPos.x][thisPos.y] == unit ? null : Unit.array[thisPos.x][thisPos.y];
			int? prevTombstoneTurn = Tombstone.array[thisPos.x][thisPos.y]?.turn;
			isBuying = true;

			// Local lambda
			System.Action cancelUnit = async () =>
			{
				army.money += unit.cost;
				(await unit.Die())?.Destroy();
				Unit.array[thisPos.x][thisPos.y] = prevUnit;
				if (prevTombstoneTurn != null) Tombstone.NewOrUpdate(transform.position, (int)prevTombstoneTurn);
				isBuying = false;
			};

			if (prevUnit) --prevUnit.spriteRenderer.sortingOrder;
			var buyAction = new BuyAction(thisPos, unit.name);
			while (true)
			{
				R.input.click = null;
				actions.Clear(); actions.Add(buyAction);
				var result = await unit.OnAction(actions);

				switch (result)
				{
					case Unit.ResultOnAction.SUCCESSFULED:
						isBuying = false;
						if (prevUnit) ++prevUnit.spriteRenderer.sortingOrder;
						return unit;

					case Unit.ResultOnAction.FAILED:
						cancelUnit();
						if (prevUnit) ++prevUnit.spriteRenderer.sortingOrder;
						return null;

					case Unit.ResultOnAction.CANCELED:
					case Unit.ResultOnAction.NOACTION:
						actions.RemoveAt(actions.Count - 1);
						if (result == Unit.ResultOnAction.NOACTION || (!prevUnit && R.input.click == thisPos))
						{
							if (await Play(actions, buyAction))
							{
								unit.isSleep = true;
								goto case Unit.ResultOnAction.SUCCESSFULED;
							}
							goto case Unit.ResultOnAction.FAILED;
						}
						break;
				}
			}
		}


		private static async Task<bool> Play(List<Action> actionList, Action thisAction)
		{
			actionList.Add(thisAction);
			verifiedActions = null;
			Battle.instance.player.Play(actionList);
			var token = Battle.endTurnCancel;
			while (verifiedActions == null && !token.IsCancellationRequested) await Task.Delay(1);
			return verifiedActions?.Contains(thisAction) == true;
		}


		public override async void OnClick(Vector3Int index)
		{
			if (army != Battle.instance.army) return;
			await R.input.Wait(R.camCtrl.AutoFocus(transform.position));

			var actions = new List<Action>();
			var unit = await R.input.Wait(Buy(actions));
			if (unit) R.SendEventAndReport(actions);
		}


		public override string ToString() =>
			$"Castle: army?.color= {army?.color}, isBuying= {isBuying}, " + base.ToString();


		// ===============  DATA  ================


		public new class Data : AETerrain.Data
		{
			protected override Type type => Type.CASTLE;
			public Army.Color? armyColor;


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

					return terrain;
				}
			}


			public Data(Castle castle) : base(castle)
			{
				armyColor = castle.army?.color;
			}


			public Data() { }


			public override object Load(bool use = true)
			{
				var t = Instantiate(R.asset.prefab.castle);
				Set(t);
				t.army = armyColor != null ? Army.dict[armyColor.Value] : null;

				if (use) t.Use();
				return t;
			}
		}
	}
}
