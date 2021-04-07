using System;
using UnityEngineNetwork;
using UnityEngineNetwork.Server;

namespace ConsoleExample.Server {
  public class ClientRepository : BaseClientRepository {
    public void SendNumbers(Protocol protocol) {
      Console.WriteLine();
      Console.WriteLine("Server: Sending numbers!");
      using (Packet packet = new Packet((int)RequestId.SumNum)) {
        packet.Write(3);
        packet.Write(5);

        if (protocol.Equals(Protocol.TCP)) SendTCPDataToClient(1, packet);
        if (protocol.Equals(Protocol.UDP)) SendUDPDataToClient(1, packet);
      }
    }

    public void HandleNumbers(int clientId, Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"Server: The client {clientId} says that the sum is: {num1 + num2}!");
    }
  }
}
