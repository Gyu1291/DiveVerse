using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class SocketServer : MonoBehaviour
{
  public Socket socketServer;
  private const int bufSize = 8 * 1024;
  private State state = new State();
  private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
  private AsyncCallback recv = null;

  public JObject payload = null;


  public TMP_Text TMP_SampleText;
  public string socketMsg = "Sample Text";


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

        this.payload = JObject.Parse(Encoding.ASCII.GetString(so.buffer, 0, bytes));

        // payload["LMs"]["NOSE"]["x"]
        // payload["LMs"]["NOSE"]["y"]
        // payload["LMs"]["NOSE"]["z"]
        // ...
        // ...

        this.socketMsg = (string)Encoding.ASCII.GetString(so.buffer, 0, bytes);

      }
      catch
      {

      }

    }, state);
  }


  void Start()
  {
    Server("127.0.0.1", 8885);
  }

  void Update()
  {
    TMP_SampleText.text = this.socketMsg;

  }

  void OnApplicationQuit()
  {
    socketServer.Close();
  }

}