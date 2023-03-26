using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
  PlayerController playerController;

  public bool isEnabledDebugOutput = false;

  void Awake()
  {
    playerController = GetComponentInParent<PlayerController>();
  }

  void OnTriggerEnter(Collider other)
  {
    if (isEnabledDebugOutput) Debug.Log("6");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(true);
  }

  void OnTriggerExit(Collider other)
  {
    if (isEnabledDebugOutput) Debug.Log("5");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(false);
  }

  void OnTriggerStay(Collider other)
  {
    if (isEnabledDebugOutput) Debug.Log("4");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(true);
  }



  private void OnCollisionEnter(Collision other)
  {
    if (isEnabledDebugOutput) Debug.Log("1");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(true);
  }

  private void OnCollisionExit(Collision other)
  {
    if (isEnabledDebugOutput) Debug.Log("2");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(false);
  }

  private void OnCollisionStay(Collision other)
  {
    if (isEnabledDebugOutput) Debug.Log("3");
    if (other.gameObject == playerController.gameObject)
      return;

    playerController.SetGroundedState(true);
  }
}