using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ConcussionVR
{
    [DefaultExecutionOrder(-10000)]
    public sealed class PrototypeSessionController : MonoBehaviour
    {
        [SerializeField] private float calmDurationSeconds = 12f;
        [SerializeField] private float pursuitDurationSeconds = 15f;
        [SerializeField] private int memoryTrialCount = 6;

        private PrototypeView _view;
        private TaskEnvironment _environment;
        private SessionRecord _record;
        private Coroutine _activeTask;
        private bool _isInitialized;
        private bool _isFinished;

        private void Awake()
        {
            Camera camera = Camera.main;
            _view = new PrototypeView(camera);
            _environment = new TaskEnvironment(camera);
            _record = new SessionRecord();
            _isInitialized = true;
            ShowPreAssessment();
        }

        private void Update()
        {
            if (!_isInitialized || _isFinished) return;
            if (Input.GetKeyDown(KeyCode.Escape)) StopSession("participant_or_observer_stop");
        }

        private void ShowPreAssessment() =>
            _view.ShowAssessment("Pre-session symptom check", _record.pre, StartCalm);

        private void StartCalm()
        {
            GameObject guide = _environment.ShowCalm();
            Text status = _view.ShowTaskHud("Nature Calm", "Remain seated | follow the breathing guide | ESC stops the session");
            _activeTask = StartCoroutine(RunCalm(guide, status));
        }

        private IEnumerator RunCalm(GameObject guide, Text status)
        {
            float elapsed = 0f;
            while (elapsed < calmDurationSeconds)
            {
                elapsed += Time.deltaTime;
                float phase = (Mathf.Sin(elapsed * Mathf.PI * 0.25f) + 1f) * 0.5f;
                guide.transform.localScale = Vector3.one * Mathf.Lerp(0.42f, 0.88f, phase);
                status.text = $"{(phase > 0.5f ? "Breathe in" : "Breathe out")} | {Mathf.CeilToInt(calmDurationSeconds - elapsed)} sec | ESC: stop";
                yield return null;
            }
            _record.calmSeconds = elapsed;
            _environment.Clear();
            _view.ShowCheckpoint("After Nature Calm", _record.afterCalm, () => ShowRest(StartPursuit), StartPursuit);
        }

        private void StartPursuit()
        {
            _environment.ShowPursuit();
            Text status = _view.ShowTaskHud("Gaze Stabilization", "Keep the camera still | follow only the teal target");
            _activeTask = StartCoroutine(RunPursuit(status));
        }

        private IEnumerator RunPursuit(Text status)
        {
            float elapsed = 0f;
            float errorSum = 0f;
            int samples = 0;
            while (elapsed < pursuitDurationSeconds)
            {
                elapsed += Time.deltaTime;
                PursuitDirection direction = elapsed < pursuitDurationSeconds * 0.5f
                    ? PursuitDirection.Horizontal
                    : PursuitDirection.Vertical;
                errorSum += _environment.UpdatePursuit(elapsed, direction);
                samples++;
                status.text = $"{direction} pursuit | {Mathf.CeilToInt(pursuitDurationSeconds - elapsed)} sec | ESC: stop";
                yield return null;
            }
            _record.pursuitSamples = samples;
            _record.pursuitMeanErrorDegrees = samples == 0 ? 0f : errorSum / samples;
            _environment.Clear();
            _view.ShowCheckpoint("After Gaze Stabilization", _record.afterPursuit, () => ShowRest(StartMemory), StartMemory);
        }

        private void StartMemory() => _activeTask = StartCoroutine(RunMemoryTrials());

        private IEnumerator RunMemoryTrials()
        {
            for (int trial = 0; trial < memoryTrialCount; trial++)
            {
                int answer = UnityEngine.Random.Range(0, 3);
                _environment.ShowMemory(answer);
                Text status = _view.ShowTaskHud("Memory & Reaction", $"Remember the enlarged object | {trial + 1}/{memoryTrialCount}");
                yield return new WaitForSeconds(1.2f);
                _environment.HideMemoryCue();
                status.text = "Choose the remembered position: 1 | 2 | 3";
                float started = Time.time;
                int response = -1;
                while (response < 0)
                {
                    response = ReadNumberResponse();
                    yield return null;
                }
                _record.memoryTrials++;
                if (response == answer)
                {
                    _record.memoryCorrect++;
                    _record.correctReactionTimes.Add(Time.time - started);
                    status.text = "Correct | response recorded";
                }
                else status.text = "Response recorded";
                yield return new WaitForSeconds(0.6f);
            }
            _environment.Clear();
            _view.ShowAssessment("Post-session symptom check", _record.post, CompleteSession);
        }

        private void ShowRest(Action onReady) =>
            _view.ShowMessage("Rest", "Remove visual demand and remain seated.\nContinue only when ready and supervised.", "Continue", onReady);

        private static int ReadNumberResponse()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
            return -1;
        }

        private void CompleteSession()
        {
            _isFinished = true;
            _record.status = "completed";
            _record.endedUtc = DateTime.UtcNow.ToString("O");
            string path = Export();
            _view.ShowSummary(_record, path, Restart);
        }

        private void StopSession(string reason)
        {
            _isFinished = true;
            if (_activeTask != null) StopCoroutine(_activeTask);
            _environment.Clear();
            _record.status = "stopped";
            _record.stopReason = reason;
            _record.endedUtc = DateTime.UtcNow.ToString("O");
            string path = Export();
            _view.ShowMessage("Session safely stopped", $"Do not continue activity. Notify the supervising clinician.\n\nSaved to:\n{path}", "New session", Restart);
        }

        private string Export()
        {
            string directory = Path.Combine(Application.persistentDataPath, "ConcussionVR", "Sessions");
            return CsvSessionExporter.Export(_record, directory);
        }

        private void Restart()
        {
            _record = new SessionRecord();
            _isFinished = false;
            ShowPreAssessment();
        }
    }
}
