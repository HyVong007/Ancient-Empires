using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AncientEmpires.Units.Kings;
using AncientEmpires.Terrains;
using AncientEmpires.Util;


namespace AncientEmpires.Units
{
	[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
	public abstract class Unit : MonoBehaviour, IUsable, IClickHandler
	{
		public const float OVER_LIMIT_RANDOM_ATTACK = 3f,
			INCREASE_ATTACK_SPEED = 0.1f, INCREASE_DEFEND_SPEED = 0.1f;


		public Army army { get; protected set; }

		public float minAttack, maxAttack, defend;

		public int moveRange => _moveRange;

		public int cost => _cost;

		[SerializeField] private int _moveRange, _cost;

		public float increase_exp_speed => _increase_exp_speed;

		[SerializeField] private float _increase_exp_speed = 1;

		public float experience
		{
			get { return _experience; }

			protected set
			{
				_experience = value; ;
				levelNameGenerator.MoveNext();
			}
		}

		private float _experience;

		private CancellationTokenSource cts = new CancellationTokenSource();

		public float health
		{
			get { return _health; }

			set
			{
				value = Mathf.Clamp(value, 0f, 100f);
				int delta = (int)(value - _health);
				_health = value;
				changeHealthUI.text = (delta > 0 ? "+" : "") + delta.ToString();
				changeHealthUI.gameObject.SetActive(true);
				cts.Cancel();
				cts = new CancellationTokenSource();
				R.WaitToHide(changeHealthUI.gameObject, 5f, cts.Token);
				healthUI.text = _health < 100f ? ((int)_health).ToString() : "";
			}
		}
		private float _health = 100f;
		[NonSerialized] public int poisonTurn, powerTurn;

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public Animator animator => _animator;

		public GameObject poisonUI => _poisonUI;

		public GameObject powerUI => _powerUI;

		public Text healthUI => _healthUI;

		public Text changeHealthUI => _changeHealthUI;

		public IReadOnlyDictionary<int, string> levelNames => _levelNames;

		private IEnumerator<string> levelNameGenerator;

		public string levelName => levelNameGenerator.Current;

		public static Unit[][] array { get; private set; }

		protected static IReadOnlyList<Action> verifiedActions;

		public Sprite sleepSprite;
		[SerializeField] protected ArmyColor_Anim anims;
		[SerializeField] private SpriteRenderer _spriteRenderer;
		[SerializeField] private GameObject _poisonUI, _powerUI;
		[SerializeField] private Animator _animator;
		[SerializeField] private Text _healthUI, _changeHealthUI;
		[SerializeField] private Int_String _levelNames;

		public enum Name
		{
			SOLDIER, ARCHER, ELEMENTAL, SORCERESS, DIREWOLF,
			GOLEM, DRAGON, SKELETON, WISP, CRYSTAL, CATAPULT,
			SAETH, GALAMAR, VALADORN, DEMONLORD
		}

		public abstract new Name name { get; }


		static Unit()
		{
			Battle.onReset += () =>
			  {
				  var a = Config.instance.map.terrainArray;
				  var size = new Vector2Int(a.Length, a[0].Length);
				  array = new Unit[size.x][];
				  for (int x = 0; x < size.x; ++x) array[x] = new Unit[size.y];
			  };

			Battle.onPlayerAction += (IReadOnlyList<Action> actions) => { verifiedActions = actions; };
		}


		public static Unit DeSerialize(int ID, Vector3 wPos, bool use = true)
		{
			Unit unit = null;

			if (ID == 62 || ID == 74 || ID == 86 || ID == 98)
			{
				unit = King.DeSerialize(ID, wPos, use);
				return unit;
			}

			var name = Name.ARCHER;
			var color = Army.Color.BLACK;

			switch (ID)
			{
				case 53:
					name = Name.SOLDIER;
					color = Army.Color.BLACK;
					break;

				case 54:
					name = Name.ARCHER;
					color = Army.Color.BLACK;
					break;

				case 55:
					name = Name.ELEMENTAL;
					color = Army.Color.BLACK;
					break;

				case 56:
					name = Name.SORCERESS;
					color = Army.Color.BLACK;
					break;

				case 57:
					name = Name.WISP;
					color = Army.Color.BLACK;
					break;

				case 58:
					name = Name.DIREWOLF;
					color = Army.Color.BLACK;
					break;

				case 59:
					name = Name.GOLEM;
					color = Army.Color.BLACK;
					break;

				case 60:
					name = Name.CATAPULT;
					color = Army.Color.BLACK;
					break;

				case 61:
					name = Name.DRAGON;
					color = Army.Color.BLACK;
					break;

				case 63:
					name = Name.SKELETON;
					color = Army.Color.BLACK;
					break;

				case 64:
					name = Name.CRYSTAL;
					color = Army.Color.BLACK;
					break;

				case 65:
					name = Name.SOLDIER;
					color = Army.Color.BLUE;
					break;

				case 66:
					name = Name.ARCHER;
					color = Army.Color.BLUE;
					break;

				case 67:
					name = Name.ELEMENTAL;
					color = Army.Color.BLUE;
					break;

				case 68:
					name = Name.SORCERESS;
					color = Army.Color.BLUE;
					break;

				case 69:
					name = Name.WISP;
					color = Army.Color.BLUE;
					break;

				case 70:
					name = Name.DIREWOLF;
					color = Army.Color.BLUE;
					break;

				case 71:
					name = Name.GOLEM;
					color = Army.Color.BLUE;
					break;

				case 72:
					name = Name.CATAPULT;
					color = Army.Color.BLUE;
					break;

				case 73:
					name = Name.DRAGON;
					color = Army.Color.BLUE;
					break;

				case 75:
					name = Name.SKELETON;
					color = Army.Color.BLUE;
					break;

				case 76:
					name = Name.CRYSTAL;
					color = Army.Color.BLUE;
					break;

				case 77:
					name = Name.SOLDIER;
					color = Army.Color.GREEN;
					break;

				case 78:
					name = Name.ARCHER;
					color = Army.Color.GREEN;
					break;

				case 79:
					name = Name.ELEMENTAL;
					color = Army.Color.GREEN;
					break;

				case 80:
					name = Name.SORCERESS;
					color = Army.Color.GREEN;
					break;

				case 81:
					name = Name.WISP;
					color = Army.Color.GREEN;
					break;

				case 82:
					name = Name.DIREWOLF;
					color = Army.Color.GREEN;
					break;

				case 83:
					name = Name.GOLEM;
					color = Army.Color.GREEN;
					break;

				case 84:
					name = Name.CATAPULT;
					color = Army.Color.GREEN;
					break;

				case 85:
					name = Name.DRAGON;
					color = Army.Color.GREEN;
					break;

				case 87:
					name = Name.SKELETON;
					color = Army.Color.GREEN;
					break;

				case 88:
					name = Name.CRYSTAL;
					color = Army.Color.GREEN;
					break;

				case 89:
					name = Name.SOLDIER;
					color = Army.Color.RED;
					break;

				case 90:
					name = Name.ARCHER;
					color = Army.Color.RED;
					break;

				case 91:
					name = Name.ELEMENTAL;
					color = Army.Color.RED;
					break;

				case 92:
					name = Name.SORCERESS;
					color = Army.Color.RED;
					break;

				case 93:
					name = Name.WISP;
					color = Army.Color.RED;
					break;

				case 94:
					name = Name.DIREWOLF;
					color = Army.Color.RED;
					break;

				case 95:
					name = Name.GOLEM;
					color = Army.Color.RED;
					break;

				case 96:
					name = Name.CATAPULT;
					color = Army.Color.RED;
					break;

				case 97:
					name = Name.DRAGON;
					color = Army.Color.RED;
					break;

				case 99:
					name = Name.SKELETON;
					color = Army.Color.RED;
					break;

				case 100:
					name = Name.CRYSTAL;
					color = Army.Color.RED;
					break;

				default: return null;
			}
			if (!Army.dict[color]) return null;

			unit = New(name, Army.dict[color], wPos);
			unit.experience = 0f;
			unit.healthUI.text = unit.changeHealthUI.text = "";
			if (use) unit.Use();
			return unit;
		}


		public static Unit New(Name name, Army army, Vector3? wPos = null)
		{
			var unit = Instantiate(R.asset.prefab.units[name], wPos != null ? wPos.Value : Vector3.zero, Quaternion.identity);
			unit.levelNameGenerator = UnitAlgorithm.LevelNameGenerator(unit);
			unit.army = army;
			return unit;
		}


		protected bool isUsed;

		public virtual void Use()
		{
			isUsed = true;
			var pos = transform.position.WorldToArray();
			if (!array[pos.x][pos.y]) array[pos.x][pos.y] = this;
			army.units.Add(this);
			transform.parent = army.unitAnchor;
			animator.runtimeAnimatorController = anims[army.color];
		}


		private void Start()
		{
			if (health == 100f)
			{
				healthUI.text = "";
				changeHealthUI.text = "";
			}
		}


		// =====================================================================


		public bool isOnHealingTerrain
		{
			get
			{
				var pos = transform.position.WorldToArray();
				var terrain = AETerrain.array[pos.x][pos.y];
				var name = (terrain as NormalTerrain)?.name;
				if (name == NormalTerrain.Name.BARREL || name == NormalTerrain.Name.SILVER_TENT || name == NormalTerrain.Name.PLANE) return true;
				return (terrain as IOccupyable)?.army?.group == army.group;
			}
		}


		public virtual bool isSleep
		{
			get { return _isSleep; }

			set
			{
				_isSleep = value;
				animator.enabled = !value;
				if (value) spriteRenderer.sprite = sleepSprite;
				if (value) R.pool.selectRect.transform.position = transform.position;
			}
		}
		private bool _isSleep;


		public virtual async Task<Tombstone> Die()
		{
			Tombstone tombstone = null;

			if (isUsed)
			{
				var pos = transform.position.WorldToArray();
				if (array[pos.x][pos.y] == this) array[pos.x][pos.y] = null;
				army.units.Remove(this);
				transform.parent = null;
				gameObject.SetActive(false);
				tombstone = Tombstone.NewOrUpdate(transform.position);
				R.pool.Show(ObjectPool.POOL_KEY.BLUE_FIRE, transform.position);
				R.pool.Show(ObjectPool.POOL_KEY.EXPLOSION_SMOKE, transform.position);
				R.onDie?.Invoke(this);

				await Task.Delay(600);
				R.pool.Hide(ObjectPool.POOL_KEY.BLUE_FIRE);
				R.pool.Hide(ObjectPool.POOL_KEY.EXPLOSION_SMOKE);
			}

			Destroy(gameObject);
			return tombstone;
		}


		public virtual IReadOnlyList<Vector3Int> FindAttackTargets()
		{
			var result = new List<Vector3Int>();
			foreach (var pos in UnitAlgorithm.FindTarget(transform.position.WorldToArray(), 0, 0))
			{
				var unit = array[pos.x][pos.y];
				if (unit && unit.army.group != army.group) result.Add(pos);
			}

			return result;
		}


		public virtual async Task Attack(Vector3Int target, float eDeltaH, float thisDeltaH)
		{
			var enemy = array[target.x][target.y];
			if (!R.pool.selectCircle.activeSelf)
			{
				R.pool.selectCircle.SetActive(true);
				R.pool.selectCircle.transform.position = target.ArrayToWorld();
			}
			else await R.Move(R.pool.selectCircle, target.ArrayToWorld(), 0.5f);

			R.info.armyUI.UpdateTerrain(target);
			R.pool.Show(ObjectPool.POOL_KEY.YELLOW_FIRE, enemy.transform.position);
			await Task.Delay(600);
			R.pool.Hide(ObjectPool.POOL_KEY.YELLOW_FIRE);

			// Calculate attack health
			enemy.health -= (army == Battle.instance.army) ? eDeltaH : thisDeltaH;
			if (enemy.health <= 0f)
			{
				bool isActive = army == Battle.instance.army;
				if (isActive) UpdateUnitInfo(eDeltaH);
				else
				{
					UpdateUnitInfo(thisDeltaH);
					enemy.UpdateUnitInfo(eDeltaH);
				}

				R.input.Wait(enemy.Die());
				if (isActive) isSleep = true;
				R.pool.selectCircle.SetActive(false);
				return;
			}

			// Can enemy re-attack ?
			var thisPos = transform.position.WorldToArray();
			if (army == Battle.instance.army)
			{
				if (thisDeltaH >= 0f) await enemy.Attack(thisPos, eDeltaH, thisDeltaH);
				else UpdateUnitInfo(eDeltaH);
			}
			else
			{
				UpdateUnitInfo(thisDeltaH);
				enemy.UpdateUnitInfo(eDeltaH);
			}

			isSleep = array[thisPos.x][thisPos.y]?.army == Battle.instance.army;
			R.pool.selectCircle.SetActive(false);
		}


		private void UpdateUnitInfo(float enemyDeltaHealth)
		{
			experience += increase_exp_speed * (enemyDeltaHealth / (1 + experience));
			minAttack += experience * INCREASE_ATTACK_SPEED;
			maxAttack += experience * INCREASE_ATTACK_SPEED;
			defend += experience * INCREASE_DEFEND_SPEED;
		}


		public async Task<Vector3Int?> Attack(List<Action> actions)
		{
			var targets = FindAttackTargets();
			foreach (var pos in targets) R.pool.Show(ObjectPool.POOL_KEY.YELLOW_ALPHA, pos.ArrayToWorld());

			while (true)
			{
				var click = await R.input.WaitForClicking();
				if (Battle.endTurnCancel.IsCancellationRequested || !targets.Contains(click.Value))
				{
					R.pool.Hide(ObjectPool.POOL_KEY.YELLOW_ALPHA);
					R.pool.selectCircle.SetActive(false);
					return null;
				}

				var pos = click.Value;
				if (!R.pool.selectCircle.activeSelf)
				{
					R.pool.selectCircle.transform.position = pos.ArrayToWorld();
					R.pool.selectCircle.SetActive(true);
					R.info.UpdateUnitUI(pos);
					continue;
				}
				else if (pos != R.pool.selectCircle.transform.position.WorldToArray())
				{
					await R.Move(R.pool.selectCircle, pos.ArrayToWorld(), 0.5f);
					R.info.UpdateUnitUI(pos);
					continue;
				}

				R.pool.Hide(ObjectPool.POOL_KEY.YELLOW_ALPHA);
				float eDeltaH = -1f, thisDeltaH = -1f;
				var unit = array[pos.x][pos.y];
				if (unit && unit.army.group != army.group)
					UnitAlgorithm.CalculateAttack(this, unit, out eDeltaH, out thisDeltaH);

				bool result = await Play(actions, new AttackAction(transform.position.WorldToArray(), pos, eDeltaH, thisDeltaH));
				R.pool.selectCircle.SetActive(false);
				R.info.passiveUnitUI.SetActive(false);
				if (result)
				{
					await Attack(pos, eDeltaH, thisDeltaH);
					R.info.UpdateUnitUI(transform.position.WorldToArray());
					return pos;
				}
				return null;
			}
		}


		public IReadOnlyList<UnitAlgorithm.Target> FindMoveTargets() => UnitAlgorithm.FindMove(this);


		private async Task Move(UnitAlgorithm.Target target, float speed = 0.15f)
		{
			if (army.type != Army.Type.LOCAL)
			{
				R.DrawMovePaths(transform.position, target);
				R.pool.targetRect.transform.position = target.pos.ArrayToWorld();
				R.pool.targetRect.SetActive(true);
				await Task.Delay(200);
			}

			R.DrawMovePaths();
			R.pool.targetRect.SetActive(false);
			var start = transform.position.WorldToArray();
			if (array[start.x][start.y] == this) array[start.x][start.y] = null;

			var stopW = transform.position;
			int smokeDelay = 3;
			int smokeCount = 0;
			foreach (var path in target.paths)
				await R.Move(gameObject, stopW += path, speed,
					() =>
					{
						R.camCtrl.Focus(transform.position);
						if (++smokeCount < smokeDelay) return;
						smokeCount = 0;
						R.pool.Show(ObjectPool.POOL_KEY.MOVING_SMOKE, transform.position);
					});

			R.pool.Hide(ObjectPool.POOL_KEY.MOVING_SMOKE);
			var stop = target.pos;
			array[stop.x][stop.y] = this;
			R.info.UpdateUnitUI(stop);
		}


		public async Task Move(Vector3Int target, float speed = 0.15f)
		{
			foreach (var t in FindMoveTargets())
				if (t.pos == target)
				{
					await Move(t, speed);
					break;
				}
		}


		public async Task<Vector3Int?> Move(List<Action> actions)
		{
			var targets = FindMoveTargets();
			var posList = new List<Vector3Int>();
			foreach (var t in targets)
			{
				posList.Add(t.pos);
				R.pool.Show(ObjectPool.POOL_KEY.RED_ALPHA, t.pos.ArrayToWorld());
			}

			while (true)
			{
				var click = await R.input.WaitForClicking();
				if (Battle.endTurnCancel.IsCancellationRequested || !posList.Contains(click.Value))
				{
					R.pool.Hide(ObjectPool.POOL_KEY.RED_ALPHA);
					R.DrawMovePaths();
					R.pool.targetRect.SetActive(false);
					R.info.armyUI.UpdateTerrain(transform.position.WorldToArray());
					return null;
				}

				var pos = click.Value;
				UnitAlgorithm.Target target = default(UnitAlgorithm.Target);
				foreach (var t in targets) if (t.pos == pos) { target = t; break; }
				R.DrawMovePaths();
				if (!R.pool.targetRect.activeSelf)
				{
					R.pool.targetRect.transform.position = pos.ArrayToWorld();
					R.pool.targetRect.SetActive(true);
					R.DrawMovePaths(transform.position, target);
					R.info.armyUI.UpdateTerrain(pos);
					continue;
				}
				else if (pos != R.pool.targetRect.transform.position.WorldToArray())
				{
					await R.Move(R.pool.targetRect, pos.ArrayToWorld(), 0.5f);
					R.DrawMovePaths(transform.position, target);
					R.info.armyUI.UpdateTerrain(pos);
					continue;
				}

				// Move to pos using target and do actions if any
				R.pool.Hide(ObjectPool.POOL_KEY.RED_ALPHA);
				R.pool.targetRect.SetActive(false);
				R.pool.selectRect.SetActive(false);
				var moveAction = new MoveAction(transform.position.WorldToArray(), pos);
				actions.Add(moveAction);
				await Move(target, 0.15f);
				R.pool.targetRect.transform.position = transform.position;
				R.pool.targetRect.SetActive(true);
				var result = await OnAction(actions);

				// Check OnAction result
				click = result == ResultOnAction.NOACTION ? await R.input.WaitForClicking() : R.input.click;
				R.pool.targetRect.SetActive(false);
				switch (result)
				{
					case ResultOnAction.FAILED:
						await Move(target.Reverse, 0.15f);
						return null;

					case ResultOnAction.NOACTION:
					case ResultOnAction.CANCELED:
						if (Battle.endTurnCancel.IsCancellationRequested) goto case ResultOnAction.FAILED;
						actions.RemoveAt(actions.Count - 1);
						if (click == pos)
						{
							R.pool.targetRect.SetActive(true);
							bool ok = await Play(actions, moveAction);
							R.pool.targetRect.SetActive(false);
							if (ok)
							{
								isSleep = true;
								goto case ResultOnAction.SUCCESSFULED;
							}
							goto case ResultOnAction.FAILED;
						}
						else
						{
							await Move(target.Reverse, 0.15f);
							R.pool.selectRect.SetActive(true);
							R.info.armyUI.UpdateTerrain(transform.position.WorldToArray());
							foreach (var point in posList) R.pool.Show(ObjectPool.POOL_KEY.RED_ALPHA, point.ArrayToWorld());
							continue;
						}

					case ResultOnAction.SUCCESSFULED:
						R.pool.selectRect.transform.position = pos.ArrayToWorld();
						R.pool.selectRect.SetActive(true);
						return pos;
				}
			}
		}


		public IReadOnlyList<Action.Name> FindAllActions()
		{
			var result = new List<Action.Name>();
			var pos = transform.position.WorldToArray();
			var terrain = AETerrain.array[pos.x][pos.y];

			if (!isMoving && FindMoveTargets().Count > 0) result.Add(Action.Name.MOVE);
			if (FindAttackTargets().Count > 0) result.Add(Action.Name.ATTACK);
			if (!isMoving && !Castle.isBuying && this is Wisp && (this as Wisp).FindHealTargets().Count > 0) result.Add(Action.Name.HEAL);
			else if (this is Sorceress && (this as Sorceress).FindRaiseTargets().Count > 0) result.Add(Action.Name.RAISE);
			else if (this is IOccupier && (this as IOccupier).CanOccupy(terrain)) result.Add(Action.Name.OCCUPY);
			if (!isMoving && !Castle.isBuying && terrain is Castle && (terrain as Castle).army == Battle.instance.army) result.Add(Action.Name.BUY);
			return result;
		}


		protected bool isMoving { get; private set; }

		public enum ResultOnAction
		{
			SUCCESSFULED, FAILED, CANCELED, NOACTION
		}

		public async Task<ResultOnAction> OnAction(List<Action> actions)
		{
			var battleActions = FindAllActions();
			Action.Name action;
			if (battleActions.Count > 1 || battleActions.Contains(Action.Name.OCCUPY) || battleActions.Contains(Action.Name.HEAL))
			{
				R.DrawActionButtons(transform.position, battleActions);
				var button = await R.pool.WaitForButton();
				R.DrawActionButtons(Vector3.zero, null);
				if (Battle.endTurnCancel.IsCancellationRequested) return ResultOnAction.FAILED;
				if (button == null) return ResultOnAction.CANCELED;
				action = button.Value;
			}
			else if (battleActions.Count == 1) action = (battleActions as List<Action.Name>)[0];
			else return ResultOnAction.NOACTION;

			var pos = transform.position.WorldToArray();
			var endTurn = Battle.endTurnCancel;
			ResultOnAction result = default(ResultOnAction);
			switch (action)
			{
				case Action.Name.ATTACK:
					{
						var target = await Attack(actions);
						result = endTurn.IsCancellationRequested ? ResultOnAction.FAILED : target != null ? ResultOnAction.SUCCESSFULED : ResultOnAction.CANCELED;
						break;
					}

				case Action.Name.BUY:
					var unit = await (AETerrain.array[pos.x][pos.y] as Castle).Buy(actions);
					result = endTurn.IsCancellationRequested ? ResultOnAction.FAILED : unit ? ResultOnAction.SUCCESSFULED : ResultOnAction.CANCELED;
					break;

				case Action.Name.HEAL:
					if (await Play(actions, new HealAction(pos)))
					{
						await (this as Wisp).Heal();
						result = ResultOnAction.SUCCESSFULED;
					}
					else result = ResultOnAction.FAILED;
					break;

				case Action.Name.MOVE:
					{
						isMoving = true;
						var target = await Move(actions);
						isMoving = false;
						result = endTurn.IsCancellationRequested ? ResultOnAction.FAILED : target != null ? ResultOnAction.SUCCESSFULED : ResultOnAction.CANCELED;
						break;
					}

				case Action.Name.OCCUPY:
					if (await Play(actions, new OccupyAction(pos)))
					{
						(this as IOccupier).Occupy();
						result = ResultOnAction.SUCCESSFULED;
					}
					else result = ResultOnAction.FAILED;
					break;

				case Action.Name.RAISE:
					{
						var skeleton = await (this as Sorceress).Raise(actions);
						result = endTurn.IsCancellationRequested ? ResultOnAction.FAILED : skeleton ? ResultOnAction.SUCCESSFULED : ResultOnAction.CANCELED;
						break;
					}
			}

			return result;
		}


		protected static async Task<bool> Play(List<Action> actionList, Action thisAction)
		{
			actionList.Add(thisAction);
			verifiedActions = null;
			Battle.instance.player.Play(actionList);
			var token = Battle.endTurnCancel;
			while (verifiedActions == null && !token.IsCancellationRequested) await Task.Delay(1);
			return verifiedActions?.Contains(thisAction) == true;
		}


		public virtual async void OnClick(Vector3Int index)
		{
			var actions = new List<Action>();
			var result = await R.input.Wait(OnAction(actions));
			if (result == ResultOnAction.SUCCESSFULED) R.SendEventAndReport(actions);
		}


		public override string ToString() =>
			$"Unit: name= {name}, army.color= {army.color}, aPos= {transform.position.WorldToArray()}, health= {health}, minAttack= {minAttack}, maxAttack= {maxAttack}, defend= {defend}, moveRange= {moveRange}," +
			$" cost= {cost}, increase_exp_speed= {increase_exp_speed}, experience= {experience}," +
			$" poisonTurn= {poisonTurn}, powerTurn= {powerTurn}, levelName= {levelName}, (verifiedActions !=null)= {verifiedActions != null}, isUsed= {isUsed}, isOnHealingTerrain= {isOnHealingTerrain}," +
			$" isSleep= {isSleep}, isMoving= {isMoving}\n";


		// ========================  SAVED DATA  ===============================


		[Serializable]
		public abstract class Data : ILoadable
		{
			public abstract Name name { get; }
			public Vector3Int pos;
			public Army.Color armyColor;
			public float minAttack, maxAttack, defend;
			public int moveRange, cost;
			public float increase_exp_speed, experience, health;
			public int poisonTurn, powerTurn;
			public bool isSleep;


			protected static byte[] mSerialize(object obj)
			{
				var unit = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					w.Write((byte)unit.name);
					w.Write(unit.pos.x); w.Write(unit.pos.y);
					w.Write((byte)unit.armyColor);
					w.Write(unit.minAttack); w.Write(unit.maxAttack); w.Write(unit.defend);
					w.Write(unit.moveRange); w.Write(unit.cost);
					w.Write(unit.increase_exp_speed); w.Write(unit.experience); w.Write(unit.health);
					w.Write(unit.poisonTurn); w.Write(unit.powerTurn);
					w.Write(unit.isSleep);

					return m.ToArray();
				}
			}


			protected static Data mDeSerialize(byte[] data)
			{
				Data unit = null;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					var name = (Name)r.ReadByte();
					switch (name)
					{
						case Name.ARCHER: unit = new Archer.Data(); break;
						case Name.CATAPULT: unit = new Catapult.Data(); break;
						case Name.CRYSTAL: unit = new Crystal.Data(); break;
						case Name.DEMONLORD: unit = new DemonLord.Data(); break;
						case Name.DIREWOLF: unit = new DireWolf.Data(); break;
						case Name.DRAGON: unit = new Dragon.Data(); break;
						case Name.ELEMENTAL: unit = new Elemental.Data(); break;
						case Name.GALAMAR: unit = new Galamar.Data(); break;
						case Name.GOLEM: unit = new Golem.Data(); break;
						case Name.SAETH: unit = new Saeth.Data(); break;
						case Name.SKELETON: unit = new Skeleton.Data(); break;
						case Name.SOLDIER: unit = new Soldier.Data(); break;
						case Name.SORCERESS: unit = new Sorceress.Data(); break;
						case Name.VALADORN: unit = new Valadorn.Data(); break;
						case Name.WISP: unit = new Wisp.Data(); break;
					}

					unit.pos = new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0);
					unit.armyColor = (Army.Color)r.ReadByte();
					unit.minAttack = r.ReadSingle(); unit.maxAttack = r.ReadSingle(); unit.defend = r.ReadSingle();
					unit.moveRange = r.ReadInt32(); unit.cost = r.ReadInt32();
					unit.increase_exp_speed = r.ReadSingle(); unit.experience = r.ReadSingle(); unit.health = r.ReadSingle();
					unit.poisonTurn = r.ReadInt32(); unit.powerTurn = r.ReadInt32();
					unit.isSleep = r.ReadBoolean();

					return unit;
				}
			}


			public static byte[] Serialize(object obj)
			{
				switch ((obj as Data).name)
				{
					case Name.ARCHER: return Archer.Data.Serialize(obj);
					case Name.CATAPULT: return Catapult.Data.Serialize(obj);
					case Name.CRYSTAL: return Crystal.Data.Serialize(obj);
					case Name.DEMONLORD: return DemonLord.Data.Serialize(obj);
					case Name.DIREWOLF: return DireWolf.Data.Serialize(obj);
					case Name.DRAGON: return Dragon.Data.Serialize(obj);
					case Name.ELEMENTAL: return Elemental.Data.Serialize(obj);
					case Name.GALAMAR: return Galamar.Data.Serialize(obj);
					case Name.GOLEM: return Golem.Data.Serialize(obj);
					case Name.SAETH: return Saeth.Data.Serialize(obj);
					case Name.SKELETON: return Skeleton.Data.Serialize(obj);
					case Name.SOLDIER: return Soldier.Data.Serialize(obj);
					case Name.SORCERESS: return Sorceress.Data.Serialize(obj);
					case Name.VALADORN: return Valadorn.Data.Serialize(obj);
					case Name.WISP: return Wisp.Data.Serialize(obj);

					default: throw new Exception();
				}
			}


			public static Data DeSerialize(byte[] data)
			{
				Name name;
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					name = (Name)r.ReadByte();
				}

				switch (name)
				{
					case Name.ARCHER: return Archer.Data.DeSerialize(data);
					case Name.CATAPULT: return Catapult.Data.DeSerialize(data);
					case Name.CRYSTAL: return Crystal.Data.DeSerialize(data);
					case Name.DEMONLORD: return DemonLord.Data.DeSerialize(data);
					case Name.DIREWOLF: return DireWolf.Data.DeSerialize(data);
					case Name.DRAGON: return Dragon.Data.DeSerialize(data);
					case Name.ELEMENTAL: return Elemental.Data.DeSerialize(data);
					case Name.GALAMAR: return Galamar.Data.DeSerialize(data);
					case Name.GOLEM: return Golem.Data.DeSerialize(data);
					case Name.SAETH: return Saeth.Data.DeSerialize(data);
					case Name.SKELETON: return Skeleton.Data.DeSerialize(data);
					case Name.SOLDIER: return Soldier.Data.DeSerialize(data);
					case Name.SORCERESS: return Sorceress.Data.DeSerialize(data);
					case Name.VALADORN: return Valadorn.Data.DeSerialize(data);
					case Name.WISP: return Wisp.Data.DeSerialize(data);

					default: throw new Exception();
				}
			}


			protected Data(Unit unit)
			{
				pos = unit.transform.position.WorldToArray();
				armyColor = unit.army.color;
				minAttack = unit.minAttack; maxAttack = unit.maxAttack; defend = unit.defend;
				moveRange = unit.moveRange; cost = unit.cost;
				increase_exp_speed = unit.increase_exp_speed; experience = unit.experience; health = unit.health;
				poisonTurn = unit.poisonTurn; powerTurn = unit.powerTurn;
				isSleep = unit.isSleep;
			}


			protected Data() { }


			public virtual object Load(bool use = true)
			{
				var unit = New(name, Army.dict[armyColor], pos.ArrayToWorld());
				Set(unit);
				if (use) unit.Use();
				return unit;
			}


			protected virtual void Set(Unit unit)
			{
				unit.minAttack = minAttack; unit.maxAttack = maxAttack; unit.defend = defend;
				unit._moveRange = moveRange; unit._cost = cost;
				unit._increase_exp_speed = increase_exp_speed; unit.experience = experience; unit.health = health;
				if ((unit.poisonTurn = poisonTurn) > 0) unit.poisonUI.SetActive(true);
				if ((unit.powerTurn = powerTurn) > 0) unit.powerUI.SetActive(true);
				unit.isSleep = isSleep;
			}


			public static Data Save(Unit unit)
			{
				switch (unit.name)
				{
					case Name.ARCHER: return new Archer.Data(unit as Archer);
					case Name.CATAPULT: return new Catapult.Data(unit as Catapult);
					case Name.CRYSTAL: return new Crystal.Data(unit as Crystal);
					case Name.DEMONLORD: return new DemonLord.Data(unit as DemonLord);
					case Name.DIREWOLF: return new DireWolf.Data(unit as DireWolf);
					case Name.DRAGON: return new Dragon.Data(unit as Dragon);
					case Name.ELEMENTAL: return new Elemental.Data(unit as Elemental);
					case Name.GALAMAR: return new Galamar.Data(unit as Galamar);
					case Name.GOLEM: return new Golem.Data(unit as Golem);
					case Name.SAETH: return new Saeth.Data(unit as Saeth);
					case Name.SKELETON: return new Skeleton.Data(unit as Skeleton);
					case Name.SOLDIER: return new Soldier.Data(unit as Soldier);
					case Name.SORCERESS: return new Sorceress.Data(unit as Sorceress);
					case Name.VALADORN: return new Valadorn.Data(unit as Valadorn);
					case Name.WISP: return new Wisp.Data(unit as Wisp);

					default: throw new Exception();
				}
			}
		}
	}



	public interface IOccupier
	{
		bool CanOccupy(AETerrain terrain);

		void Occupy();
	}
}
