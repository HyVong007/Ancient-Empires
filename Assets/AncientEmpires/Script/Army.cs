using UnityEngine;
using System.Collections.Generic;
using System;
using AncientEmpires.Terrains;
using AncientEmpires.Units;
using AncientEmpires.Units.Kings;
using System.Threading.Tasks;
using System.IO;
using AncientEmpires.GamePlay.Online;


namespace AncientEmpires
{
	public class Army : MonoBehaviour, ITurnbaseListener
	{
		public const float HEALING_HEALTH = 15f;
		public const int INCREASE_MONEY = 110;

		public Color color { get; private set; }

		public Group group { get; private set; }

		public Type type { get; private set; }

		public int money
		{
			get { return _money; }

			set
			{
				_money = value;
				if (!R.info) return;
				if (!hasRemoteArmy)
				{
					if (type == Type.AI) return;
				}
				else if (GetLocalArmies[0].group != group) return;
				R.info.armyUI.money.text = _money.ToString();
			}
		}
		private int _money;

		public static IReadOnlyDictionary<Color, Army> dict { get; private set; } = new Dictionary<Color, Army>();

		public readonly List<AETerrain> buildings = new List<AETerrain>();

		public readonly UnitList units = new UnitList();

		public static bool hasRemoteArmy
		{
			get
			{
				foreach (var army in dict.Values)
					if (army?.type == Type.REMOTE) return true;
				return false;
			}
		}


		public static IReadOnlyList<Army> GetLocalArmies
		{
			get
			{
				var result = new List<Army>();
				foreach (var army in dict.Values)
					if (army?.type == Type.LOCAL) result.Add(army);
				return result;
			}
		}


		public class UnitList : List<Unit>
		{
			public new void Add(Unit unit)
			{
				base.Add(unit);
				if (R.info) R.info.armyUI.unitCount.text = Count.ToString();
			}


			public new void Remove(Unit unit)
			{
				base.Remove(unit);
				if (R.info) R.info.armyUI.unitCount.text = Count.ToString();
			}


			public new void Clear()
			{
				base.Clear();
				if (R.info) R.info.armyUI.unitCount.text = "0";
			}
		}

		public Transform unitAnchor => _unitAnchor;

		public Transform buildingAnchor => _buildingAnchor;

		public Vector3 focusPoint;

		[SerializeField] private Transform _unitAnchor, _buildingAnchor;

		public enum Color
		{
			BLACK, BLUE, GREEN, RED
		}

		public enum Group
		{
			A, B, C, D
		}

		public enum Type
		{
			LOCAL, REMOTE, AI
		}


		static Army()
		{
			Battle.onReset += () =>
			  {
				  var cfg = Config.instance;

				  // check to load data
				  if (GameData.dataToLoad != null)
				  {
					  var tmp = new Dictionary<Color, Army>();
					  foreach (var data in GameData.dataToLoad.armyDatas)
					  {
						  var army = Instantiate(R.asset.prefab.army, Battle.instance.armyAnchor);
						  army.color = data.color;
						  army.group = data.group;
						  army.money = data.money;
						  army.focusPoint = data.focusPoint;
						  army.name = data.name;
						  tmp[army.color] = army;
					  }
					  foreach (Color color in Enum.GetValues(typeof(Color)))
						  if (!tmp.ContainsKey(color)) tmp[color] = null;

					  dict = tmp;
				  }
				  else
				  {
					  var infos = cfg.armyInfos;
					  int money = cfg.money;
					  var battle = Battle.instance;
					  var tmp = new Dictionary<Color, Army>();

					  foreach (var info in infos)
					  {
						  var army = Instantiate(R.asset.prefab.army, battle.armyAnchor);
						  army.color = info.color;
						  army.group = info.group;
						  army.name = info.name;
						  army.money = money;
						  tmp[army.color] = army;
					  }
					  foreach (Color color in Enum.GetValues(typeof(Color)))
						  if (!tmp.ContainsKey(color)) tmp[color] = null;

					  dict = tmp;
				  }

				  foreach (var army in dict.Values)
					  if (!army) continue;
					  else if (cfg.aiConfig != null && Array.Exists(cfg.aiConfig.armyColors, (Color color) => color == army.color)) army.type = Type.AI;
					  else switch (cfg.playMode)
						  {
							  case Config.PlayMode.CAMPAIGN:
							  case Config.PlayMode.SKIRMISH:
								  army.type = Type.LOCAL;
								  break;

							  case Config.PlayMode.ONLINE:
								  army.type = (cfg.playModeConfig as OnlineConfig).IsLocalArmy(army.color) ? Type.LOCAL : Type.REMOTE;
								  break;

							  case Config.PlayMode.LAN:
								  throw new NotImplementedException();
						  }
			  };
		}


		private void OnEnable()
		{
			Battle.onStart += _OnStart;
			GameData.onLoaded += AutoFocus;
		}

		private void OnDisable()
		{
			Battle.onStart -= _OnStart;
			GameData.onLoaded -= AutoFocus;
		}

		private void _OnStart()
		{
			if (GameData.dataToLoad != null)
			{
				var data = default(Data);
				foreach (var d in GameData.dataToLoad.armyDatas)
					if (d.color == color) { data = d; break; }

				focusPoint = data.focusPoint;
			}
			else
			{
				var king = King.dict[color];
				if (king.gameObject.activeSelf) focusPoint = king.transform.position;
				else if (units.Count > 0) focusPoint = units[0].transform.position;
				else
				{
					Castle castle = null;
					foreach (var building in buildings)
						if (building is Castle)
						{
							castle = building as Castle;
							break;
						}
					focusPoint = castle.transform.position;
				}
			}
		}


		private void AutoFocus()
		{
			if (this != Battle.instance.army) return;
			R.input.Wait(R.camCtrl.AutoFocus(R.pool.selectRect.transform.position = focusPoint));
		}


		public static IEnumerator<Army> NextArmyGenerator()
		{
			while (true) foreach (var army in dict.Values)
					if (army) yield return army;
		}


		// ========================== TURN BASE EVENTS ==========================


		public void OnTurnBegin()
		{
			foreach (var unit in units)
				if (unit.health < 100f && unit.isOnHealingTerrain) unit.health += HEALING_HEALTH;

			money += buildings.Count * INCREASE_MONEY;
			R.input.Wait(R.camCtrl.AutoFocus(R.pool.selectRect.transform.position = focusPoint));
		}


		public void OnTurnComplete()
		{
			foreach (var unit in units) if (unit.isSleep) unit.isSleep = false;
		}


		public void OnTimeEnd()
		{
			foreach (var unit in units) if (unit.isSleep) unit.isSleep = false;
		}


		public async void OnPlayerAction(IReadOnlyList<Action> actions)
		{
			if (this != Battle.instance.army) return; // Mới sửa chức năng Online !
			if (type != Type.LOCAL)
			{
				foreach (var action in actions)
				{
					var target = action.currentPos.ArrayToWorld();
					await Task.WhenAll(R.camCtrl.AutoFocus(target), R.Move(R.pool.selectRect, target, 0.1f));

					var unit = Unit.array[action.currentPos.x][action.currentPos.y];
					switch (action.name)
					{
						case Action.Name.OCCUPY:
							(unit as IOccupier).Occupy();
							break;

						case Action.Name.HEAL:
							await (unit as Wisp).Heal();
							break;

						case Action.Name.RAISE:
							var raiseAction = action as RaiseAction;
							await (unit as Sorceress).Raise(raiseAction.targetPos);
							break;

						case Action.Name.ATTACK:
							var attackAction = action as AttackAction;
							await unit.Attack(attackAction.targetPos, attackAction.enemyDeltaHealth, attackAction.thisDeltaHealth);
							break;

						case Action.Name.MOVE:
							var moveAction = action as MoveAction;
							await unit.Move(moveAction.targetPos);
							if (actions[actions.Count - 1] == moveAction) unit.isSleep = true;
							break;

						case Action.Name.BUY:
							var buyAction = action as BuyAction;
							var castle = AETerrain.array[buyAction.currentPos.x][buyAction.currentPos.y] as Castle;
							unit = castle.Buy(buyAction.unitName);
							if (actions.Count == 1) unit.isSleep = true;
							break;
					}
				}

				await Task.Delay(200);
				R.SendEventAndReport(actions);
			}
		}


		public override string ToString() =>
			$"Army: color= {color}, group= {group}, type= {type}, money= {money}," +
			$" units.Count= {units.Count}, buildings.Count= {buildings.Count}, focusPoint= {focusPoint}\n";


		// ==================  DATA  ========================


		[Serializable]
		public struct Data
		{
			public Color color;
			public Group group;
			public int money;
			public Vector3 focusPoint;
			public string name;


			public static byte[] Serialize(object obj)
			{
				var army = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					w.Write((byte)army.color);
					w.Write((byte)army.group);
					w.Write(army.money);
					w.Write(army.focusPoint.x); w.Write(army.focusPoint.y);
					w.Write(army.name);

					return m.ToArray();
				}
			}


			public static Data DeSerialize(byte[] data)
			{
				var army = new Data();
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					army.color = (Color)r.ReadByte();
					army.group = (Group)r.ReadByte();
					army.money = r.ReadInt32();
					army.focusPoint = new Vector3(r.ReadSingle(), r.ReadSingle(), 0f);
					army.name = r.ReadString();

					return army;
				}
			}


			public Data(Army army)
			{
				color = army.color; group = army.group; /*type = army.type;*/ money = army.money;
				focusPoint = army.focusPoint;
				name = army.name;
			}
		}
	}
}
