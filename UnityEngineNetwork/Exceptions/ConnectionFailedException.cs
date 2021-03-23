using System;

namespace UnityEngineNetwork {
  /// <summary>Thrwo when a connection couldn't be established.</summary>
  public class ConnectionFailedException : Exception {

    /// <summary>Attach a message to the exception.</summary>
    /// <param name="message">The message</param>
    public ConnectionFailedException(string message)
        : base(message) {
    }

    /// <summary>Attach a message and the inner exception.</summary>
    /// <param name="message">The message</param>
    /// <param name="inner">The inner exception</param>
    public ConnectionFailedException(string message, Exception inner)
        : base(message, inner) {
    }
  }
}
