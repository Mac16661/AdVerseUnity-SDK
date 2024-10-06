using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class AudioCaptureService : MonoBehaviour
{
    private const string BackendUrl = "https://content-ranking-1050562161100.us-central1.run.app/save-record-web-beta";
    private const int RecordingDuration = 5;
    private const int SampleRate = 44100;
    private AudioClip _audioClip;
    private string _filePath;

    private void Start()
    {
        InvokeRepeating(nameof(RecordAndSendAudio), 0f, RecordingDuration);
    }

    private void RecordAndSendAudio()
    {
        Debug.Log("Recording has started");
        _audioClip = Microphone.Start(null, false, RecordingDuration, SampleRate);
        _filePath = Path.Combine(Application.persistentDataPath, "audio.wav");
        Invoke(nameof(StopRecording), RecordingDuration);
    }

    private void StopRecording()
    {
        Microphone.End(null);
        byte[] wavFileData = AudioFileConverter.ConvertAudioClipToWav(_audioClip);
        File.WriteAllBytes(_filePath, wavFileData);
        Debug.Log($"Saved audio file to: {_filePath}");
        StartCoroutine(SendAudioToBackend(_filePath));
    }

    private static IEnumerator SendAudioToBackend(string filePath)
    {
        Debug.Log("Sending Audio to Backend");

        WWWForm form = new();
        byte[] fileData = File.ReadAllBytes(filePath);
        form.AddBinaryData("file", fileData, "audio.wav", "audio/wav");

        using UnityWebRequest www = UnityWebRequest.Post(BackendUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            Debug.Log("Audio file upload complete!");
            string jsonResponse = www.downloadHandler.text;
            Debug.Log($"Response: {jsonResponse}");

            if (jsonResponse != "[]")
            {
                List<ResponseData> responseArray = JsonConvert.DeserializeObject<List<ResponseData>>(jsonResponse);
                foreach (ResponseData item in responseArray)
                {
                    Debug.Log($"Name: {item.name}");
                    ResponseWrapper.data.Add(item);
                }
            }
        }
    }
}