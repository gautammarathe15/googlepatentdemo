using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GooglePatentsAutomation.Reporting
{
    /// <summary>
    /// Represents test execution result data
    /// </summary>
    public class TestExecutionResult
    {
        public string ScenarioName { get; set; }
        public string Status { get; set; } // PASS, FAIL, SKIP
        public double Duration { get; set; } // in seconds
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ErrorMessage { get; set; }
        public string Browser { get; set; }
        public List<string> Screenshots { get; set; } = new List<string>();
        public List<string> DownloadedFiles { get; set; } = new List<string>();
        public Dictionary<string, object> CustomMetrics { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Manages test execution statistics
    /// </summary>
    public class ExecutionStatistics
    {
        public int TotalScenarios { get; set; }
        public int PassedScenarios { get; set; }
        public int FailedScenarios { get; set; }
        public int SkippedScenarios { get; set; }
        public double TotalExecutionTime { get; set; } // in seconds
        public double PeakMemoryUsage { get; set; } // in MB
        public DateTime ExecutionStartTime { get; set; }
        public DateTime ExecutionEndTime { get; set; }
        public List<TestExecutionResult> ScenarioResults { get; set; } = new List<TestExecutionResult>();
        public Dictionary<string, int> DownloadedFilesCount { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Calculate pass rate percentage
        /// </summary>
        public double GetPassRate()
        {
            if (TotalScenarios == 0) return 0;
            return (double)PassedScenarios / TotalScenarios * 100;
        }

        /// <summary>
        /// Calculate average execution time
        /// </summary>
        public double GetAverageExecutionTime()
        {
            if (ScenarioResults.Count == 0) return 0;
            return ScenarioResults.Average(r => r.Duration);
        }

        /// <summary>
        /// Get slowest scenario
        /// </summary>
        public TestExecutionResult GetSlowestScenario()
        {
            return ScenarioResults.OrderByDescending(r => r.Duration).FirstOrDefault();
        }

        /// <summary>
        /// Get failed scenarios
        /// </summary>
        public List<TestExecutionResult> GetFailedScenarios()
        {
            return ScenarioResults.Where(r => r.Status == "FAIL").ToList();
        }

        /// <summary>
        /// Get flaky tests (inconsistent results across runs)
        /// </summary>
        public List<string> IdentifyFlakyTests(List<ExecutionStatistics> previousRuns)
        {
            var flakyTests = new List<string>();

            foreach (var result in ScenarioResults)
            {
                var previousStatuses = previousRuns
                    .SelectMany(s => s.ScenarioResults)
                    .Where(r => r.ScenarioName == result.ScenarioName)
                    .Select(r => r.Status)
                    .ToList();

                if (previousStatuses.Any() && previousStatuses.Distinct().Count() > 1)
                {
                    flakyTests.Add(result.ScenarioName);
                }
            }

            return flakyTests;
        }
    }

    /// <summary>
    /// Report generation configuration
    /// </summary>
    public class ReportConfiguration
    {
        public string ReportDirectory { get; set; } = "reports";
        public string ReportName { get; set; } = $"report_{DateTime.Now:yyyyMMdd_HHmmss}";
        public bool IncludeScreenshots { get; set; } = true;
        public bool IncludeMetrics { get; set; } = true;
        public bool IncludeTrends { get; set; } = true;
        public string CompanyName { get; set; } = "QA Team";
        public string ProjectName { get; set; } = "Google Patents Automation";
        public List<string> ReportFormats { get; set; } = new List<string> { "HTML", "JSON", "CSV" };

        /// <summary>
        /// Get report file path for format
        /// </summary>
        public string GetReportPath(string format)
        {
            var extension = format.ToLower() switch
            {
                "html" => ".html",
                "json" => ".json",
                "csv" => ".csv",
                _ => ".txt"
            };
            return Path.Combine(ReportDirectory, $"{ReportName}{extension}");
        }

        /// <summary>
        /// Get screenshots directory
        /// </summary>
        public string GetScreenshotsDirectory()
        {
            return Path.Combine(ReportDirectory, ReportName, "screenshots");
        }

        /// <summary>
        /// Get passed screenshots directory
        /// </summary>
        public string GetPassedScreenshotsDirectory()
        {
            return Path.Combine(GetScreenshotsDirectory(), "passed");
        }

        /// <summary>
        /// Get failed screenshots directory
        /// </summary>
        public string GetFailedScreenshotsDirectory()
        {
            return Path.Combine(GetScreenshotsDirectory(), "failed");
        }
    }

    /// <summary>
    /// Base report generator
    /// </summary>
    public abstract class ReportGenerator
    {
        protected ExecutionStatistics Statistics { get; set; }
        protected ReportConfiguration Configuration { get; set; }

        public ReportGenerator(ExecutionStatistics statistics, ReportConfiguration configuration)
        {
            Statistics = statistics;
            Configuration = configuration;
        }

        /// <summary>
        /// Generate report
        /// </summary>
        public abstract string GenerateReport();

        /// <summary>
        /// Save report to file
        /// </summary>
        public virtual void SaveReport(string reportContent, string format)
        {
            Directory.CreateDirectory(Configuration.ReportDirectory);
            var filePath = Configuration.GetReportPath(format);
            File.WriteAllText(filePath, reportContent);
        }

        /// <summary>
        /// Get report title
        /// </summary>
        protected string GetReportTitle()
        {
            return $"Test Execution Report - {Configuration.ProjectName}";
        }

        /// <summary>
        /// Get timestamp
        /// </summary>
        protected string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
