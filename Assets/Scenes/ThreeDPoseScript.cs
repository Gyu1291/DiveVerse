/*
 * UseWebCam : WebCam을 사용한다면 true, video file을 사용한다면 false로 설정한다. 240번 줄.
 * 파일명을 바꿔야 한다면, "public class ThreeDPoseScript : MonoBehaviour"을 수정해주어야 함.
 * Unity avatar는 "bone"의 rotation을 바꾸면서 다양한 포즈를 취할 수 있다.
 * 이 코드에서 사용하는 AI 모델은 "관절"의 위치를 반환하기 때문에,
 * 관절의 position을 bone의 rotation으로 바꾸는 연산 과정이 있다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Photon.Pun;

// Ex.  jointPoint[5]  ==>  유니티쨩 avatar의 왼쪽 어깨 관절을 나타냄.
public enum PositionIndex : int
{
    rShldrBend = 0,
    rForearmBend,
    rHand,
    rThumb2,
    rMid1,

    lShldrBend,
    lForearmBend,
    lHand,
    lThumb2,
    lMid1,

    lEar,
    lEye,
    rEar,
    rEye,
    Nose,

    rThighBend,
    rShin,
    rFoot,
    rToe,

    lThighBend,
    lShin,
    lFoot,
    lToe,

    abdomenUpper,

    //Calculated coordinates
    // 모델을 통해 직접 구하지는 않고, 다른 좌표값들로부터 추론해서 구하는 것들.
    hip,
    head,
    neck,
    spine,

    Count,
    None,
}

public static partial class EnumExtend
{
    public static int Int(this PositionIndex i)
    {
        return (int)i;
    }
}

public class ThreeDPoseScript : MonoBehaviour
{

    int framecount;    // 현재 frame 수

    /***************** 통신 관련 *******************/
    public Socket socketServer;
    private const int bufSize = 8 * 1024;
    private State state = new State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv = null;

    public JObject payload = null;    // Mediapipe 로부터 받은 관절의 정보가 담김.

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
                // Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));

                payload = JObject.Parse(Encoding.ASCII.GetString(so.buffer, 0, bytes));

            }
            catch { }

        }, state);
        /********************************************/
    }

    void OnEnable()
    {
        Init();
    }


    void OnApplicationQuit()
    {
        socketServer.Close();
    }



    // Ex.  payload["LMs"][MPIndex[2]]  ==>  Mediapipe로부터 받은 오른쪽 손목 관절의 정보를 나타냄.
    public string[] MPIndex =
    {
      "RIGHT_SHOULDER",
      "RIGHT_ELBOW",
      "RIGHT_WRIST",
      "RIGHT_THUMB",
      "RIGHT_INDEX",

      "LEFT_SHOULDER",
      "LEFT_ELBOW",
      "LEFT_WRIST",
      "LEFT_THUMB",
      "LEFT_INDEX",

      "LEFT_EAR",
      "LEFT_EYE",
      "RIGHT_EAR",
      "RIGHT_EYE",
      "NOSE",

      "RIGHT_HIP",
      "RIGHT_KNEE",
      "RIGHT_HEEL",
      "RIGHT_FOOT_INDEX",

      "LEFT_HIP",
      "LEFT_KNEE",
      "LEFT_HEEL",
      "LEFT_FOOT_INDEX",
    };

    // 유니티쨩의 각 관절
    public class JointPoint
    {
        public Vector2 Pos2D = new Vector2();
        public float score2D;

        public Vector3 Pos3D = new Vector3();
        public Vector3 Now3D = new Vector3();
        public Vector3 PrevPos3D = new Vector3();
        public float score3D;

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;

        public JointPoint Child = null;
    }

    // Joint position and bone
    public JointPoint[] jointPoints;

    private Vector3 initPosition; // Initial center position

    // UnityChan
    public GameObject UnityChan;
    public GameObject Nose;
    private Animator anim;

    // For camera play
    public bool UseWebCam;
    private WebCamTexture webCamTexture;

    // For video play


    private Texture2D texture;
    private int videoScreenWidth = 2560;
    private float videoWidth, videoHeight;
    public float clipScale;


    //public GameObject TextureObject;
    private PhotonView pv;

    private const int inputImageSize = 224;
    private const int JointNum = 24;

    void Start()
    {
        framecount = 0;
        pv = UnityChan.GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            Server("127.0.0.1", 8885);    // localhost 서버를 연다.
        }

        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for (var i = 0; i < PositionIndex.Count.Int(); i++) jointPoints[i] = new JointPoint();

        anim = UnityChan.GetComponent<Animator>();
        Init();
    }



    // 유니티쨩의 bone transform 을 jointPoints[] 와 연결해준다.
    void Init()
    {

        // Right Arm
        jointPoints[PositionIndex.rShldrBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[PositionIndex.rForearmBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.rHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.rThumb2.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.rMid1.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        // Left Arm
        jointPoints[PositionIndex.lShldrBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[PositionIndex.lForearmBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[PositionIndex.lHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.lThumb2.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.lMid1.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

        // Face
        jointPoints[PositionIndex.lEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.lEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[PositionIndex.rEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.rEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[PositionIndex.Nose.Int()].Transform = Nose.transform;

        // Right Leg
        jointPoints[PositionIndex.rThighBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[PositionIndex.rShin.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.rFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.rToe.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightToes);

        // Left Leg
        jointPoints[PositionIndex.lThighBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.lShin.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.lFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.lToe.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftToes);

        // etc
        jointPoints[PositionIndex.abdomenUpper.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.hip.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[PositionIndex.head.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.neck.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.spine.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);

        // Child Settings
        // Right Arm
        jointPoints[PositionIndex.rShldrBend.Int()].Child = jointPoints[PositionIndex.rForearmBend.Int()];
        jointPoints[PositionIndex.rForearmBend.Int()].Child = jointPoints[PositionIndex.rHand.Int()];

        // Left Arm
        jointPoints[PositionIndex.lShldrBend.Int()].Child = jointPoints[PositionIndex.lForearmBend.Int()];
        jointPoints[PositionIndex.lForearmBend.Int()].Child = jointPoints[PositionIndex.lHand.Int()];

        // Fase

        // Right Leg
        jointPoints[PositionIndex.rThighBend.Int()].Child = jointPoints[PositionIndex.rShin.Int()];
        jointPoints[PositionIndex.rShin.Int()].Child = jointPoints[PositionIndex.rFoot.Int()];
        jointPoints[PositionIndex.rFoot.Int()].Child = jointPoints[PositionIndex.rToe.Int()];

        // Left Leg
        jointPoints[PositionIndex.lThighBend.Int()].Child = jointPoints[PositionIndex.lShin.Int()];
        jointPoints[PositionIndex.lShin.Int()].Child = jointPoints[PositionIndex.lFoot.Int()];
        jointPoints[PositionIndex.lFoot.Int()].Child = jointPoints[PositionIndex.lToe.Int()];

        // etc
        jointPoints[PositionIndex.spine.Int()].Child = jointPoints[PositionIndex.neck.Int()];
        jointPoints[PositionIndex.neck.Int()].Child = jointPoints[PositionIndex.head.Int()];
        //jointPoints[PositionIndex.head.Int()].Child = jointPoints[PositionIndex.Nose.Int()];

        // Set Inverse
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child);
            }
        }
        initPosition = jointPoints[PositionIndex.hip.Int()].Transform.position;
        var forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Transform.position, jointPoints[PositionIndex.lThighBend.Int()].Transform.position, jointPoints[PositionIndex.rThighBend.Int()].Transform.position);
        jointPoints[PositionIndex.hip.Int()].Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));

        // For Head Rotation
        jointPoints[PositionIndex.head.Int()].InitRotation = jointPoints[PositionIndex.head.Int()].Transform.rotation;
        var gaze = jointPoints[PositionIndex.Nose.Int()].Transform.position - jointPoints[PositionIndex.head.Int()].Transform.position;
        jointPoints[PositionIndex.head.Int()].Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));

        jointPoints[PositionIndex.lHand.Int()].InitRotation = jointPoints[PositionIndex.lHand.Int()].Transform.rotation;
        jointPoints[PositionIndex.lHand.Int()].Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Transform.position - jointPoints[PositionIndex.lMid1.Int()].Transform.position));

        jointPoints[PositionIndex.rHand.Int()].InitRotation = jointPoints[PositionIndex.rHand.Int()].Transform.rotation;
        jointPoints[PositionIndex.rHand.Int()].Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Transform.position - jointPoints[PositionIndex.rMid1.Int()].Transform.position));
    }

    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    private Quaternion GetInverse(JointPoint p1, JointPoint p2)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position));
    }


    private void CameraPlayStart()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture(devices[0].name);

        GameObject videoScreen = GameObject.Find("VideoScreen");
        RawImage screen = videoScreen.GetComponent<RawImage>();
        var sd = screen.GetComponent<RectTransform>();
        screen.texture = webCamTexture;

        webCamTexture.Play();

        sd.sizeDelta = new Vector2(videoScreenWidth, (int)(videoScreenWidth * webCamTexture.height / webCamTexture.width));

        texture = new Texture2D(webCamTexture.width, webCamTexture.height);
    }


    void Update()
    {
        PoseUpdate((result) => { //Debug.Log(result.eulerAngles.ToString());
        });
    }

    // Pose Update
    void PoseUpdate(Action<Quaternion> result)
    {
        //var img = ResizeTexture(texture);

        // "관절"의 위치 예측
        Predict();

        // 센터의 이동과 회전
        var forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
        jointPoints[PositionIndex.hip.Int()].Transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.01f + new Vector3(initPosition.x, 0f, initPosition.z);
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].Inverse * jointPoints[PositionIndex.hip.Int()].InitRotation;

        // 각 "bone"의 회전
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.Inverse * jointPoint.InitRotation;
            }
        }

        // Head Rotation
        var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        var head = jointPoints[PositionIndex.head.Int()];
        head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.Inverse * head.InitRotation;

        // Wrist rotation (Test code)
        var lf = TriangleNormal(jointPoints[PositionIndex.lHand.Int()].Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        var lHand = jointPoints[PositionIndex.lHand.Int()];
        lHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.Inverse * lHand.InitRotation;
        var rf = TriangleNormal(jointPoints[PositionIndex.rHand.Int()].Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        var rHand = jointPoints[PositionIndex.rHand.Int()];
        rHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.Inverse * rHand.InitRotation;

        result(rHand.Transform.rotation);
        //yield return null;
    }



    /// <summary>
    /// Predict
    /// </summary>
    /// <param name="img"></param>
    public void Predict()
    {
        // Mediapipe로부터 받아온 관절의 좌표를 저장
        for (var j = 0; j < JointNum - 1; j++)
        {
            if ((float)payload["LMs"][MPIndex[j]]["visibility"] > 0.5)
            {
                jointPoints[j].Now3D.x = (float)payload["LMs"][MPIndex[j]]["x"] * (float)inputImageSize;    // 부호 바꾸면 머리가 뒤집힘...
                jointPoints[j].Now3D.y = (1f - (float)payload["LMs"][MPIndex[j]]["y"]) * (float)inputImageSize;    // 부호 바꿔줘야 함. 안그러면 물구나무 섬...
                jointPoints[j].Now3D.z = (float)payload["LMs"][MPIndex[j]]["z"] * (float)inputImageSize;    // 부호 그대로.
            }
        }

        // abdomenUpper (배) 의 위치는 Mediapipe에서 가져올 수 없기 때문에, 대충 hip과 shoulder의 위치를 이용해 계산하여 간접적으로 구함.
        jointPoints[JointNum - 1].Now3D
            = (jointPoints[PositionIndex.rShldrBend.Int()].Now3D + jointPoints[PositionIndex.lShldrBend.Int()].Now3D) / 2f * 0.25f
            + (jointPoints[PositionIndex.rThighBend.Int()].Now3D + jointPoints[PositionIndex.lThighBend.Int()].Now3D) / 2f * 0.75f;

        // Calculate hip location
        var lc = (jointPoints[PositionIndex.rThighBend.Int()].Now3D + jointPoints[PositionIndex.lThighBend.Int()].Now3D) / 2f;
        jointPoints[PositionIndex.hip.Int()].Now3D = (jointPoints[PositionIndex.abdomenUpper.Int()].Now3D + lc) / 2f;
        // Calculate neck location
        jointPoints[PositionIndex.neck.Int()].Now3D = (jointPoints[PositionIndex.rShldrBend.Int()].Now3D + jointPoints[PositionIndex.lShldrBend.Int()].Now3D) / 2f;
        // Calculate head location
        var cEar = (jointPoints[PositionIndex.rEar.Int()].Now3D + jointPoints[PositionIndex.lEar.Int()].Now3D) / 2f;
        var hv = cEar - jointPoints[PositionIndex.neck.Int()].Now3D;
        var nhv = Vector3.Normalize(hv);
        var nv = jointPoints[PositionIndex.Nose.Int()].Now3D - jointPoints[PositionIndex.neck.Int()].Now3D;
        jointPoints[PositionIndex.head.Int()].Now3D = jointPoints[PositionIndex.neck.Int()].Now3D + nhv * Vector3.Dot(nhv, nv);
        // Calculate spine location
        jointPoints[PositionIndex.spine.Int()].Now3D = jointPoints[PositionIndex.abdomenUpper.Int()].Now3D;

        // Low pass filter
        foreach (var jp in jointPoints)
        {
            jp.Pos3D = jp.PrevPos3D * 0.5f + jp.Now3D * 0.5f;
            jp.PrevPos3D = jp.Pos3D;
        }
    }

    /// <summary>
    /// Resize Texture and Convrt to Mat
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    /**private Mat ResizeTexture(Texture2D src)
    {
        float bbLeft = clipRect.xMin;
        float bbRight = clipRect.xMax;
        float bbTop = clipRect.yMin;
        float bbBottom = clipRect.yMax;
        float bbWidth = clipRect.width;
        float bbHeight = clipRect.height;

        float videoLongSide = (videoWidth > videoHeight) ? videoWidth : videoHeight;
        float videoShortSide = (videoWidth > videoHeight) ? videoHeight : videoWidth;
        float aspectWidth = videoWidth / videoShortSide;
        float aspectHeight = videoHeight / videoShortSide;

        float left = bbLeft;
        float right = bbRight;
        float top = bbTop;
        float bottom = bbBottom;

        left /= videoShortSide;
        right /= videoShortSide;
        top /= videoShortSide;
        bottom /= videoShortSide;

        src.filterMode = FilterMode.Trilinear;
        src.Apply(true);

        RenderTexture rt = new RenderTexture(224, 224, 32);
        Graphics.SetRenderTarget(rt);
        GL.LoadPixelMatrix(left, right, bottom, top);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new UnityEngine.Rect(0, 0, aspectWidth, aspectHeight), src);

        UnityEngine.Rect dstRect = new UnityEngine.Rect(0, 0, 224, 224);
        Texture2D dst = (Texture2D)TextureObject.GetComponent<Renderer>().material.mainTexture;
        dst.ReadPixels(dstRect, 0, 0, true);
        Graphics.SetRenderTarget(null);
        Destroy(rt);

        dst.Apply();

        TextureObject.GetComponent<Renderer>().material.mainTexture = dst;

        // Convrt to Mat
        Color32[] c = dst.GetPixels32();
        var m = new Mat(224, 224, MatType.CV_8UC3);
        var videoSourceImageData = new Vec3b[224 * 224];
        for (var i = 0; i < 224; i++)
        {
            for (var j = 0; j < 224; j++)
            {
                var col = c[j + i * 224];
                var vec3 = new Vec3b
                {
                    Item0 = col.b,
                    Item1 = col.g,
                    Item2 = col.r
                };
                videoSourceImageData[j + i * 224] = vec3;
            }
        }
        m.SetArray(0, 0, videoSourceImageData);

        return m.Flip(FlipMode.X);
    }**/
}
