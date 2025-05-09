using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class PortControl : MonoBehaviour
{

    public static string sceneName = string.Empty; //跳转时场景的名称
    public static bool isFeedbacking; //是否开启指脉连接


  
    public static PortControl ins;
    public int baudRate = 115200;//波特率
   // public Parity parity = Parity.None;//效验位
    public int dataBits = 8;//数据位
   // public StopBits stopBits = StopBits.One;//停止位
    List<SerialPort> sp = null;
    Thread dataReceiveThread;

    [Header("断线重连按钮")]
    public UnityEngine.UI.Button ReconectedBtn;
    public Text XinLvText;
    public int Heart;
    public Text XunYangText;
    byte[] buffer = new byte[1024];
    int bytes = 0;
    public int heartRate =0;
    public int OxRate = 0;

    private int minAvergeHeart;
    private int maxAvergeHeart;
    
    public string starttimer;
    public string endtimer;
  
    private string data1;
    private string data2;
    private string data3;
  
    private bool IsEnd = false;

    private List<int> heartList = new List<int>();
    private List<int> oxList = new List<int>();
    int totleHeartlist;
    int totleOxlist;
    public int averageHeartlist;
    public int maxHeartList;
    public int mixHeartList;
    int averageOxlist;
    private float rate = 0.3f;//  3

    public
     bool IsStartGame = false;
    public Text warningText;

    [HideInInspector]
    public CanvasGroup canvasGroup;



    public Dictionary<string, List<(int,int, int)> > XinlvXueyangDic = new Dictionary<string, List<(int, int, int)>>();

    public List<int> xinlvMarkList;
    public List<int> xueyangMarkList;

    public int currentMinus = 0;

    /// <summary>
    /// 每分钟更新的取完平均值的值
    /// </summary>
    public event Action<int, int, int> OnMinutesUpdate;

    public event Action<int> OnMaxHeartUpdate;
    public event Action<int> OnMinHeartUpdate;
    public event Action<int> OnMaxOxgenUpdate;
    public event Action<int> OnMinOxgenUpdate;
    /// <summary>
    /// 重连的次数
    /// </summary>
    public int ReconnectTimes = 0;


    /// <summary>
    /// 累计时间
    /// </summary>
    private float m_interval;

    /// <summary>
    /// 当前的时间
    /// </summary>
    private float m_duration_seconds;


    public int MaxHeart;
    public int MinHeart = 100;
    public int MaxOxgen;
    public int MinOxgen = 100;

    private float startTime;

    /// <summary>
    /// 正在重新连接中
    /// </summary>
    bool isReconecting = false;
    /// <summary>
    /// 是否处于连接状态
    /// </summary>
    bool isConnecting = false;

    #region 当跳转场景时，需要暂停接受数据

    public bool canMark = true;
    public void PauseMark()
    {
        canMark = false;
    }
    public void StartMark()
    {
        canMark = true;
    }

    #endregion
    public void ReConnectPort(){
        if(!isFeedbacking){
         ClosePort();
         //reconnectBtn.interactable=false;
          warningText.text = "指脉设备连接中...";
            isReconecting = true;
          //== Hide blueTooth =====
          //isFeedbacking = false;
           //=====================

          //          dataReceiveThread.Abort();
          buffer =new byte[1024];
          dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
          dataReceiveThread.Start();
          IsEnd=false;
           IsStartGame = true;
            ReconectedBtn.interactable = false;
          Invoke("ShowMsg",5);

        }
    }
    void ShowMsg(){
        if(!isFeedbacking){
            warningText.text = "指脉设备连接失败，请检查设备状态";
            //ReConnectPort();
            ReconectedBtn.interactable = true;
            //  reconnectBtn.interactable=true;
            ReconectedBtn.gameObject.SetActive(true);         
        }
        else{
            ReconnectTimes++;
             warningText.text = "指脉设备连接成功";
            ReconectedBtn.interactable = false;
            ReconectedBtn.gameObject.SetActive(false);
        }

        isReconecting = false;
       
    }

    /// <summary>
    /// 心率血氧缓存
    /// </summary>
    List<(int,int)> OneSecondCaches =new List<(int, int)> ();

    int usefulXinlv;
    int usefylXueYang;
    void Start()
    {
       ins = this;
       warningText.text = "";
       //minAvergeHeart=int.Parse(ConfigurationManager.appSettings["最低正常心率"]);
       //maxAvergeHeart=int.Parse(ConfigurationManager.appSettings["最高正常心率"]);
       //print("最低心率："+minAvergeHeart+"   "+"最高心率："+maxAvergeHeart);
       canvasGroup=transform.Find("Canvas/Panel").GetComponent<CanvasGroup>();


        //Hide blueTooth
        isFeedbacking = false;
        heartList = new List<int>();
       XinLvText.text="00";
       XunYangText.text="00";
        ReconectedBtn.onClick.AddListener(ReConnectPort);
        //ReConnectPort();
        StartCoroutine(AutoReconnect());
    //  dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
    //    dataReceiveThread.Start();

        //    IsStartGame = true;
        //    print("设备连接中......请等待");
        //    Invoke("ShowMsg",25);
        //HideCanvasPanel();



    }
    // void StartPort(){

    //      ClosePort();
    //      warningText.text = "指脉设备连接中...";
    //    isFeedbacking=false;
    //    dataReceiveThread.Abort();
    //    buffer=new byte[1024];
    //    dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
    //    dataReceiveThread.Start();
    //    IsEnd=false;
    //    IsStartGame = true;
    //    Invoke("ShowMsg",25);

    // }
    public void ShowCanvasPanel(){
        canvasGroup.alpha=1;
        canvasGroup.interactable=true;

    }
    public void HideCanvasPanel(){
        canvasGroup.alpha=0;
        canvasGroup.interactable=false;

    }

    void FixedUpdate()
    {


        //计时
        //m_interval += 0.2f;
        //CalculateData();
        //if (m_interval >=1)
        //{

        //    m_interval = 0;
        //    Debug.Log("过了一秒");

        //    if (OneSecondCaches.Count>0)
        //    {
        //        MarkData(sceneName, OneSecondCaches[0].Item1, OneSecondCaches[0].Item2);
        //        OneSecondCaches.Clear();
        //        m_duration_seconds++;
        //        Debug.Log("记录成功");
        //    }
        //    else
        //    {
        //        Debug.Log("这一秒没有有效数据");
        //    }

        //}
        if (!canMark)
        {
            return;
        }
        m_interval += 0.02f;
        if (m_interval>0.1&& m_interval<1)
        {
            CalculateData();
        }
        else if (m_interval>=1)
        {
            m_interval = 0;

            if (OneSecondCaches.Count > 0)
            {
                MarkData(sceneName, OneSecondCaches[0].Item1, OneSecondCaches[0].Item2);
                OneSecondCaches.Clear();
                m_duration_seconds++;
            }
            else
            {
                Debug.Log("这一秒没有有效数据");
            }

        }





    }


    private void CalculateData()
    {

        if (IsStartGame && IsEnd == false)
        {
            if (System.Int32.Parse(buffer[0].ToString()) > 127 && System.Int32.Parse(buffer[1].ToString()) > 127)  //>127有效数据
            {
                XinLvText.text = buffer[4].ToString();
                XunYangText.text = buffer[5].ToString();

                OneSecondCaches.Add((System.Int32.Parse(buffer[4].ToString()), System.Int32.Parse(buffer[5].ToString())));


                heartRate = System.Int32.Parse(buffer[4].ToString());
                OxRate = System.Int32.Parse(buffer[5].ToString());
                if (heartRate != 0 && OxRate != 0)
                {
                    if (!string.IsNullOrEmpty(warningText.text))
                    {
                        warningText.text = "";
                    }
                    isFeedbacking = true;
                    // reconnectBtn.interactable=false;

                }
                else
                {

                }
                //Debug.Log("记录了心率血氧一次，心率数值为：" + buffer[4].ToString() + "血氧数值为：" + buffer[5]);
            }
            else if (System.Int32.Parse(buffer[5].ToString()) == 127 || System.Int32.Parse(buffer[6].ToString()) == 127)  //断开USB串口
            {
                IsEnd = true;
                // reconnectBtn.interactable=true;
                warningText.text = "指脉设备连接断开，请检查设备状态";
                XinLvText.text = "00";
                XunYangText.text = "00";
                //Hide blueTooth
                isFeedbacking = false;
                ReconectedBtn.interactable = true;
                ReconectedBtn.gameObject.SetActive(true);
            }
            else 
            {
                //Debug.Log("其他情况  "+ "心率数值为：" + buffer[4].ToString() + "血氧数值为：" + buffer[5]+"buffer0"+ buffer[0].ToString()+"buffer1"+ buffer[1].ToString());
            }
        }
        else
        {
            //Debug.Log("处于不可检测状态，检查变量值");
        }

    }
    public void ContinuePromTask(){
        heartList.Clear();
        rate=0.3f;

    }
    private bool isPort;
    #region 创建串口，并打开串口
    public void OpenPort()
    {

        sp = new List<SerialPort>();
        string[] strs = SerialPort.GetPortNames();
        for (int i = 0; i < strs.Length; i++)
        {
            print(strs[i]+"串口号");
            isPort = true;
            SerialPort sr = new SerialPort(strs[i], baudRate, Parity.None, dataBits, StopBits.One);
            try
            {
                if(!sr.IsOpen){
                    sr.Open();
                }             
            }
            catch
            {
                print("有异常");
                isPort = false;
                sr.Close();
            }
            if (!isPort)
            {
                continue;
            }
            else
            {
                sr.ReadTimeout = 400;
               // print(strs[i]);
                sp.Add(sr);
                // break;
            }
        }
    }
    #endregion



    #region 程序退出时关闭串口
    void OnApplicationQuit()
    {
        ClosePort();
    }
    private void OnDestroy()
    {


        ClosePort();

    }
    public void ClosePort()
    {
        try
        {
            IsEnd = true;
            for (int i = 0; i < sp.Count; i++)
            {
                if (sp[i] != null)
                {
                    sp[i].Close();
                }

            }
          if(dataReceiveThread!=null){
                dataReceiveThread.Abort();
            }
           
        }
        catch (System.Exception ex)
        {
            // warningtext.text = ex.Message;
        }

    }
    #endregion


  
    void DataReceiveFunction()
    {
        OpenPort();
        while (true)
        {
            for (int i = 0; i < sp.Count; i++)
            {
                if (sp[i] != null && sp[i].IsOpen)
                {
                    try
                    {
                        //Debug.Log("start" );
                        bytes = sp[i].Read(buffer, 0, buffer.Length);//接收字节
                        if (bytes == 0)
                        {
                            //Debug.Log( "try"+ bytes + "长度");
                            continue;
                        }
                        else
                        {
                            //Debug.Log("catch");
                           // string strbytes = Encoding.Default.GetString(buffer);
                        }
                    }
                    catch (System.Exception ex)
                    {
//                         warningText.text = "操作有误，请联系管理员";
                       if(sp.Count>0){
                            sp[i].Close();
                       }
                        //    warningstring = "操作有误，请联系管理员";
                        //Debug.Log("操作有误，请联系管理员");
                        if (ex.GetType() != typeof(ThreadAbortException))
                        {

                        }
                    }
                }


            }
            Thread.Sleep(10);
        }

    }
    
  
    public void WriteData(string dataStr)
    {
        //if (sp.IsOpen)
        //{
        //    sp.Write(dataStr);
        //}
    }
   
    public void MarkData(string sceneName, int xinlv,int xueyang)
    {
        if (sceneName==string.Empty)
        {
            return;
        }
        xinlvMarkList.Add(xinlv);
        xueyangMarkList.Add(xueyang);

        RecordMaxMinData(xinlv,xueyang);
        if (xinlvMarkList.Count==50)
        {
            if (!XinlvXueyangDic.ContainsKey(sceneName))
            {
                List<(int, int, int)> data = new List<(int, int, int)>();
                XinlvXueyangDic.Add(sceneName, data);
                currentMinus = 0;
            }
            int xinlvAverage = 0;
            int xueyangAverage = 0;
            foreach (var item in xinlvMarkList)
            {
                xinlvAverage += item;
            }
            foreach (var item in xueyangMarkList)
            {
                xueyangAverage += item;
            }
            xinlvAverage = Mathf.FloorToInt(xinlvAverage/50);
            xueyangAverage=Mathf.FloorToInt(xueyangAverage/50);
            currentMinus++;

            XinlvXueyangDic[sceneName].Add((currentMinus, xinlvAverage, xueyangAverage));
            xinlvMarkList.Clear();
            xueyangMarkList.Clear();
            OnMinutesUpdate?.Invoke(currentMinus, xinlvAverage,xueyangAverage);
        }
    }

    private void RecordMaxMinData(int heart,int oxgen)
    {
        if (heart>MaxHeart)
        {
            MaxHeart = heart;
            OnMaxHeartUpdate?.Invoke(MaxHeart);
        }
        else if (heart<MinHeart) 
        {
            MinHeart = heart;
            OnMinHeartUpdate?.Invoke(MinHeart);
        }
        else
        {
            //位于最大值和最小值之间
        }


        if (oxgen > MaxOxgen)
        {
            MaxOxgen = oxgen;
            OnMaxOxgenUpdate?.Invoke(MaxOxgen);
        }
        else if (oxgen < MinOxgen)
        {
            MinOxgen = oxgen;
            OnMinOxgenUpdate?.Invoke(MinOxgen);
        }
        else
        {
            //位于最大值和最小值之间
        }
    }


    private IEnumerator AutoReconnect()
    {
        while (true) 
        {
            yield return new WaitForSecondsRealtime(1);
            if (!isReconecting&&!isFeedbacking)
            {
                ReConnectPort();
                Debug.Log("重新连接了一次");
            }
        }
    }
}
