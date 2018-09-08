using System.Collections.Generic;

public class Hand {
	// TODO: HandCardSlot унаследовать от SingleSlot
	// переработать Ремувы

	public List<HandCardSlot> cardsSlots = new List<HandCardSlot>();

	public void Add(Card card) {
		string sId = "HA" + (cardsSlots.Count + 1);
		cardsSlots.Add(new HandCardSlot(sId, card));
		SetId();
	}

	/// <summary>
	/// Remove Card by Slot ID and Remove slot.
	/// </summary>
	/// <param name="slotId">Card slot id.</param>
	public void TakeCard(string slotId) {
		GetSlotAtId(slotId).RemoveCard();
		cardsSlots.Remove(GetSlotAtId(slotId));
	}
	/// <summary>
	/// Add Card to Pile deck, Send_RemoveCard().
	/// Remove Card by Slot ID and Remove slot
	/// </summary>
	/// <param name="slotId">Card slot id.</param>
	public void RemoveCard(string slotId) {
		GameManager.Instance.treasurePile.Add(GetSlotAtId(slotId).GetCard());
		Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, slotId);

		TakeCard(slotId);
	}

	public void SetId() {
		int i = 0;
		foreach (HandCardSlot slot in cardsSlots) {
			slot.SetSlotId("HA" + i);
			i++;
		}
	}

	public HandCardSlot GetSlotAtId(string sId) {
		return cardsSlots.Find(slot => slot.GetSlotId() == sId);
	}
}

public class HandCardSlot {
	private string slotId;
	private Card SelfCard;

	public HandCardSlot(string sId, Card c) {
		slotId = sId;
		SelfCard = c;
	}

	public void AddCard(Card card) {
		SelfCard = card;
	}
	public void RemoveCard() {
		SelfCard = null;
	}

	public void SetSlotId(string sId) {
		slotId = sId;
	}

	public string GetSlotId() {
		return slotId;
	}
	public Card GetCard() {
		if (SelfCard != null)
			return SelfCard;
		return null;
	}
}