using System;
using System.Linq;

namespace UnityEngineNetwork.Server {
  /// <summary>Controlls all requests from and to the client</summary>
  public abstract class BaseClientRepository {
    #region events
    /// <summary>Gets invoked when the welcome message was received</summary>
    public event OnWelcomeReceivedEventHandler OnWelcomeReceived;
    #endregion

    #region Send TCP/UDP
    /// <summary>Sends a packet to the client via tcp</summary>
    /// <param name="clientId">The client id to send to</param>
    /// <param name="packet">The packet to send</param>
    protected void SendTCPDataToClient(int clientId, Packet packet) {
      packet.WriteLength();
      Server.Instance.Clients[clientId].Tcp.SendData(packet);
    }

    /// <summary>Sends a packet to all clients via tcp</summary>
    /// <param name="packet">The packet to send</param>
    protected void SendTCPDataToAllClients(Packet packet) {
      SendTCPDataToAllClients(packet, -1);
    }

    /// <summary>Sends a packet to all clients with some exceptions via tcp</summary>
    /// <param name="packet">The packet to send</param>
    /// <param name="blockList">The array of client ids which will not receive the packet</param>
    protected void SendTCPDataToAllClients(Packet packet, params int[] blockList) {
      packet.WriteLength();
      foreach (var client in Server.Instance.Clients) {
        if (!blockList.Contains(client.Key)) {
          client.Value.Tcp.SendData(packet);
        }
      }
    }

    /// <summary>Sends udp data to a client</summary>
    /// <param name="clientId">The client id to send to</param>
    /// <param name="packet">The packet to send</param>
    protected void SendUDPDataToClient(int clientId, Packet packet) {
      packet.WriteLength();
      Server.Instance.Clients[clientId].Udp.SendData(packet);
    }

    /// <summary>Sends a packet to all clients via udo</summary>
    /// <param name="packet"></param>
    protected void SendUDPDataToAllClients(Packet packet) {
      SendUDPDataToAllClients(packet, -1);
    }

    /// <summary>Sends a packet to all cliets with some exceptions</summary>
    /// <param name="packet">The packet to send</param>
    /// <param name="blockList">The array of client ids which will not receive the packet</param>
    protected void SendUDPDataToAllClients(Packet packet, params int[] blockList) {
      packet.WriteLength();
      foreach (var client in Server.Instance.Clients) {
        if (!blockList.Contains(client.Key)) {
          client.Value.Udp.SendData(packet);
        }
      }
    }
    #endregion

    /// <summary>Sends a welcome message</summary>
    /// <param name="receiverClientId">The receiver client id</param>
    /// <param name="message">The message to send</param>
    public void SendWelcome(int receiverClientId, string message) {
      using (Packet packet = new Packet(0)) {
        packet.Write(message);
        packet.Write(receiverClientId);

        SendTCPDataToClient(receiverClientId, packet);
      }
    }

    /// <summary>Handles the welcome message which was sent back</summary>
    /// <param name="clientId">The client id, which sent the welcome message</param>
    /// <param name="packet">The packet</param>
    public void HandleWelcomeReceived(int clientId, Packet packet) {
      int clientIdCheck = packet.ReadInt();
      string username = packet.ReadString();

      if (clientIdCheck != clientId) {
        Server.Instance.Clients[clientId].Disconnect();
        throw new ConnectionFailedException($"Player \"{username}\" (ID: {clientId}) has assumed the wrong client ID ({clientIdCheck})!");
      }

      Server.Instance.Clients[clientId].Username = username;

      OnWelcomeReceived?.Invoke(this, new WelcomeEventArgs(username));
    }
  }
}
