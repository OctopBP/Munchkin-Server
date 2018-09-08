using UnityEngine;

public class PlayerInfo {
	public string name;
	public int number;
	public int connectionId;
}

public class Munchkin {

	public Hand hand = new Hand();

	public ThingCardSlot weapon1Slot = new ThingCardSlot(ThingCard.ThingType.WEAPON, "W1");
	public ThingCardSlot weapon2Slot = new ThingCardSlot(ThingCard.ThingType.WEAPON, "W2");
	public ThingCardSlot headSlot =	new ThingCardSlot(ThingCard.ThingType.HEAD, "HE");
	public ThingCardSlot armorSlot = new ThingCardSlot(ThingCard.ThingType.ARMOR, "AR");
	public ThingCardSlot shoesSlot = new ThingCardSlot(ThingCard.ThingType.SHOES, "SH");

	public ClassCardSlot classSlot = new ClassCardSlot();

	public int lvl = 1;
	public int Damage {
		get {
			int dmg = lvl;

			dmg += weapon1Slot.GetCard() != null ? weapon1Slot.GetCard().bonus : 0;
			dmg += weapon2Slot.GetCard() != null ? weapon2Slot.GetCard().bonus : 0;
			dmg += headSlot.GetCard() != null ? headSlot.GetCard().bonus : 0;
			dmg += armorSlot.GetCard() != null ? armorSlot.GetCard().bonus : 0;
			dmg += shoesSlot.GetCard() != null ? shoesSlot.GetCard().bonus : 0;

			return dmg;
		}
	}
	public void LvlUp(int lvls = 1) {
		lvl += lvls;
		lvl = Mathf.Max(1, lvl);

		Server.Instance.Send_NewValues();
	}

	public SingleSlot GetSlotById(string slotId) {
		switch (slotId) {
			case "W1": return weapon1Slot;
			case "W2": return weapon2Slot;
			case "HE": return headSlot;
			case "AR": return armorSlot;
			case "SH": return shoesSlot;
			case "CL": return classSlot;
		}
		return null;
	}
	public ThingCardSlot GetThingLostById(string slotId) {
		if (slotId != "CL")
			return GetSlotById(slotId) as ThingCardSlot;
		return null;
	}

	//public void AddClass(ClassCard classCard) {
	//	classSlot.AddCard(classCard);
	//	OnRemoveClass();
	//}
	//public void RemoveClass() {
	//	AddClass(null);
	//	OnRemoveClass();
	//}
	public void OnRemoveClass() {
		int classNumber = classSlot.GetClassNumber();

		if (weapon1Slot.GetCard() != null)	weapon1Slot.OnClassChanges(classNumber);
		if (weapon2Slot.GetCard() != null)	weapon2Slot.OnClassChanges(classNumber);
		if (headSlot.GetCard() != null)		headSlot.OnClassChanges(classNumber);
		if (armorSlot.GetCard() != null)	armorSlot.OnClassChanges(classNumber);
		if (shoesSlot.GetCard() != null)	shoesSlot.OnClassChanges(classNumber);
	}
}