using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Xml;
using System.IO;
using UnityEngine.UI;
using System.Text;
using Unity.Collections;

namespace OA 
{
    public class PushServerExample : MonoBehaviour
    {
        public RawImage QRImage;

        [Header("注意，正常屏幕下texture的贴图尺寸如果过大，需要调小至64/128")]
        public Texture2D QRTexture;
        public int resultCode;

        [Header("联网情况下的请求地址")]
        public string serverUrl = "https://apifoxmock.com/m1/5105109-0-default/api/license";

        [Header("联网情况下获取激活状态的请求地址")]
        public string getActiveStateUrl = "https://apifoxmock.com/m1/5105109-0-default/api/license?Id=";

        [Header("断网情况下的小程序扫码地址")]
        public string NoNetworkUrl = "https://apifoxmock.com/m1/5105109-0-default/api/license";


        public string plainText = "";
        public string key = "12345678";

        /// <summary>
        /// 请求licid的协程
        /// </summary>
        private IEnumerator RequestLicIdIenumerator;

        /// <summary>
        /// licid轮询激活状态的协程
        /// </summary>
        private IEnumerator GetLicIdStateIenurator;

        private void Awake()
        {
            plainText = DESEncryption.GenerateRandomChars();
            Debug.Log("plainText:"+plainText);
        }
        private void Start()
        {
            RefreshQR();


            #region 加密解密测试
            //string encryptedText = DESEncryption.Encrypt(plainText, key);

            //string decryptedText=DESEncryption.Decrypt(encryptedText, key);

            //if (encryptedText != null)
            //{
            //    Debug.LogFormat("Encrypted Text: {0}  and  Decrypted Text:{1}",encryptedText,decryptedText);
            //}

            #endregion
        }

        /// <summary>
        /// 根据网络连接情况，刷新在线二维码
        /// </summary>
        public void RefreshQR()
        {
            bool isOnline = IsConnectedToInternet();

            if (isOnline)
            {
                //如果在线，根据licid请求二维码
                OAAppsettings oAAppsettings = new OAAppsettings(Path.Combine(Environment.CurrentDirectory, "OAApp.exe.config"));
                var json = JsonConvert.SerializeObject(oAAppsettings);
                RequestLicIdIenumerator = PostGameDataMsg(serverUrl, json, PushDataCallback);
                GetLicIdStateIenurator = GetActiveStateFromServer(getActiveStateUrl, resultCode, GetActiveStateCallback);
                StartCoroutine(RequestLicIdIenumerator);
            }
            else 
            {
                var pushData = GetOffLinePostData();
                //如果离线，根据小程序的扫码地址生成二维码
                ZXingQRCodeWrapper_GenerateQRCode.SetRawImageQRCode(QRImage, QRTexture, pushData, Color.blue);
                var qrCode = JsonConvert.DeserializeObject<NoErrorRequestData>(pushData);
                var verifyCode = DESEncryption.Decrypt(qrCode.verifyCode, key);
                Debug.Log("当前是断网状态,解密后的验证码为："+ verifyCode);
            }

        }

        /// <summary>
        /// 根据当前xml文件，请求QR码数据
        /// </summary>
        /// <param name="_url">请求的地址</param>
        /// <param name="jsonString">当前项目的配置信息</param>
        /// <param name="ac">回调事件</param>
        /// <returns></returns>
        public static IEnumerator PostGameDataMsg(string _url, string jsonString, Action<string> ac)
        {
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            string s = reg.Replace(jsonString, delegate (Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
            // string url = string.Format("http://{0}/Base_Manage/Base_Train/SaveData", _url);
            //print("url:" + url);
            //  print("*************************************************");
            // print("Json:" + s);
            // print("url:" + _url);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            UnityWebRequest request = new UnityWebRequest(_url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.certificateHandler = new WebReqSkipCert();
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            request.useHttpContinue = false;
            request.downloadHandler = dH;
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            // request.SetRequestHeader("Authorization", "Bearer " + SystemDefine.Instances.Data);
            // request.SetRequestHeader("Loginversion",);
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("上传数据失败！");
                //Debug.Log(request.error);
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("上传数据成功！");
                //print(request.downloadHandler.text);
                if (ac != null)
                {
                    ac.Invoke(request.downloadHandler.text);
                }

            }
        }


        public static IEnumerator GetActiveStateFromServer(string _url,int licId,Action<string> ac)
        {
            // 定义请求的URL
            string TargetUrl = _url + licId;

            // 创建一个UnityWebRequest对象，使用GET方法
            using (UnityWebRequest webRequest = UnityWebRequest.Get(TargetUrl))
            {
                // 发送请求并等待响应
                yield return webRequest.SendWebRequest();

                // 检查请求的结果
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    // 如果发生错误，打印错误信息
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    // 如果请求成功，打印响应内容
                    ac?.Invoke(webRequest.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// 提交应用、设备信息、换取licid事件的回调
        /// </summary>
        /// <param name="ServerData"></param>
        private void PushDataCallback(string ServerData) 
        {
            var data= JsonConvert.DeserializeObject<RequestServerResponseData>(ServerData);
            Debug.Log(ServerData);
            var QRCode = data.result;
            resultCode = Convert.ToInt32(QRCode); 
            ZXingQRCodeWrapper_GenerateQRCode.SetRawImageQRCode(QRImage, QRTexture, QRCode, Color.blue);


            //获取到licid后，开始轮询激活状态
            StartCoroutine(GetLicIdStateIenurator);
        }


        /// <summary>
        /// 获取licid激活状态的回调
        /// </summary>
        /// <param name="jsonData"></param>
        private void GetActiveStateCallback(string jsonData) 
        {
            var data = JsonConvert.DeserializeObject<YGXJ_QRActiveData>(jsonData);
            Debug.Log(jsonData);
            if (data.result.activated)
            {
                Debug.Log("激活成功");
            }
            else
            {
                Debug.Log("激活失败");
            }
        }

        /// <summary>
        /// 判断当前是否连接到互联网
        /// </summary>
        /// <returns>如果是联网，则返回true，否则为false</returns>
        private bool IsConnectedToInternet()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }


        /// <summary>
        /// 获取离线的url
        /// </summary>
        /// <returns></returns>
        private string GetOffLinePostData() 
        {
            string path = Path.Combine(Environment.CurrentDirectory,"NoInternet.json");
            if (File.Exists(path))
            {
                string jsonContent=File.ReadAllText(path);
                NoErrorRequestData noErrorRequestData = JsonUtility.FromJson<NoErrorRequestData>(jsonContent);
                noErrorRequestData.verifyCode = DESEncryption.Encrypt(plainText, key);
                return JsonConvert.SerializeObject(noErrorRequestData);
            }

            else
            {
                Debug.LogError("当前目录下不存在该文件");
                return null;
            }
        }
    }

    #region 有网络状态下的请求

    #region   请求QR码的数据类
    public class OAAppsettings 
    {
        private Dictionary<string, string> dict;
        public OAAppsettings(string path)
        {
            this.dict = new Dictionary<string, string>();
            string filename = path;
            XmlDocument xmldoc = new XmlDocument();


            xmldoc.Load(filename);
            XmlNodeList topM = xmldoc.DocumentElement.ChildNodes;
            foreach (XmlElement element in topM)
            {

                if (element.Name.ToLower() == "appsettings")
                {
                    XmlNodeList nodelist = element.ChildNodes;
                    if (nodelist.Count > 0)
                    {
                        foreach (XmlElement el in nodelist)
                        {
                            if (!dict.ContainsKey(el.Attributes["key"].Value))
                            {
                                dict.Add(el.Attributes["key"].Value, el.Attributes["value"].Value);
                            }
                            else
                            {
                                dict[el.Attributes["key"].Value] = el.Attributes["value"].Value;
                            }
                        }
                    }
                }


            }
            xmldoc.Save(filename);
        }
        private void Add(string key, string value)
        {
            if (!this.dict.ContainsKey(key))
            {
                this.dict.Add(key, value);
            }
            else
            {
                this.dict[key] = value;
            }
        }
        public string this[string key]
        {
            get
            {
                if (!this.dict.ContainsKey(key))
                {
                    throw new Exception("404");
                }
                return this.dict[key];
            }
            set
            {
                if (this.dict.ContainsKey(key))
                {
                    this.dict[key] = value;
                }
                else
                {
                    this.dict.Add(key, value);
                }
            }
        }

        // 添加一个方法用于获取 AppSettings 数据
        // 添加一个方法用于获取 AppSettings 数据
        public YGXJ_OARequestQRDate.AppSettings[] GetAppSettings()
        {
            if (dict.ContainsKey("appSettings"))
            {
                string appSettingsJson = dict["appSettings"];
                // 去除多余的转义字符
                appSettingsJson = appSettingsJson.Replace("\\\"", "\"");

                try
                {
                    return JsonConvert.DeserializeObject<YGXJ_OARequestQRDate.AppSettings[]>(appSettingsJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"反序列化 AppSettings 时出错: {ex.Message}");
                }
            }
            return null;
        }
    }

    public class YGXJ_OARequestQRDate
    {
        public string appName;
        public string appCode;
        public string appDesc;
        public string installPath;
        public string appVersion;
        public AppSettings[] appSettings;
        public Device device;

        public class AppSettings
        {
            public string groupName;
            public string label;
            public string labelText;
            public string labelDesc;
            public string value;

            public enum UiType
            {
                Input = 0,
                Switch = 1,
                Select = 2,
                Radio = 3,
                Checkbox = 4,
            }

            public UiType uiType;

            public string placeholder;

            public Dictionary<string, string> data;


        }
        public class Device
        {
            public string ip;
            public string hostName;
            public string machine;
            public string osVersion;
        }
    }

    /// <summary>
    /// 请求二维码数据时，服务器返回的所有数据
    /// </summary>
    public class RequestServerResponseData
    {
        public int code;
        public string type;
        public string message;
        public string result;
        public string extras;
        public string time;
    }

    #endregion



    #region LicID轮询请求的数据类

    public class YGXJ_QRActiveData
    {
        public string code;
        public string type;
        public string message;
        public Active_Result result;

        public string extras;
        public string time;
        public class Active_Result 
        {
            public int id;
            public string signature; 
            public string appName;
            public string appCode;
            public string appVersion;
            public string appDesc;
            public int appSettingsVer;
            public YGXJ_OARequestQRDate.AppSettings[] appSettings;
            public string installPath;
            public string orderId;
            public int deviceId;
            public bool activated;
            public string activationTime;
            public string updateTime;
            public int userId;
        }


    }
    #endregion

    #endregion


    #region 无网络状态下请求的数据类

    public class NoErrorRequestData
    {
        public string appDesc;
        public string appIdent;
        public string appName;
        public string appPath;
        public string assemblyName;
        public string projectName;
        public string tenantName;
        public string verifyCode;    
    }




    #endregion

}
