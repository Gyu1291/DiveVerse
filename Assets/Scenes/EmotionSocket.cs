using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Photon.Pun;
public class EmotionSocket : MonoBehaviourPun, IPunObservable
{

    private Socket socketServer;
    private const int bufSize = 8 * 1024;
    private State state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv = null;
    private PhotonView pv;


    public TMP_Text emoji;
    public string socketMsg = "<sprite=0>";

    
    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }

    public void Server(string address, int port)
    {
        socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socketServer.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        socketServer.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        socketServer.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
        {
            try
            {
                State so = (State)ar.AsyncState;
                int bytes = socketServer.EndReceiveFrom(ar, ref epFrom);
                socketServer.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                // https://docs.unity3d.com/kr/530/ScriptReference/Debug.LogFormat.html
                Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));

                socketMsg = Encoding.ASCII.GetString(so.buffer, 0, bytes);

            }
            catch
            {

            }

        }, state);
    }


    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            Server("127.0.0.1", 8875);
        }
        else
        {

        }
    }

    void Update()
    {
        if(socketMsg != null)
        {
            if (socketMsg == "neutral")
            {
                emoji.text = "<sprite=0>";
            } else if(socketMsg == "surprise")
            {
                emoji.text = "<sprite=7>";
            } else if(socketMsg == "happy")
            {
                emoji.text = "<sprite=5>";
            } else if(socketMsg == "sad")
            {
                emoji.text = "<sprite=15>";
            }
            else
            {
                emoji.text = "<sprite=12>";
            }
        }
    }

    

    void OnApplicationQuit()
    {
        socketServer.Close();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Write
        if(pv != null)
        {
            if (stream.IsWriting)
            {
                if (pv.IsMine) //자신의 것인 경우 write진행하기
                {
                    stream.SendNext((string)socketMsg); //local threeDPoseScript.jointPoints

                }
            }
            else
            {
                if (!pv.IsMine) //자신의 것이 아닌 경우 read만 진행하기, 전적으로 이것에 의존해 state결정됨
                {
                    socketMsg = (string)stream.ReceiveNext();
                }
            }
        }
    }
}