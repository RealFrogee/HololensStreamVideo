using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif


public class FroDatagarmUDP 
{
   public List<string> msg = new List<string>();
    public int port = 8006;
    public string hostNameString = "224.3.0.5";
#if UNITY_EDITOR
    private Thread _ReadThread;
    private UdpClient _Socket;
#else
    DataWriter writer;
    IOutputStream outputStream;
    DatagramSocket _Socket=null;
#endif
    private void OnMessageReceivedEvent(string message, IPEndPoint remoteEndpoint)
    {

        msg.Add(message);

    }

  //  private void Awake()
  //  {
  //      InitServer();
  //  }
  //  void Start()
  //  {
  //
  //      InvokeRepeating("SendMsgRepeat", 3f, 3f);
  //
  //  }
  //
  //
  //  void Update()
  //  {
  //      if (Input.GetKeyDown(KeyCode.X))
  //      {
  //          SendMsg(GetLocalIP());
  //      }
  //
  //      HandleMsg();
  //
  //  }

    public void CloseDatagarmSocket()
    {
#if UNITY_EDITOR
        if (_ReadThread.IsAlive)
        {
            _ReadThread.Abort();
        }
        if (_Socket != null)
        {
            _Socket.Close();
            _Socket = null;
        }
#else
       // CancelInvoke();
        if (_Socket != null)
        {
            _Socket.Dispose();
            _Socket = null;
        }
#endif
    }

    public string HandleMsg()
    {
        if (msg.Count > 0)
        {
           // Debug.Log(string.Format(" get :  {0}", msg[0]));
            string tempMsg = msg[0];
            msg.RemoveAt(0);
            return tempMsg;
        }
        return string.Empty;
    }

    public void SendMsgRepeat()
    {
#if UNITY_EDITOR
        SendMsgEditor(GetLocalIP());
#else
        SendMsgUWP(GetLocalIP());
#endif
    }

    public void SendMsg(string msg)
    {
#if UNITY_EDITOR
        SendMsgEditor(msg);
#else
        SendMsgUWP(msg);
#endif


    }

    public void InitServer()
    {
#if UNITY_EDITOR
        InitServerEditor();
#else
        InitServerUWP();
#endif
    }


#if UNITY_EDITOR
    private void InitServerEditor()
    {
        _ReadThread = new Thread(new ThreadStart(delegate
        {
            try
            {
                IPEndPoint temp = new IPEndPoint(IPAddress.Parse(GetLocalIP()), port);
                _Socket = new UdpClient(temp);

                _Socket.JoinMulticastGroup(IPAddress.Parse(hostNameString), IPAddress.Parse(GetLocalIP()));
                //_Socket.EnableBroadcast = true;
                Debug.LogFormat("Receiving on port {0}", port);
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
                return;
            }
            while (true)
            {
                try
                {
                    // receive bytes
                    //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    // IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(hostNameString), port);
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(GetLocalIP()), port);
                    byte[] data = _Socket.Receive(ref anyIP);

                    // encode UTF8-coded bytes to text format
                    string message = Encoding.UTF8.GetString(data);
                    OnMessageReceivedEvent(message, anyIP);
                }
                catch(ThreadAbortException ex)
                {

                }
                catch (Exception err)
                {
                    Debug.LogError(err.ToString());
                }
            }
        }));
        _ReadThread.IsBackground = true;
        _ReadThread.Start();
    }

    private void SendMsgEditor(string msg)
    {
        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
        //  IPEndPoint tempIP = new IPEndPoint(IPAddress.Broadcast, port);
        IPEndPoint tempIP = new IPEndPoint(IPAddress.Parse(hostNameString), port);
        //  IPEndPoint tempIP = new IPEndPoint(IPAddress.Parse("192.0.0.126"), port);
        _Socket.Send(msgBytes, msgBytes.Length, tempIP);
    }

#else
    private async void InitServerUWP()
    {
         _Socket=new DatagramSocket();
        _Socket.Control.MulticastOnly=true;
        await _Socket.BindServiceNameAsync(port.ToString());
        _Socket.JoinMulticastGroup(new HostName(hostNameString));
        _Socket.MessageReceived+=_Socket_MessageReceived;
         Debug.Log(string.Format("HOLO  multicast on {0}", port));
        await Task.Delay(1000);

          Debug.Log(string.Format("start  multicast on {0}", port));
          outputStream=await _Socket.GetOutputStreamAsync(new HostName(hostNameString),port.ToString());
          writer=new DataWriter(outputStream);
       //  writer.WriteString(new HostName("localhost").RawName);
       //  await writer.StoreAsync();
       //  Debug.Log(string.Format("multicast end on {0}", port));
    }


     private async void _Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        try
        {
          // Stream streamIn = args.GetDataStream().AsStreamForRead();
          // StreamReader reader = new StreamReader(streamIn, Encoding.UTF8);
          // 
          // string message = await reader.ReadLineAsync();
          // IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.RawName), Convert.ToInt32(args.RemotePort));
          // OnMessageReceivedEvent(message, remoteEndpoint);
           


            using(var streamIn=args.GetDataStream().AsStreamForRead())
            {
                using(var reader=new StreamReader(streamIn,Encoding.UTF8))
                {
                    string message = await reader.ReadLineAsync();
                    IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.RawName), Convert.ToInt32(args.RemotePort));
                    OnMessageReceivedEvent(message, remoteEndpoint);
                }
            }



            // using(var reader=args.GetDataReader())
            //   {
            //       var buf=new byte[reader.UnconsumedBufferLength];
            //        reader.ReadBytes(buf);
            //       string message=Encoding.UTF8.GetString(buf);
            //       IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.RawName), Convert.ToInt32(args.RemotePort));
            //       OnMessageReceivedEvent(message, remoteEndpoint);
            //   }
    
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private async void SendMsgUWP(string msg)
    {
    writer.WriteString(msg);
    await writer.StoreAsync();
    }

#endif

    public string GetLocalIP()
    {



#if UNITY_EDITOR
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }


        }
        return "";
#else
       string ip = null;
        foreach (HostName localHostName in NetworkInformation.GetHostNames())
        {
            if (localHostName.IPInformation != null)
            {
                if (localHostName.Type == HostNameType.Ipv4)
                {
                    ip = localHostName.ToString();
                    break;
                }
            }
        }
        return ip;

#endif
    }

}
