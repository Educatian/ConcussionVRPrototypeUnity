using UnityEngine;

namespace ConcussionVR
{
    public enum PursuitDirection { Horizontal, Vertical }

    public sealed class TaskEnvironment
    {
        private readonly Camera _camera;
        private readonly InterventionSceneBuilder _highFidelity;
        private readonly Shader _shader;
        private GameObject _root;
        private GameObject _target;
        private Vector3[] _memoryBaseScales;
        private GameObject[] _memoryChoices;

        public TaskEnvironment(Camera camera)
        {
            _camera = camera;
            _highFidelity = new InterventionSceneBuilder(camera);
            _shader = Resources.Load<Shader>("PrototypeUnlit");
        }

        public GameObject ShowCalm()
        {
            Clear();
            _highFidelity.BuildCalm();
            _root = _highFidelity.Root;
            return _highFidelity.Guide;
        }

        public void ShowPursuit()
        {
            Clear();
            _highFidelity.BuildPursuit();
            _root = _highFidelity.Root;
            _target = _highFidelity.Target;
        }

        public float UpdatePursuit(float elapsed, PursuitDirection direction)
        {
            float wave = Mathf.Sin(elapsed * 0.72f);
            _target.transform.position = direction == PursuitDirection.Horizontal
                ? new Vector3(wave * 2.2f, 0f, 5.9f)
                : new Vector3(0f, wave * 1.35f, 5.9f);
            Vector3 targetDirection = (_target.transform.position - _camera.transform.position).normalized;
            return Vector3.Angle(_camera.transform.forward, targetDirection);
        }

        public void ShowMemory(int highlighted)
        {
            Clear();
            _highFidelity.BuildMemory(highlighted);
            _root = _highFidelity.Root;
            _memoryChoices = _highFidelity.Choices;
            _memoryBaseScales = new Vector3[_memoryChoices.Length];
            for (int index = 0; index < _memoryChoices.Length; index++)
            {
                _memoryBaseScales[index] = _memoryChoices[index].transform.localScale;
                if (index == highlighted) _memoryBaseScales[index] /= 1.16f;
            }
        }

        public void HideMemoryCue()
        {
            for (int index = 0; index < _memoryChoices.Length; index++)
                _memoryChoices[index].transform.localScale = _memoryBaseScales[index];
        }

        public void Clear()
        {
            if (_root != null) Object.Destroy(_root);
            _memoryBaseScales = null;
            _root = null;
            _target = null;
            _memoryChoices = null;
        }

        private void BuildClinicRoom()
        {
            _camera.backgroundColor = new Color(0.3f, 0.39f, 0.44f);
            Create(PrimitiveType.Cube, "Back Wall", new Vector3(0f, 0f, 7f), new Vector3(9f, 5f, 0.15f), new Color(0.31f, 0.4f, 0.45f));
            Create(PrimitiveType.Plane, "Floor", new Vector3(0f, -1.45f, 5f), new Vector3(1.1f, 1f, 1.1f), new Color(0.54f, 0.5f, 0.43f));
            Create(PrimitiveType.Cube, "Bench", new Vector3(3.2f, -0.9f, 5.7f), new Vector3(1.8f, 0.4f, 0.65f), new Color(0.38f, 0.43f, 0.44f));
        }

        private GameObject BuildBackpack(Vector3 position)
        {
            var group = NewGroup("Backpack", position);
            CreateChild(group, PrimitiveType.Cube, "Pack Body", Vector3.zero, new Vector3(0.8f, 1f, 0.42f), new Color(0.32f, 0.43f, 0.25f));
            CreateChild(group, PrimitiveType.Cube, "Front Pocket", new Vector3(0f, -0.2f, -0.28f), new Vector3(0.58f, 0.38f, 0.18f), new Color(0.25f, 0.35f, 0.2f));
            CreateChild(group, PrimitiveType.Cube, "Handle", new Vector3(0f, 0.62f, 0f), new Vector3(0.38f, 0.1f, 0.12f), new Color(0.2f, 0.29f, 0.17f));
            return group;
        }

        private GameObject BuildBall(Vector3 position)
        {
            var group = NewGroup("Ball", position);
            CreateChild(group, PrimitiveType.Sphere, "Ball Shape", Vector3.zero, Vector3.one * 0.82f, new Color(0.86f, 0.39f, 0.12f));
            return group;
        }

        private GameObject BuildBottle(Vector3 position)
        {
            var group = NewGroup("Bottle", position);
            CreateChild(group, PrimitiveType.Cylinder, "Bottle Body", Vector3.zero, new Vector3(0.34f, 0.72f, 0.34f), new Color(0.2f, 0.43f, 0.66f));
            CreateChild(group, PrimitiveType.Cylinder, "Bottle Cap", new Vector3(0f, 0.84f, 0f), new Vector3(0.2f, 0.13f, 0.2f), new Color(0.12f, 0.2f, 0.28f));
            return group;
        }

        private GameObject NewGroup(string name, Vector3 position)
        {
            var group = new GameObject(name);
            group.transform.SetParent(_root.transform);
            group.transform.position = position;
            return group;
        }

        private void Reset(string name)
        {
            Clear();
            _root = new GameObject(name);
        }

        private GameObject Create(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject item = MakePrimitive(type, name, position, scale, color);
            item.transform.SetParent(_root.transform, true);
            return item;
        }

        private void CreateChild(GameObject parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject item = MakePrimitive(type, name, localPosition, scale, color);
            item.transform.SetParent(parent.transform, false);
        }

        private GameObject MakePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            var material = new Material(_shader) { color = color };
            item.GetComponent<Renderer>().material = material;
            return item;
        }
    }
}
