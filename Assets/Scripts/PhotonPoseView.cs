// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [AddComponentMenu("Photon Networking/Photon Pose View")]
    public class PhotonPoseView : MonoBehaviourPun, IPunObservable
    {
        public bool m_SynchronizePosition = true;
        public ThreeDPoseScript threeDPoseScript;
        private JObject networkJointPoints;


        private void Awake()
        {
            networkJointPoints = new JObject();   //initial data settings
        }

        public void Update()
        {
            if (!photonView.IsMine) //if Character is not mine...
            {
                threeDPoseScript.payload = networkJointPoints;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // Write
            if (stream.IsWriting)
            {
                if (m_SynchronizePosition)
                {
                    string strObj = JsonConvert.SerializeObject(threeDPoseScript.payload);
                    stream.SendNext(strObj); //local threeDPoseScript.jointPoints
                }
            }
            else
            {
                if (m_SynchronizePosition)
                {
                    string networkStrObj = (string)stream.ReceiveNext();
                    networkJointPoints = JsonConvert.DeserializeObject<JObject>(networkStrObj);
                }
            }
        }
    }
}