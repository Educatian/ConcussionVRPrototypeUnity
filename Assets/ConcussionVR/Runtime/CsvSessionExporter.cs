using System.Globalization;
using System.IO;
using System.Text;

namespace ConcussionVR
{
    public static class CsvSessionExporter
    {
        public static string Export(SessionRecord record, string directory)
        {
            Directory.CreateDirectory(directory);
            string participant = Sanitize(record.participantId);
            string path = Path.Combine(directory, $"{participant}_{record.sessionId}.csv");
            var csv = new StringBuilder();
            csv.AppendLine("participant_id,session_id,started_utc,ended_utc,status,stop_reason,pre_total,after_calm_total,after_pursuit_total,post_total,max_symptom_increase,calm_seconds,pursuit_mean_error_degrees,pursuit_samples,memory_correct,memory_trials,median_correct_reaction_seconds");
            csv.Append(Escape(record.participantId)).Append(',')
                .Append(Escape(record.sessionId)).Append(',')
                .Append(Escape(record.startedUtc)).Append(',')
                .Append(Escape(record.endedUtc)).Append(',')
                .Append(Escape(record.status)).Append(',')
                .Append(Escape(record.stopReason)).Append(',')
                .Append(record.pre.Total).Append(',')
                .Append(record.afterCalm.Total).Append(',')
                .Append(record.afterPursuit.Total).Append(',')
                .Append(record.post.Total).Append(',')
                .Append(record.post.MaximumIncreaseFrom(record.pre)).Append(',')
                .Append(record.calmSeconds.ToString("F2", CultureInfo.InvariantCulture)).Append(',')
                .Append(record.pursuitMeanErrorDegrees.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                .Append(record.pursuitSamples).Append(',')
                .Append(record.memoryCorrect).Append(',')
                .Append(record.memoryTrials).Append(',')
                .Append(record.MedianReactionTime().ToString("F3", CultureInfo.InvariantCulture)).AppendLine();
            File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
            return path;
        }

        private static string Sanitize(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "anonymous" : value.Trim();
        }

        private static string Escape(string value)
        {
            char quote = (char)34;
            return quote + value.Replace(quote.ToString(), new string(quote, 2)) + quote;
        }
    }
}
