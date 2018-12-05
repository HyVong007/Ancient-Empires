using AncientEmpires.Terrains;
using System.IO;

namespace AncientEmpires.Units
{
	public class Soldier : Unit, IOccupier
	{
		public override Name name => Name.SOLDIER;


		public bool CanOccupy(AETerrain terrain) =>
			terrain is House && (terrain as House).army?.group != army.group;


		public void Occupy()
		{
			var pos = transform.position.WorldToArray();
			(AETerrain.array[pos.x][pos.y] as House).Occupy(this);
			isSleep = true;
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.SOLDIER;


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


			public Data(Soldier soldier) : base(soldier)
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
