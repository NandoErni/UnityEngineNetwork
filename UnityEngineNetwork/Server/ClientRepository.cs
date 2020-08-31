using System;
using System.Linq;

namespace UnityEngineNetwork.Server {
  public abstract class ClientRepository {
    #region events
    public event OnWelcomeReceivedEventHandler OnWelcomeReceived;
    #endregion

    #region Send TCP/UDP
    protected void SendTCPDataToClient(int clientId, Packet packet) {
      packet.WriteLength();
      Server.Instance.Clients[clientId].Tcp.SendData(packet);
    }

    protected void SendTCPDataToAllClients(Packet packet) {
      SendTCPDataToAllClients(packet, -1);
    }

    protected void SendTCPDataToAllClients(Packet packet, params int[] blockList) {
      packet.WriteLength();
      foreach (var client in Server.Instance.Clients) {
        if (!blockList.Contains(client.Key)) {
          client.Value.Tcp.SendData(packet);
        }
      }
    }

    protected void SendUDPDataToClient(int clientId, Packet packet) {
      packet.WriteLength();
      Server.Instance.Clients[clientId].Udp.SendData(packet);
    }

    protected void SendUDPDataToAllClients(Packet packet) {
      SendUDPDataToAllClients(packet, -1);
    }

    protected void SendUDPDataToAllClients(Packet packet, params int[] blockList) {
      packet.WriteLength();
      foreach (var client in Server.Instance.Clients) {
        if (!blockList.Contains(client.Key)) {
          client.Value.Udp.SendData(packet);
        }
      }
    }
    #endregion

    public void SendWelcome(int receiverClientId, string message) {
      using (Packet packet = new Packet(0)) {
        packet.Write(message);
        packet.Write(receiverClientId);

        SendTCPDataToClient(receiverClientId, packet);
      }
    }

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
