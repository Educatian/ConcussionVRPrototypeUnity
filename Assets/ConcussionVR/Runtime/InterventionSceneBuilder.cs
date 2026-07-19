using UnityEngine;

namespace ConcussionVR
{
    internal sealed class InterventionSceneBuilder
    {
        private static readonly Color Teal = new(0.10f, 0.88f, 0.79f, 0.94f);
        private readonly EnvironmentArtFactory _art;

        public InterventionSceneBuilder(Camera camera) => _art = new EnvironmentArtFactory(camera);

        public GameObject Root { get; private set; }
        public GameObject Guide { get; private set; }
        public GameObject Target { get; private set; }
        public GameObject[] Choices { get; private set; }

        public void BuildCalm()
        {
            Begin("Nature Calm");
            _art.ConfigureNature();
            _art.MeshObject(ProceduralMeshFactory.CreateLake("Lake Surface Mesh", 38f, 42f, 48, 42),
                "Lake", new Vector3(0f, -1.34f, 0.5f), Vector3.one, _art.Water());
            _art.MeshObject(ProceduralMeshFactory.CreateMountainRange("Mountain Range Mesh", 46f, 24f, 52, 24),
                "Mountain Range", Vector3.zero, Vector3.one,
                _art.Surface("Distant Forest", new Color(0.12f, 0.29f, 0.23f), 0.12f));
            _art.Primitive(PrimitiveType.Cube, "Stable Horizon", new Vector3(0f, 0.12f, 14f),
                new Vector3(44f, 0.035f, 0.06f), "Horizon", new Color(0.18f, 0.36f, 0.31f));

            for (int plank = -5; plank <= 5; plank++)
            {
                Color wood = plank % 2 == 0 ? new Color(0.42f, 0.27f, 0.16f) : new Color(0.50f, 0.33f, 0.19f);
                _art.Primitive(PrimitiveType.Cube, $"Deck Plank {plank}", new Vector3(plank * 0.68f, -1.34f, 3.55f),
                    new Vector3(0.64f, 0.12f, 3.7f), $"Deck Wood {plank % 2}", wood, 0.32f);
            }
            _art.Primitive(PrimitiveType.Cube, "Deck Front Fascia", new Vector3(0f, -1.48f, 1.7f),
                new Vector3(7.6f, 0.28f, 0.18f), "Dark Wood", new Color(0.25f, 0.15f, 0.09f), 0.25f);

            for (int index = 0; index < 8; index++)
            {
                float side = index % 2 == 0 ? -1f : 1f;
                Vector3 pinePosition = new Vector3(side * (5.2f + (index % 3) * 1.25f), -1.02f, 5.5f + index * 1.4f);
                _art.Primitive(PrimitiveType.Sphere, $"Pine Shore {index}",
                    new Vector3(pinePosition.x, -1.22f, pinePosition.z), new Vector3(1.45f, 0.34f, 1.05f),
                    "Pine Shore Moss", new Color(0.13f, 0.24f, 0.17f), 0.16f);
                BuildPine($"Pine {index}", pinePosition,
                    2.5f + (index % 4) * 0.45f);
            }
            for (int index = 0; index < 10; index++)
            {
                float side = index % 2 == 0 ? -1f : 1f;
                _art.Primitive(PrimitiveType.Sphere, $"Shore Rock {index}",
                    new Vector3(side * (4.6f + index * 0.36f), -1.05f, 7f + index * 0.8f),
                    new Vector3(0.7f + index % 3 * 0.18f, 0.42f, 0.55f),
                    "Shore Stone", new Color(0.34f, 0.38f, 0.35f), 0.2f);
            }

            GameObject sun = _art.Primitive(PrimitiveType.Sphere, "Soft Morning Sun", new Vector3(-7f, 7f, 34f),
                Vector3.one * 1.35f, "Sun Glow", new Color(1f, 0.79f, 0.48f, 0.85f));
            _art.AssignGlow(sun, "Sun Glow", new Color(1f, 0.78f, 0.46f, 0.84f), new Color(0.5f, 0.27f, 0.08f));

            Guide = _art.Primitive(PrimitiveType.Sphere, "Breathing Guide", new Vector3(0f, 0.05f, 4.4f),
                Vector3.one * 0.48f, "Breathing Guide Glow", Teal);
            _art.AssignGlow(Guide, "Breathing Guide Glow", Teal, new Color(0.04f, 0.58f, 0.52f));
            _art.MeshObject(ProceduralMeshFactory.CreateTorus("Breathing Halo Mesh", 48, 12, 0.62f, 0.025f),
                "Breathing Halo", new Vector3(0f, 0.05f, 4.4f), Vector3.one,
                _art.Glow("Breathing Halo Glow", new Color(0.18f, 0.9f, 0.82f, 0.5f), new Color(0.02f, 0.35f, 0.31f)));
        }

        public void BuildPursuit()
        {
            Begin("Gaze Stabilization");
            BuildClinic();
            _art.Primitive(PrimitiveType.Cube, "Fixation Horizontal", new Vector3(0f, 0f, 6.12f),
                new Vector3(0.56f, 0.045f, 0.035f), "Fixation White", new Color(0.92f, 0.96f, 0.95f), 0.55f);
            _art.Primitive(PrimitiveType.Cube, "Fixation Vertical", new Vector3(0f, 0f, 6.12f),
                new Vector3(0.045f, 0.56f, 0.035f), "Fixation White", new Color(0.92f, 0.96f, 0.95f), 0.55f);
            for (int index = -5; index <= 5; index++)
            {
                GameObject dot = _art.Primitive(PrimitiveType.Sphere, $"Path Dot {index}",
                    new Vector3(index * 0.42f, 0f, 6.02f), Vector3.one * 0.045f,
                    "Path Dot", new Color(0.38f, 0.56f, 0.58f));
                _art.AssignGlow(dot, "Path Dot Glow", new Color(0.32f, 0.62f, 0.62f, 0.65f), new Color(0.03f, 0.12f, 0.12f));
            }

            Target = _art.Group("Pursuit Target", new Vector3(-2.2f, 0f, 5.86f));
            GameObject core = _art.Primitive(PrimitiveType.Sphere, "Target Core", Vector3.zero, Vector3.one * 0.28f,
                "Target Glow", Teal, parent: Target.transform);
            _art.AssignGlow(core, "Target Glow", Teal, new Color(0.04f, 0.68f, 0.59f));
            _art.MeshObject(ProceduralMeshFactory.CreateTorus("Target Ring Mesh", 40, 10, 0.42f, 0.035f),
                "Target Ring", Vector3.zero, Vector3.one,
                _art.Glow("Target Ring Glow", new Color(0.24f, 0.94f, 0.87f, 0.8f), new Color(0.05f, 0.45f, 0.4f)),
                Target.transform);
        }

        public void BuildMemory(int highlighted)
        {
            Begin("Memory and Reaction");
            BuildClinic();
            for (int index = -1; index <= 1; index++)
                _art.Primitive(PrimitiveType.Cylinder, $"Object Pedestal {index + 2}",
                    new Vector3(index * 1.75f, -1.05f, 5.58f), new Vector3(0.68f, 0.22f, 0.68f),
                    "Pedestal", new Color(0.70f, 0.66f, 0.56f), 0.55f);

            Choices = new[]
            {
                Backpack(new Vector3(-1.75f, -0.27f, 5.55f)),
                Ball(new Vector3(0f, -0.25f, 5.55f)),
                Bottle(new Vector3(1.75f, -0.36f, 5.55f))
            };
            Choices[highlighted].transform.localScale *= 1.16f;
        }

        private void Begin(string name)
        {
            Root = new GameObject(name);
            _art.SetRoot(Root);
        }

        private void BuildPine(string name, Vector3 position, float height)
        {
            _art.Model("Pine", name, position, Vector3.one * (height / 3.26f));
        }

        private void BuildClinic()
        {
            _art.ConfigureClinic();
            _art.Primitive(PrimitiveType.Cube, "Floor", new Vector3(0f, -1.52f, 4.5f),
                new Vector3(11f, 0.18f, 10f), "Clinic Floor", new Color(0.45f, 0.39f, 0.31f), 0.42f);
            _art.Primitive(PrimitiveType.Cube, "Back Wall", new Vector3(0f, 0.75f, 7.25f),
                new Vector3(11f, 4.6f, 0.16f), "Warm Wall", new Color(0.53f, 0.60f, 0.59f), 0.22f);
            _art.Primitive(PrimitiveType.Cube, "Left Wall", new Vector3(-5.35f, 0.7f, 4f),
                new Vector3(0.16f, 4.6f, 7f), "Side Wall", new Color(0.34f, 0.43f, 0.44f), 0.18f);
            _art.Primitive(PrimitiveType.Cube, "Right Wall", new Vector3(5.35f, 0.7f, 4f),
                new Vector3(0.16f, 4.6f, 7f), "Side Wall", new Color(0.34f, 0.43f, 0.44f), 0.18f);

            for (int panel = -2; panel <= 2; panel++)
            {
                _art.Primitive(PrimitiveType.Cube, $"Acoustic Panel {panel}", new Vector3(panel * 1.45f, 1.45f, 7.08f),
                    new Vector3(1.14f, 1.55f, 0.08f), "Acoustic Teal", new Color(0.19f, 0.38f, 0.39f), 0.28f);
                _art.Primitive(PrimitiveType.Cube, $"Panel Trim {panel}", new Vector3(panel * 1.45f, 0.63f, 6.98f),
                    new Vector3(1.28f, 0.06f, 0.09f), "Panel Trim", new Color(0.73f, 0.67f, 0.55f), 0.38f);
            }
            for (int light = -1; light <= 1; light += 2)
                _art.Primitive(PrimitiveType.Cube, $"Ceiling Light {light}", new Vector3(light * 2.5f, 2.85f, 4.4f),
                    new Vector3(1.7f, 0.06f, 0.6f), "Ceiling Light", new Color(0.83f, 0.91f, 0.91f), 0.65f, 0f,
                    new Color(0.18f, 0.22f, 0.2f));

            _art.Primitive(PrimitiveType.Cube, "Bench Seat", new Vector3(3.85f, -0.78f, 5.85f),
                new Vector3(2.1f, 0.28f, 0.75f), "Bench Fabric", new Color(0.24f, 0.34f, 0.34f), 0.18f);
            _art.Primitive(PrimitiveType.Cube, "Bench Back", new Vector3(3.85f, -0.18f, 6.15f),
                new Vector3(2.1f, 0.95f, 0.22f), "Bench Fabric", new Color(0.24f, 0.34f, 0.34f), 0.18f);
            _art.Primitive(PrimitiveType.Cylinder, "Planter", new Vector3(-4.25f, -0.75f, 6.1f),
                new Vector3(0.46f, 0.72f, 0.46f), "Planter Ceramic", new Color(0.66f, 0.58f, 0.47f), 0.58f);
            _art.Primitive(PrimitiveType.Cube, "Clinic Rug", new Vector3(0f, -1.40f, 3.75f),
                new Vector3(5.6f, 0.025f, 3.2f), "Clinic Rug", new Color(0.18f, 0.34f, 0.35f), 0.14f);
            _art.Primitive(PrimitiveType.Cube, "Back Wall Baseboard", new Vector3(0f, -1.28f, 7.10f),
                new Vector3(10.8f, 0.22f, 0.12f), "Baseboard", new Color(0.68f, 0.61f, 0.49f), 0.42f);
            _art.Primitive(PrimitiveType.Cube, "Left Console", new Vector3(-4.15f, -0.62f, 5.9f),
                new Vector3(1.35f, 0.12f, 0.55f), "Console Wood", new Color(0.39f, 0.25f, 0.15f), 0.38f);
            for (int leg = -1; leg <= 1; leg += 2)
                _art.Primitive(PrimitiveType.Cylinder, $"Console Leg {leg}", new Vector3(-4.15f + leg * 0.48f, -1.02f, 5.9f),
                    new Vector3(0.06f, 0.42f, 0.06f), "Console Metal", new Color(0.10f, 0.13f, 0.14f), 0.68f, 0.65f);
            for (int leaf = 0; leaf < 6; leaf++)
            {
                float angle = leaf * Mathf.PI * 2f / 6f;
                _art.Primitive(PrimitiveType.Capsule, $"Plant Leaf {leaf}",
                    new Vector3(-4.25f + Mathf.Cos(angle) * 0.24f, 0.12f + leaf % 2 * 0.16f, 6.1f + Mathf.Sin(angle) * 0.24f),
                    new Vector3(0.14f, 0.55f, 0.14f), "Plant Green", new Color(0.12f, 0.34f, 0.20f), 0.16f,
                    rotation: Quaternion.Euler(Mathf.Sin(angle) * 20f, 0f, Mathf.Cos(angle) * 26f));
            }
        }

        private GameObject Backpack(Vector3 position)
        {
            GameObject backpack = _art.Model("Backpack", "Backpack", position, Vector3.one * 0.72f);
            backpack.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return backpack;
        }

        private GameObject Ball(Vector3 position)
        {
            GameObject ball = _art.Model("Ball", "Ball", position, Vector3.one);
            _art.AssignBasketballSurface(ball);
            return ball;
        }

        private GameObject Bottle(Vector3 position)
        {
            return _art.Model("Bottle", "Bottle", position, Vector3.one * 0.78f);
        }
    }
}
