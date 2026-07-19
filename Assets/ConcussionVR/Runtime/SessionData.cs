using System;
using System.Collections.Generic;

namespace ConcussionVR
{
    [Serializable]
    public sealed class Symptoms
    {
        public int headache;
        public int dizziness;
        public int nausea;
        public int eyeStrain;
        public int lightSensitivity;
        public int concentrationDifficulty;
        public int Total => headache + dizziness + nausea + eyeStrain + lightSensitivity + concentrationDifficulty;

        public int MaximumIncreaseFrom(Symptoms baseline)
        {
            int maximum = headache - baseline.headache;
            maximum = Math.Max(maximum, dizziness - baseline.dizziness);
            maximum = Math.Max(maximum, nausea - baseline.nausea);
            maximum = Math.Max(maximum, eyeStrain - baseline.eyeStrain);
            maximum = Math.Max(maximum, lightSensitivity - baseline.lightSensitivity);
            return Math.Max(maximum, concentrationDifficulty - baseline.concentrationDifficulty);
        }
    }

    [Serializable]
    public sealed class SessionRecord
    {
        public string participantId = "P001";
        public string sessionId = Guid.NewGuid().ToString("N");
        public string startedUtc = DateTime.UtcNow.ToString("O");
        public string endedUtc = string.Empty;
        public string status = "in_progress";
        public string stopReason = string.Empty;
        public Symptoms pre = new();
        public Symptoms afterCalm = new();
        public Symptoms afterPursuit = new();
        public Symptoms post = new();
        public float calmSeconds;
        public float pursuitMeanErrorDegrees;
        public int pursuitSamples;
        public int memoryCorrect;
        public int memoryTrials;
        public List<float> correctReactionTimes = new();

        public float MedianReactionTime()
        {
            if (correctReactionTimes.Count == 0) return 0f;
            var sorted = new List<float>(correctReactionTimes);
            sorted.Sort();
            int middle = sorted.Count / 2;
            return sorted.Count % 2 == 0 ? (sorted[middle - 1] + sorted[middle]) * 0.5f : sorted[middle];
        }
    }

    public sealed class SessionRules
    {
        public bool CanProgress(Symptoms baseline, Symptoms current, int allowedIncrease)
        {
            if (baseline == null) throw new ArgumentNullException(nameof(baseline));
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (allowedIncrease < 0) throw new ArgumentOutOfRangeException(nameof(allowedIncrease));
            return current.MaximumIncreaseFrom(baseline) <= allowedIncrease;
        }
    }
}
