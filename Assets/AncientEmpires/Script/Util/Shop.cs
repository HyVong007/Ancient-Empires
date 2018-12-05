using System.Collections.Generic;
using System.Threading.Tasks;
using AncientEmpires.Terrains;
using AncientEmpires.Units;
using AncientEmpires.Units.Kings;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace AncientEmpires.Util
{
	public class Shop : MonoBehaviour
	{
		public static Shop instance { get; private set; }

		public IReadOnlyList<Unit.Name> allNormalNamesCanBuy { get; private set; }

		[SerializeField] private UnitName_Button buttons;
		[SerializeField] private Button kingButton;
		private Unit.Name? selectedUnitName;
		private Unit.Name castleKingName;
		[SerializeField] private ScrollRect scrollRect;
		[SerializeField] private Button cancelButton;
		private bool isCanceled;


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }

			R.shop = this;
			var tmp = new List<Unit.Name>();
			foreach (Unit.Name name in Enum.GetValues(typeof(Unit.Name)))
				if (name != Unit.Name.CRYSTAL && name != Unit.Name.SKELETON && !King.kingNames.Contains(name)) tmp.Add(name);
			allNormalNamesCanBuy = tmp;

			foreach (var name in allNormalNamesCanBuy) buttons[name].onClick.AddListener(() =>
			{
				if (selectedUnitName == null && canBuyList.Contains(name)) selectedUnitName = name;
			});
			kingButton.onClick.AddListener(() =>
			{
				if (selectedUnitName == null && canBuyList.Contains(castleKingName)) selectedUnitName = castleKingName;
			});
			cancelButton.onClick.AddListener(() => { isCanceled = true; });
		}


		private IReadOnlyList<Unit.Name> canBuyList;
		private Castle currentCastle;

		private void PrepareToBuy()
		{
			var king = King.dict[currentCastle.army.color];

			// Calculate canBuyList
			var tmp = new List<Unit.Name>();
			foreach (var name in allNormalNamesCanBuy) if (currentCastle.CanBuy(name)) tmp.Add(name);
			castleKingName = king.name;
			if (currentCastle.CanBuy(castleKingName)) tmp.Add(castleKingName);
			canBuyList = tmp;

			if (!king.gameObject.activeSelf)
			{
				kingButton.gameObject.SetActive(true);
				kingButton.image.sprite = canBuyList.Contains(castleKingName) ? R.asset.prefab.units[castleKingName].spriteRenderer.sprite : king.sleepSprite;
			}
			else kingButton.gameObject.SetActive(false);

			// Modify all buttons by current battle conditions
			foreach (var name in allNormalNamesCanBuy)
				buttons[name].image.sprite = canBuyList.Contains(name) ? R.asset.sprite.units[name][currentCastle.army.color] : R.asset.prefab.units[name].sleepSprite;
		}


		public async Task<Unit> Buy(Castle castle)
		{
			bool origin = R.input.lockInput;
			R.input.lockInput = true;
			currentCastle = castle;
			PrepareToBuy();
			selectedUnitName = null;
			isCanceled = false;
			scrollRect.gameObject.SetActive(true);
			cancelButton.gameObject.SetActive(true);
			var endTurn = Battle.endTurnCancel;
			while (!endTurn.IsCancellationRequested && !isCanceled && selectedUnitName == null) await Task.Delay(1);

			if (endTurn.IsCancellationRequested) selectedUnitName = null;
			cancelButton.gameObject.SetActive(false);
			scrollRect.gameObject.SetActive(false);
			var unit = selectedUnitName != null ? currentCastle.Buy(selectedUnitName.Value) : null;
			await Task.Delay(1);
			R.input.lockInput = origin;
			return unit;
		}
	}
}
