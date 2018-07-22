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
		}
		catch(Exception e) {
			Debug.LogError(e.Message);
		}
	}
}