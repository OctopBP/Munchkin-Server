using System.Collections.Generic;
using UnityEngine;

public class LoseEvents : MonoBehaviour {

	public static LoseEvents Instance { get; set; }

	private void Awake() {
		Instance = this;
	}

	// if monster hane no OnLoseEvent
	public void _DefaultOnLose() {
	}

	// id: 33
	public void _DropThreeCards() {
		// ??

		int numberOfCardToDrop = 3;
		List<string> newCardsToSelect = new List<string>();

		foreach (HandCardSlot slot in GameManager.Instance.GetCurPlayer().munchkin.hand.cardsSlots) {
			if (slot.GetCard().cardType == Card.CardType.THING)
				newCardsToSelect.Add(slot.GetSlotId());
		}

		if (!GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.IsEmpty())
			newCardsToSelect.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.IsEmpty())
			newCardsToSelect.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.headSlot.IsEmpty())
			newCardsToSelect.Add(GameManager.Instance.GetCurPlayer().munchkin.headSlot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.armorSlot.IsEmpty())
			newCardsToSelect.Add(GameManager.Instance.GetCurPlayer().munchkin.armorSlot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.IsEmpty())
			newCardsToSelect.Add(GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.GetSlotId());

		numberOfCardToDrop = Mathf.Min(newCardsToSelect.Count, numberOfCardToDrop);

		if (newCardsToSelect.Count > 0)
			Server.Instance.Send_SelectionCard(newCardsToSelect, numberOfCardToDrop);
		else
			GameManager.Instance.turnController.ChangeTurn();
	}

	// id: 40, 56
	public void _LoseOneLvl() {
		GameManager.Instance.GetCurPlayer().munchkin.LvlUp(-1);
	}

	// id: 60
	public void _LoseArmor() {
		GameManager.Instance.GetCurPlayer().munchkin.armorSlot.RemoveCard();
	}
}