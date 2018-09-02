public class SingleSlot {
	protected string slotId;
	protected Card SelfCard;

	public bool IsEmpty() {
		return SelfCard == null;
	}

	public virtual void AddCard(Card card) {
		if (SelfCard != null)
			GameManager.Instance.treasurePile.Add(SelfCard);

		SelfCard = card;
	}
	public virtual void RemoveCard() {
		if (SelfCard != null) {
			GameManager.Instance.treasurePile.Add(SelfCard);
			Server.Instance.Send_RemoveCard(GameManager.Instance.GetCurPlayer().info.number, slotId);
		}

		SelfCard = null;
	}

	public string GetSlotId() {
		return slotId;
	}
}

public class ThingCardSlot: SingleSlot {
	public ThingCardSlot(ThingCard.ThingType sType, string sId) {
		slotType = sType;
		slotId = sId;
	}

	public ThingCard.ThingType slotType;
	public ThingCard GetCard() {
		return SelfCard as ThingCard;
	}

	public void OnClassChanges(int newClassNumber) {
		if (!(SelfCard as ThingCard).restriction.Contain(newClassNumber))
			RemoveCard();
	}
}

public class ClassCardSlot: SingleSlot {
	public ClassCardSlot() {
		slotId = "CL";
	}

	public ClassCard GetCard() {
		return SelfCard as ClassCard;
	}
	public int GetClassNumber() {
		return SelfCard == null ? 4 : (int)(SelfCard as ClassCard).className;
	}

	public override void AddCard(Card card) {
		base.AddCard(card);
		GameManager.Instance.GetCurPlayer().munchkin.OnRemoveClass();
	}
	public override void RemoveCard() {
		base.RemoveCard();
		GameManager.Instance.GetCurPlayer().munchkin.OnRemoveClass();
	}
}