using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class HidenCard {
	public int closeId = 0;

	public DeckType deckType;
	public enum DeckType {
		DOOR,
		TREASURE,
		PERK
	}
}
[Serializable] public class Card: HidenCard {
	public int id;

	public CardType cardType;
	public enum CardType {
		MONSTER,
		CLASS,
		LVLUP,
		THING,
		EXPLOSIVE,
		TRAP
	}

	public string name;
	public string texName;
}
[Serializable] public class MonsterCard: Card {
	public int lvl;
	public int[] classBonus = { 0, 0, 0, 0, 0 };

	public string onLose;
	public OnLoseType onLoseType = OnLoseType.NOTHING;
	public enum OnLoseType {
		INSTANT,
		CARD_SELECTION,
		NOTHING
	}

	public int numberOfTreasure = 1;
}
[Serializable] public class ClassCard: Card {
	public ClassName className;
	public enum ClassName {
		WANDERER,	// 0
		PALADIN,	// 1
		RAIDER,		// 2
		SCIENTIST	// 3
		//NOCLASS		// 4
	}
}
[Serializable] public class LvlupCard: Card {
}
[Serializable] public class ThingCard: Card {
	public ThingType thingType;
	public enum ThingType {
		WEAPON,
		HEAD,
		ARMOR,
		SHOES
	}
	public bool twoHandWeapon = false;

	public int[] restriction = { 0, 1, 2, 3, 4 }; // 4 - нет класса
	//public ClassCard.ClassName[] restriction = {
	//	ClassCard.ClassName.WANDERER,
	//	ClassCard.ClassName.PALADIN,
	//	ClassCard.ClassName.RAIDER,
	//	ClassCard.ClassName.SCIENTIST,
	//	ClassCard.ClassName.NOCLASS
	//};
	public int bonus;
}
[Serializable] public class ExplosiveCard: Card {
	public int dmg;
}
[Serializable] public class TrapCard: Card {
	public string ability;

	public TrapType trapType;
	public enum TrapType {
		INSTANTLY,
		EFFECT
	}
}

public static class CardManagerData {
	public static List<Card> allDoorCards = new List<Card>();
    public static List<Card> allTreasureCards = new List<Card>();
}

public class CardManager : MonoBehaviour {
	
	void Awake() {
		try {
			CardManagerData.allDoorCards.AddRange(JsonReader.ReadJson<MonsterCard>("MonsterCards"));
			CardManagerData.allDoorCards.AddRange(JsonReader.ReadJson<ClassCard>("ClassCards"));
			CardManagerData.allDoorCards.AddRange(JsonReader.ReadJson<TrapCard>("TrapCards"));

			CardManagerData.allTreasureCards.AddRange(JsonReader.ReadJson<LvlupCard>("LvlupCards"));
			CardManagerData.allTreasureCards.AddRange(JsonReader.ReadJson<ThingCard>("ThingCards"));
			CardManagerData.allTreasureCards.AddRange(JsonReader.ReadJson<ExplosiveCard>("ExplosiveCards"));

			//MakeDoorDeck();
			//MakeTreasureDeck();
		}
		catch(Exception e) {
			Debug.LogError(e.Message);
		}
	}

	public void MakeDoorDeck() {
		List<Card> newDoorDeck = new List<Card>();

		AddCardAtId(newDoorDeck, 40); // Monster
		AddCardAtId(newDoorDeck, 50); // Monster
		AddCardAtId(newDoorDeck, 33); // Monster

		CardManagerData.allDoorCards.Remove(CardManagerData.allDoorCards.Find((Card obj) => obj.id == 40));
		CardManagerData.allDoorCards.Remove(CardManagerData.allDoorCards.Find((Card obj) => obj.id == 50));
		CardManagerData.allDoorCards.Remove(CardManagerData.allDoorCards.Find((Card obj) => obj.id == 33));
		newDoorDeck.AddRange(CardManagerData.allDoorCards);
		CardManagerData.allDoorCards = newDoorDeck;
	}
	public void MakeTreasureDeck() {
		List<Card> newTreasureDeck = new List<Card>();

		AddCardAtId(newTreasureDeck, 174); // Gatling Laser
		AddCardAtId(newTreasureDeck, 198); // Tesla Armor
		newTreasureDeck.AddRange(CardManagerData.allTreasureCards);

		//AddCardAtId(newTreasureDeck, 170); // Thing
		//AddCardAtId(newTreasureDeck, 172); // Thing
		//AddCardAtId(newTreasureDeck, 181); // Thing
		//AddCardAtId(newTreasureDeck, 179); // Thing
		//AddCardAtId(newTreasureDeck, 201); // Thing
		//AddCardAtId(newTreasureDeck, 206); // Thing
		//AddCardAtId(newTreasureDeck, 208); // Thing
		//AddCardAtId(newTreasureDeck, 212); // Thing
		//AddCardAtId(newTreasureDeck, 150); // LvlUp
		//AddCardAtId(newTreasureDeck, 153); // LvlUp
		//AddCardAtId(newTreasureDeck, 159); // LvlUp
		//AddCardAtId(newTreasureDeck, 165); // Expl
		//AddCardAtId(newTreasureDeck, 163); // Expl
		//AddCardAtId(newTreasureDeck, 152); // LvlUp
		//AddCardAtId(newTreasureDeck, 164); // Expl
		//AddCardAtId(newTreasureDeck, 166); // Expl

		CardManagerData.allTreasureCards = newTreasureDeck;
	}

	private void AddCardAtId(List<Card> deck, int id) {
		Card card = CardManagerData.allDoorCards.Find(c => c.id == id);
		if (card == null)
			card = CardManagerData.allTreasureCards.Find(c => c.id == id);
		
		deck.Add(card);
	}
}