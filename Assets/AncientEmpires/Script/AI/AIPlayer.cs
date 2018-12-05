using System.Collections.Generic;
using UnityEngine;


namespace AncientEmpires.AI
{
	public class AIPlayer : MonoBehaviour, ITurnbaseListener
	{
		public static IReadOnlyDictionary<Army, AIPlayer> dict { get; private set; }

		public Army army { get; private set; }


		static AIPlayer()
		{
			Battle.onStart += () =>
			  {
				  var tmp = new Dictionary<Army, AIPlayer>();
				  foreach (var army in Army.dict.Values)
					  if (army?.type != Army.Type.AI) { if (army) tmp[army] = null; }
					  else
					  {
						  var ai = Instantiate(R.asset.prefab.aiPlayer, army.transform);
						  ai.army = army;
						  tmp[army] = ai;
					  }

				  dict = tmp;
			  };
		}


		// =======================   TURN BASE EVENTS  ==========================


		public void OnTurnBegin()
		{
			if (army == Battle.instance.army) Battle.instance.player.CompleteTurn();
		}


		public void OnTurnComplete()
		{

		}


		public void OnPlayerAction(IReadOnlyList<Action> actions)
		{

		}


		public void OnTimeEnd()
		{

		}


		// =======================================================================


		public override string ToString() => $"AI Player: army.color= {army.color}";
	}
}
