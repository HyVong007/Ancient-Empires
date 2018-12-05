using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace AncientEmpires.Units
{
	public class Archer : Unit
	{
		public override Name name => Name.ARCHER;


		public override IReadOnlyList<Vector3Int> FindAttackTargets()
		{
			if (army != Battle.instance.army) return base.FindAttackTargets();
			var result = new List<Vector3Int>();
			foreach (var pos in UnitAlgorithm.FindTarget(transform.position.WorldToArray(), 0, 1))
			{
				var unit = array[pos.x][pos.y];
				if (unit && unit.army.group != army.group) result.Add(pos);
			}

			return result;
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.ARCHER;


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


			public Data(Archer archer) : base(archer)
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
