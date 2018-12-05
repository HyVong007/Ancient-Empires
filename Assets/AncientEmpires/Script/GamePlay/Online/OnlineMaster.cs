using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;


namespace AncientEmpires.GamePlay.Online
{
	public class OnlineMaster : MonoBehaviourPunCallbacks, IOnEventCallback
	{
		public static OnlineMaster instance { get; private set; }


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }

			DontDestroyOnLoad(this);
		}


		private new void OnEnable()
		{
			base.OnEnable();
			SceneManager.sceneUnloaded += mDestroy;
		}


		private new void OnDisable()
		{
			base.OnDisable();
			SceneManager.sceneUnloaded -= mDestroy;
		}


		private void mDestroy(Scene scene)
		{
			if (scene.buildIndex == R.SceneBuildIndex.BATTLE)
			{
				Destroy(gameObject);
			}
		}


		private void Start()
		{
			// Nếu RoomData.Turn > 0 : Restore Safepoint !
			PhotonNetwork.LoadLevel(R.SceneBuildIndex.BATTLE);
		}


		private readonly RaiseEventOptions evOpt = new RaiseEventOptions()
		{
			CachingOption = EventCaching.AddToRoomCache,
			Receivers = ReceiverGroup.All
		};

		private int reportCount;

		public void OnEvent(EventData photonEvent)
		{
			if (photonEvent.Code > 199) return;

			var ht = photonEvent.CustomData as Hashtable;
			int senderTurn = ht != null ? (int)ht[OnlineConstants.KEY_TURN] : -1;
			var actionData = ht?.ContainsKey(OnlineConstants.KEY_ACTION_DATA) == true ? (IReadOnlyList<Action>)ht[OnlineConstants.KEY_ACTION_DATA] : null;

			// Đồng bộ Turn và Time

			switch (photonEvent.Code)
			{
				case OnlineConstants.EV_REPORT_INIT:
					if (reportCount == 0) ++reportCount;
					else
					{
						reportCount = 0;
						print("Master: EV_RESPONSE_TURN_BEGIN");
						PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_TURN_BEGIN, null, evOpt, SendOptions.SendReliable);
					}
					break;

				case OnlineConstants.EV_REPORT_TURN_BEGIN:
					if (reportCount == 0) ++reportCount;
					else
					{
						reportCount = 0;
						time = Time.time + Config.instance.turnTime;
						print("Master: Receive All EV_REPORT_TURN_BEGIN");
					}
					break;

				case OnlineConstants.EV_REQUEST_PLAY_ACTION:
					PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_PLAY_ACTION, photonEvent.CustomData, evOpt, SendOptions.SendReliable);
					break;

				case OnlineConstants.EV_REPORT_PLAY_ACTION:
					if (reportCount == 0) ++reportCount;
					else
					{
						reportCount = 0;
						print("Master: Receive All EV_REPORT_PLAY_ACTION");
					}
					break;

				case OnlineConstants.EV_REQUEST_TURN_COMPLETE:
					PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_TURN_COMPLETE, null, evOpt, SendOptions.SendReliable);
					break;

				case OnlineConstants.EV_REPORT_TURN_COMPLETE:
					if (reportCount == 0) ++reportCount;
					else
					{
						reportCount = 0;
						print("Master: Receive All EV_REPORT_TURN_COMPLETE and Begin Turn");
						PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_TURN_BEGIN, null, evOpt, SendOptions.SendReliable);
					}
					break;

				case OnlineConstants.EV_REPORT_TURN_TIME_END:
					if (reportCount == 0) ++reportCount;
					else
					{
						reportCount = 0;
						print("Master: EV_RESPONSE_TURN_BEGIN");
						PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_TURN_BEGIN, null, evOpt, SendOptions.SendReliable);
					}
					break;
			}
		}


		private float? time;

		private void Update()
		{
			if (time != null && Time.time >= time)
			{
				time = null;
				PhotonNetwork.RaiseEvent(OnlineConstants.EV_RESPONSE_TURN_TIME_END, null, evOpt, SendOptions.SendReliable);
			}
		}
	}
}
