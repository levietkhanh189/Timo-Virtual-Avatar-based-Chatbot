using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System;

public class TextToSpeechAPI : MonoBehaviour
{
    private string apiUrl = "https://api.openai.com/v1/audio/speech";

    public void ConvertToSpeech(string input, Action<bool, AudioClip> onProcess)
    {
        StartCoroutine(ConvertTextToSpeechOpenAI(input, onProcess));
    }

    public IEnumerator ConvertTextToSpeechOpenAI(string input,Action<bool, AudioClip> onProcess)
    {
        var requestBody = new Dictionary<string, string>
        {
            { "model", "tts-1" },     
            { "input", input },
            { "voice", "alloy" },       
            { "response_format", "pcm" } 
        };

        string json = JsonConvert.SerializeObject(requestBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + APIKey.Get());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            onProcess?.Invoke(false,null);
        }
        else
        {
            byte[] audioData = request.downloadHandler.data;

            float[] samples = Convert16BitToFloat(audioData);

            AudioClip clip = AudioClip.Create("ConvertedSpeech", samples.Length, 1, 24000, false);
            clip.SetData(samples, 0);
            onProcess?.Invoke(true, clip);
        }
    }

    private float[] Convert16BitToFloat(byte[] data)
    {
        float[] samples = new float[data.Length / 2];

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)BitConverter.ToInt16(data, i * 2) / short.MaxValue;
        }

        return samples;
    }
}
