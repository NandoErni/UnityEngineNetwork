using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngineNetwork {
  public class ConnectionFailedException : Exception {

    public ConnectionFailedException(string message)
        : base(message) {
    }

    public ConnectionFailedException(string message, Exception inner)
        : base(message, inner) {
    }
  }
}
