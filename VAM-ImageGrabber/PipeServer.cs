using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace VAM_ImageGrabber
{
    public class PipeServer : IDisposable
    {
        public PipeServer( string aPipeName )
        {
            this._pipeName = aPipeName;
            this._connectionCallback = new AsyncCallback(this.HandleConnection);
            this._readCallback = new AsyncCallback(this.HandleRead);
            this._readBuffer = new byte[4096];
            this._handlers = new Dictionary<string, Action<JSONNode>>();
            this.StartServer();
        }

        ~PipeServer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose( bool aDisposing )
        {
            if ( !_disposed )
            {
                this.Disconnect();
                _disposed = true;
            }
        }

        private void HandleConnection(IAsyncResult ar)
        {
            this._pipeServer.EndWaitForConnection(ar);
            if (this._pipeServer.IsConnected)
            {
                this._pipeServer.BeginRead(this._readBuffer, 0, this._readBuffer.Length, this._readCallback, null);
            }
        }

        private void HandleRead(IAsyncResult ar)
        {
            int num = this._pipeServer.EndRead(ar);
            if (num == 0)
            {
                this.Disconnect();
                // Other side closed connection. Re-open server
                this.StartServer();
                return;
            }
            this._recvdString += Encoding.Default.GetString(this._readBuffer, 0, num);
            string text = "<EOM>";
            int num2;
            while ((num2 = this._recvdString.IndexOf(text)) >= 0)
            {
                string aJSON = this._recvdString.Substring(0, num2);
                this._recvdString = this._recvdString.Substring(num2 + text.Length);
                JSONNode aMsg = JSON.Parse(aJSON);
                this.HandleMessage(aMsg);
            }
            this._pipeServer.BeginRead(this._readBuffer, 0, this._readBuffer.Length, this._readCallback, null);
        }

        private void Disconnect()
        {
            if (this._pipeServer.IsConnected)
            {
                this._pipeServer.Disconnect();
            }
            else
            {
                /* There is no way to cancel BeginWaitForConnection... so just open the other side of the pipe. */
                try
                {
                    var file = System.IO.File.Open("\\\\.\\\\pipe\\\\" + this._pipeName, System.IO.FileMode.Open);
                    file.Close();
                }
                catch
                {
                }
            }
            this._pipeServer.Dispose();
            this._pipeServer = null;
        }

        private void StartServer()
        {
            if (this._pipeServer != null)
            {
                this.Disconnect();
            }

            this._pipeServer = new NamedPipeServerStream(this._pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous );

            this._pipeServer.BeginWaitForConnection(this._connectionCallback, null);
        }

        private void HandleMessage(JSONNode aMsg)
        {
            string value = aMsg["cmd"].Value;
            if (this._handlers.ContainsKey(value))
            {
                this._handlers[value](aMsg);
            }
        }

        public void RegisterHandler(string aCmd, Action<JSONNode> aHandler)
        {
            this._handlers[aCmd] = aHandler;
        }

        private AsyncCallback _connectionCallback;

        private AsyncCallback _readCallback;

        private NamedPipeServerStream _pipeServer;

        private string _pipeName;

        private byte[] _readBuffer;

        private string _recvdString;

        private Dictionary<string, Action<JSONNode>> _handlers;

        private bool _disposed = false;
    }
}
