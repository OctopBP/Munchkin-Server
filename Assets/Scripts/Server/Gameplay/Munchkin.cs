using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
	public string name;
	public int number;
	public int connectionId;
}

public class Munchkin : MonoBehaviour {

	public List<Card> hand = new List<Card>();

	public ThingCardSlot weapon1Slot = new ThingCardSlot();
	public ThingCardSlot weapon2Slot = new ThingCardSlot();
	public ThingCardSlot headSlot = new ThingCardSlot();
	public ThingCardSlot armorSlot = new ThingCardSlot();
	public ThingCardSlot shoesSlot = new ThingCardSlot();

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
	}

	public void SetCloseId() {
		for (int i = 0; i < hand.Count; i++)
			hand[i].closeId = i;
	}

	public CardSlot GetSlotByName(string slotName) {
		switch (slotName) {
			case "WEAPON1": return weapon1Slot;
			case "WEAPON2":	return weapon2Slot;
			case "HEAD":	return headSlot;
			case "ARMOR":	return armorSlot;
			case "SHOES":	return shoesSlot;
			case "CLASS":	return classSlot;
		}
		return null;
	}
}

public class CardSlot {
	protected Card SelfCard;

	public virtual bool CanDropCard(Card card) {
		return false;
	}

	public bool IsEmpty() {
		return SelfCard == null;
	}

	public void AddCard(Card card) {
		RemoveCard();
		SelfCard = card;
	}
	public void RemoveCard() {
		// Add selfCard to pile
		SelfCard = null;
	}
}

public class ThingCardSlot: CardSlot {
	public ThingCard.ThingType slotType;

	override public bool CanDropCard(Card card) {
		return (card.cardType == Card.CardType.THING && (card as ThingCard).thingType == slotType);
	}
	public bool AddCardIfCan(ThingCard card) {
		if (CanDropCard(card))
			return false;

		AddCard(card);
		return true;
	}

	public ThingCard GetCard() {
		return SelfCard as ThingCard;
	}
}

public class ClassCardSlot: CardSlot {
	override public bool CanDropCard(Card card) {
		return card.cardType == Card.CardType.CLASS;
	}
	public bool AddCardIfCan(ClassCard card) {
		if (CanDropCard(card))
			return false;

		AddCard(card);
		return true;
	}

	public ClassCard GetCard() {
		return SelfCard as ClassCard;
	}
}