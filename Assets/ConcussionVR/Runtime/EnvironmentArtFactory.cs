using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConcussionVR
{
    internal sealed class EnvironmentArtFactory
    {
        private readonly Camera _camera;
        private readonly Shader _surfaceShader;
        private readonly Shader _waterShader;
        private readonly Shader _glowShader;
        private readonly Shader _skyShader;
        private readonly Dictionary<string, Material> _materials = new();
        private Transform _root;

        public EnvironmentArtFactory(Camera camera)
        {
            _camera = camera;
            _surfaceShader = Resources.Load<Shader>("ClinicalSurface");
            _waterShader = Resources.Load<Shader>("CalmWater");
            _glowShader = Resources.Load<Shader>("GuideGlow");
            _skyShader = Resources.Load<Shader>("CalmSky");
        }

        public void SetRoot(GameObject root) => _root = root.transform;

        public void ConfigureNature()
        {
            ConfigureQuality();
            AddSkyDome();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.43f, 0.66f, 0.75f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.68f, 0.79f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.52f, 0.49f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.21f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.48f, 0.66f, 0.68f);
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 58f;
            AddDirectionalLight("Warm Morning Key", new Color(1f, 0.88f, 0.72f), 1.05f, Quaternion.Euler(36f, -28f, 0f), 0.42f);
            AddDirectionalLight("Cool Sky Fill", new Color(0.58f, 0.74f, 0.82f), 0.24f, Quaternion.Euler(52f, 148f, 0f), 0f, false);
        }

        public void ConfigureClinic()
        {
            ConfigureQuality();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.12f, 0.16f, 0.18f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.63f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.25f, 0.30f, 0.31f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.14f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.24f, 0.31f, 0.32f);
            RenderSettings.fogStartDistance = 14f;
            RenderSettings.fogEndDistance = 30f;
            AddDirectionalLight("Window Key", new Color(0.82f, 0.94f, 1f), 0.62f, Quaternion.Euler(38f, 28f, 0f), 0.16f);
            AddPointLight("Ceiling Fill Left", new Vector3(-2.7f, 2.5f, 4.3f), new Color(0.78f, 0.94f, 1f), 2.2f, 7f);
            AddPointLight("Ceiling Fill Right", new Vector3(2.7f, 2.5f, 4.3f), new Color(1f, 0.89f, 0.74f), 1.8f, 7f);
        }

        public GameObject Group(string name, Vector3 position)
        {
            var group = new GameObject(name);
            group.transform.SetParent(_root, false);
            group.transform.localPosition = position;
            return group;
        }

        public GameObject Primitive(
            PrimitiveType type, string name, Vector3 position, Vector3 scale, string materialToken,
            Color color, float smoothness = 0.3f, float metallic = 0f, Color? emission = null,
            Transform parent = null, Quaternion? rotation = null)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = name;
            item.transform.SetParent(parent == null ? _root : parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.transform.localRotation = rotation ?? Quaternion.identity;
            Collider collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);
            }
            item.GetComponent<Renderer>().sharedMaterial = Surface(materialToken, color, smoothness, metallic, emission ?? Color.black);
            return item;
        }

        public GameObject MeshObject(Mesh mesh, string name, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(parent == null ? _root : parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            return item;
        }

        public GameObject Model(string resourceName, string name, Vector3 position, Vector3 scale, Transform parent = null)
        {
            GameObject source = Resources.Load<GameObject>($"Blender/{resourceName}");
            if (source == null) return null;
            GameObject item = Object.Instantiate(source, parent == null ? _root : parent);
            item.name = name;
            item.transform.localPosition = position;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = scale;
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                if (Application.isPlaying) Object.Destroy(collider);
                else Object.DestroyImmediate(collider);
            }
            return item;
        }

        public void AssignBasketballSurface(GameObject item)
        {
            if (item == null) return;
            const string key = "Surface/Basketball";
            if (!_materials.TryGetValue(key, out Material basketball))
            {
                basketball = new Material(_surfaceShader) { name = "Basketball Pebble Surface" };
                basketball.SetColor("_Color", new Color(0.82f, 0.16f, 0.025f));
                basketball.SetFloat("_Smoothness", 0.38f);
                basketball.SetFloat("_PebbleDensity", 52f);
                basketball.SetFloat("_PebbleStrength", 0.12f);
                _materials[key] = basketball;
            }
            Renderer renderer = item.GetComponentInChildren<Renderer>();
            if (renderer == null || renderer.sharedMaterials.Length == 0) return;
            Material[] materials = renderer.sharedMaterials;
            materials[0] = basketball;
            renderer.sharedMaterials = materials;
        }

        public Material Surface(string token, Color color, float smoothness = 0.3f, float metallic = 0f, Color? emission = null)
        {
            string key = $"Surface/{token}";
            if (_materials.TryGetValue(key, out Material existing)) return existing;
            var material = new Material(_surfaceShader) { name = token };
            material.SetColor("_Color", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetColor("_EmissionColor", emission ?? Color.black);
            _materials[key] = material;
            return material;
        }

        public Material Water()
        {
            const string token = "Calm Water";
            if (_materials.TryGetValue("Water/Calm", out Material existing)) return existing;
            var material = new Material(_waterShader) { name = token };
            material.SetColor("_DeepColor", new Color(0.025f, 0.19f, 0.26f));
            material.SetColor("_ShallowColor", new Color(0.12f, 0.48f, 0.54f));
            material.SetFloat("_Smoothness", 0.9f);
            material.SetFloat("_WaveAmount", 0.006f);
            material.SetFloat("_NormalStrength", 1.4f);
            _materials["Water/Calm"] = material;
            return material;
        }

        public Material Glow(string token, Color color, Color emission)
        {
            string key = $"Glow/{token}";
            if (_materials.TryGetValue(key, out Material existing)) return existing;
            var material = new Material(_glowShader) { name = token };
            material.SetColor("_Color", color);
            material.SetColor("_EmissionColor", emission);
            _materials[key] = material;
            return material;
        }

        public void AssignGlow(GameObject item, string token, Color color, Color emission) =>
            item.GetComponent<Renderer>().sharedMaterial = Glow(token, color, emission);

        private void AddSkyDome()
        {
            GameObject sky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sky.name = "Calm Sky Dome";
            sky.transform.SetParent(_root, false);
            sky.transform.localPosition = _camera.transform.position;
            sky.transform.localScale = Vector3.one * 60f;
            Collider collider = sky.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider);
            else Object.DestroyImmediate(collider);
            if (!_materials.TryGetValue("Sky/Calm", out Material material))
            {
                material = new Material(_skyShader) { name = "Calm Dawn Sky" };
                material.SetColor("_HorizonColor", new Color(0.48f, 0.72f, 0.76f));
                material.SetColor("_ZenithColor", new Color(0.19f, 0.48f, 0.65f));
                material.SetColor("_DawnColor", new Color(1f, 0.76f, 0.48f));
                _materials["Sky/Calm"] = material;
            }
            Renderer renderer = sky.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        private void AddDirectionalLight(string name, Color color, float intensity, Quaternion rotation, float shadowStrength, bool castShadows = true)
        {
            var lightObject = new GameObject(name, typeof(Light));
            lightObject.transform.SetParent(_root, false);
            lightObject.transform.localRotation = rotation;
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = shadowStrength;
        }

        private void AddPointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name, typeof(Light));
            lightObject.transform.SetParent(_root, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private void ConfigureQuality()
        {
            _camera.allowHDR = true;
            _camera.allowMSAA = true;
            _camera.fieldOfView = 62f;
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 80f;
            QualitySettings.antiAliasing = 4;
            QualitySettings.pixelLightCount = 6;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = 48f;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
    }
}
