using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Util;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace AncientEmpires.Units
{
	public class Sorceress : Unit
	{
		public override Name name => Name.SORCERESS;


		public IReadOnlyList<Vector3Int> FindRaiseTargets()
		{
			var result = new List<Vector3Int>();
			foreach (var pos in UnitAlgorithm.FindTarget(transform.position.WorldToArray(), 0, 0))
				if (Tombstone.array[pos.x][pos.y] && !array[pos.x][pos.y]) result.Add(pos);

			return result;
		}


		public async Task<Skeleton> Raise(Vector3Int tombstonePos)
		{
			var tombstone = Tombstone.array[tombstonePos.x][tombstonePos.y];
			var pos = tombstone.transform.position;
			R.pool.selectCircle.transform.position = pos;
			R.pool.Show(ObjectPool.POOL_KEY.YELLOW_FIRE, pos);
			await Task.Delay(600);

			R.pool.selectCircle.SetActive(false);
			tombstone.Destroy();
			var skeleton = (Skeleton)New(Name.SKELETON, army, pos);
			skeleton.Use();
			R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, pos);
			R.pool.Show(ObjectPool.POOL_KEY.EXPLOSION_SMOKE, pos);
			await Task.Delay(600);

			R.pool.Hide();
			isSleep = true;
			return skeleton;
		}


		public async Task<Skeleton> Raise(List<Action> actions)
		{
			var targets = FindRaiseTargets();
			foreach (var point in targets) R.pool.Show(ObjectPool.POOL_KEY.GREEN_ALPHA, point.ArrayToWorld());

			while (true)
			{
				var click = await R.input.WaitForClicking();
				if (Battle.endTurnCancel.IsCancellationRequested || !targets.Contains(click.Value))
				{
					R.pool.Hide(ObjectPool.POOL_KEY.GREEN_ALPHA);
					R.pool.selectCircle.SetActive(false);
					return null;
				}

				var pos = click.Value;
				if (!R.pool.selectCircle.activeSelf)
				{
					R.pool.selectCircle.SetActive(true);
					R.pool.selectCircle.transform.position = pos.ArrayToWorld();
					R.info.armyUI.UpdateTerrain(pos);
					continue;
				}
				else if (R.pool.selectCircle.transform.position.WorldToArray() != pos)
				{
					await R.Move(R.pool.selectCircle, pos.ArrayToWorld(), 0.5f);
					R.info.armyUI.UpdateTerrain(pos);
					continue;
				}

				R.pool.Hide(ObjectPool.POOL_KEY.GREEN_ALPHA);
				bool result = await Play(actions, new RaiseAction(transform.position.WorldToArray(), pos));
				R.pool.selectCircle.SetActive(false);
				return result ? await Raise(pos) : null;
			}
		}


		// ======================  SAVED DATA  ===================================


		public new class Data : Unit.Data
		{
			public override Name name => Name.SORCERESS;


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


			public Data(Sorceress sorceress) : base(sorceress)
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
