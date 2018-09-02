using System.Collections;
using TMPro;
using UnityEngine;

public enum TurnStage {
	preparation,
	waiting,
	after_door,
	fight_player,
	fight_enemy,
	select_cards,
	completion
}

public class TurnController : MonoBehaviour {
	
	private TurnStage currentTurnStage;
	public TurnStage CurrentTurnStage { get { return currentTurnStage; } }

	private readonly int[] turnTimeArr = {
		20,	// preparation
		3,	// waiting
		15,	// after_door
		15,	// fight_player
		15,	// fight_enemy
		20,	// select_cards
		10	// completion
	};
	private int TurnTime { get { return turnTimeArr[(int)currentTurnStage]; } }

	private int turnNumber;
	public int CurPlayerTurnNum { get { return turnNumber % 2; } }

	// for debug
	public TextMeshProUGUI timeText;
	public TextMeshProUGUI stageText;
	public TextMeshProUGUI playerText;

	public void TryChangeTurn(int pNum) {
		bool isPlayerTurn = GameManager.Instance.turnController.CurPlayerTurnNum == GameManager.Instance.GetPlayerAt(pNum).info.number;
		bool canEndTurn = (isPlayerTurn ^ (currentTurnStage == TurnStage.fight_enemy)) && (currentTurnStage != TurnStage.waiting);

		if (canEndTurn)
			ChangeTurn();
	}
	public void StatFirstTurn() {
		currentTurnStage = TurnStage.preparation;
		turnNumber = 0;

		SendChangeTurn();

		StartCoroutine(TurnFunc());
	}
	public void MonsterPlayed() {
		StopAllCoroutines();

		currentTurnStage = TurnStage.fight_player;
		SendChangeTurn();

		StartCoroutine(TurnFunc());
	}

	private IEnumerator TurnFunc() {
		int timeToEndTurn = TurnTime;
		stageText.text = currentTurnStage.ToString();
		playerText.text = "pNum: " + CurPlayerTurnNum;

		while (timeToEndTurn >= 0) {
			timeText.text = timeToEndTurn.ToString();
			yield return new WaitForSeconds(1);
			timeToEndTurn--;
		}

		ChangeTurn();
	}

	public void ChangeTurn() {
		StopAllCoroutines();

		switch (currentTurnStage) {
			case TurnStage.preparation:
				OpenDoor();
				break;

			case TurnStage.waiting:
				AfterWait();
				break;

			case TurnStage.after_door:
				currentTurnStage = TurnStage.preparation;
				turnNumber++;
				SendChangeTurn();
				break;

			case TurnStage.fight_player:
				CheckWinAfterPlayerTurn();
				break;

			case TurnStage.fight_enemy:
				CheckWinAfterEnemyTurn();
				break;

			case TurnStage.select_cards:
				currentTurnStage = TurnStage.completion;
				SendChangeTurn();
				break;

			case TurnStage.completion:
				currentTurnStage = TurnStage.preparation;
				turnNumber++;
				SendChangeTurn();
				break;
		}

		StartCoroutine(TurnFunc());
	}

	private void OpenDoor() {
		bool isMonster;
		GameManager.Instance.OpenDoor(out isMonster);

		if (isMonster)
			currentTurnStage = TurnStage.fight_player;
		else
			currentTurnStage = TurnStage.waiting;

		SendChangeTurn();
	}
	private void AfterWait() {
		GameManager.Instance.warTable.UseCardInWT(CurPlayerTurnNum);

		currentTurnStage = TurnStage.after_door;
	}
	private void CheckWinAfterPlayerTurn() {
		if (GameManager.Instance.warTable.PlayerCanWin) {
			currentTurnStage = TurnStage.fight_enemy;
		}
		else {
			// lose
			bool needSelection;

			GameManager.Instance.warTable.OnLose(out needSelection);

			if (needSelection) {
				currentTurnStage = TurnStage.select_cards;
			}
			else {
				currentTurnStage = TurnStage.completion;
				Server.Instance.Send_EndFight(playerWin: false);
			}
		}
		SendChangeTurn();
	}
	private void CheckWinAfterEnemyTurn() {
		if (GameManager.Instance.warTable.PlayerCanWin) {
			// win
			GameManager.Instance.OnPlayerWinFight();
			currentTurnStage = TurnStage.completion;

			Server.Instance.Send_EndFight(playerWin: true);
		}
		else {
			currentTurnStage = TurnStage.fight_player;
		}

		SendChangeTurn();
	}

	public void SendChangeTurn() {
		Server.Instance.Send_ChangeTurn(currentTurnStage, CurPlayerTurnNum, TurnTime);
	}
}
