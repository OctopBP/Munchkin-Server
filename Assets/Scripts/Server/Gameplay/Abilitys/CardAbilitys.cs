using System.Collections.Generic;
using UnityEngine;

public class CardAbilitys: MonoBehaviour {

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

		if (GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.GetCard() != null)
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.slotName);
		
		if (GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.GetCard() != null)
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.slotName);
		
		if (GameManager.Instance.GetCurPlayer().munchkin.headSlot.GetCard() != null)
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.headSlot.slotName);
		
		if (GameManager.Instance.GetCurPlayer().munchkin.armorSlot.GetCard() != null)
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.armorSlot.slotName);
		
		if (GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.GetCard() != null)
			allThings.Add(GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.slotName);

		for (int numbOfCard = Mathf.Min(allThings.Count, 2); numbOfCard > 0; numbOfCard--) {
			int randomCard = Random.Range(0, numbOfCard);

			string slotName = allThings[randomCard].ToUpper();

			GameManager.Instance.GetCurPlayer().munchkin.GetSlotByName(slotName).RemoveCard();

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