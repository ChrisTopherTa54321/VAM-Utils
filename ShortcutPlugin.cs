using IllusionPlugin;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace VAM_Utils
{
    class ShortcutPlugin : IPlugin
    {
        private delegate void ActionFunction();
        private Dictionary<string, ActionFunction> _shortcuts;

        private float _worldScaleStep;
        private float _worldScaleMin;
        private float _worldScaleMax;

        private float _timeScaleStep;
        private float _timeScaleMin;
        private float _timeScaleMax;

        private float _animationSpeedStep;
        private float _animationSpeedMin;
        private float _animationSpeedMax;

        private float _shiftMultiplier;

        private bool _swapTimeAndAnim;

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
            // Create the UserData folder if it doesn't exist
            System.IO.Directory.CreateDirectory(Path.Combine(System.Environment.CurrentDirectory, "UserData"));

            const string invalidPresetKey = "";
            const float invalidPresetVal = -1000f;
            const int maxPresets = 100;
            _worldScaleStep =       ModPrefs.GetFloat("ShortcutPluginVars", "WorldScaleStepSize", 0.01f, true);
            _worldScaleMin =        ModPrefs.GetFloat("ShortcutPluginVars", "WorldScaleMin", 0.01f, true);
            _worldScaleMax =        ModPrefs.GetFloat("ShortcutPluginVars", "WorldScaleMax", 10.0f, true);
            _timeScaleStep =        ModPrefs.GetFloat("ShortcutPluginVars", "TimeScaleStepSize", 0.01f, true);
            _timeScaleMin =         ModPrefs.GetFloat("ShortcutPluginVars", "TimeScaleMin", 0.1f, true);
            _timeScaleMax =         ModPrefs.GetFloat("ShortcutPluginVars", "TimeScaleMax", 1.0f, true);
            _animationSpeedStep =   ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedStepSize", 0.05f, true);
            _animationSpeedMin =    ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedMin", -1.0f, true);
            _animationSpeedMax =    ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedMax", 5.0f, true);
            _shiftMultiplier = ModPrefs.GetFloat("ShortcutPluginVars", "ShiftKeyMultiplier", 5.0f, true);

            _swapTimeAndAnim = false;

            // Put some sample presets in the INI
            ModPrefs.GetFloat("ShortcutPluginVars", "TimeScalePreset0", 1.0f, true);
            ModPrefs.GetFloat("ShortcutPluginVars", "TimeScalePreset1", 0.5f, true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetTimeScalePreset0", ";", true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetTimeScalePreset1", "'", true);

            ModPrefs.GetFloat("ShortcutPluginVars", "WorldScalePreset0", 1.0f, true);
            ModPrefs.GetFloat("ShortcutPluginVars", "WorldScalePreset1", 1.15f, true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetWorldScalePreset0", "[", true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetWorldScalePreset1", "]", true);

            ModPrefs.GetFloat("ShortcutPluginVars", "WorldScalePreset0", 1.0f, true);
            ModPrefs.GetFloat("ShortcutPluginVars", "WorldScalePreset1", 1.15f, true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetWorldScalePreset0", "[", true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetWorldScalePreset1", "]", true);

            ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedPreset0", 1.0f, true);
            ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedPreset1", 0.0f, true);
            ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedPreset2", -1.0f, true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetAnimationSpeedPreset0", "0", true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetAnimationSpeedPreset1", "9", true);
            ModPrefs.GetString("ShortcutPluginKeys", "SetAnimationSpeedPreset2", "8", true);

            _shortcuts = new Dictionary<string, ActionFunction>()
            {
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "MoveSelToCam", "\\", true).ToLower(),
                    () => MoveSelectedAtomToCamera()
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "IncWorldScale", "p", true).ToLower(),
                    () => ChangeWorldScale( _worldScaleStep )
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "DecWorldScale", "o", true).ToLower(),
                    () => ChangeWorldScale( -_worldScaleStep )
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "IncTimeScale", "l", true).ToLower(),
                    () => {
                        if( _swapTimeAndAnim )
                        {
                            ChangeAnimationSpeed( _animationSpeedStep );
                        }
                        else
                        {
                            ChangeTimeScale( _timeScaleStep );
                        }
                    }
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "DecTimeScale", "k", true).ToLower(),
                    () => {
                        if( _swapTimeAndAnim )
                        {
                            ChangeAnimationSpeed( -_animationSpeedStep );
                        }
                        else
                        {
                            ChangeTimeScale( -_timeScaleStep );
                        }
                    }
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "TogglePause", "j", true).ToLower(),
                    () => TogglePause()
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "IncAnimationSpeed", "7", true).ToLower(),
                    () => {
                        if( _swapTimeAndAnim )
                        {
                            ChangeTimeScale( _timeScaleStep );
                        }
                        else
                        {
                            ChangeAnimationSpeed( _animationSpeedStep );
                        }
                    }
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "DecAnimationSpeed", "6", true).ToLower(),
                    () => {
                        if( _swapTimeAndAnim )
                        {
                            ChangeTimeScale( -_timeScaleStep );
                        }
                        else
                        {
                            ChangeAnimationSpeed( -_animationSpeedStep );
                        }
                    }
                },
                {
                    ModPrefs.GetString("ShortcutPluginKeys", "SwapTimeAndAnim", "5", true).ToLower(),
                    () =>
                    {
                        _swapTimeAndAnim = !_swapTimeAndAnim;
                    }
                },

            };

            // Add presets
            for( int i = 0; i < maxPresets; ++i )
            {
                var tsPresetKey = ModPrefs.GetString("ShortcutPluginKeys", "SetTimeScalePreset" + i.ToString(), invalidPresetKey, false);
                var tsPresetVal = ModPrefs.GetFloat("ShortcutPluginVars", "TimeScalePreset" + i.ToString(), invalidPresetVal, false);
                bool tsSet = tsPresetKey != invalidPresetKey && tsPresetVal != invalidPresetVal;

                var asPresetKey = ModPrefs.GetString("ShortcutPluginKeys", "SetAnimationSpeedPreset" + i.ToString(), invalidPresetKey, false);
                var asPresetVal = ModPrefs.GetFloat("ShortcutPluginVars", "AnimationSpeedPreset" + i.ToString(), invalidPresetVal, false);
                bool asSet = asPresetKey != invalidPresetKey && asPresetVal != invalidPresetVal;

                var wsPresetKey = ModPrefs.GetString("ShortcutPluginKeys", "SetWorldScalePreset" + i.ToString(), invalidPresetKey, false);
                var wsPresetVal = ModPrefs.GetFloat("ShortcutPluginVars", "WorldScalePreset" + i.ToString(), invalidPresetVal, false);


                if( tsSet && !asSet )
                {
                    _shortcuts.Add(tsPresetKey, () => SetTimeScale(tsPresetVal));
                }
                if( asSet && !tsSet )
                {
                    _shortcuts.Add(asPresetKey, () => SetAnimationSpeed(asPresetVal));
                }
                if (tsSet && asSet)
                {
                    _shortcuts.Add(asPresetKey, () => {
                        if (_swapTimeAndAnim)
                        {
                            SetTimeScale(tsPresetVal);
                        }
                        else
                        {
                            SetAnimationSpeed(asPresetVal);
                        }
                    } );

                    _shortcuts.Add(tsPresetKey, () => {
                        if (_swapTimeAndAnim)
                        {
                            SetAnimationSpeed(asPresetVal);
                        }
                        else
                        {
                            SetTimeScale(tsPresetVal);
                        }
                    });
                }
                if (wsPresetKey != invalidPresetKey && wsPresetVal != invalidPresetVal)
                {
                    _shortcuts.Add(wsPresetKey, () => SetWorldScale(wsPresetVal));
                }

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
        }

        public void OnLevelWasLoaded(int level)
        {
        }


        public void OnUpdate()
        {
            foreach (KeyValuePair<string, ActionFunction> shortcut in _shortcuts)
            {
                if(shortcut.Key.Length > 0 && Input.GetKeyDown(shortcut.Key))
                {
                    shortcut.Value();
                }
            }
        }


        private void ChangeWorldScale( float aDelta )
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                aDelta *= _shiftMultiplier;
            }
            SetWorldScale( SuperController.singleton.worldScale + aDelta);
        }


        private void SetWorldScale( float aScale )
        {
            if( aScale > _worldScaleMax )
            {
                aScale = _worldScaleMax;
            }
            else if( aScale < _worldScaleMin )
            {
                aScale = _worldScaleMin;
            }
            SuperController.singleton.worldScale = aScale;
        }



        private void ChangeTimeScale(float aDelta)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                aDelta *= _shiftMultiplier;
            }
            SetTimeScale(TimeControl.singleton.currentScale + aDelta);
        }


        private void SetTimeScale(float aSpeed)
        {
            if (aSpeed > _timeScaleMax)
            {
                aSpeed = _timeScaleMax;
            }
            else if (aSpeed < _timeScaleMin)
            {
                aSpeed = _timeScaleMin;
            }
            TimeControl.singleton.currentScale = aSpeed;
        }

        private void ChangeAnimationSpeed(float aDelta)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                aDelta *= _shiftMultiplier;
            }
            SetAnimationSpeed(SuperController.singleton.motionAnimationMaster.playbackSpeed + aDelta);
        }


        private void SetAnimationSpeed(float aSpeed)
        {
            if (aSpeed > _animationSpeedMax)
            {
                aSpeed = _animationSpeedMax;
            }
            else if (aSpeed < _animationSpeedMin)
            {
                aSpeed = _animationSpeedMin;
            }
            SuperController.singleton.motionAnimationMaster.playbackSpeed = aSpeed;
        }

        private void TogglePause()
        {
            SuperController.singleton.SetFreezeAnimation( !SuperController.singleton.freezeAnimation );
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
