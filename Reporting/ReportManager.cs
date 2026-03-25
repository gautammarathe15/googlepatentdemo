using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GooglePatentsAutomation.Reporting.Generators;

namespace GooglePatentsAutomation.Reporting
{
    /// <summary>
    /// Manages report generation and distribution
    /// </summary>
    public class ReportManager
    {
        private readonly ExecutionStatistics _statistics;
        private readonly ReportConfiguration _configuration;
        private readonly Dictionary<string, ReportGenerator> _generators;

        public ReportManager(ExecutionStatistics statistics, ReportConfiguration configuration = null)
        {
            _statistics = statistics;
            _configuration = configuration ?? new ReportConfiguration();
            _generators = InitializeGenerators();
        }

        /// <summary>
        /// Initialize all report generators
        /// </summary>
        private Dictionary<string, ReportGenerator> InitializeGenerators()
        {
            return new Dictionary<string, ReportGenerator>
            {
                { "HTML", new HtmlReportGenerator(_statistics, _configuration) },
                { "JSON", new JsonReportGenerator(_statistics, _configuration) },
                { "CSV", new CsvReportGenerator(_statistics, _configuration) }
            };
        }

        /// <summary>
        /// Generate all configured reports
        /// </summary>
        public List<string> GenerateAllReports()
        {
            var generatedReports = new List<string>();

            foreach (var format in _configuration.ReportFormats)
            {
                generatedReports.Add(GenerateReport(format));
            }

            return generatedReports;
        }

        /// <summary>
        /// Generate specific format report
        /// </summary>
        public string GenerateReport(string format)
        {
            if (!_generators.ContainsKey(format.ToUpper()))
            {
                throw new ArgumentException($"Unknown report format: {format}");
            }

            var generator = _generators[format.ToUpper()];
            var reportContent = generator.GenerateReport();
            generator.SaveReport(reportContent, format);

            var reportPath = _configuration.GetReportPath(format);
            Console.WriteLine($"Report generated: {reportPath}");

            return reportPath;
        }

        /// <summary>
        /// Get report summary
        /// </summary>
        public ReportSummary GetReportSummary()
        {
            return new ReportSummary
            {
                ProjectName = _configuration.ProjectName,
                ExecutionDate = DateTime.Now,
                TotalScenarios = _statistics.TotalScenarios,
                PassedScenarios = _statistics.PassedScenarios,
                FailedScenarios = _statistics.FailedScenarios,
                SkippedScenarios = _statistics.SkippedScenarios,
                SuccessRate = _statistics.GetPassRate(),
                TotalDuration = _statistics.TotalExecutionTime,
                PeakMemory = _statistics.PeakMemoryUsage
            };
        }

        /// <summary>
        /// Create screenshots directory structure
        /// </summary>
        public void CreateScreenshotDirectories()
        {
            Directory.CreateDirectory(_configuration.GetPassedScreenshotsDirectory());
            Directory.CreateDirectory(_configuration.GetFailedScreenshotsDirectory());
        }

        /// <summary>
        /// Organize screenshots into pass/fail folders
        /// </summary>
        public void OrganizeScreenshots()
        {
            CreateScreenshotDirectories();

            foreach (var result in _statistics.ScenarioResults)
            {
                var targetDirectory = result.Status == "PASS"
                    ? _configuration.GetPassedScreenshotsDirectory()
                    : _configuration.GetFailedScreenshotsDirectory();

                foreach (var screenshot in result.Screenshots)
                {
                    if (File.Exists(screenshot))
                    {
                        var fileName = Path.GetFileName(screenshot);
                        var destinationPath = Path.Combine(targetDirectory, fileName);
                        File.Copy(screenshot, destinationPath, overwrite: true);
                    }
                }
            }
        }

        /// <summary>
        /// Generate email-friendly report
        /// </summary>
        public string GenerateEmailReport()
        {
            var htmlGenerator = new HtmlReportGenerator(_statistics, _configuration);
            var htmlContent = htmlGenerator.GenerateReport();

            // Inline CSS for email clients
            var emailContent = htmlContent.Replace(
                "<style>",
                "<style type=\"text/css\"><!--"
            ).Replace(
                "</style>",
                "--></style>"
            );

            return emailContent;
        }

        /// <summary>
        /// Generate comparison report between two executions
        /// </summary>
        public string GenerateComparisonReport(ExecutionStatistics previousStats)
        {
            var comparison = new StringBuilder();

            comparison.AppendLine("<h2>Test Execution Comparison</h2>");
            comparison.AppendLine("<table border=\"1\" cellpadding=\"5\">");
            comparison.AppendLine("<tr>");
            comparison.AppendLine("<th>Metric</th>");
            comparison.AppendLine("<th>Previous Run</th>");
            comparison.AppendLine("<th>Current Run</th>");
            comparison.AppendLine("<th>Difference</th>");
            comparison.AppendLine("</tr>");

            // Total Scenarios
            comparison.AppendLine("<tr>");
            comparison.AppendLine("<td>Total Scenarios</td>");
            comparison.AppendLine($"<td>{previousStats.TotalScenarios}</td>");
            comparison.AppendLine($"<td>{_statistics.TotalScenarios}</td>");
            var scenarioDiff = _statistics.TotalScenarios - previousStats.TotalScenarios;
            var scenarioColor = scenarioDiff >= 0 ? "green" : "red";
            var scenarioSign = scenarioDiff >= 0 ? "+" : "";
            comparison.AppendLine($"<td style=\"color: {scenarioColor}\">{scenarioSign}{scenarioDiff}</td>");
            comparison.AppendLine("</tr>");

            // Pass Rate
            var prevPassRate = previousStats.GetPassRate();
            var currPassRate = _statistics.GetPassRate();
            comparison.AppendLine("<tr>");
            comparison.AppendLine("<td>Pass Rate</td>");
            comparison.AppendLine($"<td>{prevPassRate:F2}%</td>");
            comparison.AppendLine($"<td>{currPassRate:F2}%</td>");
            var passRateDiff = currPassRate - prevPassRate;
            var passRateColor = passRateDiff >= 0 ? "green" : "red";
            var passRateSign = passRateDiff >= 0 ? "+" : "";
            comparison.AppendLine($"<td style=\"color: {passRateColor}\">{passRateSign}{passRateDiff:F2}%</td>");
            comparison.AppendLine("</tr>");

            // Execution Time
            comparison.AppendLine("<tr>");
            comparison.AppendLine("<td>Total Duration (seconds)</td>");
            comparison.AppendLine($"<td>{previousStats.TotalExecutionTime:F2}</td>");
            comparison.AppendLine($"<td>{_statistics.TotalExecutionTime:F2}</td>");
            var timeDiff = _statistics.TotalExecutionTime - previousStats.TotalExecutionTime;
            var timeColor = timeDiff <= 0 ? "green" : "red";
            var timeSign = timeDiff >= 0 ? "+" : "";
            comparison.AppendLine($"<td style=\"color: {timeColor}\">{timeSign}{timeDiff:F2}</td>");
            comparison.AppendLine("</tr>");

            // Memory Usage
            comparison.AppendLine("<tr>");
            comparison.AppendLine("<td>Peak Memory (MB)</td>");
            comparison.AppendLine($"<td>{previousStats.PeakMemoryUsage:F2}</td>");
            comparison.AppendLine($"<td>{_statistics.PeakMemoryUsage:F2}</td>");
            var memDiff = _statistics.PeakMemoryUsage - previousStats.PeakMemoryUsage;
            var memColor = memDiff <= 0 ? "green" : "red";
            var memSign = memDiff >= 0 ? "+" : "";
            comparison.AppendLine($"<td style=\"color: {memColor}\">{memSign}{memDiff:F2}</td>");
            comparison.AppendLine("</tr>");

            comparison.AppendLine("</table>");

            return comparison.ToString();
        }
    }

    /// <summary>
    /// Summary of report information
    /// </summary>
    public class ReportSummary
    {
        public string ProjectName { get; set; }
        public DateTime ExecutionDate { get; set; }
        public int TotalScenarios { get; set; }
        public int PassedScenarios { get; set; }
        public int FailedScenarios { get; set; }
        public int SkippedScenarios { get; set; }
        public double SuccessRate { get; set; }
        public double TotalDuration { get; set; }
        public double PeakMemory { get; set; }

        public override string ToString()
        {
            return $@"
Report Summary
==============
Project: {ProjectName}
Date: {ExecutionDate:yyyy-MM-dd HH:mm:ss}

Test Results:
  Total Scenarios: {TotalScenarios}
  Passed: {PassedScenarios}
  Failed: {FailedScenarios}
  Skipped: {SkippedScenarios}
  Success Rate: {SuccessRate:F2}%

Performance:
  Total Duration: {TotalDuration:F2} seconds
  Peak Memory: {PeakMemory:F2} MB
";
        }
    }
}
