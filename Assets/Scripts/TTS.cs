using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TTS : MonoBehaviour
{
    public AudioSource audioSource;
    public TMP_InputField inputField;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void ButtonClicked()
    {
        string str = inputField.text;
        inputField.text = "";
        StartCoroutine(DownloadTheAudio(str));
    }

    IEnumerator DownloadTheAudio(string str)
    {
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q={str}&tl=En-gl";
        WWW www = new WWW(url);
        yield return www;

        audioSource.clip = www.GetAudioClip(false, true, AudioType.MPEG);
        audioSource.Play();
    }
}
