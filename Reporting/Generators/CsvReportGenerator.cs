using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GooglePatentsAutomation.Reporting;

namespace GooglePatentsAutomation.Reporting.Generators
{
    /// <summary>
    /// Generates CSV reports for easy import into spreadsheet applications
    /// </summary>
    public class CsvReportGenerator : ReportGenerator
    {
        public CsvReportGenerator(ExecutionStatistics statistics, ReportConfiguration configuration)
            : base(statistics, configuration)
        {
        }

        /// <summary>
        /// Generate CSV report
        /// </summary>
        public override string GenerateReport()
        {
            var csv = new StringBuilder();

            // Add header comment
            csv.AppendLine($"# {GetReportTitle()}");
            csv.AppendLine($"# Generated: {GetTimestamp()}");
            csv.AppendLine($"# Project: {Configuration.ProjectName}");
            csv.AppendLine($"# Company: {Configuration.CompanyName}");
            csv.AppendLine();

            // Add summary section
            csv.Append(GetSummarySection());

            // Add execution statistics
            csv.Append(GetStatisticsSection());

            // Add scenario details
            csv.Append(GetScenariosSection());

            // Add failed scenarios
            csv.Append(GetFailedScenariosSection());

            // Add downloaded files summary
            csv.Append(GetDownloadedFilesSection());

            return csv.ToString();
        }

        private string GetSummarySection()
        {
            var summary = new StringBuilder();
            summary.AppendLine("## EXECUTION SUMMARY");
            summary.AppendLine("Metric,Value");
            summary.AppendLine($"Total Scenarios,{Statistics.TotalScenarios}");
            summary.AppendLine($"Passed Scenarios,{Statistics.PassedScenarios}");
            summary.AppendLine($"Failed Scenarios,{Statistics.FailedScenarios}");
            summary.AppendLine($"Skipped Scenarios,{Statistics.SkippedScenarios}");
            summary.AppendLine($"Success Rate,{Statistics.GetPassRate():F2}%");
            summary.AppendLine($"Total Duration,{Statistics.TotalExecutionTime:F2} seconds");
            summary.AppendLine($"Average Duration,{Statistics.GetAverageExecutionTime():F2} seconds");
            summary.AppendLine($"Peak Memory Usage,{Statistics.PeakMemoryUsage:F2} MB");
            summary.AppendLine();

            return summary.ToString();
        }

        private string GetStatisticsSection()
        {
            var stats = new StringBuilder();
            stats.AppendLine("## EXECUTION STATISTICS");
            stats.AppendLine("Property,Value");
            stats.AppendLine($"Execution Start Time,{Statistics.ExecutionStartTime:yyyy-MM-dd HH:mm:ss}");
            stats.AppendLine($"Execution End Time,{Statistics.ExecutionEndTime:yyyy-MM-dd HH:mm:ss}");
            stats.AppendLine($"Total Execution Time,{Statistics.TotalExecutionTime:F2} seconds");

            var slowest = Statistics.GetSlowestScenario();
            if (slowest != null)
            {
                stats.AppendLine($"Slowest Scenario,{slowest.ScenarioName}");
                stats.AppendLine($"Slowest Scenario Duration,{slowest.Duration:F2} seconds");
            }

            var fastest = Statistics.ScenarioResults.OrderBy(r => r.Duration).FirstOrDefault();
            if (fastest != null)
            {
                stats.AppendLine($"Fastest Scenario,{fastest.ScenarioName}");
                stats.AppendLine($"Fastest Scenario Duration,{fastest.Duration:F2} seconds");
            }

            stats.AppendLine();
            return stats.ToString();
        }

        private string GetScenariosSection()
        {
            var scenarios = new StringBuilder();
            scenarios.AppendLine("## SCENARIO DETAILS");
            scenarios.AppendLine("Scenario Name,Status,Duration (seconds),Browser,Start Time,End Time");

            foreach (var result in Statistics.ScenarioResults)
            {
                scenarios.AppendLine(FormatCsvLine(
                    result.ScenarioName,
                    result.Status,
                    result.Duration.ToString("F2"),
                    result.Browser,
                    result.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    result.EndTime.ToString("yyyy-MM-dd HH:mm:ss")
                ));
            }

            scenarios.AppendLine();
            return scenarios.ToString();
        }

        private string GetFailedScenariosSection()
        {
            var failed = Statistics.GetFailedScenarios();
            if (failed.Count == 0) return string.Empty;

            var failedSection = new StringBuilder();
            failedSection.AppendLine("## FAILED SCENARIOS");
            failedSection.AppendLine("Scenario Name,Duration (seconds),Error Message");

            foreach (var result in failed)
            {
                failedSection.AppendLine(FormatCsvLine(
                    result.ScenarioName,
                    result.Duration.ToString("F2"),
                    result.ErrorMessage
                ));
            }

            failedSection.AppendLine();
            return failedSection.ToString();
        }

        private string GetDownloadedFilesSection()
        {
            if (Statistics.DownloadedFilesCount.Count == 0) return string.Empty;

            var files = new StringBuilder();
            files.AppendLine("## DOWNLOADED FILES");
            files.AppendLine("File Type,Count");

            foreach (var kvp in Statistics.DownloadedFilesCount)
            {
                files.AppendLine($"{kvp.Key},{kvp.Value}");
            }

            files.AppendLine($"Total Files,{Statistics.DownloadedFilesCount.Values.Sum()}");
            files.AppendLine();

            return files.ToString();
        }

        /// <summary>
        /// Format CSV line with proper escaping
        /// </summary>
        private string FormatCsvLine(params string[] fields)
        {
            return string.Join(",", fields.Select(field =>
            {
                if (string.IsNullOrEmpty(field))
                    return string.Empty;

                // If field contains comma, quotes, or newlines, wrap in quotes and escape quotes
                if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                {
                    return "\"" + field.Replace("\"", "\"\"") + "\"";
                }

                return field;
            }));
        }
    }
}
