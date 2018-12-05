using UnityEngine;
using System.Collections.Generic;
using AncientEmpires.Units;
using System.IO;


namespace AncientEmpires
{
	public interface ITurnbaseListener
	{
		void OnTurnBegin();

		void OnTurnComplete();

		void OnTimeEnd();

		void OnPlayerAction(IReadOnlyList<Action> actions);
	}



	public interface ITurnbasePlayer
	{
		void Report(IReadOnlyList<Action> actions);

		void Report(TurnEvent ev);

		void CompleteTurn();

		void Play(IReadOnlyList<Action> actions);
	}


	public enum TurnEvent
	{
		DONE_INIT, TURN_BEGIN, TURN_COMPLETE, TIME_END, LOADED
	}


	public abstract class Action
	{
		public abstract Name name { get; }

		public readonly Vector3Int currentPos;

		public enum Name
		{
			MOVE, ATTACK, HEAL, RAISE, OCCUPY, BUY
		}


		protected Action(Vector3Int currentPos)
		{
			this.currentPos = currentPos;
		}


		public override string ToString() => $"name= {name}, currentPos= {currentPos}";


		public static byte[] Serialize(object _obj)
		{
			var obj = (Action)_obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				w.Write((byte)obj.name);
				w.Write(obj.currentPos.x); w.Write(obj.currentPos.y);
				switch (obj.name)
				{
					case Name.ATTACK:
						{
							var data = obj as AttackAction;
							w.Write(data.targetPos.x); w.Write(data.targetPos.y);
							w.Write(data.enemyDeltaHealth);
							w.Write(data.thisDeltaHealth);
						}
						break;

					case Name.BUY:
						{
							var data = obj as BuyAction;
							w.Write((byte)data.unitName);
						}
						break;

					case Name.HEAL: break;

					case Name.MOVE:
						{
							var data = obj as MoveAction;
							w.Write(data.targetPos.x); w.Write(data.targetPos.y);
						}
						break;

					case Name.OCCUPY: break;

					case Name.RAISE:
						{
							var data = obj as RaiseAction;
							w.Write(data.targetPos.x); w.Write(data.targetPos.y);
						}
						break;
				}

				return m.ToArray();
			}
		}


		public static Action DeSerialize(byte[] data)
		{
			Action action = null;
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				var name = (Name)r.ReadByte();
				var currentPos = new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0);
				switch (name)
				{
					case Name.ATTACK:
						action = new AttackAction(currentPos, new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0), r.ReadSingle(), r.ReadSingle());
						break;

					case Name.BUY:
						action = new BuyAction(currentPos, (Unit.Name)r.ReadByte());
						break;

					case Name.HEAL:
						action = new HealAction(currentPos);
						break;

					case Name.MOVE:
						action = new MoveAction(currentPos, new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0));
						break;

					case Name.OCCUPY:
						action = new OccupyAction(currentPos);
						break;

					case Name.RAISE:
						action = new RaiseAction(currentPos, new Vector3Int(r.ReadInt32(), r.ReadInt32(), 0));
						break;
				}

				return action;
			}
		}


		public static byte[] Serialize_IReadOnlyList_Action(object obj)
		{
			var list = (IReadOnlyList<Action>)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				w.Write(list.Count);
				foreach (var action in list)
				{
					byte[] data = Serialize(action);
					w.Write(data.Length);
					w.Write(data);
				}

				return m.ToArray();
			}
		}


		public static IReadOnlyList<Action> DeSerialize_IReadOnlyList_Action(byte[] data)
		{
			var list = new List<Action>();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				int count = r.ReadInt32();
				for (int i = 0; i < count; ++i) list.Add(DeSerialize(r.ReadBytes(r.ReadInt32())));

				return list;
			}
		}
	}


	public class MoveAction : Action
	{
		public override Name name => Name.MOVE;

		public readonly Vector3Int targetPos;


		public MoveAction(Vector3Int currentPos, Vector3Int targetPos) : base(currentPos)
		{
			this.targetPos = targetPos;
		}


		public override bool Equals(object obj)
		{
			var m = obj as MoveAction;
			return m?.currentPos == currentPos && m?.targetPos == targetPos;
		}


		public override int GetHashCode() => base.GetHashCode();


		public override string ToString() => base.ToString() + $" ,targetPos= {targetPos}";


		public static new MoveAction DeSerialize(byte[] data) => (MoveAction)Action.DeSerialize(data);
	}


	public class AttackAction : Action
	{
		public override Name name => Name.ATTACK;

		public readonly Vector3Int targetPos;

		public readonly float enemyDeltaHealth, thisDeltaHealth;


		public AttackAction(Vector3Int currentPos, Vector3Int targetPos,
			float enemyDeltaHealth, float thisDeltaHealth) : base(currentPos)
		{
			this.targetPos = targetPos;
			this.enemyDeltaHealth = enemyDeltaHealth;
			this.thisDeltaHealth = thisDeltaHealth;
		}


		public override bool Equals(object obj)
		{
			var a = obj as AttackAction;
			return a?.currentPos == currentPos && a?.targetPos == targetPos
				&& a?.enemyDeltaHealth == enemyDeltaHealth && a?.thisDeltaHealth == thisDeltaHealth; ;
		}


		public override int GetHashCode() => base.GetHashCode();


		public override string ToString() => base.ToString() + $" ,targetPos= {targetPos}, enemyDeltaHealth= {enemyDeltaHealth}, thisDeltaHealth= {thisDeltaHealth}";


		public static new AttackAction DeSerialize(byte[] data) => (AttackAction)Action.DeSerialize(data);
	}


	public class HealAction : Action
	{
		public override Name name => Name.HEAL;


		public HealAction(Vector3Int currentPos) : base(currentPos) { }


		public override bool Equals(object obj) => (obj as HealAction)?.currentPos == currentPos;


		public override int GetHashCode() => base.GetHashCode();


		public static new HealAction DeSerialize(byte[] data) => (HealAction)Action.DeSerialize(data);
	}


	public class RaiseAction : Action
	{
		public override Name name => Name.RAISE;

		public readonly Vector3Int targetPos;


		public RaiseAction(Vector3Int currentPos, Vector3Int targetPos) : base(currentPos)
		{
			this.targetPos = targetPos;
		}


		public override bool Equals(object obj)
		{
			var r = obj as RaiseAction;
			return r?.currentPos == currentPos && r?.targetPos == targetPos;
		}


		public override int GetHashCode() => base.GetHashCode();


		public override string ToString() => base.ToString() + $" ,targetPos= {targetPos}";


		public static new RaiseAction DeSerialize(byte[] data) => (RaiseAction)Action.DeSerialize(data);
	}


	public class OccupyAction : Action
	{
		public override Name name => Name.OCCUPY;


		public OccupyAction(Vector3Int currentPos) : base(currentPos) { }


		public override bool Equals(object obj) => (obj as OccupyAction)?.currentPos == currentPos;


		public override int GetHashCode() => base.GetHashCode();


		public static new OccupyAction DeSerialize(byte[] data) => (OccupyAction)Action.DeSerialize(data);
	}


	public class BuyAction : Action
	{
		public override Name name => Name.BUY;

		public readonly Unit.Name unitName;


		public BuyAction(Vector3Int currentPos, Unit.Name unitName) : base(currentPos)
		{
			this.unitName = unitName;
		}


		public override bool Equals(object obj)
		{
			var b = obj as BuyAction;
			return b?.currentPos == currentPos && b?.name == name;
		}


		public override int GetHashCode() => base.GetHashCode();


		public override string ToString() => base.ToString() + $" ,unitName= {unitName}";


		public static new BuyAction DeSerialize(byte[] data) => (BuyAction)Action.DeSerialize(data);
	}
}
