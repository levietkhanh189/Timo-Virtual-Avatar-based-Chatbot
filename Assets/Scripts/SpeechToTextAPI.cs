using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class SpeechToTextAPI : MonoBehaviour
{
    private string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
    private AudioClip recordedClip;
    private Action<string> onSuccess;
    public void StartRecording(Action<string> onSuccess)
    {
        this.onSuccess = onSuccess;
        int minFreq;
        int maxFreq;
        Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
        int freq = maxFreq != 0 ? maxFreq : 44100;

        recordedClip = Microphone.Start(null, false, 10, freq);
        StartCoroutine(WaitForRecording());
    }

    public void StopRecording()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }

    IEnumerator WaitForRecording()
    {
        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        byte[] wavData = ConvertAudioClipToWav(recordedClip);

        StartCoroutine(SendAudioToOpenAI(wavData));
    }

    byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] floats = new float[clip.samples * clip.channels];
        clip.GetData(floats, 0);

        byte[] bytes = new byte[floats.Length * 2];

        for (int ii = 0; ii < floats.Length; ii++)
        {
            short uint16 = (short)(floats[ii] * short.MaxValue);
            byte[] vs = BitConverter.GetBytes(uint16);
            bytes[ii * 2] = vs[0];
            bytes[ii * 2 + 1] = vs[1];
        }

        byte[] wav = new byte[44 + bytes.Length];

        byte[] header = {0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00,
                         0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
                         0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                         0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61 };

        Buffer.BlockCopy(header, 0, wav, 0, header.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(36 + bytes.Length), 0, wav, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(clip.channels), 0, wav, 22, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency), 0, wav, 24, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, wav, 28, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(clip.channels * 2), 0, wav, 32, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, wav, 40, 4);
        Buffer.BlockCopy(bytes, 0, wav, 44, bytes.Length);

        //File.WriteAllBytes(Application.dataPath + "/my.wav", wav);
        return wav;
    }

    IEnumerator SendAudioToOpenAI(byte[] audioData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "recording.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);
        request.SetRequestHeader("Authorization", "Bearer " + APIKey.Get());

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }

}
