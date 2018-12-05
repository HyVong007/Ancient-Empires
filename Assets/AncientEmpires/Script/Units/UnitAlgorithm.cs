using System.Collections.Generic;
using AncientEmpires.Terrains;
using UnityEngine;


namespace AncientEmpires.Units
{
	public static class UnitAlgorithm
	{
		static UnitAlgorithm()
		{
			System.Action f = () =>
			  {
				  if (Unit.array == null) return;
				  var size = new Vector2Int(Unit.array.Length, Unit.array[0].Length);
				  rangeCosts = new int[size.x][];
				  for (int x = 0; x < size.x; ++x) rangeCosts[x] = new int[size.y];
			  };

			Battle.onStart += f;
			f();
		}


		private static readonly Vector3Int[] DIRECTIONS = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };

		/// <summary>
		/// Find all targets inside Terrain.array started from "start" within minRange and maxRange.
		/// <para>range>=0.</para>
		/// </summary>
		/// <param name="start">The point in Array Space</param>
		/// <param name="minRange">Index begin from 0 at "start" point.</param>
		/// <param name="maxRange"></param>
		/// <returns>The targets collection</returns>
		public static IReadOnlyList<Vector3Int> FindTarget(Vector3Int start, byte minRange, byte maxRange)
		{
			var size = new Vector3Int(AETerrain.array.Length, AETerrain.array[0].Length, 0);
			var result = new List<Vector3Int>();
			var source = new List<Vector3Int>() { start };
			var target = new List<Vector3Int>();
			var visited = new List<Vector3Int>() { start };

			for (int range = 0; range <= maxRange; ++range)
			{
				foreach (var p in source)
					foreach (var dir in DIRECTIONS)
					{
						var pos = p + dir;
						if (pos.x < 0 || pos.x >= size.x || pos.y < 0 || pos.y >= size.y || visited.Contains(pos)) continue;

						visited.Add(pos);
						target.Add(pos);
					}

				if (range >= minRange) result.AddRange(target);
				var tmp = target; target = source; source = tmp;
				target.Clear();
				if (source.Count == 0) break;
			}

			return result;
		}


		/// <summary>
		/// Save array space position and paths ={direction}
		/// </summary>
		public struct Target
		{
			public readonly Vector3Int pos;
			public readonly IReadOnlyList<Vector3Int> paths;


			public Target(Vector3Int pos, IReadOnlyList<Vector3Int> m_paths, Vector3Int direction)
			{
				this.pos = pos;
				var tmp = new List<Vector3Int>(m_paths) { direction };
				paths = tmp;
			}


			public Target Reverse
			{
				get
				{
					var stop = pos;
					var path = new List<Vector3Int>(paths.Count);
					for (int i = paths.Count - 1; i >= 0; --i)
					{
						var dir = paths[i] * -1;
						stop += dir;
						path.Add(dir);
					}
					return new Target(stop, path, Vector3Int.zero);
				}
			}
		}

		private static int[][] rangeCosts;
		private static readonly List<Target> result = new List<Target>();
		private static Unit unit;
		private static Army unitArmy;

		public static IReadOnlyList<Target> FindMove(Unit unit) => FindMove(unit, unit.army);

		public static IReadOnlyList<Target> FindMove(Unit unit, Army army)
		{
			var start = unit.transform.position.WorldToArray();
			var min = new Vector3Int(start.x - unit.moveRange + 1, start.y - unit.moveRange + 1, 0);
			var max = new Vector3Int(start.x + unit.moveRange - 1, start.y + unit.moveRange - 1, 0);
			for (int x = min.x; x <= max.x; ++x)
				for (int y = min.y; y <= max.y; ++y)
					if (IsValid(x, y)) rangeCosts[x][y] = 0;

			rangeCosts[start.x][start.y] = unit.moveRange;
			result.Clear();
			UnitAlgorithm.unit = unit;
			unitArmy = army;

			FindMove(new Target(start, new List<Vector3Int>(), Vector3Int.zero), unit.moveRange);
			return result;
		}


		private static bool IsValid(int x, int y) =>
			0 <= x && x < rangeCosts.Length && 0 <= y && y < rangeCosts[0].Length;


		private static void FindMove(Target start, int range)
		{
			foreach (var dir in DIRECTIONS)
			{
				var pos = start.pos + dir;
				if (pos.x < 0 || pos.x >= Unit.array.Length || pos.y < 0 || pos.y >= Unit.array[0].Length) continue;

				int posRange;
				var posTerrain = AETerrain.array[pos.x][pos.y];
				if (unit is Dragon) posRange = range - 1;
				else if (unit is Elemental && posTerrain is NormalTerrain && (posTerrain as NormalTerrain).name == NormalTerrain.Name.WATER) posRange = range - 1;
				else posRange = range - 1 - posTerrain.decreaseMove;
				if (posRange <= rangeCosts[pos.x][pos.y]) continue;

				// This pos range is larger than current Range cost
				rangeCosts[pos.x][pos.y] = posRange;
				var target = new Target(pos, start.paths, dir);
				var posUnit = Unit.array[pos.x][pos.y];
				if (!posUnit)
				{
					result.Remove(result.Find(t => t.pos == target.pos));
					result.Add(target);
				}
				else if (posUnit && posUnit.army.group != unitArmy.group) continue;

				FindMove(target, posRange);
			}
		}


		// ========================  ATTACK & FIGHTING STATISTICS  ==================


		public static void CalculateAttack(Unit thisUnit, Unit enemy, out float enemyDeltaHealth, out float thisDetaHealth)
		{
			if (!enemy || enemy.army.group == thisUnit.army.group)
			{
				enemyDeltaHealth = -1f;
				thisDetaHealth = -1f;
				return;
			}

			float enemyHealth = enemy.health, thisHealth = thisUnit.health;
			float delta = CalculateAttack(thisUnit, thisHealth, enemy, enemyHealth);
			float h0 = enemyHealth;
			enemyHealth = Mathf.Clamp(enemyHealth - delta, 0f, 100f);
			enemyDeltaHealth = h0 - enemyHealth;
			thisDetaHealth = -1f;

			// Can enemy re-attack this ?
			if (enemyHealth == 0f || !enemy.FindAttackTargets().Contains(thisUnit.transform.position.WorldToArray())) return;

			delta = CalculateAttack(enemy, enemyHealth, thisUnit, thisHealth);
			h0 = thisHealth;
			thisHealth = Mathf.Clamp(thisHealth - delta, 0f, 100f);
			thisDetaHealth = h0 - thisHealth;
		}


		private static float CalculateAttack(Unit thisUnit, float thisHealth, Unit enemy, float enemyHealth)
		{
			var pos = enemy.transform.position.WorldToArray();
			var terrain = AETerrain.array[pos.x][pos.y];
			float eDef = (enemyHealth * 0.01f) * (enemy.defend + (enemy is Elemental && terrain is NormalTerrain && (terrain as NormalTerrain).name == NormalTerrain.Name.WATER ? Elemental.WATER_INCREASE_DEFEND : terrain.increaseDefend));
			float hD = thisHealth * 0.01f;
			float minAttack = thisUnit.minAttack * hD;
			float maxAttack = thisUnit.maxAttack * hD;
			float result = (eDef <= minAttack) ? Random.Range(minAttack, maxAttack) - eDef :
				(eDef > maxAttack) ? Random.Range(eDef, eDef + Unit.OVER_LIMIT_RANDOM_ATTACK) - eDef :
				Random.Range(eDef + 1, maxAttack) - eDef;

			// A Unit cannot kill a Full-Health-Enemy in one hit !
			return Mathf.Clamp(result, 0f, 99f);
		}


		public struct UnitInfo
		{
			public float health, minAttack, maxAttack, defend, experience;


			public UnitInfo(Unit unit)
			{
				health = unit.health;
				minAttack = unit.minAttack;
				maxAttack = unit.maxAttack;
				defend = unit.defend;
				experience = unit.experience;
			}
		}


		public static UnitInfo GetNewInfo(Unit unit, float enemyDeltaHealth)
		{
			var info = new UnitInfo(unit);
			info.experience += enemyDeltaHealth / (1 + info.experience);
			info.experience *= unit.increase_exp_speed;
			info.minAttack += info.experience * Unit.INCREASE_ATTACK_SPEED;
			info.maxAttack += info.experience * Unit.INCREASE_ATTACK_SPEED;
			info.defend += info.experience * Unit.INCREASE_DEFEND_SPEED;
			return info;
		}


		public static IEnumerator<string> LevelNameGenerator(Unit unit)
		{
			int lastExp = 0;
			string lastName = unit.levelNames[0];
			int count = 0;
			while (true)
			{
				if (count == unit.levelNames.Count || lastExp == (int)unit.experience || !unit.levelNames.ContainsKey((int)unit.experience))
				{
					yield return lastName;
					continue;
				}

				lastExp = (int)unit.experience;
				++count;
				yield return lastName = unit.levelNames[(int)unit.experience];
			}
		}
	}
}
