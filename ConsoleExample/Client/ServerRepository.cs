using System;
using UnityEngineNetwork;
using UnityEngineNetwork.Client;

namespace ConsoleExample.Client {
  public class ServerRepository {

    private IClient _client;

    public ServerRepository(IClient client) {
      _client = client;
    }

    public void SendNumbers(Protocol protocol) {
      Console.WriteLine();
      Console.WriteLine("Client: Sending numbers!");
      using (Packet packet = new Packet((int)RequestId.SumNum)) {
        packet.Write(3);
        packet.Write(5);

        if (protocol.Equals(Protocol.TCP)) _client.SendTCPData(packet);
        if (protocol.Equals(Protocol.UDP)) _client.SendUDPData(packet);

      }
    }

    public void HandleNumbers(Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"Client: The sum is: {num1 + num2}!");
    }
  }
}
