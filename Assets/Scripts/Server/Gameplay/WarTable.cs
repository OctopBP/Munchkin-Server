using System.Collections.Generic;

public class WarTable {
	
	private readonly List<Card> playerCards = new List<Card>();
	private readonly List<Card> monsterCards = new List<Card>();
	// TODO: Add Card Slot waiting Card

	public int playerDmg = 0;
	public int monsterDmg = 0;

	public bool PlayerCanWin { get { return playerDmg > monsterDmg; } }

	public void PlayCard(Card card, bool playerDS) {
		if (card.cardType == Card.CardType.LVLUP) {
			GameManager.Instance.GetCurPlayer().munchkin.LvlUp(1);
			return;
		}

		if (card.cardType == Card.CardType.MONSTER) {
			StartFight(card);
			GameManager.Instance.turnController.MonsterPlayed();
			return;
		}

		if (card.cardType == Card.CardType.EXPLOSIVE) {
			if (playerDS)
				playerCards.Add(card);
			else
				monsterCards.Add(card);

			CalculateDmg();
		}
	}
	public void StartFight(Card monster) {
		monsterCards.Add(monster);
		CalculateDmg();
	}
	public void OpenCard(Card card) {
		playerCards.Add(card);
	}

	public void CalculateDmg() {
		monsterDmg = 0;
		foreach (Card card in monsterCards) {
			if (card.cardType == Card.CardType.MONSTER)
				monsterDmg += (card as MonsterCard).lvl;
			else
				monsterDmg += (card as ExplosiveCard).dmg;
		}

		playerDmg = GameManager.Instance.GetCurPlayer().munchkin.Damage;
		foreach (Card card in playerCards) {
			if (card.cardType == Card.CardType.EXPLOSIVE)
				playerDmg += (card as ExplosiveCard).dmg;
		}
	}

	public void PlaseCardToHand(int pNum) {
		Munchkin munchkin = GameManager.Instance.GetPlayerAt(pNum).munchkin;
		munchkin.hand.Add(playerCards[0]);
		munchkin.SetCloseId();

		playerCards.RemoveAt(0);
	}
	public void ClearTable() {
		playerDmg = 0;
		monsterDmg = 0;

		playerCards.Clear();
		monsterCards.Clear();
	}

	public int GetNumberOfTreasure() {
		int number = 0;

		foreach (Card card in monsterCards)
			if (card.cardType == Card.CardType.MONSTER)
				number += (card as MonsterCard).numberOfTreasure;

		return number;
	}

	public Card GetCardInWT() {
		return playerCards[0];
	}

	/*
	public void PlayCard(CardInfo card, bool playerDS) {
		if (card.typeIs(Card.CardType.LVLUP)) {
			playerSide.AddCard(card);
			gameManager.player.LvlUp(1);
			Destroy(card.gameObject);
			return;
		}

		if (card.typeIs(Card.CardType.MONSTER) && gameManager.CurrentTS_Is(TurnStage.after_door)) {
			//gameManager.turnController.MonsterPlayed();
			AddCard(card, false);
		}

		if (card.typeIs(Card.CardType.EXPLOSIVE) && gameManager.CurrentTS_Is(TurnStage.fight_player)) {
			AddCard(card, playerDS);
			CalculateDmg();
		}
	}

	public void AddCard(CardInfo card, bool playerDS) {
		if (playerDS)
			playerSide.AddCard(card);
		else
			monsterSide.AddCard(card);
	}

	public void StartFight(CardInfo monster) {
		AddCard(monster, false);
		CalculateDmg();
	}

	public void CalculateDmg() {
		monsterSide.dmg = 0;
		foreach (CardInfo card in monsterSide.cards) {
			if (card.typeIs(Card.CardType.MONSTER))
				monsterSide.dmg += (card.selfCard as MonsterCard).lvl;
			else
				monsterSide.dmg += (card.selfCard as ExplosiveCard).dmg;
		}

		playerSide.dmg = gameManager.player.damage;
		foreach (CardInfo card in playerSide.cards) {
			if (card.typeIs(Card.CardType.EXPLOSIVE))
				playerSide.dmg += (card.selfCard as ExplosiveCard).dmg;
		}

		monsterSide.dmgText.text = monsterSide.dmg.ToString();
		playerSide.dmgText.text = playerSide.dmg.ToString();
	}

	// TODO: Remove from Update()
	void Update() {
		if (gameManager.CurrentTS_Is(TurnStage.fight_enemy) || gameManager.CurrentTS_Is(TurnStage.fight_player)) {
			CalculateDmg();
		}
		else {
			monsterSide.dmgText.text = "0";
			playerSide.dmgText.text = "0";
		}
	}



	public void PlaseCardToHand() {
		gameManager.player.hand.AddCard(playerSide.cards[0]);
		playerSide.cards.RemoveAt(0);
	}


	*/
}