﻿using System.IO;
using UnityEngine;


namespace AncientEmpires.Units.Kings
{
	public class Galamar : King
	{
		public override Name name => Name.GALAMAR;


		// ======================  SAVED DATA  ===================================


		public new class Data : King.Data
		{
			public override Name name => Name.GALAMAR;


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


			public Data(Galamar galamar) : base(galamar)
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
