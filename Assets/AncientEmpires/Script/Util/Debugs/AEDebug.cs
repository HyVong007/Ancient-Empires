using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using AncientEmpires.Terrains;
using AncientEmpires.Units;
using AncientEmpires.AI;


namespace AncientEmpires.Util.Debugs
{
	public class AEDebug : MonoBehaviour
	{
		public bool DEBUG =
#if DEBUG
		true;
#else
		false;
#endif

		private static AEDebug instance;
		//[SerializeField] private uREPL.Main uReplMain;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }
			DontDestroyOnLoad(gameObject);
		}


		private void OnEnable()
		{
			Battle.onStart += _OnStart;
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneExited;
			AEInput.onClick += OnClick;
		}


		private void OnDisable()
		{
			Battle.onStart -= _OnStart;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneUnloaded -= OnSceneExited;
			AEInput.onClick -= OnClick;
		}


		private bool gameStart;

		private void _OnStart()
		{
			gameStart = true;
		}


		private readonly UnityAction<Scene, LoadSceneMode> OnSceneLoaded = (Scene scene, LoadSceneMode mode) =>
		{

		};


		private readonly UnityAction<Scene> OnSceneExited = (Scene scene) =>
		{

		};


		private readonly System.Action<Vector3Int> OnClick = (Vector3Int pos) =>
		{
			if (!instance.gameStart) return;
			if (pos.x < 0 || pos.x >= Unit.array.Length || pos.y < 0 || pos.y >= Unit.array[0].Length) return;
			print("\nAE Debug: Turn= " + Battle.instance.turn);
			var unit = Unit.array[pos.x][pos.y];
			var terrain = AETerrain.array[pos.x][pos.y];
			var tombstone = Tombstone.array[pos.x][pos.y];
			print(terrain);
			if (tombstone) print(tombstone);
			if (unit) print(unit);
		};


		[IngameDebugConsole.ConsoleMethod("battle", "")]
		public static void ShowBattleInfo()
		{
			print(Battle.instance);
		}


		[IngameDebugConsole.ConsoleMethod("army", "")]
		public static void ShowAllArmyInfo()
		{
			print("\nAE Debug: Turn= " + Battle.instance.turn);
			foreach (var army in Army.dict.Values) print(army);
		}


		[IngameDebugConsole.ConsoleMethod("ai", "")]
		public static void ShowAIInfo()
		{
			print("\nAE Debug: Turn= " + Battle.instance.turn);
			foreach (var ai in AIPlayer.dict.Values) if (ai) print(ai);
		}


		[IngameDebugConsole.ConsoleMethod("urepl", "")]
		public static void SetUREPLActive()
		{
			//instance.uReplMain.gameObject.SetActive(!instance.uReplMain.gameObject.activeSelf);
		}


		[IngameDebugConsole.ConsoleMethod("load", "")]
		public static async void Load()
		{
			var data = new GameData(true);
			await data.Load();
			print("Loaded !");
		}
	}
}
