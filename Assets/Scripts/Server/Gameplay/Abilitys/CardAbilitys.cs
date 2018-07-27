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
		// send that lvl change
	}

	// id: 24
	public void _TrapTornBackpack() {
		List<string> allThings = new List<string>();

		if (GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.GetCard() != null) allThings.Add("WEAPON1");
		if (GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.GetCard() != null) allThings.Add("WEAPON2");
		if (GameManager.Instance.GetCurPlayer().munchkin.headSlot.GetCard() != null) allThings.Add("HEAD");
		if (GameManager.Instance.GetCurPlayer().munchkin.armorSlot.GetCard() != null) allThings.Add("ARMOR");
		if (GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.GetCard() != null) allThings.Add("SHOES");

		for (int numbOfCard = Mathf.Min(allThings.Count, 2); numbOfCard > 0; numbOfCard--) {
			int randomCard = Random.Range(0, numbOfCard);

			string slotName = allThings[randomCard].ToUpper();

			Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, slotName);
			GameManager.Instance.GetCurPlayer().munchkin.GetSlotByName(slotName).RemoveCard();

			allThings.RemoveAt(randomCard);
		}

	}

	// id: 26
	public void _TrapRadioactivityPuddles() {
		if (GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.IsEmpty())
			return;
		
		Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, "SHOES");
		GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.RemoveCard();
	}

	// id: 28
	public void _TrapConcussion() {
		if (GameManager.Instance.GetCurPlayer().munchkin.classSlot.IsEmpty()) {
			GameManager.Instance.GetCurPlayer().munchkin.LvlUp(-1);
			// send that lvl change
		}
		else {
			Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, "CLASS");
			GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.RemoveCard();
		}
	}

}