using NUnit.Framework;

namespace ConcussionVR.Tests
{
    public sealed class SessionRulesTests
    {
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
