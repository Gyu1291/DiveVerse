using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    public AudioSource audio;
    public Button title_find, title_create, create_create, room_leave, room_start, error_ok, find_back, sceneBtn1, sceneBtn2;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        title_find.onClick.AddListener(PlaySound);
        title_create.onClick.AddListener(PlaySound);
        create_create.onClick.AddListener(PlaySound);
        room_leave.onClick.AddListener(PlaySound);
        room_start.onClick.AddListener(PlaySound);
        error_ok.onClick.AddListener(PlaySound);
        find_back.onClick.AddListener(PlaySound);
        sceneBtn1.onClick.AddListener(PlaySound);
        sceneBtn2.onClick.AddListener(PlaySound);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnButtonClick()
    {
        Debug.Log("Button Clicked");
        audio.Play();
    }

    private void PlaySound()
    {
        audio.Play();
    }
}
