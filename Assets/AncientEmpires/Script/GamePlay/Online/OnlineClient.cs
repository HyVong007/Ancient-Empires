using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;


namespace AncientEmpires.GamePlay.Online
{
	public class OnlineClient : MonoBehaviourPunCallbacks, IOnEventCallback, ITurnbasePlayer
	{
		public static OnlineClient instance { get; private set; }
		private Battle battle;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }
		}


		static OnlineClient()
		{
			Battle.onStart += () =>
			  {
				  var config = Config.instance;
				  if (config.playMode != Config.PlayMode.ONLINE) return;
				  instance = Instantiate(R.asset.prefab.onlineClient);
				  instance.battle = Battle.instance;
				  instance.battle.player = instance;
			  };
		}


		private readonly RaiseEventOptions evOpt = new RaiseEventOptions()
		{
			CachingOption = EventCaching.AddToRoomCache,
			Receivers = ReceiverGroup.MasterClient
		};


		public void Report(IReadOnlyList<Action> actions)
		{
			PhotonNetwork.RaiseEvent(OnlineConstants.EV_REPORT_PLAY_ACTION, new Hashtable()
			{
				[OnlineConstants.KEY_TURN] = battle.turn,
				[OnlineConstants.KEY_ACTION_DATA] = actions
			}, evOpt, SendOptions.SendReliable);
		}


		public void Report(TurnEvent ev)
		{
			PhotonNetwork.RaiseEvent(OnlineConstants.GetEvReportTurnEvent(ev), new Hashtable()
			{
				[OnlineConstants.KEY_TURN] = battle.turn
			}, evOpt, SendOptions.SendReliable);
		}


		public void CompleteTurn()
		{
			PhotonNetwork.RaiseEvent(OnlineConstants.EV_REQUEST_TURN_COMPLETE, new Hashtable()
			{
				[OnlineConstants.KEY_TURN] = battle.turn
			}, evOpt, SendOptions.SendReliable);
		}


		public void Play(IReadOnlyList<Action> actions)
		{
			PhotonNetwork.RaiseEvent(OnlineConstants.EV_REQUEST_PLAY_ACTION, new Hashtable()
			{
				[OnlineConstants.KEY_TURN] = battle.turn,
				[OnlineConstants.KEY_ACTION_DATA] = actions
			}, evOpt, SendOptions.SendReliable);
		}


		public void OnEvent(EventData photonEvent)
		{
			if (photonEvent.Code > 199) return;

			var ht = photonEvent.CustomData as Hashtable;
			int senderTurn = ht != null ? (int)ht[OnlineConstants.KEY_TURN] : -1;
			var actionData = ht?.ContainsKey(OnlineConstants.KEY_ACTION_DATA) == true ? (IReadOnlyList<Action>)ht[OnlineConstants.KEY_ACTION_DATA] : null;

			switch (photonEvent.Code)
			{
				case OnlineConstants.EV_RESPONSE_TURN_BEGIN:
					print("Client: battle.OnTurnBegin()");
					battle.OnTurnBegin();
					break;

				case OnlineConstants.EV_RESPONSE_PLAY_ACTION:
					print("Client: battle.OnPlayerAction(actionData)");
					battle.OnPlayerAction(actionData);
					break;

				case OnlineConstants.EV_RESPONSE_TURN_COMPLETE:
					print("Client: battle.OnTurnComplete()");
					battle.OnTurnComplete();
					break;

				case OnlineConstants.EV_RESPONSE_TURN_TIME_END:
					print("Client: battle.OnTimeEnd()");
					battle.OnTimeEnd();
					break;

			}
		}
	}
}
