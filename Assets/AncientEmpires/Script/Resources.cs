using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AncientEmpires.Util;
using AncientEmpires.Units;
using AncientEmpires.Terrains;


namespace AncientEmpires
{
	public class Resources : MonoBehaviour
	{
		public AssetManager asset;

		private static Resources instance;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance)
			{
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);
			R._Init(this);
		}
	}



	public static class R
	{
		public static AssetManager asset;

		public static Camera camera;

		public static ObjectPool pool;

		public static CameraController camCtrl;

		public static Shop shop;

		public static InfoUI info;

		public static AEInput input;


		internal static void _Init(Resources r)
		{
			asset = r.asset;
		}


		public static async Task Move(GameObject obj, Vector3 stop, float speed, System.Action callback = null)
		{
			var transform = obj.transform;
			while (transform.position != stop)
			{
				transform.position = Vector3.MoveTowards(transform.position, stop, speed);
				callback?.Invoke();
				await Task.Delay(1);
			}
			transform.position = stop;
		}


		public static async Task WaitToHide(GameObject obj, float delaySeconds, CancellationToken token = default(CancellationToken))
		{
			await Task.Delay(Mathf.CeilToInt(delaySeconds * 1000));
			if (token.IsCancellationRequested) return;
			obj?.SetActive(false);
		}


		public static void DrawMovePaths(Vector3 start, UnitAlgorithm.Target target)
		{
			foreach (Vector3 path in target.paths)
			{
				var center = start + path * 0.5f;
				pool.Show(path.x == 0f ? ObjectPool.POOL_KEY.VERTICAL_PATH : ObjectPool.POOL_KEY.HORIZONTAL_PATH, center);
				start += path;
			}
		}


		public static void DrawMovePaths()
		{
			pool.Hide(ObjectPool.POOL_KEY.HORIZONTAL_PATH);
			pool.Hide(ObjectPool.POOL_KEY.VERTICAL_PATH);
		}


		public static void DrawActionButtons(Vector3 pos, IReadOnlyList<Action.Name> list)
		{
			if (list == null)
			{
				foreach (Action.Name action in Enum.GetValues(typeof(Action.Name))) pool.buttons[action].SetActive(false);
				pool.buttonGroup.SetActive(false);
				return;
			}

			pool.buttonGroup.SetActive(true);
			float a = camera.orthographicSize * camera.aspect;
			Vector3 left = new Vector3(camera.transform.position.x - a, pos.y);
			Vector3 right = new Vector3(camera.transform.position.x + a, pos.y);
			if ((pos - left).sqrMagnitude >= pool.buttonGroupWidthPow2)
			{
				// place button to the left of this pos
				pool.buttonGroup.transform.position = new Vector3(pos.x - pool.buttonGroupWidth, pos.y);
			}
			else
			{
				// place button to the right of this pos
				pool.buttonGroup.transform.position = pos;
			}

			foreach (var action in list) pool.buttons[action].SetActive(true);
		}


		// ===========================  Action Events  ========================================


		public static event Action<Unit> onOccupy;
		public static event Action<Wisp> onHeal;
		public static event Action<Sorceress, Skeleton> onRaise;
		public static event Action<Unit, Vector3Int, float, float> onAttack;
		public static event Action<Unit, Vector3Int> onMove;
		public static event Action<Castle, Unit.Name> onBuy;
		public static Action<Unit> onDie;


		internal static void SendEventAndReport(IReadOnlyList<Action> actions)
		{
			foreach (var action in actions)
				switch (action.name)
				{
					case Action.Name.OCCUPY:
						onOccupy?.Invoke(Unit.array[action.currentPos.x][action.currentPos.y]);
						break;

					case Action.Name.HEAL:
						onHeal?.Invoke(Unit.array[action.currentPos.x][action.currentPos.y] as Wisp);
						break;

					case Action.Name.RAISE:
						var raiseAction = action as RaiseAction;
						onRaise?.Invoke(Unit.array[raiseAction.currentPos.x][raiseAction.currentPos.y] as Sorceress, Unit.array[raiseAction.targetPos.x][raiseAction.targetPos.y] as Skeleton);
						break;

					case Action.Name.ATTACK:
						var attackAction = action as AttackAction;
						onAttack?.Invoke(Unit.array[attackAction.currentPos.x][attackAction.currentPos.y], attackAction.targetPos, attackAction.enemyDeltaHealth, attackAction.thisDeltaHealth);
						break;

					case Action.Name.MOVE:
						var moveAction = action as MoveAction;
						onMove?.Invoke(Unit.array[moveAction.currentPos.x][moveAction.currentPos.y], moveAction.targetPos);
						break;

					case Action.Name.BUY:
						var buyAction = action as BuyAction;
						onBuy?.Invoke(AETerrain.array[buyAction.currentPos.x][buyAction.currentPos.y] as Castle, buyAction.unitName);
						break;
				}

			Battle.instance.player.Report(actions);
		}


		public static class SceneBuildIndex
		{
			public const int MAIN_MENU = 0, SKIRMISH_CONFIG_MENU = 1,
				BATTLE = 2, ONLINE_CONFIG_MENU = 3;
		}
	}
}

