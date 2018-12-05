using System.Collections.Generic;
using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Units;
using UnityEngine;
using UnityEngine.UI;


namespace AncientEmpires.Util
{
	public class InfoUI : MonoBehaviour
	{
		public const float ALPHA = 0.6f;

		public static readonly IReadOnlyDictionary<Army.Color, Color> armyColors = new Dictionary<Army.Color, Color>()
		{
			[Army.Color.BLACK] = new Color(0, 0, 0, ALPHA),
			[Army.Color.BLUE] = new Color(0, 0, 1, ALPHA),
			[Army.Color.GREEN] = new Color(0, 1, 0, ALPHA),
			[Army.Color.RED] = new Color(1, 0, 0, ALPHA)
		};

		[System.Serializable]
		public struct ArmyUI
		{
			public RectTransform rectTransform;
			public Text turn, unitCount, money, turnRemainTime, name;
			public Image panel;
			public Button button;
			[SerializeField] private Text terrainDefend;
			[SerializeField] private Image terrainIcon;


			public void UpdateTerrain(Vector3Int pos)
			{
				var t = AETerrain.array[pos.x][pos.y];
				terrainIcon.sprite = t.spriteRenderer.sprite;
				terrainDefend.text = t.increaseDefend.ToString();
			}
		}
		public ArmyUI armyUI;

		[System.Serializable]
		public struct UnitUI
		{
			[SerializeField] private RectTransform rectTransform;
			[SerializeField] private Text name, attack, defend, experience;
			[SerializeField] private Image attackArrow, defendArrow, panel;
			[SerializeField] private Sprite upArrow, downArrow;


			public void Update(Unit unit, bool show = true)
			{
				name.text = unit.levelName;
				attack.text = (int)unit.minAttack + " - " + (int)unit.maxAttack;
				var pos = unit.transform.position.WorldToArray();
				var t = AETerrain.array[pos.x][pos.y];
				defend.text = (int)(unit.defend + (unit is Elemental && t is NormalTerrain && (t as NormalTerrain).name == NormalTerrain.Name.WATER
					? Elemental.WATER_INCREASE_DEFEND : t.increaseDefend)) + "";
				experience.text = (int)unit.experience + "";
				panel.color = armyColors[unit.army.color];
				bool isPowered = unit.powerTurn > 0;
				bool isPoisoned = unit.poisonTurn > 0;

				// Calculate attackArrow
				attackArrow.sprite = null;
				attackArrow.gameObject.SetActive(false);
				if (isPoisoned && isPowered)
					attackArrow.sprite =
						(Wisp.POWER_DELTA > DireWolf.POISON_DELTA) ? upArrow
						: (Wisp.POWER_DELTA < DireWolf.POISON_DELTA) ? downArrow : null;
				else if (isPowered) attackArrow.sprite = upArrow;
				else if (isPoisoned) attackArrow.sprite = downArrow;
				if (attackArrow.sprite) attackArrow.gameObject.SetActive(true);

				// Calculate defendArrow
				defendArrow.sprite = null;
				defendArrow.gameObject.SetActive(false);
				float increaseDefend = (unit is Elemental && t is NormalTerrain && (t as NormalTerrain).name == NormalTerrain.Name.WATER) ? Elemental.WATER_INCREASE_DEFEND : t.increaseDefend;
				if (isPoisoned) defendArrow.sprite =
						(DireWolf.POISON_DELTA > increaseDefend) ? downArrow
						: (DireWolf.POISON_DELTA < increaseDefend) ? upArrow : null;
				else if (increaseDefend > 0) defendArrow.sprite = upArrow;
				if (defendArrow.sprite) defendArrow.gameObject.SetActive(true);
				SetActive(show);
			}


			public void SetActive(bool isActive) => rectTransform.gameObject.SetActive(isActive);
		}
		public UnitUI activeUnitUI, passiveUnitUI;

		public static InfoUI instance { get; private set; }


		// =======================================================================


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(this); return; }

			R.info = this;
			armyUI.button.onClick.AddListener(() =>
			{
				if (R.input.lockInput || !R.input.isAllTaskCompleted) return;
				R.input.lockInput = true;
				Battle.instance.player.CompleteTurn();
			});
		}


		public void HideAllUnitUI()
		{
			activeUnitUI.SetActive(false);
			passiveUnitUI.SetActive(false);
		}


		public void UpdateUnitUI(Vector3Int pos, bool show = true)
		{
			armyUI.UpdateTerrain(pos);
			var unit = Unit.array[pos.x][pos.y];
			if (!unit) return;
			var ui = unit.army == Battle.instance.army ? activeUnitUI : passiveUnitUI;
			ui.Update(unit, show);
		}


		public UnitUI GetUnitUI(Unit unit) => unit.army == Battle.instance.army ? activeUnitUI : passiveUnitUI;


		public async Task WaitToHideUnitUI(Unit unit)
		{
			var ui = (unit.army == Battle.instance.army) ? activeUnitUI : passiveUnitUI;
			await R.input.WaitForClicking();
			ui.SetActive(false);
		}


		// =========================  TURN BASE EVENTS  ===========================


		private void OnEnable()
		{
			Battle.onTurnBegin += UpdateArmyUI;
			GameData.onLoaded += UpdateArmyUI;
		}


		private void OnDisable()
		{
			Battle.onTurnBegin -= UpdateArmyUI;
			GameData.onLoaded -= UpdateArmyUI;
		}


		private void UpdateArmyUI()
		{
			var army = Battle.instance.army;
			armyUI.panel.color = armyColors[army.color];
			armyUI.button.interactable = army.type == Army.Type.LOCAL;
			armyUI.unitCount.text = army.units.Count.ToString();
			armyUI.name.text = army.name;
		}
	}
}
