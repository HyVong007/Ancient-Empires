using ExitGames.Client.Photon;
using System.Collections.Generic;


namespace AncientEmpires.GamePlay.Online
{
	public static class OnlineConstants
	{
		static OnlineConstants()
		{
			PhotonPeer.RegisterType(typeof(Action), 0, Action.Serialize, Action.DeSerialize);
			PhotonPeer.RegisterType(typeof(MoveAction), 1, Action.Serialize, MoveAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(AttackAction), 2, Action.Serialize, AttackAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(HealAction), 3, Action.Serialize, HealAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(RaiseAction), 4, Action.Serialize, RaiseAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(OccupyAction), 5, Action.Serialize, OccupyAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(BuyAction), 6, Action.Serialize, BuyAction.DeSerialize);
			PhotonPeer.RegisterType(typeof(List<Action>), 7, Action.Serialize_IReadOnlyList_Action, Action.DeSerialize_IReadOnlyList_Action);
			PhotonPeer.RegisterType(typeof(IReadOnlyList<Action>), 8, Action.Serialize_IReadOnlyList_Action, Action.DeSerialize_IReadOnlyList_Action);
		}


		public const byte
			// Requests
			EV_REQUEST_TURN_COMPLETE = 0,
			EV_REQUEST_PLAY_ACTION = 1,

			// Reports
			EV_REPORT_PLAY_ACTION = 2,
			EV_REPORT_TURN_COMPLETE = 3,
			EV_REPORT_TURN_BEGIN = 4,
			EV_REPORT_TURN_TIME_END = 5,
			EV_REPORT_INIT = 6,

			// Responses
			EV_RESPONSE_TURN_BEGIN = 7,
			EV_RESPONSE_TURN_COMPLETE = 8,
			EV_RESPONSE_TURN_TIME_END = 9,
			EV_RESPONSE_PLAY_ACTION = 10;

		// Keys
		public const string KEY_CONFIG = "AE:0",
			KEY_TURN = "AE:1",
			KEY_ACTION_DATA = "AE:2";


		public static byte GetEvReportTurnEvent(TurnEvent ev)
		{
			switch (ev)
			{
				case TurnEvent.DONE_INIT: return EV_REPORT_INIT;
				case TurnEvent.TIME_END: return EV_REPORT_TURN_TIME_END;
				case TurnEvent.TURN_BEGIN: return EV_REPORT_TURN_BEGIN;
				case TurnEvent.TURN_COMPLETE: return EV_REPORT_TURN_COMPLETE;
			}
			throw new System.Exception();
		}
	}
}
