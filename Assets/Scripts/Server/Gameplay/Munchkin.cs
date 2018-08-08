using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
	public string name;
	public int number;
	public int connectionId;
}

public class Munchkin {

	public Hand hand = new Hand();

	public ThingCardSlot weapon1Slot = new ThingCardSlot(ThingCard.ThingType.WEAPON, "WEAPON1");
	public ThingCardSlot weapon2Slot = new ThingCardSlot(ThingCard.ThingType.WEAPON, "WEAPON2");
	public ThingCardSlot headSlot = new ThingCardSlot(ThingCard.ThingType.HEAD, "HEAD");
	public ThingCardSlot armorSlot = new ThingCardSlot(ThingCard.ThingType.ARMOR, "ARMOR");
	public ThingCardSlot shoesSlot = new ThingCardSlot(ThingCard.ThingType.SHOES, "SHOES");

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

	public ThingCardSlot GetSlotByName(string slotName) {
		switch (slotName) {
			case "WEAPON1": return weapon1Slot;
			case "WEAPON2":	return weapon2Slot;
			case "HEAD":	return headSlot;
			case "ARMOR":	return armorSlot;
			case "SHOES":	return shoesSlot;
		}
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
		int classNumber = classSlot.GetCard() == null ? 4 : (int)classSlot.GetCard().className;

		if (weapon1Slot.GetCard() != null)	weapon1Slot.OnClassChanges(classNumber);
		if (weapon2Slot.GetCard() != null)	weapon2Slot.OnClassChanges(classNumber);
		if (headSlot.GetCard() != null)		headSlot.OnClassChanges(classNumber);
		if (armorSlot.GetCard() != null)	armorSlot.OnClassChanges(classNumber);
		if (shoesSlot.GetCard() != null)	shoesSlot.OnClassChanges(classNumber);
	}
}

public class Hand {
	public List<Card> cards = new List<Card>();

	public void Add(Card card) {
		cards.Add(card);
		SetCloseId();
	}
	public void Remove(Card card) {
		if (card.deckType == HidenCard.DeckType.TREASURE)
			GameManager.Instance.treasurePile.Add(card);
		else
			GameManager.Instance.doorPile.Add(card);

		cards.Remove(card);
		SetCloseId();
	}

	public void SetCloseId() {
		for (int i = 0; i < cards.Count; i++)
			cards[i].closeId = i;
	}

	public Card GetCardAtId(int id) {
		return cards.Find(card => card.id == id);
	}
}

public class CardSlot {
	protected Card SelfCard;
	public readonly string slotName;

	public bool IsEmpty() {
		return SelfCard == null;
	}

	public virtual void AddCard(Card card) {
		if (SelfCard != null)
			GameManager.Instance.treasurePile.Add(SelfCard);
		
		SelfCard = card;
	}
	public virtual void RemoveCard() {
		if (SelfCard != null) {
			GameManager.Instance.treasurePile.Add(SelfCard);
			Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, slotName);
		}

		SelfCard = null;
	}
}

public class ThingCardSlot: CardSlot {
	public ThingCardSlot(ThingCard.ThingType sType, string sName) {
		slotType = sType;
		slotName = sName;
	}

	public ThingCard.ThingType slotType;
	public ThingCard GetCard() {
		return SelfCard as ThingCard;
	}

	public void OnClassChanges(int newClassNumber) {
		if (!(SelfCard as ThingCard).restriction.Contain(newClassNumber))
			RemoveCard();
	}
}

public class ClassCardSlot: CardSlot {
	public ClassCardSlot() {
		slotName = "CLASS";
	}

	public ClassCard GetCard() {
		return SelfCard as ClassCard;
	}

	public override void AddCard(Card card) {
		base.AddCard(card);
		GameManager.Instance.GetCurPlayer().munchkin.OnRemoveClass();
	}
	public override void RemoveCard() {
		base.RemoveCard();
		GameManager.Instance.GetCurPlayer().munchkin.OnRemoveClass();
	}
}