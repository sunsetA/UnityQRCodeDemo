using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Buffers.Text;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class XunfeiVoice : MonoBehaviour
{
    private string HOST = "openspeech.bytedance.com";
    private string API_URL = "https://" + "openspeech.bytedance.com" + "/api/v1/tts";
    private string ACCESS_TOKEN = "c4lgtyUgXA96N2MGPr7FL0jPrhwJ783w";


    private AudioSource audioSource;

    [Header("NPC音色类型")]
    public string NPCVoiceType;

    //public Text text;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="messages"></param>
    public void PostWords(string messages) 
    {
        TtsRequest ttsRequest = new TtsRequest(messages);

        if (!string.IsNullOrEmpty(NPCVoiceType))
        {
            ttsRequest.GetAudio().SetVoice_type(NPCVoiceType);
        }
        var sendMsg = JsonConvert.SerializeObject(ttsRequest);
        StartCoroutine(PostRequest(API_URL, sendMsg));

    }

    


    private System.Collections.IEnumerator ConvertBase64ToAudioClipCoroutine(AudioSource audioSource, string base64Audio)
    {

        var audioBytes = Convert.FromBase64String(base64Audio);
        var tempPath = Path.Combine(Application.persistentDataPath , "tmpMP3Base64.mp3") ;

        File.WriteAllBytes(tempPath, audioBytes);
#if !UNITY_EDITOR 
        tempPath = "file://" + tempPath;
#else

#endif
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(tempPath, AudioType.MPEG)) 
        {
            yield return www.SendWebRequest();
            while (!www.isDone) 
            {
                //text.text="正在下载音频文件..."+ www.downloadProgress * 100 + "%";
                Debug.Log("正在下载音频文件..." + www.downloadProgress * 100 + "%");
            }
            if (www.result == UnityWebRequest.Result.Success) 
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.Play();
                //text.text = "下载完成，播放音频";
                Debug.Log("下载完成，播放音频");
            }

            else
            {
                Debug.LogError("下载失败"+www.error);
                //text.text = "下载失败"+www.error;
            }
        }


    }
    public IEnumerator PostRequest(string url, string json)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        request.SetRequestHeader("Authorization", "Bearer; " + ACCESS_TOKEN);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("推送成功并受到回调:"+ request.downloadHandler.text);

            RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(request.downloadHandler.text);

            var base64Data= rootObject.data;
            Debug.Log("回调的base64编码为:" + base64Data);
            yield return ConvertBase64ToAudioClipCoroutine(audioSource, base64Data);
        }

        request.Dispose();
    }
}

// 根数据类
public class RootObject
{
    public string reqid { get; set; }

    public int code { get; set; }

    public string operation { get; set; }


    public string message { get; set; }

    public int sequence { get; set; }


    public string data { get; set; }

    public Addition addition { get; set; }
}

// addition 数据类
public class Addition
{

    public string description { get; set; }


    public string duration { get; set; }


    public Frontend frontend { get; set; }
}

// frontend 数据类
public class Frontend
{

    public List<Word> words { get; set; }


    public List<Phoneme> phonemes { get; set; }
}

// word 数据类
public class Word
{

    public string word { get; set; }


    public double start_time { get; set; }


    public double end_time { get; set; }
}

// phoneme 数据类
public class Phoneme
{

    public string phone { get; set; }


    public double start_time { get; set; }


    public double end_time { get; set; }
}