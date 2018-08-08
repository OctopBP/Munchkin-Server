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

	public void UseCardInWT(int CurPlayerTurnNum) {
		if (playerCards[0].cardType == Card.CardType.CLASS) {
			PlaseCardToHand(CurPlayerTurnNum);

			Server.Instance.Send_TakeCardFromWT();
		}
		else {
			CardAbilitys.Instance.Invoke((GameManager.Instance.warTable.GetCardInWT() as TrapCard).ability, 0);
			Server.Instance.Send_NewValues();
			Server.Instance.Send_ChangeTurn(TurnStage.after_door, CurPlayerTurnNum);

			ClearTable();
		}
	}
	public void PlaseCardToHand(int pNum) {
		Munchkin munchkin = GameManager.Instance.GetPlayerAt(pNum).munchkin;
		munchkin.hand.Add(playerCards[0]);

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

	// TODO: Rework
	public Card GetCardInWT() {
		return playerCards[0];
	}
}