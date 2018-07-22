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
		List<ThingCard> allThings = new List<ThingCard>();

		allThings.AddIfNotNull(GameManager.Instance.GetCurPlayer().munchkin.weapon1Slot.GetCard());
		allThings.AddIfNotNull(GameManager.Instance.GetCurPlayer().munchkin.weapon2Slot.GetCard());
		allThings.AddIfNotNull(GameManager.Instance.GetCurPlayer().munchkin.headSlot.GetCard());
		allThings.AddIfNotNull(GameManager.Instance.GetCurPlayer().munchkin.armorSlot.GetCard());
		allThings.AddIfNotNull(GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.GetCard());

		Debug.Log("allThings.Count " + allThings.Count);

		for (int numbOfCard = Mathf.Min(allThings.Count, 2); numbOfCard > 0; numbOfCard--) {
			int randomCard = Random.Range(0, numbOfCard);

			string slotName = allThings[randomCard].thingType.ToString().ToUpper();

			Debug.Log("Remove " + slotName + " (" + randomCard + ") card");

			Server.Instance.SendRemoveCard(GameManager.Instance.GetCurPlayer().info.number, slotName);
			GameManager.Instance.GetCurPlayer().munchkin.GetSlotByName(slotName).RemoveCard();

			allThings.RemoveAt(randomCard);
		}

	}

	// id: 26
	public void _TrapRadioactivityPuddles() {
		if (GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.IsEmpty())
			return;
		
		Server.Instance.SendRemoveCard(GameManager.Instance.GetCurPlayer().info.number, "SHOES");
		GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.RemoveCard();
	}

	// id: 28
	public void _TrapConcussion() {
		if (GameManager.Instance.GetCurPlayer().munchkin.classSlot.IsEmpty()) {
			GameManager.Instance.GetCurPlayer().munchkin.LvlUp(-1);
			// send that lvl change
		}
		else {
			Server.Instance.SendRemoveCard(GameManager.Instance.GetCurPlayer().info.number, "CLASS");
			GameManager.Instance.GetCurPlayer().munchkin.shoesSlot.RemoveCard();
		}
	}

}