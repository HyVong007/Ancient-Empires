using System.Collections.Generic;
using UnityEngine;
using AncientEmpires.Terrains;
using System.Threading.Tasks;
using AncientEmpires.Util;
using System.IO;

namespace AncientEmpires.Units
{
	public class Catapult : Unit
	{
		public override Name name => Name.CATAPULT;


		public override IReadOnlyList<Vector3Int> FindAttackTargets()
		{
			var result = new List<Vector3Int>();
			if (isMoving || Castle.isBuying || army != Battle.instance.army) return result;

			foreach (var pos in UnitAlgorithm.FindTarget(transform.position.WorldToArray(), 1, 3))
			{
				var unit = array[pos.x][pos.y];
				var terrain = AETerrain.array[pos.x][pos.y];
				if (unit && unit.army.group != army.group || terrain is House && (terrain as House).CanBreak())
					result.Add(pos);
			}

			return result;
		}


		public override async Task Attack(Vector3Int target, float eDeltaH, float thisDeltaH)
		{
			var house = AETerrain.array[target.x][target.y] as House;
			if (!house)
			{
				await base.Attack(target, eDeltaH, thisDeltaH);
				return;
			}

			var wPos = house.transform.position;
			R.pool.Show(ObjectPool.POOL_KEY.YELLOW_FIRE, wPos);
			await Task.Delay(600);
			R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, wPos);
			R.pool.Show(ObjectPool.POOL_KEY.EXPLOSION_SMOKE, wPos);
			house.Break();
			await Task.Delay(600);
			R.pool.Hide();
			isSleep = true;
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.CATAPULT;


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


			public Data(Catapult catapult) : base(catapult)
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
