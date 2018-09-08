using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

	public static Server Instance { get; set; }

	private const int MAX_CONNECTION = 2;
	private int connectionNumber = 0;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;

	private bool isStarted = false;
	private byte error;

	private void Start() {
		//C# version
		//Debug.Log(System.Environment.Version);

		Instance = this;

		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();

		reliableChannel = cc.AddChannel(QosType.Reliable);
		//unreliableChannel = cc.AddChannel(QosType.Unreliable);

		HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

		hostId = NetworkTransport.AddHost(topo, port, null);
		webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

		isStarted = true;
	}

	private void Update() {
		if (!isStarted)
			return;

		int recHostId;
		int connectionId;
		int channelId;
		byte[] recBuffer = new byte[1024];
		int bufferSize = 1024;
		int dataSize;
		byte err;

		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out err);

		switch (recData) {
			
			case NetworkEventType.ConnectEvent:
				Debug.Log("Player " + connectionId + " has connected");
				OnConnection(connectionId);
				break;

			case NetworkEventType.DataEvent:
				string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
				Debug.Log("Receiving from " + connectionId + " : " + msg);
				GameManager.Instance.debugText.text = "Receiving from " + connectionId + " : " + msg;
				string[] splitData = msg.Split('|');

				switch (splitData[0]) {
					case SendNames.nameis:
						OnNameIs(splitData[1], connectionId);
						break;

					case SendNames.trydropcard:
						TryDrop(int.Parse(splitData[1]), splitData[2], splitData[3]);
						break;

					case SendNames.endturn:
						EndTurn(int.Parse(splitData[1]));
						break;

					case SendNames.cardtodrop:
						DropSelectedCard(int.Parse(splitData[1]), splitData[2]);
						break;

					default:
						Debug.Log("Invalid message: " + msg);
						break;
				}

				break;

			case NetworkEventType.DisconnectEvent:
				Debug.Log("Player " + connectionId + " has disconnected");
				OnDicconnection(connectionId);
				break;
		}
	}

	// Receive
	private void OnDicconnection(int cnnId) {
		if (GameManager.Instance.player1.info.connectionId == cnnId && GameManager.Instance.player1 != null)
			GameManager.Instance.player1 = null;
		else
			GameManager.Instance.player2 = null;
		
		connectionNumber--;
	}

	private void OnNameIs(string playerName, int cnnId) {
		Player p = new Player {
			info = new PlayerInfo {
				connectionId = cnnId,
				name = playerName,
				number = connectionNumber
			}
		};
		if (connectionNumber == 0)
			GameManager.Instance.player1 = p;
		else
			GameManager.Instance.player2 = p;

		connectionNumber++;

		if (connectionNumber == 2) {
			Send_StartGameInfo();
			GameManager.Instance.StarGame();
		}
	}

	private void TryDrop(int pNum, string parentSlotId, string targetSlot) {
		GameManager.Instance.TryDropCard(pNum, parentSlotId, targetSlot);
	}
	private void EndTurn(int pNum) {
		GameManager.Instance.turnController.TryChangeTurn(pNum);
	}

	private void DropSelectedCard(int pNum, string data) {
		GameManager.Instance.warTable.ClearTable();

		string[] slotIdArr = data.Split('%');

		foreach(string slotId in slotIdArr) {
			if (slotId.StartsWith("HA", System.StringComparison.CurrentCulture))
				GameManager.Instance.GetPlayerAt(pNum).munchkin.hand.RemoveCard(slotId);
			else
				GameManager.Instance.GetPlayerAt(pNum).munchkin.GetSlotById(slotId).RemoveCard();
		}
		//GameManager.Instance.GetPlayerAt(pNum).munchkin.hand.RemoveEmptySlots(); // TODO: Rework

		EndTurn(pNum);
	}

	// Send
	private void OnConnection(int cnnId) {
		string msg = SendNames.askname + "|" + cnnId;
		Send(msg, reliableChannel, cnnId);
	}

	public void Send_TurnAllowed(int pNum, string slotId, string targetSlot, int cardId) {
		string msg = SendNames.dropallowed + "|" + pNum + "|" + slotId + "|" + targetSlot + "|" + cardId;
		Send(msg, reliableChannel);

		Send_NewValues();
	}
	public void Send_TurnDisllowed(int pNum, string slotId, string reason) {
		string msg = SendNames.dropdisallowed + "|" + slotId + "|" + reason;
		Send(msg, reliableChannel, GameManager.Instance.GetPlayerAt(pNum).info.connectionId);
	}

	public void Send_CardToHand(int pNum, Card card) {
		Debug.Log("SendCardToHand pNum: " + pNum + " card.id: " + card.id);
		Send_CardToPlayerHand(GameManager.Instance.player1, pNum, card);
		Send_CardToPlayerHand(GameManager.Instance.player2, pNum, card);
	}
	private void Send_CardToPlayerHand(Player player, int pNum, Card card) {
		int cardId = player.info.number == pNum ? card.id : 0;
		string msg = SendNames.cardtohand + "|" + pNum + "|" + card.deckType + "|" + cardId;

		Send(msg, reliableChannel, player.info.connectionId);
	}

	public void Send_ChangeTurn(TurnStage stage, int playerTurnNumber, int time) {
		string msg = SendNames.newstage + "|" + playerTurnNumber + "|" + stage + "|" + time;
		Send(msg, reliableChannel);
	}
	public void Send_OpenDoor(int playerTurnNumber, int cardId, bool isMonster) {
		string msg = SendNames.opendoor + "|" + playerTurnNumber + "|" + cardId + "|";
		if (isMonster)
			msg += 1 + "|" + GameManager.Instance.warTable.PlayerDmg + "|" + GameManager.Instance.warTable.MonsterDmg;
		else
			msg += 0;

		Send(msg, reliableChannel);

		if (isMonster)
			Send_NewValues();
	}

	public void Send_EndFight(bool playerWin) {
		string msg = SendNames.endfigth + "|" + (playerWin ? 1 : 0);
		Send(msg, reliableChannel);

		Send_NewValues();
	}
	public void Send_TakeCardFromWT() {
		string msg = SendNames.takecardfromwt + "|" + GameManager.Instance.turnController.CurPlayerTurnNum;
		Send(msg, reliableChannel);
	}

	public void Send_SelectionCard(List<string> cards, int numberOfCardsToDrop) {
		string msg = SendNames.cardselectionstage + "|" + numberOfCardsToDrop + "|";
		foreach (string slotId in cards) {
			msg += slotId + "%";
		}
		msg = msg.Trim('%');
		Send(msg, reliableChannel, GameManager.Instance.GetCurPlayer().info.connectionId);

		string msg2 = SendNames.en_cardselectionstage + "|" + numberOfCardsToDrop;
		Send(msg2, reliableChannel, GameManager.Instance.GetUncurPlayer().info.connectionId);
	}

	public void Send_RemoveCard(int pNum, string slotId) {
		string msg = SendNames.removecard + "|" + pNum + "|" + slotId;
		Send(msg, reliableChannel);

		Send_NewValues();
	}

	public void Send_StartGameInfo() {
		string msg = SendNames.startgame;
		msg += "|" + GameManager.Instance.player1.info.connectionId + "%" + GameManager.Instance.player1.info.name + "%" + GameManager.Instance.player1.info.number;
		msg += "|" + GameManager.Instance.player2.info.connectionId + "%" + GameManager.Instance.player2.info.name + "%" + GameManager.Instance.player2.info.number;

		Send(msg, reliableChannel);
	}

	public void Send_NewValues() {
		string msg = SendNames.values;
		msg += "|" + GameManager.Instance.player1.munchkin.Damage;
		msg += "|" + GameManager.Instance.player1.munchkin.lvl;
		msg += "|" + GameManager.Instance.player2.munchkin.Damage;
		msg += "|" + GameManager.Instance.player2.munchkin.lvl;
		msg += "|" + GameManager.Instance.warTable.MonsterDmg;

		if (GameManager.Instance.turnController.CurrentTurnStage == TurnStage.fight_player
		    || GameManager.Instance.turnController.CurrentTurnStage == TurnStage.fight_enemy)
			msg += "|" + GameManager.Instance.warTable.PlayerDmg;
		else
			msg += "|" + 0;

		msg += "|" + GameManager.Instance.turnController.CurrentTurnStage;
			

		Send(msg, reliableChannel);
	}

	public void Send_HidwWeapon(int pNum) {
		string msg = SendNames.hideweapon + "|" + pNum;
		Send(msg, reliableChannel);
	}
	public void Send_ShowWeapon(int pNum) {
		string msg = SendNames.showweapon + "|" + pNum;
		Send(msg, reliableChannel);
	}

	private void Send(string message, int channelId, int cnnId) {
		Debug.Log("Sending to " + cnnId + ": " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);
		NetworkTransport.Send(hostId, cnnId, channelId, msg, message.Length * sizeof(char), out error);
	}
	private void Send(string message, int channelId) {
		Send(message, channelId, GameManager.Instance.player1.info.connectionId);
		Send(message, channelId, GameManager.Instance.player2.info.connectionId);
	}
}