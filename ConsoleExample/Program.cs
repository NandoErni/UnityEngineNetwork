using ConsoleExample.Client;
using ConsoleExample.Server;
using System;
using System.Threading.Tasks;
using UnityEngineNetwork;

namespace ConsoleExample {
  class Program : MonoBehaviour {
    private ServerNetworkManager _server;

    private ClientNetworkManager _client;

    static async Task Main(string[] args) {
      Program consoleExample = new Program();

      await consoleExample.InstantiateNetworkManager();
      //await consoleExample.SendTCPAndUDP();
      await consoleExample.ReconnectToServer();

      while (true) {}
    }

    private async Task SendTCPAndUDP() {
      Console.WriteLine("TCP:");
      _server.SendSomeNums(Protocol.TCP);
      await Delay();
      _client.SendSomeNums(Protocol.TCP);
      await Delay();

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("UDP:");
      _server.SendSomeNums(Protocol.UDP);
      await Delay();
      _client.SendSomeNums(Protocol.UDP);
      await Delay();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
    }

    private async Task ReconnectToServer() {
      Console.WriteLine("Disconnecting...");
      _client.Client.Disconnect();
      Console.WriteLine();
      await Delay();
      Console.WriteLine("Connecting...");
      _client.Client.ConnectToServer();
      await Delay();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      await SendTCPAndUDP();
    }

    #region Methods
    /// <summary>We instantiate the network managers. 
    /// In the Constructor of the client network manager we connect to the server and add the sumNumHandler to the PackageHandlers.
    /// In the constructor of the server network manager we start the server and also add the sumNumHandler to the PackageHandlers.
    /// It's important that we first start the server before trying to connect to it!</summary>
    private async Task InstantiateNetworkManager() {
      _server = new ServerNetworkManager(new ClientRepository());
      _client = new ClientNetworkManager();
      await Task.Delay(100); // Delay, because the client and server need some time to set up
    }

    /// <summary>Represents Unitys Update method</summary>
    protected override void Update() {
      if (_server == null || _client == null) return;

      _server.Update();
      _client.Update();
    }

    /// <summary>2 second delay</summary>
    /// <returns></returns>
    private Task Delay() {
      return Task.Delay(1000);
    }
    #endregion
  }
}
