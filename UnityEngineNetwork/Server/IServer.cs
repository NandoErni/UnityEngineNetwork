using System;
using System.Collections.Generic;

namespace UnityEngineNetwork.Server {
  /// <summary>The interface for the server singleton. You should never access it directly</summary>
  public interface IServer {
    #region Properties
    /// <summary>The repository for managing all request from and to the clients.</summary>
    BaseClientRepository ClientRepository { get; }

    /// <summary>All available clients. To get all connected clients, call <see cref="Client.IsConnected"/></summary>
    Dictionary<int, Client> Clients { get; }

    /// <summary>The maximum number of clients allowed.</summary>
    int MaxClients { get; }

    /// <summary>The port of the server.</summary>
    int Port { get; }

    /// <summary>Indicates whether the server is runnign or not.</summary>
    bool IsRunning { get; }
    #endregion

    #region Methods
    /// <summary>Starts the server.</summary>
    /// <param name="clientRepository">The client repository</param>
    /// <param name="maxClients">The maximum nuber of players</param>
    /// <param name="port">The port of the server.</param>
    void Start(BaseClientRepository clientRepository, int maxClients, int port = Constants.DefaultPort);

    /// <summary>Stops the server.</summary>
    void Stop();

    /// <summary>Adds a packet handler. The packet handler is used to handle incomming packets from the clients. 
    /// The id cannot be 0, because 0 is already used to send client id..</summary>
    /// <param name="id">The id of the packet handler. This id must match with the sender packet id of the clients.</param>
    /// <param name="handler">The handler method</param>
    /// <exception cref="ArgumentException">Is thrown when the id is below 1</exception>
    void AddPacketHandler(int id, Server.PacketHandlerDelegate handler);

    /// <summary>Gets a random ClientId</summary>
    /// <param name="hasToBeConnected">Indicates whether the client has to be connected or not.</param>
    /// <returns>the client id.</returns>
    int GetRandomClientId(bool hasToBeConnected = true);

    /// <summary>Filters out any blocked string.</summary>
    /// <param name="message">the message to filter</param>
    /// <param name="replacementChar">the character to replace the blocked string.</param>
    /// <returns>The filtered string</returns>
    string FilterOutBlockedStrings(string message, char replacementChar = '*');

    /// <summary>Adds a blocked string, so the <see cref="FilterOutBlockedStrings(string, char)"/> Method can filter it out.</summary>
    /// <param name="stringToBlock">The string to block.</param>
    void AddStringToBlock(string stringToBlock);

    /// <summary>Call this method inside of Unitys Update() method.</summary>
    void UpdateMain();
    #endregion

    #region Events
    /// <summary>Gets called when a Client connects to the server.</summary>
    event OnClientConnectedEventHandler OnClientConnected;

    /// <summary>Gets called when a Client disconnects from the server.</summary>
    event OnClientDisconnectedEventHandler OnClientDisconnected;

    /// <summary>Gets called when the server starts.</summary>
    event OnServerStartedEventHandler OnServerStarted;

    /// <summary>Gets called when the server stops.</summary>
    event OnServerStoppedEventHandler OnServerStopped;
    #endregion
  }
}
