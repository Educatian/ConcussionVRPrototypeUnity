using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConcussionVR
{
    public sealed class PrototypeView
    {
        private static readonly Color Ink = new(0.05f, 0.17f, 0.21f);
        private static readonly Color Paper = new(0.91f, 0.9f, 0.84f);
        private static readonly Color Teal = new(0.12f, 0.48f, 0.5f);
        private static readonly Color Orange = new(0.76f, 0.29f, 0.12f);

        private readonly Font _font;
        private readonly RectTransform _root;
        private readonly List<GameObject> _content = new();

        public PrototypeView(Camera camera)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Research UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            _root = canvasObject.GetComponent<RectTransform>();
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem));
        }

        public void Clear()
        {
            foreach (GameObject item in _content) UnityEngine.Object.Destroy(item);
            _content.Clear();
        }

        public void ShowMessage(string title, string body, string buttonLabel, Action onContinue)
        {
            Clear();
            CreatePanel(new Vector2(0.22f, 0.2f), new Vector2(0.78f, 0.8f));
            CreateText(title, 38, new Vector2(0.27f, 0.65f), new Vector2(0.73f, 0.77f), Ink);
            CreateText(body, 24, new Vector2(0.29f, 0.34f), new Vector2(0.71f, 0.63f), Ink);
            CreateButton(buttonLabel, onContinue, new Vector2(0.37f, 0.23f), new Vector2(0.63f, 0.33f), Teal);
        }

        public void ShowAssessment(string title, Symptoms symptoms, Action onContinue)
        {
            Clear();
            CreateAssessmentCard(title, symptoms);
            CreateButton("Continue", onContinue, new Vector2(0.38f, 0.055f), new Vector2(0.62f, 0.145f), Teal);
        }

        public void ShowCheckpoint(string title, Symptoms symptoms, Action onRest, Action onContinue)
        {
            Clear();
            CreateAssessmentCard(title, symptoms);
            CreateButton("Rest", onRest, new Vector2(0.24f, 0.055f), new Vector2(0.47f, 0.145f), new Color(0.48f, 0.62f, 0.59f));
            CreateButton("Continue", onContinue, new Vector2(0.53f, 0.055f), new Vector2(0.76f, 0.145f), Teal);
        }

        public Text ShowTaskHud(string title, string instruction)
        {
            Clear();
            CreateText(title, 28, new Vector2(0.04f, 0.91f), new Vector2(0.48f, 0.975f), Color.white, TextAnchor.MiddleLeft, true);
            return CreateText(instruction, 22, new Vector2(0.18f, 0.035f), new Vector2(0.82f, 0.105f), Color.white, TextAnchor.MiddleCenter, true);
        }

        public void ShowSummary(SessionRecord record, string path, Action onRestart)
        {
            Clear();
            CreatePanel(new Vector2(0.24f, 0.12f), new Vector2(0.76f, 0.88f));
            CreateText("Block Summary", 38, new Vector2(0.3f, 0.76f), new Vector2(0.7f, 0.85f), Ink);
            string body = $"Symptoms  {record.pre.Total} ? {record.post.Total}\n\nHead-gaze error  {record.pursuitMeanErrorDegrees:F1}?\n\nMemory accuracy  {record.memoryCorrect}/{record.memoryTrials}\n\nMedian reaction  {record.MedianReactionTime():F2}s\n\nSaved: {path}";
            CreateText(body, 24, new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.72f), Ink, TextAnchor.MiddleLeft);
            CreateButton("New session", onRestart, new Vector2(0.35f, 0.16f), new Vector2(0.65f, 0.26f), Teal);
        }

        private void CreateAssessmentCard(string title, Symptoms symptoms)
        {
            CreatePanel(new Vector2(0.13f, 0.025f), new Vector2(0.87f, 0.975f));
            CreateText(title, 34, new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.95f), Ink);
            CreateText("0  none     ?     10  severe", 20, new Vector2(0.25f, 0.81f), new Vector2(0.75f, 0.86f), new Color(0.31f, 0.4f, 0.42f));
            string[] labels = { "Headache", "Dizziness", "Nausea", "Eye strain", "Light sensitivity", "Concentration difficulty" };
            Func<int>[] reads = { () => symptoms.headache, () => symptoms.dizziness, () => symptoms.nausea, () => symptoms.eyeStrain, () => symptoms.lightSensitivity, () => symptoms.concentrationDifficulty };
            Action<int>[] writes = { v => symptoms.headache = v, v => symptoms.dizziness = v, v => symptoms.nausea = v, v => symptoms.eyeStrain = v, v => symptoms.lightSensitivity = v, v => symptoms.concentrationDifficulty = v };
            for (int index = 0; index < labels.Length; index++)
                CreateRow(labels[index], reads[index], writes[index], 0.78f - index * 0.1f);
        }

        private void CreateRow(string label, Func<int> read, Action<int> write, float top)
        {
            CreateText(label, 22, new Vector2(0.22f, top - 0.06f), new Vector2(0.45f, top), Ink, TextAnchor.MiddleLeft);
            Text value = CreateText(read().ToString(), 26, new Vector2(0.67f, top - 0.06f), new Vector2(0.73f, top), Ink);
            CreateButton("-", () => { write(Mathf.Max(0, read() - 1)); value.text = read().ToString(); }, new Vector2(0.55f, top - 0.06f), new Vector2(0.63f, top), Teal);
            CreateButton("+", () => { write(Mathf.Min(10, read() + 1)); value.text = read().ToString(); }, new Vector2(0.77f, top - 0.06f), new Vector2(0.85f, top), Teal);
        }

        private void CreatePanel(Vector2 min, Vector2 max)
        {
            GameObject panel = CreateObject("Card", _root, typeof(Image));
            panel.GetComponent<Image>().color = Paper;
            Stretch(panel.GetComponent<RectTransform>(), min, max);
        }

        private Text CreateText(string content, int size, Vector2 min, Vector2 max, Color color, TextAnchor alignment = TextAnchor.MiddleCenter, bool darkPlate = false)
        {
            GameObject item = CreateObject("Text", _root, typeof(Text));
            Text text = item.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.text = content;
            if (darkPlate) text.gameObject.AddComponent<Shadow>().effectColor = Ink;
            Stretch(text.rectTransform, min, max);
            return text;
        }

        private void CreateButton(string label, Action action, Vector2 min, Vector2 max, Color color)
        {
            GameObject item = CreateObject(label, _root, typeof(Image), typeof(Button));
            item.GetComponent<Image>().color = color;
            Button button = item.GetComponent<Button>();
            button.onClick.AddListener(() => action());
            item.AddComponent<DirectButtonInput>().Initialize(button, label is "Continue" or "New session");
            Stretch(item.GetComponent<RectTransform>(), min, max);
            Text text = CreateObject("Label", item.transform, typeof(Text)).GetComponent<Text>();
            text.font = _font;
            text.fontSize = 23;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            Stretch(text.rectTransform, Vector2.zero, Vector2.one);
        }

        private GameObject CreateObject(string name, Transform parent, params Type[] components)
        {
            var item = new GameObject(name, components);
            item.transform.SetParent(parent, false);
            _content.Add(item);
            return item;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
