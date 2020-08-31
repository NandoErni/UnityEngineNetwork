﻿using System;
using System.Net.Sockets;

namespace UnityEngineNetwork.Client {
  public class TCP {
    public TcpClient Socket;

    private NetworkStream _networkStream;

    private Packet _receivedData;

    private byte[] _receiveBuffer;

    public void Connect() {
      Socket = new TcpClient {
        ReceiveBufferSize = Constants.DataBufferSize,
        SendBufferSize = Constants.DataBufferSize,
      };

      _receiveBuffer = new byte[Constants.DataBufferSize];

      Socket.BeginConnect(Client.Instance.ServerIpAddress, Client.Instance.Port, ConnectCallback, Socket);
    }

    private void ConnectCallback(IAsyncResult result) {
      if (Socket.Connected) {
        Socket.EndConnect(result);
      }
      else {
        throw new ConnectionFailedException($"Unable to connect to Server {Client.Instance.ServerIpAddress}.");
      }

      Client.Instance.IsConnected = true; // todo: event

      _networkStream = Socket.GetStream();

      _receivedData = new Packet();

      _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        int byteLength = _networkStream.EndRead(result);
        if (byteLength <= 0) {
          Client.Instance.Disconnect();
          return;
        }

        byte[] data = new byte[byteLength];
        Array.Copy(_receiveBuffer, data, byteLength);

        _receivedData.Reset(HandleData(data));

        _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
      }
      catch (Exception ex) {
        Console.WriteLine($"Error receiving TCP data: {ex}");
        Client.Instance.Disconnect();
      }
    }

    public void SendData(Packet packet) {
      if (!Client.Instance.IsConnected) {
        throw new ConnectionFailedException("The client is not connected to the server!");
      }
      if (Socket != null) {
        _networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
      }
    }

    private bool HandleData(byte[] data) {
      int packetLength = 0;

      _receivedData.SetBytes(data);

      if (_receivedData.UnreadLength() >= 4) {
        packetLength = _receivedData.ReadInt();
        if (packetLength <= 0) {
          return true;
        }
      }

      while (packetLength > 0 && packetLength <= _receivedData.UnreadLength()) {
        byte[] packetBytes = _receivedData.ReadBytes(packetLength);
        Client.Instance.ExecuteOnMainThread(() => {
          using (Packet packet = new Packet(packetBytes)) {
            Client.Instance.HandleReceivedPacket(packet);
          }
        });

        packetLength = 0;
        if (_receivedData.UnreadLength() >= 4) {
          packetLength = _receivedData.ReadInt();
          if (packetLength <= 0) {
            return true;
          }
        }
      }

      if (packetLength <= 1) {
        return true;
      }

      return false;
    }

    public void Disconnect() {
      Socket.Close();
      _networkStream = null;
      _receiveBuffer = null;
      _receivedData = null;
      Socket = null;
    }
  }
}
