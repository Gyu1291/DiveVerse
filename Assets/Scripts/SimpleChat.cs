using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.EventSystems;
public class SimpleChat : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_InputField inputField;
    public Button sendButton;
    private PhotonView pv;
    public GameObject chatBox;
    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            GameObject go = GameObject.Find("InGameUI");
            inputField = go.transform.Find("InputField").GetComponent<TMP_InputField>();
            sendButton = go.transform.Find("Button").GetComponent<Button>();
            sendButton.onClick.AddListener(SendChat);
        }
        else
        {
            enabled = false;
        }
    }

    void SendChat()
    {
        StartCoroutine(ChatBubble());
    }

    void Update()
    {

    }

    IEnumerator ChatBubble()
    {
        if (inputField.text != null)
        {
            if (inputField.text.Length < 200)
            {
                string content = inputField.text;
                pv.RPC("chatBoxAble", RpcTarget.All, content);
                EventSystem.current.SetSelectedGameObject(null);
                inputField.text = "";
                yield return new WaitForSeconds(2.5f);
                pv.RPC("chatBoxDisable", RpcTarget.All);
            }
        }
        yield return null;
    }

    [PunRPC]
    public void chatBoxAble(string content)
    {
        chatBox.SetActive(true);
        chatBox.transform.Find("Image").transform.Find("Text").GetComponent<TMP_Text>().text = content;
    }

    [PunRPC]
    public void chatBoxDisable()
    {
        chatBox.SetActive(false);
    }
}
