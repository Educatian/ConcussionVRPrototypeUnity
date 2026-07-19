using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ConcussionVR.PlayModeTests
{
    public sealed class PrototypeSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator SceneBootsIntoPreSessionAssessment()
        {
            SceneManager.LoadScene("ConcussionVRPrototype");
            yield return null;
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<PrototypeSessionController>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<Canvas>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<DirectButtonInput>(), Is.Not.Null);
            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            Assert.That(labels, Has.Some.Matches<Text>(label => label.text.Contains("Pre-session symptom check")));
            Button continueButton = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(button => button.name == "Continue");
            continueButton.onClick.Invoke();
            yield return null;
            Assert.That(GameObject.Find("Nature Calm"), Is.Not.Null);
            Assert.That(GameObject.Find("Stable Horizon"), Is.Not.Null);
            Assert.That(GameObject.Find("Lake").GetComponent<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(2000));
            Assert.That(GameObject.Find("Mountain Range").GetComponent<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(1000));
            Renderer guide = GameObject.Find("Breathing Guide").GetComponent<Renderer>();
            Assert.That(guide.material.shader.name, Is.EqualTo("ConcussionVR/GuideGlow"));
            GameObject pine = GameObject.Find("Pine 0");
            Assert.That(pine, Is.Not.Null);
            Assert.That(pine.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(250));
            Assert.That(pine.GetComponentInChildren<MeshFilter>().sharedMesh.subMeshCount, Is.EqualTo(3));
            Assert.That(GameObject.Find("Cool Sky Fill").GetComponent<Light>().shadows, Is.EqualTo(LightShadows.None));
            Assert.That(GameObject.Find("Calm Sky Dome").GetComponent<Renderer>().sharedMaterial.shader.name,
                Is.EqualTo("ConcussionVR/CalmSky"));
            Assert.That(guide.material.color.g, Is.GreaterThan(guide.material.color.r));
        }

        [UnityTest]
        public IEnumerator InterventionEnvironmentsMatchStoryboardContract()
        {
            var cameraObject = new GameObject("Contract Test Camera");
            var camera = cameraObject.AddComponent<Camera>();
            var environment = new TaskEnvironment(camera);

            environment.ShowPursuit();
            yield return null;
            Assert.That(GameObject.Find("Gaze Stabilization"), Is.Not.Null);
            Assert.That(GameObject.Find("Fixation Horizontal"), Is.Not.Null);
            Assert.That(GameObject.Find("Fixation Vertical"), Is.Not.Null);
            Assert.That(GameObject.Find("Target Ring").GetComponent<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(400));
            Assert.That(GameObject.Find("Back Wall").GetComponent<Renderer>().sharedMaterial.shader.name,
                Is.EqualTo("ConcussionVR/ClinicalSurface"));

            environment.UpdatePursuit(Mathf.PI / 1.44f, PursuitDirection.Horizontal);
            Vector3 horizontal = GameObject.Find("Pursuit Target").transform.position;
            environment.UpdatePursuit(Mathf.PI / 1.44f, PursuitDirection.Vertical);
            Vector3 vertical = GameObject.Find("Pursuit Target").transform.position;
            Assert.That(Mathf.Abs(horizontal.x), Is.GreaterThan(1f));
            Assert.That(Mathf.Abs(horizontal.y), Is.LessThan(0.01f));
            Assert.That(Mathf.Abs(vertical.x), Is.LessThan(0.01f));
            Assert.That(Mathf.Abs(vertical.y), Is.GreaterThan(0.5f));

            environment.ShowMemory(1);
            yield return null;
            Assert.That(GameObject.Find("Memory and Reaction"), Is.Not.Null);
            GameObject backpack = GameObject.Find("Backpack");
            Assert.That(backpack, Is.Not.Null);
            Assert.That(backpack.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(1200));
            Assert.That(backpack.GetComponentInChildren<MeshFilter>().sharedMesh.subMeshCount, Is.EqualTo(2));
            Assert.That(Quaternion.Angle(backpack.transform.localRotation, Quaternion.Euler(0f, 180f, 0f)), Is.LessThan(0.1f));
            GameObject ball = GameObject.Find("Ball");
            Assert.That(ball, Is.Not.Null);
            Mesh ballMesh = ball.GetComponentInChildren<MeshFilter>().sharedMesh;
            Assert.That(ballMesh.subMeshCount, Is.EqualTo(2));
            Assert.That(ballMesh.vertexCount, Is.GreaterThan(5000));
            Assert.That(ball.GetComponentInChildren<Renderer>().sharedMaterials[0].name, Is.EqualTo("Basketball Pebble Surface"));
            Assert.That(ball.GetComponentInChildren<Renderer>().sharedMaterials[0].GetFloat("_PebbleStrength"), Is.GreaterThan(0f));
            GameObject bottle = GameObject.Find("Bottle");
            Assert.That(bottle, Is.Not.Null);
            Assert.That(bottle.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount, Is.GreaterThan(1500));
            Assert.That(bottle.GetComponentInChildren<MeshFilter>().sharedMesh.subMeshCount, Is.EqualTo(3));
            Assert.That(ball.transform.localScale.x, Is.EqualTo(1.16f).Within(0.001f));

            string[] choiceNames = { "Backpack", "Ball", "Bottle" };
            float[] baseScales = { 0.72f, 1f, 0.78f };
            for (int highlighted = 0; highlighted < choiceNames.Length; highlighted++)
            {
                environment.ShowMemory(highlighted);
                yield return null;
                for (int index = 0; index < choiceNames.Length; index++)
                {
                    float expected = baseScales[index] * (index == highlighted ? 1.16f : 1f);
                    Assert.That(GameObject.Find(choiceNames[index]).transform.localScale.x,
                        Is.EqualTo(expected).Within(0.001f));
                }
                environment.HideMemoryCue();
                for (int index = 0; index < choiceNames.Length; index++)
                    Assert.That(GameObject.Find(choiceNames[index]).transform.localScale.x,
                        Is.EqualTo(baseScales[index]).Within(0.001f));
            }


            environment.Clear();
            Object.Destroy(cameraObject);
        }
    }
}
