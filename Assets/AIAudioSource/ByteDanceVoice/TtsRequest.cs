using System;
using UnityEngine;

public class TtsRequest
{
    public const string APP_ID = "5698365677";
    public const string CLUSTER = "volcano_tts";

    public TtsRequest()
    {
    }

    public TtsRequest(string text)
    {
        this.request.text = text;
    }

    public App app = new App();
    public User user = new User();
    public Audio audio = new Audio();
    public Request request = new Request();

    public App GetApp()
    {
        return app;
    }

    public void SetApp(App app)
    {
        this.app = app;
    }

    public User GetUser()
    {
        return user;
    }

    public void SetUser(User user)
    {
        this.user = user;
    }

    public Audio GetAudio()
    {
        return audio;
    }

    public void SetAudio(Audio audio)
    {
        this.audio = audio;
    }

    public Request GetRequest()
    {
        return request;
    }

    public void SetRequest(Request request)
    {
        this.request = request;
    }

    public class App
    {
        public string appid = APP_ID;
        public string token = "c4lgtyUgXA96N2MGPr7FL0jPrhwJ783w"; // 目前未生效，填写默认值：access_token
        public string cluster = CLUSTER;

        public string GetAppid()
        {
            return appid;
        }

        public void SetAppid(string appid)
        {
            this.appid = appid;
        }

        public string GetToken()
        {
            return token;
        }

        public void SetToken(string token)
        {
            this.token = token;
        }

        public string GetCluster()
        {
            return cluster;
        }

        public void SetCluster(string cluster)
        {
            this.cluster = cluster;
        }
    }

    public class User
    {
        public string uid = "388808087185088"; // 目前未生效，填写一个默认值就可以

        public string GetUid()
        {
            return uid;
        }

        public void SetUid(string uid)
        {
            this.uid = uid;
        }
    }

    public class Audio
    {
        public string voice_type = "BV009_streaming";
        public string encoding = "mp3";
        public float speed_ratio = 1.0f;
        public float volume_ratio = 1;
        public float pitch_ratio = 1;
        public string emotion = "happy";

        public string GetVoice_type()
        {
            return voice_type;
        }

        public void SetVoice_type(string voice_type)
        {
            this.voice_type = voice_type;
        }

        public string GetEncoding()
        {
            return encoding;
        }

        public void SetEncoding(string encoding)
        {
            this.encoding = encoding;
        }

        public float GetSpeedRatio()
        {
            return speed_ratio;
        }

        public void SetSpeedRatio(float speed_ratio)
        {
            this.speed_ratio = speed_ratio;
        }

        public float GetVolumeRatio()
        {
            return volume_ratio;
        }

        public void SetVolumeRatio(float volume_ratio)
        {
            this.volume_ratio = volume_ratio;
        }

        public float GetPitchRatio()
        {
            return pitch_ratio;
        }

        public void SetPitchRatio(float pitch_ratio)
        {
            this.pitch_ratio = pitch_ratio;
        }

        public string GetEmotion()
        {
            return emotion;
        }

        public void SetEmotion(string emotion)
        {
            this.emotion = emotion;
        }
    }

    public class Request
    {
        public string reqid = Guid.NewGuid().ToString();
        public string text;
        public string text_type = "plain";
        public string operation = "query";

        public string GetReqid()
        {
            return reqid;
        }

        public void SetReqid(string reqid)
        {
            this.reqid = reqid;
        }

        public string GetText()
        {
            return text;
        }

        public void SetText(string text)
        {
            this.text = text;
        }

        public string GetText_type()
        {
            return text_type;
        }

        public void SetText_type(string text_type)
        {
            this.text_type = text_type;
        }

        public string GetOperation()
        {
            return operation;
        }

        public void SetOperation(string operation)
        {
            this.operation = operation;
        }
    }
}