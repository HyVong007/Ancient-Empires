using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


namespace AncientEmpires.Units
{
	public class Crystal : Unit
	{
		public override Name name => Name.CRYSTAL;


		public override IReadOnlyList<Vector3Int> FindAttackTargets() => new List<Vector3Int>();


		public override Task Attack(Vector3Int target, float eDeltaH, float thisDeltaH)
		{
			throw new System.Exception("Crystal cannot Attack !");
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.CRYSTAL;


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


			public Data(Crystal crystal) : base(crystal)
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
