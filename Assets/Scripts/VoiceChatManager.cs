using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
	string appID = "295e0216a3264758be6fdd56e141cf0b";//보안상의 이유로 따로 전달

	public static VoiceChatManager Instance;

	IRtcEngine rtcEngine;

	void Awake()
	{
		if(Instance)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	void Start()
	{
		if(string.IsNullOrEmpty(appID))
		{
			Debug.LogError("App ID not set in VoiceChatManager script");
			return;
		}

		rtcEngine = IRtcEngine.GetEngine(appID);
		Debug.Log("Success Agora!");
		rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
		rtcEngine.OnLeaveChannel += OnLeaveChannel;
		//rtcEngine.OnError += OnError;

		rtcEngine.EnableSoundPositionIndication(true);
	}

	void OnError(int error, string msg)
	{
		Debug.LogError("Error with Agora: " + msg);
	}

	void OnLeaveChannel(RtcStats stats)
	{
		Debug.Log("Left channel with duration " + stats.duration);
	}

	void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
	{
		Debug.Log("Joined channel " + channelName);

		Hashtable hash = new Hashtable();
		hash.Add("agoraID", uid.ToString());
		PhotonNetwork.SetPlayerCustomProperties(hash); //Customproperty를 통한 정보 관리
	}

	public IRtcEngine GetRtcEngine()
	{
		return rtcEngine;
	}

	public override void OnJoinedRoom()
	{
		rtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name); //Photon과 연관되어, 현재 RoomName에 따라 채널 연결
	}

	public override void OnLeftRoom()
	{
		rtcEngine.LeaveChannel();
	}

	void OnDestroy()
	{
		IRtcEngine.Destroy();
	}
}
