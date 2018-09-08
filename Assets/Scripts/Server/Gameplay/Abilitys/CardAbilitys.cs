using System.Collections.Generic;
using UnityEngine;

public class CardAbilitys : MonoBehaviour {

	public static CardAbilitys Instance { get; set; }

	private void Awake() {
		Instance = this;
	}

	// id: 21
	public void _TrapPoisoning() {
		GameManager.Instance.GetCurPlayer().munchkin.LvlUp(-1);
	}

	// id: 24
	public void _TrapTornBackpack() {
		List<string> allThings = new List<string>();

		foreach (HandCardSlot slot in GameManager.Instance.GetCurPlayer().munchkin.hand.cardsSlots) {
			if (slot.GetCard().cardType == Card.CardType.THING)
				allThings.Add(slot.GetSlotId());
		}

		if (!GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.IsEmpty())
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.IsEmpty())
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.headSlot.IsEmpty())
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.headSlot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.armorSlot.IsEmpty())
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.armorSlot.GetSlotId());
		
		if (!GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.IsEmpty())
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.GetSlotId());

		for (int numbOfCard = Mathf.Min(allThings.Count, 2); numbOfCard > 0; numbOfCard--) {
			int randomCard = Random.Range(0, numbOfCard);

			string slotId = allThings[randomCard];

			GameManager.Instance.GetCurPlayer().munchkin.GetSlotById(slotId).RemoveCard();

			allThings.RemoveAt(randomCard);
		}

	}

	// id: 26
	public void _TrapRadioactivityPuddles() {
		if (GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.IsEmpty())
			return;
		
		GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.RemoveCard();
	}

	// id: 28
	public void _TrapConcussion() {
		if (GameManager.Instance.GetCurPlayer().munchkin.classSlot.IsEmpty()) {
			GameManager.Instance.GetCurPlayer().munchkin.LvlUp(-1);
		}
		else {
			GameManager.Instance.GetCurPlayer().munchkin.classSlot.RemoveCard();
		}
	}

}