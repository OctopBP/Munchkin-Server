using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player {
	public PlayerInfo info = new PlayerInfo();
	public Munchkin munchkin = new Munchkin();
}

public class GameManager: MonoBehaviour {

	public static GameManager Instance { get; set; }

	public Player player1, player2;

	public Player GetPlayerAt(int num) {
		return num == 0 ? player1 : player2;
	}
	public Player GetCurPlayer() {
		return GetPlayerAt(turnController.CurPlayerTurnNum);
	}

	public List<Card> doorDeck, treasureDeck, doorPile, treasurePile;

	public TurnController turnController;
	public WarTable warTable;

	public TextMeshProUGUI debugText;
	public TextMeshProUGUI doorDeckCountText;
	public TextMeshProUGUI treasureDeckCountText;

	private void Awake() {
		Instance = this;

		turnController = GetComponent<TurnController>();

		player1 = new Player();
		player2 = new Player();
		warTable = new WarTable();
	}

	private void Start() {
		doorDeck = CardManagerData.allDoorCards;
		treasureDeck = CardManagerData.allTreasureCards;

		doorPile = new List<Card>();
		treasurePile = new List<Card>();

		doorDeck.Shaffle();
		treasureDeck.Shaffle();
	}

	public void StarGame() {
		GiveStartCards();
		turnController.StatFirstTurn();
	}

	public void GiveStartCards() {
		GiveHandCards(numberOfCards: 3, sp: player1, deck: doorDeck);
		GiveHandCards(numberOfCards: 3, sp: player1, deck: treasureDeck);
		GiveHandCards(numberOfCards: 3, sp: player2, deck: doorDeck);
		GiveHandCards(numberOfCards: 3, sp: player2, deck: treasureDeck);
	}
	private void GiveHandCards(int numberOfCards, Player sp, List<Card> deck) {
		for (int i = 0; i < numberOfCards; i++)
			GiveCardToHand(sp, deck);
	}
	private void GiveCardToHand(Player sp, List<Card> deck) {
		if (deck.Count == 0)
			return;

		Card card = deck[0];
		deck.RemoveAt(0);
		doorDeckCountText.text = doorDeck.Count + " Doors";
		treasureDeckCountText.text = treasureDeck.Count + " Treasures";
		sp.munchkin.hand.Add(card);

		Server.Instance.Send_CardToHand(sp.info.number, card);
	}

	public void TryDropCard(int pNum, int cardId, string targetSlot) {
		Card card = GetPlayerAt(pNum).munchkin.hand.GetCardAtId(cardId);

		if (card == null) {
			TurnDisallowed(pNum, cardId, "no card in hand");
			return;
		}

		switch (card.cardType) {
			case Card.CardType.EXPLOSIVE:
				if (targetSlot == "WT_MONSTER" || targetSlot == "WT_PLAYER") {
					if (((turnController.currentTurnStage == TurnStage.fight_player) && (turnController.CurPlayerTurnNum == pNum)) ||
						((turnController.currentTurnStage == TurnStage.fight_enemy) && (turnController.CurPlayerTurnNum != pNum))) {

						warTable.PlayCard(card, targetSlot == "WT_PLAYER");
						TurnAllowed(pNum, card, targetSlot);
						return;
					}
				}
				break;

			case Card.CardType.LVLUP:
				if (targetSlot == "WT_MONSTER" || targetSlot == "WT_PLAYER") {
					if (turnController.CurPlayerTurnNum == pNum) {
						warTable.PlayCard(card, true);
						TurnAllowed(pNum, card, targetSlot);
						return;
					}
				}
				break;

			case Card.CardType.THING:
				if (GetPlayerAt(pNum).munchkin.GetSlotByName(targetSlot).slotType == (card as ThingCard).thingType) {
					if (new TurnStage[] { TurnStage.preparation, TurnStage.completion, TurnStage.after_door }.Contain(turnController.currentTurnStage)
						&& turnController.CurPlayerTurnNum == pNum) {

						int classNameIndex = GetPlayerAt(pNum).munchkin.classSlot.IsEmpty() ? 4 : (int)GetPlayerAt(pNum).munchkin.classSlot.GetCard().className;
						if ((card as ThingCard).restriction.Contain(classNameIndex)) {
							GetPlayerAt(pNum).munchkin.GetSlotByName(targetSlot).AddCard(card as ThingCard);
							TurnAllowed(pNum, card, targetSlot);
							return;
						}
					}
				}
				break;

			case Card.CardType.CLASS:
				if (targetSlot == "CLASS") {
					GetPlayerAt(pNum).munchkin.classSlot.AddCard(card);
					TurnAllowed(pNum, card, targetSlot);
					return;
				}
				break;

			case Card.CardType.MONSTER:
				if (targetSlot == "WT_MONSTER" || targetSlot == "WT_PLAYER") {
					if (turnController.currentTurnStage == TurnStage.after_door && turnController.CurPlayerTurnNum == pNum) {
						warTable.PlayCard(card, false);
						TurnAllowed(pNum, card, targetSlot);
						return;
					}
				}
				break;
		}
		TurnDisallowed(pNum, cardId, "reason");
	}
	private void TurnAllowed(int pNum, Card card, string targetSlot) {
		GetPlayerAt(pNum).munchkin.hand.Remove(card);

		Server.Instance.Send_TurnAllowed(pNum, card.id, card.closeId, targetSlot);
	}
	private void TurnDisallowed(int pNum, int cardId, string reason) {
		Server.Instance.Send_TurnDisllowed(pNum, cardId, reason);
	}

	public void OpenDoor(out bool isMonster) {
		isMonster = false;

		if (doorDeck.Count == 0)
			return;

		Card card = doorDeck[0];
		doorDeck.RemoveAt(0);
		doorDeckCountText.text = doorDeck.Count + " Doors";

		isMonster = card.cardType == Card.CardType.MONSTER;

		if (isMonster)
			warTable.StartFight(card);
		else
			warTable.OpenCard(card);

		Server.Instance.Send_OpenDoor(turnController.CurPlayerTurnNum, card.id, isMonster);
	}

	public void OnPlayerWinFight() {
		GiveHandCards(warTable.GetNumberOfTreasure(), GetCurPlayer(), treasureDeck);

		GetCurPlayer().munchkin.LvlUp(1);
		warTable.ClearTable();
	}
}