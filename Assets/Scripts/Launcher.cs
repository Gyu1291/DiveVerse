using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;

	[SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;

	private string roomName;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true; //automatically sync scene with room master
	}

	public override void OnJoinedLobby()
	{
		MenuManager.Instance.OpenMenu("Title");
		Debug.Log("Joined Lobby");
	}

	public void CreateRoom(int sceneNumber)
	{
		if(string.IsNullOrEmpty(roomName))
		{
			return;
		}
		RoomOptions roomOptions = new RoomOptions();
		ExitGames.Client.Photon.Hashtable cp = new ExitGames.Client.Photon.Hashtable();
		cp.Add("Scene", sceneNumber);
		Debug.Log("SceneNumber: "+sceneNumber);
		roomOptions.CustomRoomProperties = cp;
		PhotonNetwork.CreateRoom(roomName, roomOptions);
		MenuManager.Instance.OpenMenu("Loading");
	}

	public void roomNameSelect()
    {
		if (string.IsNullOrEmpty(roomNameInputField.text))
		{
			return;
		}
		roomName = roomNameInputField.text;
		MenuManager.Instance.OpenMenu("Scene Select");
    }

	public void JoinRandom()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinedRoom()
	{
		MenuManager.Instance.OpenMenu("Room");
		roomNameText.text = PhotonNetwork.CurrentRoom.Name;

		Player[] players = PhotonNetwork.PlayerList;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		errorText.text = "Room Creation Failed: " + message;
		Debug.LogError("Room Creation Failed: " + message);
		MenuManager.Instance.OpenMenu("Error");
	}

	public void StartGame()
	{
		int sceneNum = (int)PhotonNetwork.CurrentRoom.CustomProperties["Scene"];
		PhotonNetwork.LoadLevel(sceneNum); //DayDemo = 2, NightDemo = 3
	}

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		MenuManager.Instance.OpenMenu("Loading");
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		MenuManager.Instance.OpenMenu("Loading");
	}

	public override void OnLeftRoom()
	{
		MenuManager.Instance.OpenMenu("Title");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach(Transform trans in roomListContent)
		{
			Destroy(trans.gameObject);
		}

		for(int i = 0; i < roomList.Count; i++)
		{
			if(roomList[i].RemovedFromList)
				continue;
			Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}
}