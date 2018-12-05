using System;
using System.IO;


namespace AncientEmpires.GamePlay.Campaigns
{
	[Serializable]
	public struct CampaignConfig
	{
		public static CampaignConfig instance;

		public Scenario scenario;

		public Difficulty difficulty;

		public enum Difficulty
		{
			EASY, MEDIUM, HARD, EXPERT
		}





		public static byte[] Serialize(object obj)
		{
			var cfg = (CampaignConfig)obj;
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				return m.ToArray();
			}
		}


		public static CampaignConfig DeSerialize(byte[] data)
		{
			var cfg = new CampaignConfig();
			using (var m = new MemoryStream(data))
			using (var r = new BinaryReader(m))
			{
				return cfg;
			}
		}
	}
}
