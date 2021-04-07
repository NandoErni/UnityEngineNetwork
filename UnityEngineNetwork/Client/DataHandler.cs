using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngineNetwork.Client {
  internal class DataHandler {
    private DataHandlerDelegate _dataHandler;

    public delegate void DataHandlerDelegate(byte[] data);

    public DataHandler(DataHandlerDelegate handler) {
      _dataHandler = handler;
    }

    public void Handle(byte[] data) {
      _dataHandler(data);
    }
  }
}
