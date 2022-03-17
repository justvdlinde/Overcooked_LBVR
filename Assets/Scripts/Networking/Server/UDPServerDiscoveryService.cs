using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utils.Core.Services;

public class UdpState
{
    public IPEndPoint iPEndPoint;
    public UdpClient updClient;
}

public class GameServer
{
    public string Code { get; set; }
    public string IpAddress { get; set; }
    public string Message { get; set; }

    public GameServer(string code, string address, string message)
    {
        Code = code;
        IpAddress = address;
        Message = message;
    }
}

/// <summary>
/// Class for sending and recieving data through local network. Can be used to find a server's IP address.
/// The server opens a port where it listens to broadcasts. Clients broadcast to that specific port, causing the server to respond, 
/// letting the client know which IP address to connect to.
/// </summary>
public class UDPServerDiscoveryService : MonoBehaviour, IService, IDisposable
{
    public enum ServerNotFoundCause
    {
        NotFound,
        CodeMismatch,
        Exception,
    }

    private const int BROADCAST_PORT = 15464;
    private const int LISTENING_PORT = 12983;
    private const float CLIENT_WAIT_FOR_RESPONSE_TIME = 3f;

    public GameServer ServerFound { get; protected set; }
    public bool IsListeningForClients { get; protected set; }

    private Action<GameServer> ServerFoundEvent;
    private Action<ServerNotFoundCause> ServerNotFoundEvent;

    private UdpClient broadcastClient; //client
    private UdpClient listenClient; //server
    private IPEndPoint broadcastEndPoint;

    private AsyncCallback listenServerCallback;
    private bool isProcessingMainThread = true;

    private Stack<GameServer> gameServerStack = new Stack<GameServer>(); // needed for the multithreaded part
    private string idCode;
    private string message;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Refresh()
    {
        gameServerStack.Clear();
        CloseListeningPort();
    }

    public void Dispose()
    {
        CloseListeningPort();
        StopListeningForClients();
    }

    private void OnServerFoundEvent(GameServer gameServer)
    {
        ServerFoundEvent?.Invoke(gameServer);
        ServerFoundEvent = null;
    }

    private void OnServerNotFoundEvent(ServerNotFoundCause cause)
    {
        ServerNotFoundEvent?.Invoke(cause);
        ServerNotFoundEvent = null;
    }

    #region Client functions
    public void SearchForServer(string idCode, Action<GameServer> successCallback = null, Action<ServerNotFoundCause> failCallback = null)
    {
        this.idCode = idCode;
        try
        {
            OpenListeningPort(LISTENING_PORT);
            StartSearchForServer(successCallback, failCallback);
        } 
        catch(Exception e)
        {
            Debug.LogWarning(e);
            failCallback?.Invoke(ServerNotFoundCause.Exception);
        }
    }

    private void OpenListeningPort(int port)
    {
        // open a listening port on port to receive a response back from server
        //try
        //{
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
            UdpClient udpClient = new UdpClient(iPEndPoint);
            UdpState udpState = new UdpState();
            udpState.iPEndPoint = iPEndPoint;
            udpState.updClient = udpClient;

            isProcessingMainThread = true;
            StartCoroutine(ServerResponseCallbackMainThreaded());
            listenServerCallback = new AsyncCallback(ListenServerCallbackThreaded);
            udpClient.BeginReceive(listenServerCallback, udpState);

            broadcastClient = udpClient;
            broadcastEndPoint = iPEndPoint;
            Debug.Log("Broadcast listener opened on port " + broadcastEndPoint.Port.ToString());
        //}
        //catch(Exception e)
        //{
        //    Debug.LogErrorFormat("Exception when trying to use port {0}: {1}", port, e);
        //}
    }

    private void StartSearchForServer(Action<GameServer> serverFoundCallback, Action<ServerNotFoundCause> serverNotFoundCallback)
    {
        ServerFound = null;
        ServerFoundEvent += serverFoundCallback;
        ServerNotFoundEvent += serverNotFoundCallback;

        // open a broadcast and send own broadcast listener port to the LAN
        UdpClient updClient = new UdpClient();
        byte[] sendBytes = BitConverter.GetBytes(broadcastEndPoint.Port);

        // Important: this is disabled by default, so we have to enable it
        updClient.EnableBroadcast = true;

        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT);
        updClient.BeginSend(sendBytes, sendBytes.Length, ipEndPoint, new AsyncCallback(ServerFoundCallback), updClient);

        Debug.Log("Find server message sent on broadcast listener");
    }

    private void ServerFoundCallback(IAsyncResult result)
    {
        UdpClient updClient = (UdpClient)result.AsyncState;
        int bytesSent = updClient.EndSend(result);

        // close the broadcast client
        updClient.Close();
    }

    /// <summary>
    /// Callback when receiving a response. Does not run at main thread
    /// </summary>
    /// <param name="result"></param>
    public void ListenServerCallbackThreaded(IAsyncResult result)
    {
        try
        {
            if (!isProcessingMainThread)
                return;

            IPEndPoint serverIP = ((UdpState)(result.AsyncState)).iPEndPoint;

            byte[] receiveBytes = broadcastClient.EndReceive(result, ref serverIP);
            string fullResponseMessage = Encoding.ASCII.GetString(receiveBytes);
            string code = "";
            string message = "";

            if (fullResponseMessage.StartsWith(idCode))
            {
                code = idCode;
                message = fullResponseMessage.Substring(idCode.Length);
            }

            gameServerStack.Push(new GameServer(idCode, serverIP.Address.ToString(), message));
            broadcastClient.BeginReceive(listenServerCallback, result.AsyncState);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + e.StackTrace);
        }
    }

    private IEnumerator ServerResponseCallbackMainThreaded()
    {
        float timer = 0;
        bool versionMismatch = false;

        while (isProcessingMainThread && timer < CLIENT_WAIT_FOR_RESPONSE_TIME)
        {
            timer += Time.deltaTime;
            while (gameServerStack.Count != 0)
            {
                ServerFound = gameServerStack.Pop();
                Debug.LogFormat("Response code: {0} from: {1} message {2}", ServerFound.Code, ServerFound.IpAddress, ServerFound.Message);

                if (idCode == string.Empty || ServerFound.Code == idCode)
                {
                    versionMismatch = false;
                    OnServerFoundEvent(ServerFound);
                }
                else
                {
                    Debug.LogWarning("Found server, response code mismatch");
                    versionMismatch = true;
                }
            }
            yield return null;
        }

        if(ServerFound == null)
            OnServerNotFoundEvent(ServerNotFoundCause.NotFound);
        else if(versionMismatch)
            OnServerNotFoundEvent(ServerNotFoundCause.CodeMismatch);

        CloseListeningPort();
    }

    private void CloseListeningPort()
    {
        if (!isProcessingMainThread)
            return;

        //close the broadcast listener, this is needed to start a new search
        try
        {
            isProcessingMainThread = false;
            broadcastClient.Close();
            Debug.Log("Broadcast listener closed");
        }
        catch(Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region Server Functions
    public void StartListeningForClients(string idCode, string message)
    {
        this.idCode = idCode;
        this.message = message;

        try
        {
            // Open a listening port to listen for any clients:
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, BROADCAST_PORT);
            UdpClient udpClient = new UdpClient(ipEndPoint);
            listenClient = udpClient;

            UdpState udpState = new UdpState();
            udpState.iPEndPoint = ipEndPoint;
            udpState.updClient = udpClient;

            Debug.LogFormat("Opened port {0} to listen for clients", ipEndPoint.Port);
            udpClient.BeginReceive(new AsyncCallback(ListenForClientsCallback), udpState);

            IsListeningForClients = true;
        }
        catch(Exception e)
        {
            throw e;
        }
    }

    public void StopListeningForClients()
    {
        listenClient?.Close();
        if (IsListeningForClients)
        {
            Debug.Log("server listening port closed");
            IsListeningForClients = false;
        }
    }

    private void ListenForClientsCallback(IAsyncResult result)
    {
        //Received a broadcast from a client

        UdpClient udpClient = ((UdpState)result.AsyncState).updClient;
        IPEndPoint ipEndpoint = ((UdpState)result.AsyncState).iPEndPoint;
        byte[] receiveBytes = udpClient.EndReceive(result, ref ipEndpoint);
        int clientPort = BitConverter.ToInt32(receiveBytes, 0);

        Debug.LogFormat("Received a broadcast message from client {0} port {1} ", ipEndpoint.Address, clientPort);

        //Send a response back to the client on the port they sent us
        string response = idCode + message;
        byte[] sendBytes = Encoding.ASCII.GetBytes(response);

        UdpClient client = new UdpClient();
        IPEndPoint clientIP = new IPEndPoint(ipEndpoint.Address, clientPort);

        Debug.LogFormat("Sending a response {0} back to client {1} port {2} ", response, ipEndpoint.Address, clientPort);
        client.BeginSend(sendBytes, sendBytes.Length, clientIP, new AsyncCallback(RespondClientCallback), client);

        // Important: close and re-open the broadcast listening port so that another async operation can start 
        udpClient.Close();
        StartListeningForClients(idCode, message);
    }

    private void RespondClientCallback(IAsyncResult ar)
    {
        //Reply to client has finished
        UdpClient udpClient = (UdpClient)ar.AsyncState;

        //Close the response port
        udpClient.Close();
    }
    #endregion
}
