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
	public int numberOfTreasure = 1;
}
[Serializable] public class ClassCard: Card {
	public ClassName className;
	public enum ClassName {
		WANDERER,
		PALADIN,
		RAIDER,
		SCIENTIST
	}
}
[Serializable] public class LvlupCard: Card {
}
[Serializable] public class ThingCard: Card {
	public int[] restriction = { 0, 1, 2, 3, 4 }; // 4 - нет класса
	public int bonus;

	public ThingType thingType;
	public enum ThingType {
		WEAPON,
		HEAD,
		ARMOR,
		SHOES
	}
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

		AddCardAtId(newDoorDeck, 32); // Monster
		AddCardAtId(newDoorDeck, 39); // Monster
		AddCardAtId(newDoorDeck, 21); // Trap
		AddCardAtId(newDoorDeck, 45); // Monster
		AddCardAtId(newDoorDeck, 7);  // Class
		AddCardAtId(newDoorDeck, 46); // Monster
		AddCardAtId(newDoorDeck, 49); // Monster
		AddCardAtId(newDoorDeck, 24); // Trap
		AddCardAtId(newDoorDeck, 59); // Monster
		AddCardAtId(newDoorDeck, 26); // Trap
		AddCardAtId(newDoorDeck, 28); // Trap
		AddCardAtId(newDoorDeck, 63); // Monster
		AddCardAtId(newDoorDeck, 8);  // Class
		AddCardAtId(newDoorDeck, 9);  // Class
		AddCardAtId(newDoorDeck, 10); // Class

		CardManagerData.allDoorCards = newDoorDeck;
	}
	public void MakeTreasureDeck() {
		List<Card> newTreasureDeck = new List<Card>();

		AddCardAtId(newTreasureDeck, 170); // Thing
		AddCardAtId(newTreasureDeck, 172); // Thing
		AddCardAtId(newTreasureDeck, 150); // LvlUp
		AddCardAtId(newTreasureDeck, 153); // LvlUp
		AddCardAtId(newTreasureDeck, 159); // LvlUp
		AddCardAtId(newTreasureDeck, 179); // Thing
		AddCardAtId(newTreasureDeck, 165); // Expl
		AddCardAtId(newTreasureDeck, 181); // Thing
		AddCardAtId(newTreasureDeck, 163); // Expl
		AddCardAtId(newTreasureDeck, 152); // LvlUp
		AddCardAtId(newTreasureDeck, 201); // Thing
		AddCardAtId(newTreasureDeck, 206); // Thing
		AddCardAtId(newTreasureDeck, 164); // Expl
		AddCardAtId(newTreasureDeck, 208); // Thing
		AddCardAtId(newTreasureDeck, 212); // Thing
		AddCardAtId(newTreasureDeck, 166); // Expl

		CardManagerData.allTreasureCards = newTreasureDeck;
	}

	private void AddCardAtId(List<Card> deck, int id) {
		Card card = CardManagerData.allDoorCards.Find(c => c.id == id);
		if (card == null)
			card = CardManagerData.allTreasureCards.Find(c => c.id == id);
		
		deck.Add(card);
	}
}