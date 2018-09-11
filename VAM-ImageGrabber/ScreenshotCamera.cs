using System.IO;
using UnityEngine;

namespace VAM_ImageGrabber
{
    class ScreenshotCamera
    {
        public ScreenshotCamera(int aWidth, int aHeight, Transform aTarget, int aAngle, float aDistance)
        {
            this._camera = new GameObject().AddComponent<Camera>();
            this._camera.name = "ScreenshotCamera";
            this._camera.enabled = true;
            this._camera.fieldOfView = 20f;
            this._renderTexture = RenderTexture.GetTemporary(aWidth, aHeight, 24);
            this._camera.targetTexture = this._renderTexture;
            this._texture2d = new Texture2D(this._renderTexture.width, this._renderTexture.height, TextureFormat.RGB24, false);
            this._target = aTarget;
            this._angle = aAngle;
            this._distance = aDistance;
        }

        public void SetPosition()
        {
            this._camera.transform.SetPositionAndRotation(this._target.position + new Vector3(0f, 0f, this._distance), Quaternion.identity);
            this._camera.transform.RotateAround(this._target.position, Vector3.up, (float)this._angle);
            this._camera.transform.LookAt(this._target.transform);
        }

        public void TakeScreenshot(string aFilename)
        {
            RenderTexture.active = this._renderTexture;
            this._texture2d.ReadPixels(new Rect(0f, 0f, (float)this._renderTexture.width, (float)this._renderTexture.height), 0, 0);
            this._texture2d.Apply();
            byte[] bytes = this._texture2d.EncodeToPNG();
            File.WriteAllBytes(aFilename, bytes);
            RenderTexture.active = null;
        }

        public void Release()
        {
            UnityEngine.Object.Destroy(this._camera);
            UnityEngine.Object.Destroy(this._texture2d);
            RenderTexture.ReleaseTemporary(this._renderTexture);
        }

        private Camera _camera;

        private RenderTexture _renderTexture;

        private Texture2D _texture2d;

        private Transform _target;

        public int _angle;

        private float _distance;
    }
}
