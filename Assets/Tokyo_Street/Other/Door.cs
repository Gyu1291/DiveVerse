using System;
using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Door : MonoBehaviour
{

  enum DOOR_STATE_DEF
  {
    DEFAULT = 0,

    ON_OPEN = 10,
    OPEN_TRG,
    OPENING,
    OPENING_CPLT,
    OPENED,

    ON_CLOSE = 20,
    CLOSE_TRG,
    CLOSING,
    CLOSING_CPLT,
    CLOSED,
  };

  enum DOOR_AS
  {
    LATCH_OPEN,
    CLOSED,
    MOVE,
    OPENED
  }



  // TODO: sound effect
  bool trig, open;
  bool isEnabledDebugOutput = false;
  public float smooth = 3.0f;
  public float DoorOpenAngle = 90.0f;
  private Vector3 defaulRot;
  private Vector3 openRot;

  public string soundPath = null;

  public AudioClip[] audioClip = new AudioClip[DOOR_AS.GetNames(typeof(DOOR_AS)).Length];
  private AudioSource audioSource;

  DOOR_STATE_DEF DAS = DOOR_STATE_DEF.DEFAULT; // (그 다스 아님)

  public Text txt;



  private void Awake()
  {
    soundPath = "file:///" + Application.dataPath + "/Sounds/";


    Debug.Log(soundPath);
    StartCoroutine(GetAudioClip());

    //audioSource = gameObject.GetComponent<AudioSource>();
    //audioSource = GetComponent<AudioSource>();

    audioSource = gameObject.AddComponent<AudioSource>();

  }


  IEnumerator GetAudioClip()
  {
    // TODO: for loop
    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip((soundPath + "door1_move.wav"), AudioType.WAV))
    {
      yield return www.SendWebRequest();
      audioClip[(int)DOOR_AS.MOVE] = DownloadHandlerAudioClip.GetContent(www) as AudioClip;
    }

    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip((soundPath + "wood_stop1.wav"), AudioType.WAV))
    {
      yield return www.SendWebRequest();
      audioClip[(int)DOOR_AS.CLOSED] = DownloadHandlerAudioClip.GetContent(www) as AudioClip;
    }

    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip((soundPath + "handle_pushbar_locked1.wav"), AudioType.WAV))
    {
      yield return www.SendWebRequest();
      audioClip[(int)DOOR_AS.OPENED] = DownloadHandlerAudioClip.GetContent(www) as AudioClip;
    }
  }


  void Start()
  {
    defaulRot = transform.eulerAngles;
    openRot = new Vector3(defaulRot.x, defaulRot.y + DoorOpenAngle, defaulRot.z);


  }

  // Update is called once per frame
  void Update()
  {
    if (open)
    {
      // 
      if (transform.localEulerAngles.y <= (DoorOpenAngle - 1.0f))
      {
        if (isEnabledDebugOutput)
          Debug.Log(transform.localEulerAngles.y);

        transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, openRot, Time.deltaTime * smooth);

        if (transform.localEulerAngles.y > (DoorOpenAngle - 3.0f))
        {
          if (DAS != DOOR_STATE_DEF.OPENED) DAS = DOOR_STATE_DEF.OPENING_CPLT;
        }
        else
        {
          DAS = DOOR_STATE_DEF.OPENING;

        }
      }      
    }
    else
    {
      

      transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, defaulRot, Time.deltaTime * smooth);

      if (transform.localEulerAngles.y <= 0.5f)
      {
        if (DAS != DOOR_STATE_DEF.CLOSED) DAS = DOOR_STATE_DEF.CLOSING_CPLT;
      }
      else
      {
        DAS = DOOR_STATE_DEF.CLOSING;
      }
    }

    if (Input.GetKeyDown(KeyCode.E) && trig)
    {

      if (isEnabledDebugOutput)
        Debug.Log("Pressed Btn E");

      if (open)
      {
        // on close event
        DAS = DOOR_STATE_DEF.CLOSE_TRG;
      }
      else
      {
        // on open event
        DAS = DOOR_STATE_DEF.OPEN_TRG;
        audioSource.PlayOneShot(audioClip[(int)DOOR_AS.MOVE], 0.7f);
        //audioSource.PlayOneShot(onDoorMoveAudioClip, 1.0f);
      }
      open = !open;
    }

    // Door Event triggered
    if (DAS == DOOR_STATE_DEF.CLOSING_CPLT)
    {
      audioSource.PlayOneShot(audioClip[(int)DOOR_AS.CLOSED], 0.5f);
      DAS = DOOR_STATE_DEF.CLOSED;
    }

    if (DAS == DOOR_STATE_DEF.OPENING_CPLT)
    {
      audioSource.PlayOneShot(audioClip[(int)DOOR_AS.OPENED], 0.3f);
      DAS = DOOR_STATE_DEF.OPENED;
    }



    if (trig)
    {
      if (open)
      {
        txt.text = "Close E";
      }
      else
      {
        txt.text = "Open E";
      }
    }
  }
  private void OnTriggerEnter(Collider coll)//вход и выход в\из  триггера 
  {

    if (coll.tag == "Player")
    {
      if (!open)
      {
        txt.text = "Close E ";
      }
      else
      {
        txt.text = "Open E";
      }
      trig = true;
    }
  }
  private void OnTriggerExit(Collider coll)//вход и выход в\из  триггера 
  {
    if (coll.tag == "Player")
    {
      txt.text = " ";
      trig = false;
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    //Debug.Log(collision);
  }

  private void OnCollisionStay(Collision collision)
  {
    //Debug.Log(collision);
  }
}
