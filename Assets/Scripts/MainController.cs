using System.Collections;
using System.Collections.Generic;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public ChatbotAPI chatbotAPI;
    public SpeechToTextAPI speechToTextAPI;
    public TextToSpeechAPI textToSpeechAPI;

    public AudioClip helloAudio;
    public VoiceHandler voiceHandler;
    public Button microphoneButton;

    private void Start()
    {
        StartCoroutine(PlayAudioClip(helloAudio));
    }

    IEnumerator PlayAudioClip(AudioClip audioClip)
    {
        do
        {
            yield return null;
        }
        while (voiceHandler.AudioSource == null);

        microphoneButton.interactable = false;

        voiceHandler.PlayAudioClip(audioClip);

        yield return new WaitForSeconds(audioClip.length);
        microphoneButton.interactable = true;
    }

    public void TextToSpeech(string respone)
    {
        textToSpeechAPI.ConvertToSpeech(respone, (bool complete, AudioClip audio) =>
        {
            if(complete == true)
            {
                StartCoroutine(PlayAudioClip(audio));
            }
            else
            {
                microphoneButton.interactable = true;
                Debug.LogWarning("Error :" + respone);
            }
        });
    }

    public void StartRecording()
    {
        speechToTextAPI.StartRecording(AskChatbot);
    }

    public void StopRecording()
    {
        speechToTextAPI.StopRecording();
    }

    public void AskChatbot(string query)
    {
        chatbotAPI.OnSubmitQuery(query, (bool complete, string respone) =>
        {
            if (complete == true)
            {
                microphoneButton.interactable = false;
                TextToSpeech(respone);
            }
            else
            {
                Debug.LogWarning("Error :" +respone);
            }
        });
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
