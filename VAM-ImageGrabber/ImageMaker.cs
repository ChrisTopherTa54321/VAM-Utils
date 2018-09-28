using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace VAM_ImageGrabber
{
    public class ImageMaker : MonoBehaviour
    {
        public ImageMaker()
        {
            this._event = new AutoResetEvent(false);
        }

        public void Update()
        {
            if (this.pendingAction != null)
            {
                try
                {
                    this.pendingAction();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception: " + ex.ToString());
                }
                finally
                {
                    this.pendingAction = null;
                }
            }
        }

        public void TakeScreenshot(string aName, string aJsonPath, string aOutputPath, List<float> aAngles, int aWidth, int aHeight)
        {
            this.pendingAction = delegate ()
            {
                GameObject gameObject = GameObject.Find(aName);
                if (gameObject == null)
                {
                    foreach (GameObject gameObject2 in UnityEngine.Object.FindObjectsOfType<GameObject>())
                    {
                        if (gameObject2.name.StartsWith(aName))
                        {
                            gameObject = gameObject2;
                            break;
                        }
                    }
                }
                if (gameObject == null)
                {
                    this._event.Set();
                    return;
                }
                Atom component = gameObject.GetComponent<Atom>();
                component.LoadAppearancePreset(aJsonPath);
                this.StartCoroutine(this.TakeScreenshotCo(component, aOutputPath, aAngles, aWidth, aHeight));
            };
            this._event.WaitOne();
        }

        private IEnumerator TakeScreenshotCo(Atom atom, string aOutputPath, List<float> aAngles, int aWidth, int aHeight)
        {
            List<ScreenshotCamera> cameras = new List<ScreenshotCamera>();
            Component head = null;
            Component component = null;
            foreach (Rigidbody rigidbody in atom.rigidbodies)
            {
                if (rigidbody.name == "head")
                {
                    head = rigidbody;
                }
                else if (rigidbody.name == "headControl")
                {
                    component = rigidbody;
                }
                if (null != component && null != head)
                {
                    break;
                }
            }
            foreach (int aAngle in aAngles)
            {
                cameras.Add(new ScreenshotCamera(aWidth, aHeight, head.transform, aAngle, 1f));
            }
            if (component.transform.rotation != Quaternion.identity)
            {
                component.transform.SetPositionAndRotation(component.transform.position, Quaternion.identity);
            }
            SuperController.singleton.HideMainHUD();
            do
            {
                yield return null;
            }
            while (SuperController.singleton.IsSimulationPaused());
            Vector3 prevPos = head.transform.position;
            for (;;)
            {
                yield return null;
                if ((double)(prevPos - head.transform.position).sqrMagnitude <= 0.01)
                {
                    break;
                }
                prevPos = head.transform.position;
            }
            foreach (ScreenshotCamera screenshotCamera in cameras)
            {
                screenshotCamera.SetPosition();
            }
            yield return null;
            foreach (ScreenshotCamera screenshotCamera2 in cameras)
            {
                screenshotCamera2.TakeScreenshot(string.Concat(new string[]
                {
                    aOutputPath,
                    "_",
                    screenshotCamera2._angle.ToString(),
                    ".png"
                }));
                screenshotCamera2.Release();
            }
            this._event.Set();
            yield break;
        }

        private Action pendingAction;

        private AutoResetEvent _event;
    }
}
