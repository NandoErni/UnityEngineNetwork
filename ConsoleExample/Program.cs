using ConsoleExample.Client;
using ConsoleExample.Server;
using System;
using System.Threading.Tasks;

namespace ConsoleExample {
  class Program {
    private const int FPS = 30;

    private static ServerNetworkManager _server;

    private static ClientNetworkManager _client;

    static void Main(string[] args) {
      InstantiateNetworkManager();

      Task.Run(Update); // This is just for emulating unitys update method. You don't have to do it in unity.

      _server.SendSomeNums(); // This sends the sum number example from the server to the client.

      Task.Run(SendClientRequestToServerAfter5Seconds); // This will send the Client request SendNumbers() to the server after 5 seconds. (ConsoleExample.Client.ServerRepository.SendNumbers())

      NeverEnd(); // This prevents the method from returning
    }

    /// <summary>We instantiate the network managers. 
    /// In the Constructor of the client network manager we connect to the server and add the sumNumHandler to the PackageHandlers.
    /// In the constructor of the server network manager we start the server and also add the sumNumHandler to the PackageHandlers.
    /// It's important that we first start the server before trying to connect to it!</summary>
    static void InstantiateNetworkManager() {
      _server = new ServerNetworkManager(new ClientRepository());
      _client = new ClientNetworkManager(new ServerRepository());
    }

    /// <summary>Represents Unitys Update method</summary>
    static async void Update() {
      _server.Update();
      _client.Update();

      // you don't need the rest of this function in unity. Just for demonstration.
      await Task.Delay(1000/FPS);
      Update();
    }

    static async void SendClientRequestToServerAfter5Seconds() {
      await Task.Delay(1000 / FPS);
      _client.SendSomeNums();
    }

    static void NeverEnd() {
      while (true) { }
    }
  }
}
