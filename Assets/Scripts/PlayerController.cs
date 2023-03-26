using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks
{
  [SerializeField] GameObject cameraHolder;

  [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed1, walkSpeed2, jumpForce, smoothTime;

  float verticalLookRotation;
  bool grounded;
  bool isRunning;
  bool isMoving;
  bool isEmotionEnabled = false;
  bool isEnabledDebugOutput = false;
  float movDirx = 0;
  float movDirz = 0;
  Vector3 smoothMoveVelocity;
  Vector3 moveAmount;

  Rigidbody rb;

  PhotonView PV;

  private float currentSpeed;
  PlayerManager playerManager;

  public Animator animator;
  public GameObject emotionBox;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
    PV = GetComponent<PhotonView>();

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
  }

  void Start()
  {
    if (!PV.IsMine)
    {
      AudioListener al = GetComponent<AudioListener>();
      al.enabled = false;
      Destroy(GetComponentInChildren<Camera>().gameObject);
      Destroy(rb);
    }
    else
    {
            PV.RPC("SetNickName", RpcTarget.All);
    }
  }

  void Update()
  {
    if (!PV.IsMine)
      return;

    updateAnimation();
    Look();
    Move();
    Jump();
    if (Input.GetKeyDown(KeyCode.I))
    {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
    }
    else if(Input.GetKeyDown(KeyCode.U))
    {
            PV.RPC("OpenEmotion", RpcTarget.All, PhotonNetwork.PlayerList[PV.OwnerActorNr]);
    }

  }

  void Look()
  {
    transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

    verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity * 0.2f;
    verticalLookRotation = Mathf.Clamp(verticalLookRotation, -35f, 35f);

    cameraHolder.transform.Find("Camera").transform.localEulerAngles = Vector3.left * verticalLookRotation;
  }

  void Move()
  {
    Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

    if (moveDir.magnitude > 0)
    {
      isMoving = true;
      movDirx = moveDir.x;
      movDirz = moveDir.z;
    }
    else
    {
      isMoving = false;
    }

    if (Input.GetKey(KeyCode.LeftShift))
    {
      currentSpeed = sprintSpeed;
      isRunning = true;
    }
    else
    {
      if (movDirz > 0)
      {
        currentSpeed = walkSpeed1;

      }
      else
      {
        currentSpeed = walkSpeed2;
      }
      isRunning = false;
    }
    moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref smoothMoveVelocity, smoothTime);
  }

  void Jump()
  {
    if (Input.GetKeyDown(KeyCode.Space) && grounded)
    {
      rb.AddForce(transform.up * jumpForce);
    }
  }

  public void SetGroundedState(bool _grounded)
  {
    grounded = _grounded;
  }

  void FixedUpdate()
  {
    if (!PV.IsMine)
      return;

    rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
  }

  void updateAnimation()
  {
    if (!grounded) //jump
    {
      animator.SetInteger("State", 0);
    }
    else if (isRunning) //run
    {
      animator.SetInteger("State", 1);
    }
    else if (isMoving) //walk
    {
      if (isEnabledDebugOutput)
        Debug.Log($"{movDirx}, {movDirz}");

      if (movDirz > 0)
      {
        animator.SetInteger("State", 2); //walk forward
      }
      else
      {
        if (movDirx > 0)
        {
          animator.SetInteger("State", 4); //walk right
        }
        else if (movDirz < 0)
        {
          animator.SetInteger("State", 5); //walk left
        }
        else
        {
          animator.SetInteger("State", 6); //walk backward
        }
      }
    }
    else if (Input.GetKeyDown(KeyCode.H))
    {
      animator.SetInteger("State", 7); //hello
    }
    else if (Input.GetKeyDown(KeyCode.K))
    {
      animator.SetInteger("State", 8); //dance
    }
    else //idle
    {
      animator.SetInteger("State", 3);
    }
  }
  [PunRPC]

    public void OpenEmotion()
    {
        if (isEmotionEnabled)
        {
            emotionBox.SetActive(false);
            isEmotionEnabled = false;
            GetComponent<EmotionSocket>().enabled = false;
        }
        else
        {
            emotionBox.SetActive(true);
            isEmotionEnabled = true;
            GetComponent<EmotionSocket>().enabled = true;
        }
    }

    [PunRPC]
    public void SetNickName(string nickname)
    {
        transform.Find("Name").transform.Find("Image").transform.Find("NameText").GetComponent<TMP_Text>().text = nickname;
    }
}