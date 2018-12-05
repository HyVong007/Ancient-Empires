using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace AncientEmpires.Util
{
	public class CameraController : MonoBehaviour
	{
		public new Camera camera;

		[SerializeField] private int WAIT_FRAME = 1;

		[SerializeField] private Rigidbody2D camRig;

		[System.Serializable]
		public struct BounderBox
		{
			[SerializeField] private float PHYSIC_DETA;

			public float size;
			public GameObject up, right, down, left;


			public void Init(Rect boundRect)
			{
				var scaleW = new Vector3(boundRect.width + 4f * size, 2f * size, 0f);
				var scaleH = new Vector3(2 * size, boundRect.height + 4f * size, 0f);
				left.transform.position = new Vector3(boundRect.xMin - size - PHYSIC_DETA, boundRect.center.y, 0f);
				left.transform.localScale = scaleH;
				right.transform.position = new Vector3(boundRect.xMax + size + PHYSIC_DETA, boundRect.center.y, 0f);
				right.transform.localScale = scaleH;
				up.transform.position = new Vector3(boundRect.center.x, boundRect.yMax + size + PHYSIC_DETA, 0f);
				up.transform.localScale = scaleW;
				down.transform.position = new Vector3(boundRect.center.x, boundRect.yMin - size - PHYSIC_DETA, 0f);
				down.transform.localScale = scaleW;
			}
		}
		[SerializeField] private BounderBox boxs;

		[System.Serializable]
		public struct Setting
		{
			public float mouseMoveSpeed, mouseZoomSpeed, touchMoveSpeed, touchZoomSpeed,
				focusRadius, focusDeltaTime, focusSpeed;

			[System.Serializable]
			public struct AutoFocus
			{
				public float zoomSensity, zoomSpeed, moveSpeed;
			}
			public AutoFocus autoFocus;


			public void Update(float ratio)
			{
				mouseMoveSpeed *= ratio;
				mouseZoomSpeed *= ratio;
				touchMoveSpeed *= ratio;
				touchZoomSpeed *= ratio;
				focusSpeed *= ratio;
				autoFocus.zoomSensity *= ratio;
				autoFocus.zoomSpeed *= ratio;
				autoFocus.moveSpeed *= ratio;
			}
		}
		[SerializeField] private Setting setting;

		public static CameraController instance { get; private set; }

#if DEBUG
		[SerializeField]
#endif
		private Rect originCamRect, camRect, boundRect, mapRect;

		public Rect inputRect;

		public float CAMRECT_ASPECT { get; private set; }


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }

			R.camCtrl = this;
			R.camera = camera;
			var a = Config.instance.map.terrainArray;
			var mapSize = new Vector2Int(a.Length, a[0].Length);
			mapRect = new Rect();
			mapRect.size = mapSize;
			mapRect.center = Vector2.zero;
			originCamRect = new Rect();
			float uiH = InfoUI.instance.armyUI.rectTransform.rect.height;
			float x = uiH / Screen.height;
			originCamRect.size = new Vector2(camera.orthographicSize * 2f * camera.aspect, camera.orthographicSize * 2f * (1 - x));
			//originCamRect.size = new Vector2(camera.orthographicSize * 2f * camera.aspect, camera.orthographicSize * 2f);
			originCamRect.center = Vector2.zero;
			camera.transform.position = new Vector3(0, -x / 2f, -10);
			camRect = new Rect(originCamRect);
			CAMRECT_ASPECT = camRect.width / camRect.height;
			boundRect = new Rect();
			boundRect.size = Vector2.Max(mapRect.size, originCamRect.size);
			boundRect.center = Vector2.zero;
			boxs.Init(boundRect);
			inputRect = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));
			camRig.transform.localScale = camRect.size;
			camRig.transform.position = new Vector3(0, 0, -10);
			inputRect = new Rect(0, uiH, Screen.width, Screen.height - uiH);
			//inputRect = new Rect(0, 0, Screen.width, Screen.height);
			Screen.orientation = ScreenOrientation.AutoRotation;
		}


		private int runningCount;
		private Vector3 prevMovePos;
		private CancellationTokenSource cancelSource = new CancellationTokenSource();

		private void Update()
		{
			if (R.input.lockInput) return;

			// Detect Orientation
			if (Screen.orientation == ScreenOrientation.Portrait)
				Screen.orientation = ScreenOrientation.LandscapeLeft | ScreenOrientation.AutoRotation;
			else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				Screen.orientation = ScreenOrientation.LandscapeRight | ScreenOrientation.AutoRotation;

			// Validate Input
			if (Input.touchCount == 0)
			{
				if (!inputRect.Contains(Input.mousePosition)) return;
			}
			else
			{
				if (!inputRect.Contains(Input.GetTouch(0).position)) return;
				if (Input.touchCount >= 2 && !inputRect.Contains(Input.GetTouch(1).position)) return;
			}

			if (runningCount > 0)
			{
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
				{
					cancelSource.Cancel();
					cancelSource = new CancellationTokenSource();
				}
				return;
			}

			// Process Input

			if (Input.touchCount == 1)
			{
				var touch = Input.GetTouch(0);

				switch (touch.phase)
				{
					case TouchPhase.Began:
						camRig.velocity = Vector2.zero;
						//if (!R.util.blocker.activeSelf) CheckFocus(touch.position);
						if (R.input.isAllTaskCompleted) CheckFocus(touch.position);
						break;

					case TouchPhase.Moved:
						camRig.velocity = -touch.deltaPosition * setting.touchMoveSpeed;
						break;
				}
				return;
			}

			if (Input.touchCount == 2)
			{
				var touch0 = Input.GetTouch(0);
				var touch1 = Input.GetTouch(1);
				var prevPos0 = touch0.position - touch0.deltaPosition;
				var prevPos1 = touch1.position - touch1.deltaPosition;
				var mag = (touch0.position - touch1.position).magnitude;
				var prevMag = (prevPos0 - prevPos1).magnitude;
				Zoom((mag - prevMag) * setting.touchZoomSpeed);
				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				camRig.velocity = Vector2.zero;
				prevMovePos = Input.mousePosition;
				if (R.input.isAllTaskCompleted) CheckFocus(Input.mousePosition);
			}
			else if (Input.GetMouseButton(0))
			{
				camRig.velocity = (prevMovePos - Input.mousePosition) * setting.mouseMoveSpeed;
				prevMovePos = Input.mousePosition;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") != 0f) Zoom(-Input.GetAxis("Mouse ScrollWheel") * setting.mouseZoomSpeed);
		}


		// ##################  Util APIs  ####################################


		/// <summary>
		/// Return value: True is inside the Moving Limit.
		/// <para> False is Limit reached, cannot move any more.</para>
		/// </summary>
		/// <param name="distance"></param>
		/// <returns></returns>
		public async Task<bool> Move(Vector3 distance, bool async = true)
		{
			var d = new Vector2(distance.x, distance.y);
			var origin = camRig.position;
			camRig.MovePosition(origin + d);
			if (async) await CommonUtil.WaitForFrame(WAIT_FRAME);
			var pos = camRig.position;
			return !(Mathf.Abs(pos.x - origin.x) <= 0.02f && Mathf.Abs(pos.y - origin.y) <= 0.02f);
		}


		public async Task Move(Vector3 distance, float speed)
		{
			prevFocusPos = null;
			++runningCount;
			distance.z = 0f;
			var dir = distance.normalized;
			var speedDir = dir * speed;
			float length = distance.magnitude;
			var token = cancelSource.Token;

			while (length > 0)
			{
				if (token.IsCancellationRequested) break;

				if (length <= speed)
				{
					await Move(length * dir);
					break;
				}

				length -= speed;
				if (!await Move(speedDir)) break;
			}
			--runningCount;
		}


		public void Focus(Vector3 point) => Move(point - (Vector3)camRig.position, false);


		public async Task Focus(Vector3 point, float speed) => await Move(point - (Vector3)camRig.position, speed);


		private Vector3? prevFocusPos;
		private float prevFocusTime;

		private async void CheckFocus(Vector3 pos)
		{
			if (prevFocusPos != null)
			{
				var delta = pos - prevFocusPos.Value;
				if (delta.sqrMagnitude <= setting.focusRadius && Time.time - prevFocusTime <= setting.focusDeltaTime)
					await AutoFocus(camera.ScreenToWorldPoint(pos));
			}

			prevFocusPos = pos;
			prevFocusTime = Time.time;
		}


		/// <summary>
		/// Larger size: Zoom Out (-).
		/// Smaller size: Zoom In (+).
		/// <para>Return value: True is inside the Zooming Limit.</para>
		/// <para>False is Limit reached, cannot Zoom any more.</para>
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public bool Zoom(Vector2 size)
		{
			prevFocusPos = null;
			bool ok = false;
			if (size.x > boundRect.width) size = new Vector2(boundRect.width, boundRect.width / CAMRECT_ASPECT);
			else if (size.x < originCamRect.width) size = originCamRect.size;
			else if (size.y > boundRect.height) size = new Vector2(boundRect.height * CAMRECT_ASPECT, boundRect.height);
			else if (size.y < originCamRect.height) size = originCamRect.size;
			else ok = true;

			camRect.size = size;
			float oldH = camera.orthographicSize;
			camera.orthographicSize = (camRect.width / camera.aspect) / 2f;
			camRig.transform.localScale = camRect.size;
			setting.Update(camera.orthographicSize / oldH);
			return ok;
		}


		/// <summary>
		/// | Delta | must be > 0.02f.
		/// <para>Delta > 0 : Zoom Out (-).</para>
		/// <para>Delta &lt; 0 : Zoom In (+).</para>
		/// <para>Return value: True is inside the Zooming Limit.</para>
		/// <para>False is Limit reached, cannot Zoom any more.</para>
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		public bool Zoom(float delta) => Zoom(new Vector2((camRect.height + delta) * CAMRECT_ASPECT, camRect.height + delta));


		public async Task<bool> Zoom(Vector2 size, float speed)
		{
			++runningCount;
			Vector2 newSize = camRect.size;
			var token = cancelSource.Token;

			bool isInsideLimit = false;
			do
			{
				if (token.IsCancellationRequested) break;

				newSize = Vector2.MoveTowards(newSize, size, speed);
				isInsideLimit = Zoom(newSize);
				await CommonUtil.WaitForFrame(WAIT_FRAME);
				if (!isInsideLimit) break;
			} while (newSize != size);
			--runningCount;
			return isInsideLimit;
		}


		public async Task<bool> Zoom(float delta, float speed) => await Zoom(new Vector2((camRect.height + delta) * CAMRECT_ASPECT, camRect.height + delta), speed);


		public async Task AutoFocus(Vector3 target)
		{
			target.z = 0f;
			camRect.center = camRig.position;
			var token = cancelSource.Token;

			// Find Target and check Zoom Out
			if (!camRect.Contains(target))
				do
				{
					if (token.IsCancellationRequested) return;

					bool isInsideLimit = await Zoom(setting.autoFocus.zoomSensity, setting.autoFocus.zoomSpeed);
					camRect.center = camRig.position;
					if (!isInsideLimit) break;
				} while (!camRect.Contains(target));

			// Try Zoom In and Move to target
			while (true)
			{
				if (token.IsCancellationRequested) return;

				bool isInsideLimit = await Zoom(-setting.autoFocus.zoomSensity, setting.autoFocus.zoomSpeed);
				if (!isInsideLimit)
				{
					// Zoom reached the Limit => Move to target continously
					camRig.velocity = Vector2.zero;
					await Move(target - camRig.transform.position, setting.autoFocus.moveSpeed);
					return;
				}

				camRig.velocity = (target - camRig.transform.position);
			}
		}


		private void OnApplicationQuit()
		{
			cancelSource.Cancel();
		}
	}
}
