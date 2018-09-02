using System.Collections.Generic;
using UnityEngine;

public class WarTable {
	
	private readonly List<Card> playerCards = new List<Card>();
	private readonly List<Card> monsterCards = new List<Card>();
	// TODO: Add Card Slot waiting Card

	public int PlayerDmg {
		get {
			int plaDmg = GameManager.Instance.GetCurPlayer().munchkin.Damage;
			foreach (Card card in playerCards) {
				if (card.cardType == Card.CardType.EXPLOSIVE)
					plaDmg += (card as ExplosiveCard).dmg;
			}
			return plaDmg;
		}
	}
	public int MonsterDmg {
		get {
			int monDmg = 0;
			foreach (Card card in monsterCards) {
				if (card.cardType == Card.CardType.MONSTER) {
					int classNumber = GameManager.Instance.GetCurPlayer().munchkin.classSlot.GetClassNumber();

					monDmg += (card as MonsterCard).lvl;
					monDmg += (card as MonsterCard).classBonus[classNumber];
				}
				else {
					monDmg += (card as ExplosiveCard).dmg;
				}
			}
			return monDmg;
		}
	}
	public bool PlayerCanWin { get { return PlayerDmg > MonsterDmg; } }

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
		}
	}
	public void StartFight(Card monster) {
		monsterCards.Add(monster);
	}
	public void OpenCard(Card card) {
		playerCards.Add(card);
	}

	public void UseCardInWT(int CurPlayerTurnNum) {
		if (playerCards[0].cardType == Card.CardType.CLASS) {
			PlaseCardToHand(CurPlayerTurnNum);

			Server.Instance.Send_TakeCardFromWT();
			GameManager.Instance.turnController.SendChangeTurn();
		}
		else {
			CardAbilitys.Instance.Invoke((playerCards[0] as TrapCard).ability, 0);
			Server.Instance.Send_NewValues();
			GameManager.Instance.turnController.SendChangeTurn();

			ClearTable();
		}
	}
	public void OnLose(out bool needSelection) {
		needSelection = GetMonster().onLoseType == MonsterCard.OnLoseType.CARD_SELECTION;

		if (needSelection) {
			LoseEvents.Instance.Invoke(GetMonster().onLose, 0);
		}
		else {
			if (GetMonster().onLoseType != MonsterCard.OnLoseType.NOTHING)
				LoseEvents.Instance.Invoke(GetMonster().onLose, 0);

			ClearTable();
		}
	}
	public void PlaseCardToHand(int pNum) {
		Munchkin munchkin = GameManager.Instance.GetPlayerAt(pNum).munchkin;
		munchkin.hand.Add(playerCards[0]);

		playerCards.RemoveAt(0);
	}
	public void ClearTable() {
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
	//public Card GetCardInWT() {
	//	return playerCards[0];
	//}
	private MonsterCard GetMonster() {
		foreach (Card card in monsterCards) {
			if (card.cardType == Card.CardType.MONSTER)
				return card as MonsterCard;
		}

		return null;
	}
}