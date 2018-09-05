﻿using IllusionPlugin;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace VAM_Utils
{
    class ShortcutPlugin : IPlugin
    {
        private delegate void ActionFunction();
        private Dictionary<string, ActionFunction> _shortcuts;

        public string Name
        {
            get
            {
                return "ShortcutPlugin";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public ShortcutPlugin()
        {
            _shortcuts = new Dictionary<string, ActionFunction>()
            {
                {
                    ModPrefs.GetString("ShortcutPlugin", "MoveSelToCam", "p", true).ToLower(),
                    () => MoveSelectedAtomToCamera()
                }
            };
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
        }

        public void OnLevelWasLoaded(int level)
        {
        }


        public void OnUpdate()
        {
            foreach (KeyValuePair<string, ActionFunction> shortcut in _shortcuts)
            {
                if (Input.GetKeyDown(shortcut.Key))
                {
                    shortcut.Value();
                }
            }
        }


        private void MoveSelectedAtomToCamera()
        {
            string selected = SuperController.singleton.selectAtomPopup.currentValue;
            Atom selectedAtom = SuperController.singleton.GetAtomByUid(selected);
            if (selectedAtom != null)
            {
                if (selectedAtom.freeControllers.Length > 0)
                {

                    selectedAtom.PauseSimulation(new AsyncFlag());
                    //         selectedAtom.collisionEnabled = false; XXX toggles too fast for the physics to keep up. Has to be done in an update or delegate.
                    if (Input.GetKey(KeyCode.RightShift))
                        selectedAtom.freeControllers[0].MoveTo(new Vector3(SuperController.singleton.OVRCenterCamera.transform.position.x, 0f, SuperController.singleton.OVRCenterCamera.transform.position.z));
                    else
                        selectedAtom.freeControllers[0].MoveTo(SuperController.singleton.OVRCenterCamera.transform.position);

                    //             selectedAtom.collisionEnabled = true;
                    selectedAtom.CheckResumeSimulation();
                }
                else
                    SuperController.LogError("VAM-Utils-ShortcutPlugin: No controller found to move");
            }
        }

    }
}
