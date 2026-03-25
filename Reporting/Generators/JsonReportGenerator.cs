using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GooglePatentsAutomation.Reporting;

namespace GooglePatentsAutomation.Reporting.Generators
{
    /// <summary>
    /// Generates JSON reports with detailed metrics and structured data
    /// </summary>
    public class JsonReportGenerator : ReportGenerator
    {
        public JsonReportGenerator(ExecutionStatistics statistics, ReportConfiguration configuration)
            : base(statistics, configuration)
        {
        }

        /// <summary>
        /// Generate JSON report
        /// </summary>
        public override string GenerateReport()
        {
            var reportData = new
            {
                metadata = new
                {
                    title = GetReportTitle(),
                    timestamp = GetTimestamp(),
                    projectName = Configuration.ProjectName,
                    companyName = Configuration.CompanyName,
                    generatedBy = "GooglePatentsAutomation"
                },
                summary = new
                {
                    totalScenarios = Statistics.TotalScenarios,
                    passedScenarios = Statistics.PassedScenarios,
                    failedScenarios = Statistics.FailedScenarios,
                    skippedScenarios = Statistics.SkippedScenarios,
                    successRate = Statistics.GetPassRate(),
                    totalExecutionTime = Statistics.TotalExecutionTime,
                    averageExecutionTime = Statistics.GetAverageExecutionTime(),
                    peakMemoryUsage = Statistics.PeakMemoryUsage
                },
                executionTiming = new
                {
                    startTime = Statistics.ExecutionStartTime.ToString("o"),
                    endTime = Statistics.ExecutionEndTime.ToString("o"),
                    totalDurationSeconds = Statistics.TotalExecutionTime
                },
                scenarios = Statistics.ScenarioResults.Select(r => new
                {
                    name = r.ScenarioName,
                    status = r.Status,
                    duration = r.Duration,
                    browser = r.Browser,
                    startTime = r.StartTime.ToString("o"),
                    endTime = r.EndTime.ToString("o"),
                    errorMessage = r.ErrorMessage,
                    screenshots = r.Screenshots,
                    downloadedFiles = r.DownloadedFiles,
                    customMetrics = r.CustomMetrics
                }),
                failedScenarios = GetFailedScenariosData(),
                downloadedFiles = GetDownloadedFilesData(),
                metrics = new
                {
                    slowestScenario = GetSlowestScenarioData(),
                    fastestScenario = GetFastestScenarioData(),
                    averageDuration = Statistics.GetAverageExecutionTime()
                },
                performanceData = GetPerformanceData()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(reportData, options);
        }

        private object GetFailedScenariosData()
        {
            return Statistics.GetFailedScenarios().Select(r => new
            {
                name = r.ScenarioName,
                duration = r.Duration,
                errorMessage = r.ErrorMessage,
                browser = r.Browser
            });
        }

        private object GetDownloadedFilesData()
        {
            return new
            {
                totalCount = Statistics.DownloadedFilesCount.Values.Sum(),
                byType = Statistics.DownloadedFilesCount,
                files = Statistics.ScenarioResults
                    .Where(s => s.DownloadedFiles.Any())
                    .SelectMany(s => s.DownloadedFiles.Select(f => new { scenario = s.ScenarioName, file = f }))
            };
        }

        private object GetSlowestScenarioData()
        {
            var slowest = Statistics.GetSlowestScenario();
            if (slowest == null) return null;

            return new
            {
                name = slowest.ScenarioName,
                duration = slowest.Duration,
                browser = slowest.Browser
            };
        }

        private object GetFastestScenarioData()
        {
            var fastest = Statistics.ScenarioResults.OrderBy(r => r.Duration).FirstOrDefault();
            if (fastest == null) return null;

            return new
            {
                name = fastest.ScenarioName,
                duration = fastest.Duration,
                browser = fastest.Browser
            };
        }

        private object GetPerformanceData()
        {
            return new
            {
                executionStatistics = new
                {
                    totalScenarios = Statistics.TotalScenarios,
                    executedScenarios = Statistics.ScenarioResults.Count,
                    parallelCapability = "N/A",
                    averageScenarioTime = Statistics.GetAverageExecutionTime()
                },
                systemMetrics = new
                {
                    peakMemoryMB = Statistics.PeakMemoryUsage,
                    totalDurationSeconds = Statistics.TotalExecutionTime
                },
                downloadMetrics = new
                {
                    totalFilesDownloaded = Statistics.DownloadedFilesCount.Values.Sum(),
                    averageFilesPerScenario = Statistics.TotalScenarios > 0 
                        ? (double)Statistics.DownloadedFilesCount.Values.Sum() / Statistics.TotalScenarios 
                        : 0
                }
            };
        }
    }
}
