using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System;
using System.Text;
using UnityEngine;



public class FroHoloUdp : MonoBehaviour
{
    public static FroHoloUdp Instance;
    FroDatagarmUDP server;
   // public GameObject RootObj;
    #region legacy property


    // List<string> msgList = new List<string>();
    public int msgCount { get { return server.msg.Count; } }
    [HideInInspector]
    public string localIP;
    [HideInInspector]
    public bool canSend = true;
    [HideInInspector]
    public bool canRecv = true;

    #endregion
    public GameObject markPrefab;

    private void Update()
    {
#if !UNITY_EDITOR
        if(msgCount>0)
        {
          string strVec=  HandleMsg();
            Vector3 temp = Str2Vec3(strVec);
            Debug.Log(temp);
            Ray ray = Camera.main.ScreenPointToRay(temp);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                Instantiate(markPrefab, hit.point, Quaternion.identity);
            }
        }
#else
        if(msgCount>0)
        {
            Debug.Log(HandleMsg());
        }
#endif


    }
    private void Awake()
    {
        //Debug.Log("init");
        Instance = this;
        InitUDP();


    }

    private void Start()
    {
        //  Debug.Log("start");
        // JoinGroup();
       // Invoke("JoinGroup", 1f);
    }


    void InitUDP()
    {
        server = new FroDatagarmUDP();
        server.InitServer();
        localIP = server.GetLocalIP();
    }

    public void AsyncSend(string msg)
    {
        if (canSend)
        {            
            server.SendMsg(msg);
        }
    }

    public string HandleMsg()
    {
        return server.HandleMsg();
    }


    private void OnDestroy()
    {
        server.CloseDatagarmSocket();

    }

    public  Vector3 Str2Vec3(string strVec)
    {
        string str= strVec.Replace("(", "").Replace(")", "");
        string[] s = str.Split(',');
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
    }



    


}

