using System;
using System.Collections.Generic;

namespace UnityEngineNetwork {
  internal class PacketHandler<T> {

    private Dictionary<int, T> _packetHandlers = new Dictionary<int, T>();

    /// <summary>Adds a packethandler to the list of packethandlers</summary>
    /// <param name="id">The id of the packethandler which has to be greater than 0, because the first packet handler is the welcome message packet handler</param>
    /// <param name="handler">The packet handler</param>
    public void AddPacketHandler(int id, T handler) {
      if (_packetHandlers.ContainsKey(id)) {
        throw new ArgumentException("The id must be unique.");
      }
      if (id < 0) {
        throw new ArgumentException("The id must be a positive number.");
      }
      _packetHandlers.Add(id, handler);
    }

    public bool TryAddPackerHandler(int id, T handler) {
      try {
        AddPacketHandler(id, handler);
      } catch {
        return false;
      }
      return true;
    }

    public T Get(int key) {
      _packetHandlers.TryGetValue(key, out T value);
      return value;
    }

    public void Flush() {
      _packetHandlers = new Dictionary<int, T>();
    }
  }
}
