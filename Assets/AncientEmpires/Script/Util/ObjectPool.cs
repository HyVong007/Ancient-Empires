using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Threading.Tasks;


namespace AncientEmpires.Util
{
	public class ObjectPool : MonoBehaviour
	{
		[SerializeField] private PoolKey_GameObject poolPrefabs;
		public ActionName_GameObject buttons;
		public GameObject buttonGroup;
		[NonSerialized] public float buttonGroupWidthPow2;
		[NonSerialized] public float buttonGroupWidth;
		[SerializeField] private Transform poolAnchor;
		private readonly Dictionary<POOL_KEY, Stack<GameObject>> usedPool = new Dictionary<POOL_KEY, Stack<GameObject>>(), freePool = new Dictionary<POOL_KEY, Stack<GameObject>>();

		public static ObjectPool instance { get; private set; }

		public GameObject selectRect, selectCircle, targetRect;

		public enum POOL_KEY
		{
			YELLOW_ALPHA, RED_ALPHA, GREEN_ALPHA, MOVING_SMOKE, EXPLOSION_SMOKE,
			BLUE_FIRE, YELLOW_FIRE, VERTICAL_PATH, HORIZONTAL_PATH
		}


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance)
			{
				Destroy(gameObject);
				return;
			}
			R.pool = this;

			foreach (POOL_KEY key in Enum.GetValues(typeof(POOL_KEY)))
			{
				usedPool[key] = new Stack<GameObject>();
				freePool[key] = new Stack<GameObject>();
			}

			var sprite = buttonGroup.GetComponent<SpriteRenderer>().sprite;
			buttonGroupWidth = sprite.texture.width / sprite.pixelsPerUnit;
			buttonGroupWidthPow2 = buttonGroupWidth * buttonGroupWidth;
		}


		// =======================================================================


		public GameObject Show(POOL_KEY key, Vector3? wPos = null)
		{
			var _freePool = freePool[key];
			var _usedPool = usedPool[key];
			GameObject obj = null;
			if (_freePool.Count > 0) obj = _freePool.Pop();
			else obj = Instantiate(poolPrefabs[key], poolAnchor);

			_usedPool.Push(obj);
			if (wPos != null) obj.transform.position = wPos.Value;
			obj.SetActive(true);
			return obj;
		}


		public void Hide(POOL_KEY key, GameObject obj)
		{
			usedPool[key].Pop();
			freePool[key].Push(obj);
			obj.SetActive(false);
			obj.transform.parent = poolAnchor;
		}


		public void Hide(POOL_KEY key)
		{
			var _freePool = freePool[key];
			var _usedPool = usedPool[key];

			while (_usedPool.Count > 0)
			{
				var obj = _usedPool.Pop();
				obj.SetActive(false);
				obj.transform.parent = poolAnchor;
				_freePool.Push(obj);
			}
		}


		public void Hide()
		{
			foreach (POOL_KEY key in Enum.GetValues(typeof(POOL_KEY))) Hide(key);
		}


		// ======================  BUTTON EVENTS  ==================================


		private Action.Name? buttonAction;

		public async Task<Action.Name?> WaitForButton()
		{
			buttonAction = null;
			R.input.click = null;
			var token = Battle.endTurnCancel;
			while (!token.IsCancellationRequested && buttonAction == null && R.input.click == null) await Task.Delay(1);
			return buttonAction;
		}


		public void OnAttackButttonClick(BaseEventData data) => buttonAction = Action.Name.ATTACK;

		public void OnMoveButttonClick(BaseEventData data) => buttonAction = Action.Name.MOVE;

		public void OnHealButttonClick(BaseEventData data) => buttonAction = Action.Name.HEAL;

		public void OnRaiseButttonClick(BaseEventData data) => buttonAction = Action.Name.RAISE;

		public void OnOccupyButttonClick(BaseEventData data) => buttonAction = Action.Name.OCCUPY;

		public void OnBuyButttonClick(BaseEventData data) => buttonAction = Action.Name.BUY;
	}
}
