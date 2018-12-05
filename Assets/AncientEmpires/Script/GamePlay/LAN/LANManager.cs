using System.Collections.Generic;
using UnityEngine;


namespace AncientEmpires.GamePlay.LAN
{
	public class LANManager : MonoBehaviour, ITurnbasePlayer
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
