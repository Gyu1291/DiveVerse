using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
public class InstantiateHeart : MonoBehaviourPun
{
    [SerializeField]
    public float DestroyTime = 2.7f;
    private GameObject _player;
    private Vector3[] wayPoints = new Vector3[2];
    private Vector3 wayPoint1, wayPoint2 = new Vector3(0, 0, 0);

    bool exist = false;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        // trigger Beams active state
        if (this.photonView.IsMine)
        {
            ProcessInputs();
        }
    }

    void ProcessInputs()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (!exist)
            {
                Vector3 mainCamRotation = Camera.main.transform.rotation.eulerAngles;
                Vector3 forwardRotation = new Vector3(0, mainCamRotation.y, 0);
                _player = this.gameObject;
                GameObject Heart = PhotonNetwork.Instantiate("PhotonViewHeart", new Vector3(_player.transform.position.x, _player.transform.position.y + 1.3f,
                _player.transform.position.z), Quaternion.Euler(270, 0, 0), 0);

                wayPoint1.Set(_player.transform.position.x + 0.250f, _player.transform.position.y + 1.7f, _player.transform.position.z - 0.25f);
                wayPoint2.Set(_player.transform.position.x + 0.255f, _player.transform.position.y + 2f, _player.transform.position.z - 0.25f);

                wayPoints.SetValue(wayPoint1, 0);
                wayPoints.SetValue(wayPoint2, 1);

                Heart.transform.DOPath(wayPoints, DestroyTime).SetEase(Ease.OutQuad);
                DestroyHeart(Heart);
            }
        }
    }

    static async void DestroyHeart(GameObject heart)
    {
        await Task.Delay(3000);
        PhotonNetwork.Destroy(heart);
    }
}
