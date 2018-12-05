using System.IO;
using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Util;


namespace AncientEmpires.Units
{
	public class Skeleton : Unit
	{
		public override Name name => Name.SKELETON;


		public override async Task<Tombstone> Die()
		{
			if (isUsed)
			{
				var pos = transform.position.WorldToArray();
				array[pos.x][pos.y] = null;
				army.units.Remove(this);
				transform.parent = null;
				gameObject.SetActive(false);
				R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, transform.position);
				R.pool.Show(ObjectPool.POOL_KEY.EXPLOSION_SMOKE, transform.position);
				R.onDie?.Invoke(this);

				await Task.Delay(600);
				R.pool.Hide(ObjectPool.POOL_KEY.BLUE_FIRE);
				R.pool.Hide(ObjectPool.POOL_KEY.EXPLOSION_SMOKE);
			}

			Destroy(gameObject);
			return null;
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.SKELETON;


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


			public Data(Skeleton skeleton) : base(skeleton)
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
