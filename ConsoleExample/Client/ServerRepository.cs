using System;
using UnityEngineNetwork;
using UnityEngineNetwork.Client;

namespace ConsoleExample.Client {
  public class ServerRepository : BaseServerRepository {
    public void SendNumbers() {
      Console.WriteLine("Client: Sending numbers!");
      using (Packet packet = new Packet((int)RequestId.SumNum)) {
        packet.Write(3);
        packet.Write(5);

        SendTCPData(packet);
      }
    }

    public void HandleNumbers(Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"Client: The sum is: {num1 + num2}!");
    }
  }
}
