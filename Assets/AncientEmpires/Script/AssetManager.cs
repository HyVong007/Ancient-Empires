using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using AncientEmpires.Terrains;
using AncientEmpires.AI;
using AncientEmpires.GamePlay.Campaigns;
using AncientEmpires.GamePlay.LAN;
using AncientEmpires.GamePlay.Online;
using AncientEmpires.GamePlay.Skirmishes;


namespace AncientEmpires
{
	[CreateAssetMenu]
	public class AssetManager : ScriptableObject
	{
		[Serializable]
		public struct Maps
		{
			public Map emptyMap;
			public Map[] defaultMaps;
		}
		public Maps maps;

		[Serializable]
		public struct Prefab
		{
			public House house;
			public Castle castle;
			public NormalTerrain normalTerrain;
			public Tombstone tombstone;
			public UnitName_Unit units;
			public Army army;
			public AIPlayer aiPlayer;
			public CampaignManager campaignManager;
			public SkirmishManager skirmishManager;
			public LANManager lanManager;
			public OnlineMaster onlineMaster;
			public OnlineClient onlineClient;
		}
		public Prefab prefab;

		[Serializable]
		public struct Tiles
		{
			public Tile[] terrains, units;
		}
		public Tiles tiles;

		[Serializable]
		public struct Sprite
		{
			public UnitName_ArmyColor_Sprite units;
		}
		public Sprite sprite;
	}
}
