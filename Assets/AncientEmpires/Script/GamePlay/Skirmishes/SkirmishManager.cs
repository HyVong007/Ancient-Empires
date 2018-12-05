using UnityEngine;
using System.Collections.Generic;


namespace AncientEmpires.GamePlay.Skirmishes
{
	public class SkirmishManager : MonoBehaviour, ITurnbasePlayer
	{
		private Battle battle;
		private static SkirmishManager instance;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }
		}


		static SkirmishManager()
		{
			Battle.onStart += () =>
			  {
				  var cfg = Config.instance;
				  if (cfg.playMode != Config.PlayMode.SKIRMISH) return;

				  var manager = Instantiate(R.asset.prefab.skirmishManager);
				  manager.battle = Battle.instance;
				  manager.battle.player = manager;

				  // use cfg.skirmishConfig
			  };
		}


		//  =======================================================================


		/// <summary>
		/// actions == null: CompleteTurn
		/// </summary>
		private struct Request
		{
			public readonly int turn;
			public readonly Army army;
			public readonly float time;
			public readonly IReadOnlyList<Action> actions;


			public Request(IReadOnlyList<Action> actions)
			{
				var b = Battle.instance;
				turn = b.turn;
				army = b.army;
				time = Time.time;
				this.actions = actions;
			}


			public bool TryProcess()
			{
				var b = Battle.instance;
				if (b.turn == turn && b.army == army && instance.time != null && time <= instance.time)
				{
					if (actions == null)
					{
						instance.time = null;
						b.OnTurnComplete();
					}
					else b.OnPlayerAction(actions);
					return true;
				}

				return false;
			}
		}

		private readonly Queue<Request> requestQueue = new Queue<Request>();


		public void CompleteTurn()
		{
			requestQueue.Enqueue(new Request(null));
			if (requestQueue.Count == 1) requestQueue.Peek().TryProcess();
		}


		public void Play(IReadOnlyList<Action> actions)
		{
			requestQueue.Enqueue(new Request(actions));
			if (requestQueue.Count == 1) requestQueue.Peek().TryProcess();
		}


		public void Report(TurnEvent ev)
		{
			switch (ev)
			{
				case TurnEvent.DONE_INIT: battle.OnTurnBegin(); break;
				case TurnEvent.LOADED: break;
				case TurnEvent.TIME_END: battle.OnTurnBegin(); break;
				case TurnEvent.TURN_COMPLETE:
					requestQueue.Dequeue();
					battle.OnTurnBegin(); break;

				case TurnEvent.TURN_BEGIN:
					time = Time.time + Config.instance.turnTime;
					R.info.armyUI.turnRemainTime.text = ((int)Config.instance.turnTime).ToString();
					while (requestQueue.Count > 0)
					{
						if (!requestQueue.Peek().TryProcess()) requestQueue.Dequeue();
						else break;
					}
					break;
			}
		}


		public void Report(IReadOnlyList<Action> actions)
		{
			requestQueue.Dequeue();
			while (requestQueue.Count > 0)
			{
				if (!requestQueue.Peek().TryProcess()) requestQueue.Dequeue();
				else break;
			}
		}


		private float? time;

		private void Update()
		{
			if (time != null && Time.time > time && requestQueue.Count == 0)
			{
				time = null;
				battle.OnTimeEnd();
			}
			else if (time != null) R.info.armyUI.turnRemainTime.text = ((int)(time - Time.time)).ToString();
		}
	}
}
