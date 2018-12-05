using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using AncientEmpires.Util;
using System.IO;

namespace AncientEmpires.Units
{
	public class Wisp : Unit
	{
		public const float POWER_DELTA = 10f;
		public const int POWER_LIFE_TIME = 2;

		public override Name name => Name.WISP;


		static Wisp()
		{
			Battle.onTurnBegin += OnTurnBegin;
		}


		public IReadOnlyList<Vector3Int> FindHealTargets()
		{
			var result = new List<Vector3Int>();
			foreach (var pos in UnitAlgorithm.FindTarget(transform.position.WorldToArray(), 0, 1))
				if (array[pos.x][pos.y]?.army.group == army.group) result.Add(pos);

			return result;
		}


		public async Task Heal()
		{
			var result = new List<Unit>();
			foreach (var pos in FindHealTargets())
			{
				R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, pos.ArrayToWorld());
				var unit = array[pos.x][pos.y];
				result.Add(unit);
				Heal(unit);
			}
			base.isSleep = true;
			if (result.Count > 0)
			{
				await Task.Delay(600);
				R.pool.Hide(ObjectPool.POOL_KEY.BLUE_FIRE);
				//onHeal?.Invoke(this, result);
			}
		}


		public static void Heal(Unit unit, bool isHeal = true)
		{
			if (isHeal && unit.powerTurn == 0)
			{
				unit.minAttack += POWER_DELTA;
				unit.maxAttack += POWER_DELTA;
				unit.powerUI.SetActive(true);
			}
			else if (!isHeal && unit.powerTurn > 0)
			{
				unit.minAttack -= POWER_DELTA;
				unit.maxAttack -= POWER_DELTA;
				unit.powerUI.SetActive(false);
			}
			unit.powerTurn = isHeal ? Battle.instance.turn + POWER_LIFE_TIME : 0;
		}


		public override bool isSleep
		{
			get { return base.isSleep; }

			set
			{
				if (value) R.input.Wait(Heal());
				else base.isSleep = value;
			}
		}


		// =========================  TURN BASE EVENTS  ===========================


		private static void OnTurnBegin()
		{
			int currentTurn = Battle.instance.turn;
			foreach (var army in Army.dict.Values)
			{
				if (!army) continue;
				foreach (var unit in army.units)
					if (unit.powerTurn > 0 && currentTurn >= unit.powerTurn)
					{
						unit.powerTurn = 0;
						unit.powerUI.SetActive(false);
						unit.minAttack -= POWER_DELTA;
						unit.maxAttack -= POWER_DELTA;
					}
			}
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.WISP;


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


			public Data(Wisp wisp) : base(wisp)
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
