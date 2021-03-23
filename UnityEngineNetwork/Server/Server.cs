using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Server {
  /// <summary>The server which handles all connected clients.</summary>
  public sealed class Server : IServer {
    #region Static
    /// <summary>The singleton instance</summary>
    internal static Server Instance => _instance;

    private static readonly Server _instance = new Server();
    #endregion

    #region Variables
    /// <summary>The repository for all client requests</summary>
    public BaseClientRepository ClientRepository { get; private set; }

    /// <summary>All clients with their unique ID</summary>
    public Dictionary<int, Client> Clients { get; private set; } = new Dictionary<int, Client>();

    /// <summary>Max number of clients that are able to connect</summary>
    public int MaxClients { get; private set; }

    /// <summary>The port which clients can connect to</summary>
    public int Port { get; private set; }

    /// <summary>Dictionary of all packet handlers</summary>
    public Dictionary<int, PacketHandler> PacketHandlers { get; private set; } = new Dictionary<int, PacketHandler>();

    private TcpListener _tcpListener;

    /// <summary>The udp listener</summary>
    public UdpClient UdpListener { get; private set; }

    /// <summary>Indicates whther the server is running</summary>
    public bool IsRunning { get; private set; }

    private readonly List<Action> _executeOnMainThread = new List<Action>();

    private readonly List<Action> _executeCopiedOnMainThread = new List<Action>();

    private bool _actionToExecuteOnMainThread = false;

    private List<string> _stringBlockList = new List<string>();
    #endregion

    #region Delegates
    /// <summary>Delegate for a packet handler</summary>
    /// <param name="clientId">The client id</param>
    /// <param name="packet">The packet</param>
    public delegate void PacketHandler(int clientId, Packet packet);
    #endregion

    #region Events
    /// <summary>Gets invoked when a client connects.</summary>
    public event OnClientConnectedEventHandler OnClientConnected;

    /// <summary>Gets invoked when a client diconnects.</summary>
    public event OnClientDisconnectedEventHandler OnClientDisconnected;

    /// <summary>Gets invoked when the server starts.</summary>
    public event OnServerStartedEventHandler OnServerStarted;

    /// <summary>Gets invoked when the server stopped.</summary>
    public event OnServerStoppedEventHandler OnServerStopped;
    #endregion

    #region Constructor
    static Server() { }

    private Server() {
      AddPacketHandler(0, ClientRepository.HandleWelcomeReceived);
    }
    #endregion

    #region Init
    private void InitServerData() {
      for (int i = 1; i <= MaxClients; i++) {
        Clients.Add(i, new Client(i));
        Clients[i].OnClientDisconnected += (sender, e) => { OnClientDisconnected?.Invoke(this, e); };
      }
    }
    #endregion

    #region Methods
    /// <summary>Starts the server</summary>
    /// <param name="clientRepository">The client repository which handles the communication to and from the clients</param>
    /// <param name="maxClients">The maximum amounts of connected clients</param>
    /// <param name="port">The port</param>
    public void Start(BaseClientRepository clientRepository, int maxClients, int port = Constants.DefaultPort) {
      ClientRepository = clientRepository;
      MaxClients = maxClients;
      Port = port;

      InitServerData();

      _tcpListener = new TcpListener(IPAddress.Any, Port);
      _tcpListener.Start();
      _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

      UdpListener = new UdpClient(Port);
      UdpListener.BeginReceive(UDPReceiveCallback, null);

      IsRunning = true;

      OnServerStarted?.Invoke(this, new EventArgs());
    }

    /// <summary>Stops the server</summary>
    public void Stop() {
      Clients = new Dictionary<int, Client>();
      _tcpListener.Stop();
      UdpListener.Close();
      IsRunning = false;
      OnServerStopped?.Invoke(this, new EventArgs());
    }

    /// <summary>Adds a packet handler</summary>
    /// <param name="id">The packet handler id</param>
    /// <param name="handler">The packet handler</param>
    public void AddPacketHandler(int id, PacketHandler handler) {
      if (id == 0 && PacketHandlers.Any()) {
        throw new ArgumentException("The id cannot be 0, because 0 is already used to send username and client id.");
      }
      if (id < 0) {
        throw new ArgumentException("The id must be a positive number.");
      }
      PacketHandlers.Add(id, handler);
    }

    /// <summary>Gets a random client id</summary>
    /// <param name="hasToBeConnected">Indicates whether the pciked client has to be connected to the server</param>
    /// <returns>the client id</returns>
    public int GetRandomClientId(bool hasToBeConnected = true) {
      int clientId;
      Random random = new Random();

      if (hasToBeConnected) {
        List<int> ids = Clients.Where(x => x.Value.IsConnected).Select(x => x.Value.Id).ToList();

        clientId = random.Next(0, ids.Count);
      }
      else {
        clientId = random.Next(0, MaxClients);
      }

      return clientId;
    }

    /// <summary>Replaces all characters of the blocklist from the specified string</summary>
    /// <param name="message">The string which will be cleaned</param>
    /// <param name="replacementChar">The char which will replace the blocked words</param>
    /// <returns></returns>
    public string FilterOutBlockedStrings(string message, char replacementChar = '*') {
      foreach (var offensiveWord in _stringBlockList) {
        if (message.Contains(offensiveWord)) {
          String replacement = "";

          for (int i = 0; i < offensiveWord.Length; i++) {
            replacement += replacementChar;
          }

          message = message.Replace(offensiveWord, replacement);
        }
      }

      return message;
    }

    /// <summary>Adds a string to the blocklist</summary>
    /// <param name="stringToBlock">The string to block</param>
    public void AddStringToBlock(string stringToBlock) {
      _stringBlockList.Add(stringToBlock);
    }
    #endregion

    #region Callbacks
    private void TCPConnectCallback(IAsyncResult result) {
      TcpClient client = _tcpListener.EndAcceptTcpClient(result);
      _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

      for (int i = 1; i <= MaxClients; i++) {
        if (Clients[i].Tcp.Socket == null) {
          Clients[i].Tcp.Connect(client);
          OnClientConnected?.Invoke(this, new ClientEventArgs(Clients[i], Protocol.TCP));
          return;
        }
      }

      throw new ConnectionFailedException($"{client.Client.RemoteEndPoint}: Server is full."); // probably not good
    }

    private void UDPReceiveCallback(IAsyncResult result) {
      IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
      byte[] data = UdpListener.EndReceive(result, ref clientEndPoint);
      UdpListener.BeginReceive(UDPReceiveCallback, null);

      if (data.Length < 4) {
        return;
      }

      using (Packet packet = new Packet(data)) {
        int clientId = packet.ReadInt();

        if (clientId == 0) {
          return;
        }

        if (Clients[clientId].Udp.EndPoint == null) {
          // new udp connection
          Clients[clientId].Udp.Connect(clientEndPoint);
          OnClientConnected?.Invoke(this, new ClientEventArgs(Clients[clientId], Protocol.UDP));
          return;
        }

        if (Clients[clientId].Udp.EndPoint.ToString() == clientEndPoint.ToString()) {
          Clients[clientId].Udp.HandleData(packet);
        }
      }
    }
    #endregion

    #region Managing Thread
    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="action">The action to be executed on the main thread.</param>
    public void ExecuteOnMainThread(Action action) {
      if (action == null) {
        return;
      }

      lock (_executeOnMainThread) {
        _executeOnMainThread.Add(action);
        _actionToExecuteOnMainThread = true;
      }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public void UpdateMain() {
      if (_actionToExecuteOnMainThread) {
        _executeCopiedOnMainThread.Clear();

        lock (_executeOnMainThread) {
          _executeCopiedOnMainThread.AddRange(_executeOnMainThread);
          _executeOnMainThread.Clear();
          _actionToExecuteOnMainThread = false;
        }

        for (int i = 0; i < _executeCopiedOnMainThread.Count; i++) {
          _executeCopiedOnMainThread[i]();
        }
      }
    }
    #endregion
  }
}
