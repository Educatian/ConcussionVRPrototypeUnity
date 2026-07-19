using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ConcussionVR.Editor
{
    public static class PrototypeSceneBuilder
    {
        [MenuItem("Concussion VR/Build Prototype Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.06f, 0.09f);
            var lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            lightObject.GetComponent<Light>().type = LightType.Directional;
            new GameObject("Session Bootstrap", typeof(PrototypeSessionController));
            const string scenePath = "Assets/ConcussionVR/Scenes/ConcussionVRPrototype.unity";
            System.IO.Directory.CreateDirectory("Assets/ConcussionVR/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();
        }
    }
}
