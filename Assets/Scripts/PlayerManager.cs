using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.IO;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject controller;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			CreateController();
		}
	}

	void CreateController()
	{
		//Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
		//controller = PhotonNetwork.Instantiate("PlayerController", new Vector3(0,1.5f,3f), Quaternion.identity, 0, new object[] { PV.ViewID });
		controller = PhotonNetwork.Instantiate("AIController", new Vector3(0, 1.5f, 3f), Quaternion.identity, 0, new object[] { PV.ViewID });
		//controller.transform.Find("Name").transform.Find("Image").transform.Find("NameText").GetComponent<TMP_Text>().text = PhotonNetwork.NickName;
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}
}