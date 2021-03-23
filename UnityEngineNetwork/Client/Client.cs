using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace UnityEngineNetwork.Client {
  /// <summary>The client singleton to send and handle packets to and from the Server</summary>
  public sealed class Client : IClient {
    #region Static
    /// <summary>The singleton instance</summary>
    internal static Client Instance => _instance;

    private static readonly Client _instance = new Client();
    #endregion

    #region Variables
    /// <summary>The repository for all requests from and to the server</summary>
    public BaseServerRepository ServerRepository { get; private set; }

    private TCP _tcp;

    private UDP _udp;

    /// <summary>The client id</summary>
    public int Id { get; private set; } = 0;

    /// <summary>The server ip address</summary>
    public string ServerIpAddress { get; private set; }

    /// <summary>The server port</summary>
    public int Port { get; private set; }

    /// <summary>The username of the client</summary>
    public string Username { get; private set; }

    /// <summary>Indicates whther the client is connected or not</summary>
    public bool IsConnected { get; set; } = false;

    private Dictionary<int, PacketHandler> _packetHandlers = new Dictionary<int, PacketHandler>();

    private static readonly List<Action> _executeOnMainThread = new List<Action>();

    private static readonly List<Action> _executeCopiedOnMainThread = new List<Action>();

    private static bool _actionToExecuteOnMainThread = false;
    #endregion

    #region Delegates
    /// <summary>A delegate for packet handlers</summary>
    /// <param name="packet"></param>
    public delegate void PacketHandler(Packet packet);
    #endregion

    #region Events
    /// <summary>Gets invoked when the client has disconnected from the server</summary>
    public event OnDisconnectEventHandler OnDisconnect;

    /// <summary>Gets invoked when the client has connected to the server</summary>
    public event OnConnectEventHandler OnConnect;
    #endregion

    #region Constructor
    static Client() { }

    private Client() {
      AddPacketHandler(0, ServerRepository.HandleWelcome);
    }
    #endregion

    #region Methods
    /// <summary>Reconnects to the server by sending a welcome message if there was already a connection established before</summary>
    public void ReconnectToServer() {
      AddPacketHandler(0, ServerRepository.HandleWelcome);
      
      _tcp.Connect();
    }

    /// <summary>Connects to the server</summary>
    /// <param name="serverRepository">The server repository to use with this client</param>
    /// <param name="username">The username of this client</param>
    /// <param name="ipAddress">The ip address of the server</param>
    /// <param name="port">The port to connect to the server</param>
    public void ConnectToServer(BaseServerRepository serverRepository, string username, string ipAddress = Constants.LocalHost, int port = Constants.DefaultPort) {
      if (String.IsNullOrEmpty(username.Trim())) {
        throw new ArgumentException($"The username '{username}' is not valid");
      }
      if (String.IsNullOrEmpty(ipAddress.Trim())) {
        throw new ArgumentException($"The ip address '{ipAddress}' is not valid");
      }
      if (port <= 0) {
        throw new ArgumentException($"The port '{port}' is not valid");
      }
      if (serverRepository == null) {
        throw new ArgumentException($"The serverRepository '{serverRepository}' is not valid");
      }

      ServerRepository = serverRepository;
      Client.Instance.ServerIpAddress = ipAddress;
      Client.Instance.Port = port;
      Client.Instance.Username = username;
      _tcp = new TCP();
      _udp = new UDP(ServerIpAddress, Port);
      _tcp.OnConnect += (sender, e) => { OnConnect?.Invoke(this, e); };
      ReconnectToServer();
    }

    internal void ConnectToUdp() {
      _udp.Connect(((IPEndPoint)_tcp?.Socket?.Client.LocalEndPoint).Port);
    }

    internal void InitId(int id) {
      Id = id;
    }

    /// <summary>Sends a packet over tcp to the server</summary>
    /// <param name="packet">The packet</param>
    public void SendTCPData(Packet packet) {
      _tcp.SendData(packet);
    }

    /// <summary>Sends a packet over udp to the server</summary>
    /// <param name="packet">The packet</param>
    public void SendUDPData(Packet packet) {
      _udp.SendData(packet);
    }

    /// <summary>Adds a packethandler to the list of packethandlers</summary>
    /// <param name="id">The id of the packethandler which has to be greater than 0, because the first packet handler is the welcome message packet handler</param>
    /// <param name="handler">The packet handler</param>
    public void AddPacketHandler(int id, PacketHandler handler) {
      if (id == 0 && _packetHandlers.Any()) {
        throw new ArgumentException("The id cannot be 0, because 0 is already used to send username and client id.");
      }
      if (id < 0) {
        throw new ArgumentException("The id must be a positive number.");
      }
      _packetHandlers.Add(id, handler);
    }

    internal void HandleReceivedPacket(Packet packet) {
      _packetHandlers[packet.ReadInt()](packet);
    }

    /// <summary>Disconnects from the server</summary>
    public void Disconnect() {
      if (IsConnected) {
        IsConnected = false;
        _tcp.Disconnect();
        _udp.Disconnect();

        OnDisconnect?.Invoke(this, new EventArgs());
      }
    }
    #endregion

    #region Manging Thread
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
