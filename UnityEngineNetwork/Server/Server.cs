using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Server {
  public sealed class Server : IServer {
    #region Static
    public static Server Instance => _instance;

    private static readonly Server _instance = new Server();
    #endregion

    #region Variables
    public ClientRepository ClientRepository { get; private set; }

    public Dictionary<int, Client> Clients { get; private set; } = new Dictionary<int, Client>();

    public int MaxClients { get; private set; }

    public int Port { get; private set; }

    public Dictionary<int, PacketHandler> PacketHandlers = new Dictionary<int, PacketHandler>();

    public TcpListener TcpListener { get; private set; }

    public UdpClient UdpListener { get; private set; }

    public bool IsRunning { get; private set; }

    private readonly List<Action> _executeOnMainThread = new List<Action>();

    private readonly List<Action> _executeCopiedOnMainThread = new List<Action>();

    private bool _actionToExecuteOnMainThread = false;

    private List<string> _stringBlockList = new List<string>();
    #endregion

    #region Delegates
    public delegate void PacketHandler(int clientId, Packet packet);
    #endregion

    #region Events
    public event OnClientConnectedEventHandler OnClientConnected;

    public event OnClientDisconnectedEventHandler OnClientDisconnected;

    public event OnServerStartedEventHandler OnServerStarted;

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
    public void Start(ClientRepository clientRepository, int maxPlayers, int port = Constants.DefaultPort) {
      ClientRepository = clientRepository;
      MaxClients = maxPlayers;
      Port = port;

      InitServerData();

      TcpListener = new TcpListener(IPAddress.Any, Port);
      TcpListener.Start();
      TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

      UdpListener = new UdpClient(Port);
      UdpListener.BeginReceive(UDPReceiveCallback, null);

      IsRunning = true;

      OnServerStarted?.Invoke(this, new EventArgs());
    }

    public void Stop() {
      Clients = new Dictionary<int, Client>();
      TcpListener.Stop();
      UdpListener.Close();
      IsRunning = false;
      OnServerStopped?.Invoke(this, new EventArgs());
    }

    public void AddPacketHandler(int id, PacketHandler handler) {
      if (id == 0 && PacketHandlers.Any()) {
        throw new ArgumentException("The id cannot be 0, because 0 is already used to send username and client id.");
      }
      if (id < 0) {
        throw new ArgumentException("The id must be a positive number.");
      }
      PacketHandlers.Add(id, handler);
    }

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

    public void AddStringToBlock(string stringToBlock) {
      _stringBlockList.Add(stringToBlock);
    }
    #endregion

    #region Callbacks
    private void TCPConnectCallback(IAsyncResult result) {
      TcpClient client = TcpListener.EndAcceptTcpClient(result);
      TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

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
          Clients[clientId].Udp.HanldeData(packet);
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
