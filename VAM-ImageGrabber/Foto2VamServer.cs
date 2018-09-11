using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEngine;

namespace VAM_ImageGrabber
{
    public class Foto2VamServer : IDisposable
    {
        ~Foto2VamServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                this._exitThread = true;
                this._event.Set();
                this._thread.Join();
                UnityEngine.Object.Destroy(this._imageMaker);
                this._pipeServer.Dispose();
                this._pipeServer = null;


                // Note disposing has been done.
                _disposed = true;

            }
        }

        private void HandleTakeScreenshot(JSONNode aJsonNode)
        {
            string value = aJsonNode["json"].Value;
            string value2 = aJsonNode["outputPath"].Value;
            int asInt = aJsonNode["dimensions"][0].AsInt;
            int asInt2 = aJsonNode["dimensions"][1].AsInt;
            List<int> list = new List<int>();
            foreach (object obj in aJsonNode["angles"].AsArray)
            {
                JSONNode jsonnode = (JSONNode)obj;
                list.Add(jsonnode.AsInt);
            }
            this.Enqueue(delegate
            {
                this._imageMaker.TakeScreenshot("Person", value, value2, list, asInt, asInt2);
            });
        }

        private void WorkerThread()
        {
            while (!this._exitThread)
            {
                this._event.WaitOne();
                object @lock = this._lock;
                lock (@lock)
                {
                    while (this._queue.Count > 0)
                    {
                        this._queue.Dequeue()();
                    }
                }
            }
        }

        public Foto2VamServer()
        {
            this._pipeServer = new PipeServer("foto2vamPipe");
            this._pipeServer.RegisterHandler("screenshot", new Action<JSONNode>(this.HandleTakeScreenshot));
            this._imageMaker = new GameObject().AddComponent<ImageMaker>();
            this._thread = new Thread(new ThreadStart(this.WorkerThread));
            this._queue = new Queue<Action>();
            this._event = new AutoResetEvent(false);
            this._lock = new object();
            this._exitThread = false;
            this._thread.Start();
        }

        private void Enqueue(Action aAction)
        {
            object @lock = this._lock;
            lock (@lock)
            {
                this._queue.Enqueue(aAction);
            }
            this._event.Set();
        }

        private ImageMaker _imageMaker;

        private Thread _thread;

        private Queue<Action> _queue;

        private AutoResetEvent _event;

        private bool _exitThread;

        private object _lock;

        private PipeServer _pipeServer;

        private bool _disposed = false;
    }
}
