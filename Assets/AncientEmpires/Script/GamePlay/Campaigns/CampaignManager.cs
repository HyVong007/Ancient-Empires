using System.Collections.Generic;
using UnityEngine;


namespace AncientEmpires.GamePlay.Campaigns
{
	public class CampaignManager : MonoBehaviour, ITurnbasePlayer
	{
		private Battle battle;







		public void CompleteTurn()
		{
			throw new System.NotImplementedException();
		}


		public void Play(IReadOnlyList<Action> actions)
		{
			throw new System.NotImplementedException();
		}

		public void Report(IReadOnlyList<Action> actions)
		{
			throw new System.NotImplementedException();
		}


		public void Report(TurnEvent ev)
		{
			throw new System.NotImplementedException();
		}
	}
}
