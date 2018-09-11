using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;

namespace VAM_ImageGrabber
{
    class ImageGrabberPlugin : IPlugin
    {
        Foto2VamServer _server;

        public string Name
        {
            get
            {
                return "ImageGrabber";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLevelWasInitialized(int level)
        {
            RestartServer();
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnUpdate()
        {
            if( Input.GetKeyDown("p") && Input.GetKey(KeyCode.LeftAlt) )
            {
                RestartServer();
            }
        }

        private void RestartServer()
        {
            if (null != _server)
            {
                _server.Dispose();
                _server = null;
            }
            _server = new Foto2VamServer();
        }

    }
}
