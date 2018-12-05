using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Units;
using System.Collections.Generic;
using AncientEmpires.AI;


namespace AncientEmpires
{
	public class Battle : MonoBehaviour, ITurnbaseListener
	{
		public static event System.Action onReset, onStart;

		public int turn
		{
			get { return _turn; }

			private set
			{
				_turn = value;
				R.info.armyUI.turn.text = _turn.ToString();
			}
		}
		private int _turn;

		public Army army => nextArmyGenerator.Current;

		private readonly IEnumerator<Army> nextArmyGenerator = Army.NextArmyGenerator();

		public Transform neutralTerrainAnchor => _neutralTerrainAnchor;

		public Transform armyAnchor => _armyAnchor;

		public static Battle instance { get; private set; }

		[SerializeField] private Transform _neutralTerrainAnchor, _armyAnchor;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance)
			{
				Destroy(gameObject);
				return;
			}

			onReset();
		}


		private void Start()
		{
			var map = Config.instance.map;
			var size = new Vector2Int(map.terrainArray.Length, map.terrainArray[0].Length);

			// check to load data
			if (GameData.dataToLoad != null)
			{
				var data = GameData.dataToLoad;
				for (int x = 0; x < size.x; ++x)
					for (int y = 0; y < size.y; ++y)
					{
						data.tombstones[x][y]?.Load();
						data.terrains[x][y].Load();
						data.units[x][y]?.Load();
					}
			}
			else
			{
				var index = new Vector3Int();
				for (index.x = 0; index.x < size.x; ++index.x)
					for (index.y = 0; index.y < size.y; ++index.y)
					{
						var wPos = index.ArrayToWorld();
						AETerrain.DeSerialize(map.terrainArray[index.x][index.y], wPos);
						Unit.DeSerialize(map.unitArray[index.x][index.y], wPos);
					}
			}
		}


		// =======================================================================


		private bool gameStart;

		private void Update()
		{
			if (!gameStart)
			{
				gameStart = true;
				onStart();

				if (GameData.dataToLoad != null)
				{
					var data = GameData.dataToLoad.battle;
					turn = data.turn;
					while (army?.color != data.armyColor) nextArmyGenerator.MoveNext();
					GameData.dataToLoad = null;
				}
				else player.Report(TurnEvent.DONE_INIT);
			}
		}


		// ===============  TURN BASE EVENTS  ==================================


		public static event System.Action onTurnBegin;
		public static event System.Action onTurnComplete;
		public static event System.Action onTimeEnd;
		public static event Action<IReadOnlyList<Action>> onPlayerAction;
		public ITurnbasePlayer player;
		private static CancellationTokenSource endTurnCancelSource = new CancellationTokenSource();

		public static CancellationToken endTurnCancel { get; private set; } = endTurnCancelSource.Token;


		public async void OnTurnBegin()
		{
			++turn;
			nextArmyGenerator.MoveNext();
			R.input.lockInput = army.type != Army.Type.LOCAL;
			endTurnCancelSource = new CancellationTokenSource();
			endTurnCancel = endTurnCancelSource.Token;

			onTurnBegin?.Invoke();
			army.OnTurnBegin();
			foreach (var ai in AIPlayer.dict.Values) ai?.OnTurnBegin();
			while (!R.input.isAllTaskCompleted) await Task.Delay(1);
			player.Report(TurnEvent.TURN_BEGIN);
		}


		public void OnTurnComplete()
		{
			onTurnComplete?.Invoke();
			army.OnTurnComplete();
			foreach (var ai in AIPlayer.dict.Values) ai?.OnTurnComplete();
			// Battle jobs

			player.Report(TurnEvent.TURN_COMPLETE);
		}


		public async void OnTimeEnd()
		{
			R.input.lockInput = true;
			endTurnCancelSource.Cancel();
			onTimeEnd?.Invoke();
			army.OnTimeEnd();
			foreach (var ai in AIPlayer.dict.Values) ai?.OnTimeEnd();

			// Battle jobs
			R.info.HideAllUnitUI();

			while (!R.input.isAllTaskCompleted) await Task.Delay(1);
			player.Report(TurnEvent.TIME_END);
		}


		public void OnPlayerAction(IReadOnlyList<Action> actions)
		{
			// Battle jobs

			army.OnPlayerAction(actions);
			foreach (var ai in AIPlayer.dict.Values) ai?.OnPlayerAction(actions);
			onPlayerAction?.Invoke(actions);
		}


		public override string ToString() =>
			$"Battle: turn= {turn}, army.color= {army.color}\n";


		// ============================  DATA  ===================================


		[Serializable]
		public struct Data
		{
			public int turn;
			public Army.Color armyColor;


			public static byte[] Serialize(object obj)
			{
				var battle = (Data)obj;
				using (var m = new MemoryStream())
				using (var w = new BinaryWriter(m))
				{
					w.Write(battle.turn);
					w.Write((byte)battle.armyColor);

					return m.ToArray();
				}
			}


			public static Data DeSerialize(byte[] data)
			{
				var battle = new Data();
				using (var m = new MemoryStream(data))
				using (var r = new BinaryReader(m))
				{
					battle.turn = r.ReadInt32();
					battle.armyColor = (Army.Color)r.ReadByte();

					return battle;
				}
			}


			public Data(Battle battle)
			{
				turn = battle.turn;
				armyColor = battle.army.color;
			}
		}
	}



	public interface IUsable
	{
		void Use();
	}



	public interface IClickHandler
	{
		void OnClick(Vector3Int index);
	}
}
