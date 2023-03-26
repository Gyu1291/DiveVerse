using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ControllerChange : MonoBehaviourPun, IPunObservable
{
    private PlayerController playerController;
    private ThreeDPoseScript aIController;
    // Start is called before the first frame update
    private Animator animator;
    private PhotonAnimatorView pAV;
    private bool isKeyboardControl = true;
    private bool oldKeyboardControl = true;
    private bool stateChanged = false;
    private PhotonView pv;
    private PhotonPoseView poseView;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        aIController = GetComponent<ThreeDPoseScript>();
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        pAV = GetComponent<PhotonAnimatorView>();
        poseView = GetComponent<PhotonPoseView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && pv.IsMine) //자신의 것인 경우 input으로부터 state결정하기
        {
            if (isKeyboardControl)
            {
                playerController.enabled = false;
                pAV.enabled = false;
                animator.enabled = false;
                aIController.enabled = true;
                isKeyboardControl = false;
            }
            else
            {
                animator.enabled = true;
                pAV.enabled=true;
                playerController.enabled = true;
                aIController.enabled = false;
                isKeyboardControl = true;
            }
        }

        if (!pv.IsMine && stateChanged)
        {
            Debug.Log($"stateChanged: {stateChanged}, isKeyboardControl:{isKeyboardControl}");
            if (!isKeyboardControl)
            {
                playerController.enabled = false;
                animator.enabled = false;
                aIController.enabled = true;
            }
            else
            {
                animator.enabled = true;
                playerController.enabled = true;
                aIController.enabled = false;
            }
            stateChanged = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Write
        if(pv!= null)
        {
            if (stream.IsWriting)
            {
                if (pv.IsMine) //자신의 것인 경우 write진행하기
                {
                    stream.SendNext((bool)isKeyboardControl); //local threeDPoseScript.jointPoints

                }
            }
            else
            {
                if (!pv.IsMine) //자신의 것이 아닌 경우 read만 진행하기, 전적으로 이것에 의존해 state결정됨
                {
                    isKeyboardControl = (bool)stream.ReceiveNext();
                    if (oldKeyboardControl != isKeyboardControl)
                    {
                        stateChanged = true;
                        oldKeyboardControl = isKeyboardControl;
                    }
                }
            }
        }
    }

}
