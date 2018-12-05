using UnityEngine;
using UnityEngine.EventSystems;
using AncientEmpires.Units;
using AncientEmpires.Terrains;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace AncientEmpires.Util
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class AEInput : MonoBehaviour, IPointerClickHandler, IDragHandler
	{
		[SerializeField] private BoxCollider2D col;
		public static AEInput instance { get; private set; }


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }

			R.input = this;
			var a = Config.instance.map.terrainArray;
			col.size = new Vector2(a.Length, a[0].Length);
		}


		private bool dragged;

		public void OnDrag(PointerEventData eventData) => dragged = true;


		public void OnPointerClick(PointerEventData data)
		{
			if (dragged) { dragged = false; return; }
			click = Input.mousePosition.ScreenToArray();
			onClick?.Invoke(click.Value);

			if (lockInput || !isAllTaskCompleted) return;
			var pos = click.Value;
			if (pos.x < 0 || pos.x >= Unit.array.Length || pos.y < 0 || pos.y >= Unit.array[0].Length) return;

			var unit = Unit.array[pos.x][pos.y];
			var terrain = AETerrain.array[pos.x][pos.y];
			R.info.HideAllUnitUI();
			Battle.instance.army.focusPoint = R.pool.selectRect.transform.position = terrain.transform.position;
			R.info.UpdateUnitUI(pos);
			if (unit && !unit.isSleep && unit.army == Battle.instance.army) unit.OnClick(pos);
			else terrain.OnClick(pos);
		}


		[System.NonSerialized] public bool lockInput = true;

		public bool isAllTaskCompleted => taskQueue.Count == 0;


		public Vector3Int? click;

		public static event System.Action<Vector3Int> onClick;

		public Task<T> Wait<T>(Task<T> task)
		{
			taskQueue.Enqueue(task);
			return task;
		}


		public Task Wait(Task task)
		{
			taskQueue.Enqueue(task);
			return task;
		}


		private readonly Queue<Task> taskQueue = new Queue<Task>();

		private void Update()
		{
			if (taskQueue.Count > 0)
				while (taskQueue.Count > 0 && taskQueue.Peek().IsCompleted) taskQueue.Dequeue();
		}


		public async Task<Vector3Int?> WaitForClicking()
		{
			var token = Battle.endTurnCancel;
			click = null;
			while (click == null && !token.IsCancellationRequested) await Task.Delay(1);
			return click;
		}
	}
}
