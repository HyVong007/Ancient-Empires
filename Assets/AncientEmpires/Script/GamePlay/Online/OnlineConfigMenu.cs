using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;


namespace AncientEmpires.GamePlay.Online
{
	public class OnlineConfigMenu : MonoBehaviourPunCallbacks
	{
		private void Start()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.ConnectUsingSettings();
		}


		public override void OnConnectedToMaster()
		{
			PhotonNetwork.JoinOrCreateRoom("tam", null, null);
		}


		public override void OnCreatedRoom()
		{
			var cfg = Config.instance;
			cfg.map = R.asset.maps.defaultMaps[0];
			cfg.money = 100000;
			cfg.turnTime = 10;
			cfg.armyInfos = new Config.ArmyInfo[]
			{
				new Config.ArmyInfo()
				{
					color=Army.Color.BLUE,
					group=Army.Group.A,
					name="BLUE"
				},
				new Config.ArmyInfo()
				{
					color=Army.Color.RED,
					group=Army.Group.B,
					name="RED"
				}
			};
			Config.instance = cfg;
		}


		public override void OnPlayerEnteredRoom(Player newPlayer)
		{
			(Config.instance.playModeConfig as OnlineConfig).players = new Dictionary<int, Army.Color>()
			{
				[PhotonNetwork.MasterClient.ActorNumber] = Army.Color.BLUE,
				[newPlayer.ActorNumber] = Army.Color.RED
			};

			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable()
			{
				[OnlineConstants.KEY_CONFIG] = Config.instance
			});
		}


		public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			if (propertiesThatChanged.ContainsKey(OnlineConstants.KEY_CONFIG))
			{
				Config.instance = (Config)propertiesThatChanged[OnlineConstants.KEY_CONFIG];
				DontDestroyOnLoad(Config.instance.map);
				if (PhotonNetwork.IsMasterClient) Instantiate(R.asset.prefab.onlineMaster);
				return;
			}
		}
	}
}
