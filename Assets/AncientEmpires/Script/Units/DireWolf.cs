using System.Threading.Tasks;
using UnityEngine;
using AncientEmpires.Util;
using System.IO;

namespace AncientEmpires.Units
{
	public class DireWolf : Unit
	{
		public const float POISON_DELTA = 10f;
		public const int POISON_LIFE_TIME = 2;

		public override Name name => Name.DIREWOLF;


		static DireWolf()
		{
			Battle.onTurnBegin += OnTurnBegin;
		}


		public override async Task Attack(Vector3Int target, float eDeltaH, float thisDeltaH)
		{
			await base.Attack(target, eDeltaH, thisDeltaH);
			var enemy = array[target.x][target.y];
			if (army != Battle.instance.army || !enemy) return;

			R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, enemy.transform.position);
			Poisoning(enemy);
			await Task.Delay(600);
		}


		public static void Poisoning(Unit unit, bool isPoison = true)
		{
			if (isPoison && unit.poisonTurn == 0)
			{
				unit.minAttack -= POISON_DELTA;
				unit.maxAttack -= POISON_DELTA;
				unit.defend -= POISON_DELTA;
				unit.poisonUI.SetActive(true);
			}
			else if (!isPoison && unit.poisonTurn > 0)
			{
				unit.minAttack += POISON_DELTA;
				unit.maxAttack += POISON_DELTA;
				unit.defend += POISON_DELTA;
				unit.poisonUI.SetActive(false);
			}
			unit.poisonTurn = isPoison ? Battle.instance.turn + POISON_LIFE_TIME : 0;
		}


		// =========================  TURN BASE EVENTS  ===========================


		private static void OnTurnBegin()
		{
			int currentTurn = Battle.instance.turn;
			foreach (var army in Army.dict.Values)
			{
				if (!army) continue;
				foreach (var unit in army.units)
					if (unit.poisonTurn > 0 && currentTurn >= unit.poisonTurn)
					{
						unit.poisonTurn = 0;
						unit.poisonUI.SetActive(false);
						unit.minAttack += POISON_DELTA;
						unit.maxAttack += POISON_DELTA;
						unit.defend += POISON_DELTA;
					}
			}
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.DIREWOLF;


			public static new byte[] Serialize(object obj)
			{
				var unit = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					byte[] data = mSerialize(obj);
					w.Write(data.Length);
					w.Write(data);


					return m.ToArray();
				}
			}


			public static new Data DeSerialize(byte[] data)
			{
				Data unit = null;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					unit = (Data)mDeSerialize(r.ReadBytes(r.ReadInt32()));

					return unit;
				}
			}


			public Data(DireWolf direwolf) : base(direwolf)
			{

			}


			public Data() { }


			protected override void Set(Unit unit)
			{
				base.Set(unit);
			}
		}
	}
}
