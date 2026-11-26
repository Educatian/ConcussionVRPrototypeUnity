using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace ConcussionVR.Tests
{
    public sealed class SessionRulesTests
    {
        [TestCase(5, 2, 2, 4)]
        [TestCase(2, 6, 3, 5)]
        [TestCase(1, 2, 7, 6)]
        [TestCase(4, 0, 0, 3)]
        public void Export_ReportsMaximumIncreaseAcrossAllCheckpoints(
            int afterCalm, int afterPursuit, int post, int expectedMaximum)
        {
            var record = new SessionRecord
            {
                pre = new Symptoms { headache = 1 },
                afterCalm = new Symptoms { headache = afterCalm },
                afterPursuit = new Symptoms { headache = afterPursuit },
                post = new Symptoms { headache = post }
            };
            string directory = Path.Combine(Path.GetTempPath(), "ConcussionVR-export-" + Guid.NewGuid().ToString("N"));
            try
            {
                string path = CsvSessionExporter.Export(record, directory);
                string[] values = File.ReadAllLines(path)[1].Split(',');
                Assert.That(int.Parse(values[10], CultureInfo.InvariantCulture), Is.EqualTo(expectedMaximum));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void CanProgress_AllowsIncreaseAtBoundary()
        {
            var baseline = new Symptoms { headache = 2 };
            var current = new Symptoms { headache = 4 };
            Assert.That(new SessionRules().CanProgress(baseline, current, 2), Is.True);
        }

        [Test]
        public void CanProgress_RejectsIncreaseAboveBoundary()
        {
            var baseline = new Symptoms { dizziness = 1 };
            var current = new Symptoms { dizziness = 4 };
            Assert.That(new SessionRules().CanProgress(baseline, current, 2), Is.False);
        }

        [Test]
        public void MedianReactionTime_ReturnsMiddleOfOrderedValues()
        {
            var record = new SessionRecord();
            record.correctReactionTimes.AddRange(new[] { 1.2f, 0.4f, 0.8f, 1.0f });
            Assert.That(record.MedianReactionTime(), Is.EqualTo(0.9f).Within(0.001f));
        }
    }
}
