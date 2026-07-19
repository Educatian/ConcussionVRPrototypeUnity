using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ConcussionVR.Editor
{
    public static class InterventionPreviewCapture
    {
        private const int Width = 1600;
        private const int Height = 900;
        private const string OutputDirectory = "Docs/QA/HighFidelity";

        [MenuItem("Concussion VR/Capture High Fidelity Previews")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(OutputDirectory);
            Capture("01-nature-calm.png", environment => environment.ShowCalm());
            Capture("02-gaze-stabilization.png", environment => environment.ShowPursuit());
            Capture("03-memory-reaction.png", environment => environment.ShowMemory(1));
            AssetDatabase.Refresh();
            Debug.Log($"High fidelity captures saved to {Path.GetFullPath(OutputDirectory)}");
        }

        private static void Capture(string fileName, Action<TaskEnvironment> build)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Capture Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, 0f);
            cameraObject.transform.rotation = Quaternion.identity;
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 80f;
            camera.allowHDR = true;
            camera.allowMSAA = true;

            var environment = new TaskEnvironment(camera);
            build(environment);

            var renderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                name = $"Capture {fileName}"
            };
            renderTexture.Create();
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(OutputDirectory, fileName), texture.EncodeToPNG());

            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }
}
