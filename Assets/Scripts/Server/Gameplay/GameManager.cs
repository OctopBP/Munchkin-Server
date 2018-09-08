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
	public Player GetUncurPlayer() {
		return turnController.CurPlayerTurnNum == 0 ? player2 : player1;
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
		GiveHandCards(numberOfCards: 1, sp: player1, deck: doorDeck);
		GiveHandCards(numberOfCards: 4, sp: player1, deck: treasureDeck);
		GiveHandCards(numberOfCards: 1, sp: player2, deck: doorDeck);
		GiveHandCards(numberOfCards: 4, sp: player2, deck: treasureDeck);
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

	public void TryDropCard(int pNum, string slotId, string targetSlotId) {
		Card card = GetPlayerAt(pNum).munchkin.hand.GetSlotAtId(slotId).GetCard();

		if (card == null) {
			TurnDisallowed(pNum, slotId, "no card in hand");
			return;
		}

		switch (card.cardType) {
			case Card.CardType.EXPLOSIVE:
				if (targetSlotId == "WTM" || targetSlotId == "WTP") {
					if (((turnController.CurrentTurnStage == TurnStage.fight_player) && (turnController.CurPlayerTurnNum == pNum)) ||
						((turnController.CurrentTurnStage == TurnStage.fight_enemy) && (turnController.CurPlayerTurnNum != pNum))) {

						warTable.PlayCard(card, targetSlotId == "WTP");
						TurnAllowed(pNum, slotId, card.id, targetSlotId);
						return;
					}
				}
				break;

			case Card.CardType.LVLUP:
				if (targetSlotId == "WTM" || targetSlotId == "WTP") {
					if (turnController.CurPlayerTurnNum == pNum) {
						warTable.PlayCard(card, true);
						TurnAllowed(pNum, slotId, card.id, targetSlotId);
						return;
					}
				}
				break;

			case Card.CardType.THING:
				if ((GetPlayerAt(pNum).munchkin.GetSlotById(targetSlotId) as ThingCardSlot).slotType == (card as ThingCard).thingType) {
					if (new TurnStage[] { TurnStage.preparation, TurnStage.completion, TurnStage.after_door }.Contain(turnController.CurrentTurnStage)
						&& turnController.CurPlayerTurnNum == pNum) {

						int classNameIndex = GetPlayerAt(pNum).munchkin.classSlot.GetClassNumber();
						if ((card as ThingCard).restriction.Contain(classNameIndex)) {

							// Если игрок кладёт одноручное оружие вместо двуручного возврафаям слот для второго оружия
							if (targetSlotId == "W1") {
								if (!GetPlayerAt(pNum).munchkin.weapon1Slot.IsEmpty()) {
									if (GetPlayerAt(pNum).munchkin.weapon1Slot.GetCard().twoHandWeapon) {
										if (!(card as ThingCard).twoHandWeapon)
											Server.Instance.Send_ShowWeapon(pNum);
									}
								}
							}

							// Если игрок кладёт в слот двуручное оружие - убираем оружие из другого слота
							if ((card as ThingCard).twoHandWeapon) {
								if (targetSlotId == "W1") {
									if (!GetPlayerAt(pNum).munchkin.weapon2Slot.IsEmpty()) {
										GetPlayerAt(pNum).munchkin.weapon2Slot.RemoveCard();
									}
									Server.Instance.Send_HidwWeapon(pNum);
								}
								else if (targetSlotId == "W2") {
									targetSlotId = "W1";
									if (!GetPlayerAt(pNum).munchkin.weapon1Slot.IsEmpty()) {
										GetPlayerAt(pNum).munchkin.weapon1Slot.RemoveCard();
									}
									Server.Instance.Send_HidwWeapon(pNum);
								}
							}

							GetPlayerAt(pNum).munchkin.GetSlotById(targetSlotId).AddCard(card as ThingCard);
							TurnAllowed(pNum, slotId, card.id, targetSlotId);
							return;
						}
					}
				}
				break;

			case Card.CardType.CLASS:
				if (new TurnStage[] { TurnStage.preparation, TurnStage.completion, TurnStage.after_door }.Contain(turnController.CurrentTurnStage)
						&& turnController.CurPlayerTurnNum == pNum) {
					if (targetSlotId == GetPlayerAt(pNum).munchkin.classSlot.GetSlotId()) {
						GetPlayerAt(pNum).munchkin.classSlot.AddCard(card);
						TurnAllowed(pNum, slotId, card.id, targetSlotId);
						return;
					}
				}
				break;

			case Card.CardType.MONSTER:
				if (targetSlotId == "WTM" || targetSlotId == "WTP") {
					if (turnController.CurrentTurnStage == TurnStage.after_door && turnController.CurPlayerTurnNum == pNum) {
						warTable.PlayCard(card, false);
						TurnAllowed(pNum, slotId, card.id, targetSlotId);
						return;
					}
				}
				break;
		}
		TurnDisallowed(pNum, slotId, "reason");
	}
	private void TurnAllowed(int pNum, string slotId, int cardId, string targetSlot) {
		GetPlayerAt(pNum).munchkin.hand.TakeCard(slotId);

		Server.Instance.Send_TurnAllowed(pNum, slotId, targetSlot, cardId);
	}
	private void TurnDisallowed(int pNum, string slotId, string reason) {
		Server.Instance.Send_TurnDisllowed(pNum, slotId, reason);
	}

	public void OpenDoor() {
		if (doorDeck.Count == 0)
			return;

		Card card = doorDeck[0];
		doorDeck.RemoveAt(0);
		doorDeckCountText.text = doorDeck.Count + " Doors";

		bool isMonster = card.cardType == Card.CardType.MONSTER;

		if (isMonster)
			warTable.StartFight(card);
		else
			warTable.OpenCard(card);

		turnController.OpenDoor(isMonster);
		Server.Instance.Send_OpenDoor(turnController.CurPlayerTurnNum, card.id, isMonster);
	}

	public void GiveOneDoor() {
		GiveHandCards(1, GetCurPlayer(), doorDeck);
	}
	public void OnPlayerWinFight() {
		GiveHandCards(warTable.GetNumberOfTreasure(), GetCurPlayer(), treasureDeck);

		GetCurPlayer().munchkin.LvlUp(1);
		warTable.ClearTable();
	}

	public void AddCardToPile(Card card) {
		if (card.deckType == HidenCard.DeckType.TREASURE)
			treasurePile.Add(card);
		else
			doorPile.Add(card);
	}
}