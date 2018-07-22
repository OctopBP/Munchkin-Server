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
						TryDrop(int.Parse(splitData[1]), int.Parse(splitData[2]), splitData[3]);
						break;

					case SendNames.endturn:
						EndTurn(int.Parse(splitData[1]));
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

	private void OnConnection(int cnnId) {
		string msg = SendNames.askname + "|" + cnnId;
		Send(msg, reliableChannel, cnnId);
	}
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
			Debug.Log("Start game");

			string msg = SendNames.startgame;
			msg += "|" + GameManager.Instance.player1.info.connectionId + "%" + GameManager.Instance.player1.info.name + "%" + GameManager.Instance.player1.info.number;
			msg += "|" + GameManager.Instance.player2.info.connectionId + "%" + GameManager.Instance.player2.info.name + "%" + GameManager.Instance.player2.info.number;

			Send(msg, reliableChannel);

			GameManager.Instance.StarGame();
		}
	}

	private void TryDrop(int pNum, int cardId, string targetSlot) {
		GameManager.Instance.TryDropCard(pNum, cardId, targetSlot);
	}

	public void SendTurnAllowed(int pNum, int cardId, int closeId, string targetSlot) {
		string msg = SendNames.dropallowed + "|" + pNum + "|" + cardId + "|" + closeId + "|" + targetSlot;
		Send(msg, reliableChannel);
	}
	public void SendTurnDisllowed(int pNum, int cardId, string reason) {
		string msg = SendNames.dropdisallowed + "|" + cardId + "|" + reason;
		Send(msg, reliableChannel, GameManager.Instance.GetPlayerAt(pNum).info.connectionId);
	}

	private void EndTurn(int pNum) {
		GameManager.Instance.turnController.TryChangeTurn(pNum);
	}

	public void SendCardToHand(int pNum, Card card) {
		Debug.Log("SendCardToHand pNum: " + pNum + " card.id: " + card.id);
		SendCardToPlayerHand(GameManager.Instance.player1, pNum, card);
		SendCardToPlayerHand(GameManager.Instance.player2, pNum, card);
	}
	private void SendCardToPlayerHand(Player player, int pNum, Card card) {
		int cardId = player.info.number == pNum ? card.id : 0;
		string msg = SendNames.cardtohand + "|" + pNum + "|" + card.deckType + "|" + cardId;

		Send(msg, reliableChannel, player.info.connectionId);
	}

	public void SendChangeTurn(TurnStage stage, int playerTurnNumber) {
		string msg = SendNames.newstage + "|" + playerTurnNumber + "|" + stage;
		Send(msg, reliableChannel);
	}
	public void SendOpenDoor(int playerTurnNumber, int cardId, bool isMonster) {
		string msg = SendNames.opendoor + "|" + playerTurnNumber + "|" + cardId + "|";
		if (isMonster) 
			msg += 1 + "|" + GameManager.Instance.warTable.playerDmg + "|" + GameManager.Instance.warTable.monsterDmg;
		else
			msg += 0;
		
		Send(msg, reliableChannel);
	}

	public void SendEndFight(bool playerWin) {
		string msg = SendNames.endfigth + "|" + (playerWin ? 1 : 0);
		Send(msg, reliableChannel);
	}
	public void SendTakeCardFromWT() {
		string msg = SendNames.takecardfromwt + "|" + GameManager.Instance.turnController.CurPlayerTurnNum;
		Send(msg, reliableChannel);
	}

	public void SendRemoveCard(int pNum, string cardSlot) {
		string msg = SendNames.removecard + "|" + pNum + "|" + cardSlot;
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