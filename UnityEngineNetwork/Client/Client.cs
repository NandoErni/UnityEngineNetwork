using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Client {
  /// <summary>The client singleton to send & handle packets to and from the Server</summary>
  public sealed class Client : IClient {
    #region Static
    public static Client Instance => _instance;

    private static readonly Client _instance = new Client();
    #endregion

    #region Variables
    public BaseServerRepository ServerRepository { get; private set; }

    public TCP Tcp;

    public UDP Udp;

    public int Id { get; private set; } = 0;

    public string ServerIpAddress { get; private set; }

    public int Port { get; private set; }

    public string Username { get; private set; }

    public bool IsConnected { get; set; } = false;

    private Dictionary<int, PacketHandler> _packetHandlers = new Dictionary<int, PacketHandler>();

    private static readonly List<Action> _executeOnMainThread = new List<Action>();

    private static readonly List<Action> _executeCopiedOnMainThread = new List<Action>();

    private static bool _actionToExecuteOnMainThread = false;
    #endregion

    #region Delegates
    public delegate void PacketHandler(Packet packet);
    #endregion

    #region Events
    public event OnDisconnectEventHandler OnDisconnect;

    public event OnConnectEventHandler OnConnect;
    #endregion

    #region Constructor
    static Client() { }

    private Client() {
      AddPacketHandler(0, ServerRepository.HandleWelcome);
    }
    #endregion

    #region Methods
    public void ConnectToServer() {
      AddPacketHandler(0, ServerRepository.HandleWelcome);
      
      Tcp.Connect();
    }

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
      Tcp = new TCP();
      Udp = new UDP();
      Tcp.OnConnect += (sender, e) => { OnConnect?.Invoke(this, e); };
      ConnectToServer();
    }

    internal void InitId(int id) {
      Id = id;
    }

    public void AddPacketHandler(int id, PacketHandler handler) {
      if (id == 0 && _packetHandlers.Any()) {
        throw new ArgumentException("The id cannot be 0, because 0 is already used to send username and client id.");
      }
      if (id < 0) {
        throw new ArgumentException("The id must be a positive number.");
      }
      _packetHandlers.Add(id, handler);
    }

    public void HandleReceivedPacket(Packet packet) {
      _packetHandlers[packet.ReadInt()](packet);
    }

    public void Disconnect() {
      if (IsConnected) {
        IsConnected = false;
        Tcp.Disconnect();
        Udp.Disconnect();

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
