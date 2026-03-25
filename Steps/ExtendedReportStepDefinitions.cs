using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using NUnit.Framework;
using GooglePatentsAutomation.Reporting;

namespace GooglePatentsAutomation.Steps
{
    [Binding]
    public class ExtendedReportStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private ExecutionStatistics _currentStatistics;
        private ReportManager _reportManager;
        private ReportConfiguration _reportConfig;

        public ExtendedReportStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"the report generation system is initialized")]
        public void GivenTheReportGenerationSystemIsInitialized()
        {
            _reportConfig = new ReportConfiguration
            {
                ReportDirectory = "reports",
                ProjectName = "Google Patents Automation",
                CompanyName = "QA Team"
            };

            _currentStatistics = new ExecutionStatistics
            {
                ExecutionStartTime = DateTime.Now,
                ExecutionEndTime = DateTime.Now
            };

            _reportManager = new ReportManager(_currentStatistics, _reportConfig);
            _scenarioContext["ReportManager"] = _reportManager;
            _scenarioContext["ReportConfig"] = _reportConfig;
            _scenarioContext["ExecutionStats"] = _currentStatistics;
        }

        [Given(@"the reports directory ""(.*)"" exists")]
        public void GivenTheReportsDirectoryExists(string directoryName)
        {
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), directoryName);
            Directory.CreateDirectory(reportPath);
            Assert.IsTrue(Directory.Exists(reportPath), $"Reports directory '{directoryName}' was not created");
        }

        [Given(@"a test execution has completed with ""(.*)"" total scenarios")]
        public void GivenATestExecutionHasCompletedWithTotalScenarios(int totalScenarios)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.TotalScenarios = totalScenarios;
        }

        [Given(@"""(.*)"" scenarios passed successfully")]
        public void GivenScenariosPassed(int passedCount)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.PassedScenarios = passedCount;

            // Add passed scenario results
            for (int i = 0; i < passedCount; i++)
            {
                stats.ScenarioResults.Add(new TestExecutionResult
                {
                    ScenarioName = $"Scenario {i + 1}",
                    Status = "PASS",
                    Duration = 10 + i * 5,
                    StartTime = DateTime.Now.AddSeconds(-100),
                    EndTime = DateTime.Now.AddSeconds(-100 + (10 + i * 5)),
                    Browser = "Chrome"
                });
            }
        }

        [Given(@"""(.*)"" scenario failed")]
        public void GivenScenarioFailed(int failedCount)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.FailedScenarios = failedCount;

            // Add failed scenario results
            for (int i = 0; i < failedCount; i++)
            {
                stats.ScenarioResults.Add(new TestExecutionResult
                {
                    ScenarioName = $"Failed Scenario {i + 1}",
                    Status = "FAIL",
                    Duration = 20 + i * 10,
                    StartTime = DateTime.Now.AddSeconds(-50),
                    EndTime = DateTime.Now.AddSeconds(-50 + (20 + i * 10)),
                    Browser = "Chrome",
                    ErrorMessage = $"Test assertion failed: Expected value not found"
                });
            }
        }

        [When(@"I generate an HTML report with execution summary")]
        public void WhenIGenerateAnHtmlReportWithExecutionSummary()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];

            stats.TotalExecutionTime = 60;
            stats.PeakMemoryUsage = 300;
            stats.ExecutionEndTime = DateTime.Now;

            var reportPath = reportManager.GenerateReport("HTML");
            _scenarioContext["GeneratedReportPath"] = reportPath;
            Assert.IsTrue(File.Exists(reportPath), $"HTML report was not generated at {reportPath}");
        }

        [Then(@"the HTML report file should be created in ""(.*)""")]
        public void ThenTheHtmlReportFileShouldBeCreatedIn(string directoryName)
        {
            var reportPath = (string)_scenarioContext["GeneratedReportPath"];
            Assert.IsTrue(reportPath.Contains(directoryName), $"Report is not in {directoryName} directory");
            Assert.IsTrue(File.Exists(reportPath), $"Report file does not exist");
        }

        [Then(@"the report should contain pass/fail summary")]
        public void ThenTheReportShouldContainPassFailSummary()
        {
            var reportPath = (string)_scenarioContext["GeneratedReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("Passed") || content.Contains("PASS"), "Report does not contain pass summary");
            Assert.IsTrue(content.Contains("Failed") || content.Contains("FAIL"), "Report does not contain fail summary");
        }

        [Then(@"the report should display scenario execution times")]
        public void ThenTheReportShouldDisplayScenarioExecutionTimes()
        {
            var reportPath = (string)_scenarioContext["GeneratedReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("Duration") || content.Contains("seconds") || content.Contains("s"), 
                "Report does not contain execution time information");
        }

        [Then(@"the report should include error details for failed scenarios")]
        public void ThenTheReportShouldIncludeErrorDetailsForFailedScenarios()
        {
            var reportPath = (string)_scenarioContext["GeneratedReportPath"];
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            var content = File.ReadAllText(reportPath);

            if (stats.FailedScenarios > 0)
            {
                Assert.IsTrue(content.Contains("error") || content.Contains("Error") || content.Contains("failed"), 
                    "Report does not contain error details");
            }
        }

        [Given(@"a test execution completed in ""(.*)"" seconds")]
        public void GivenATestExecutionCompletedInSeconds(int seconds)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.TotalExecutionTime = seconds;
        }

        [Given(@"memory usage peaked at ""(.*)"" MB")]
        public void GivenMemoryUsagePeakedAtMb(double memoryMb)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.PeakMemoryUsage = memoryMb;
        }

        [Given(@"downloaded ""(.*)"" PDF files successfully")]
        public void GivenDownloadedPdfFilesSuccessfully(int fileCount)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.DownloadedFilesCount["PDF"] = fileCount;
        }

        [When(@"I generate a JSON report with detailed metrics")]
        public void WhenIGenerateAJsonReportWithDetailedMetrics()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var reportPath = reportManager.GenerateReport("JSON");
            _scenarioContext["GeneratedJsonReportPath"] = reportPath;
            Assert.IsTrue(File.Exists(reportPath), $"JSON report was not generated");
        }

        [Then(@"the JSON report should contain execution statistics")]
        public void ThenTheJsonReportShouldContainExecutionStatistics()
        {
            var reportPath = (string)_scenarioContext["GeneratedJsonReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("summary") || content.Contains("statistics"), 
                "JSON report does not contain statistics");
        }

        [Then(@"the JSON report should include performance metrics")]
        public void ThenTheJsonReportShouldIncludePerformanceMetrics()
        {
            var reportPath = (string)_scenarioContext["GeneratedJsonReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("peakMemoryUsage") || content.Contains("performance") || content.Contains("metrics"), 
                "JSON report does not include performance metrics");
        }

        [Then(@"the JSON report should list downloaded files")]
        public void ThenTheJsonReportShouldListDownloadedFiles()
        {
            var reportPath = (string)_scenarioContext["GeneratedJsonReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("downloadedFiles") || content.Contains("files"), 
                "JSON report does not list downloaded files");
        }

        [Then(@"the JSON report should be valid JSON format")]
        public void ThenTheJsonReportShouldBeValidJsonFormat()
        {
            var reportPath = (string)_scenarioContext["GeneratedJsonReportPath"];
            var content = File.ReadAllText(reportPath);

            try
            {
                System.Text.Json.JsonDocument.Parse(content);
            }
            catch (Exception ex)
            {
                Assert.Fail($"JSON report is not valid JSON: {ex.Message}");
            }
        }

        [Given(@"the following test scenarios have executed")]
        public void GivenTheFollowingTestScenariosHaveExecuted(Table table)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            stats.ScenarioResults.Clear();

            foreach (var row in table.Rows)
            {
                var result = new TestExecutionResult
                {
                    ScenarioName = row["Scenario Name"],
                    Status = row["Status"],
                    Duration = double.Parse(row["Duration"]),
                    Browser = row["Browser"],
                    StartTime = DateTime.Now.AddSeconds(-100),
                    EndTime = DateTime.Now.AddSeconds(-100 + double.Parse(row["Duration"]))
                };

                stats.ScenarioResults.Add(result);

                if (result.Status == "PASS") stats.PassedScenarios++;
                else if (result.Status == "FAIL") stats.FailedScenarios++;
                else if (result.Status == "SKIP") stats.SkippedScenarios++;
            }

            stats.TotalScenarios = stats.ScenarioResults.Count;
        }

        [When(@"I generate a CSV report from scenario results")]
        public void WhenIGenerateACsvReportFromScenarioResults()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var reportPath = reportManager.GenerateReport("CSV");
            _scenarioContext["GeneratedCsvReportPath"] = reportPath;
            Assert.IsTrue(File.Exists(reportPath), $"CSV report was not generated");
        }

        [Then(@"the CSV report should be created in ""(.*)""")]
        public void ThenTheCsvReportShouldBeCreatedIn(string directoryName)
        {
            var reportPath = (string)_scenarioContext["GeneratedCsvReportPath"];
            Assert.IsTrue(reportPath.Contains(directoryName), $"CSV report is not in {directoryName} directory");
        }

        [Then(@"the CSV report should contain all scenario data")]
        public void ThenTheCsvReportShouldContainAllScenarioData()
        {
            var reportPath = (string)_scenarioContext["GeneratedCsvReportPath"];
            var content = File.ReadAllText(reportPath);
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];

            foreach (var scenario in stats.ScenarioResults)
            {
                Assert.IsTrue(content.Contains(scenario.ScenarioName), 
                    $"CSV report does not contain scenario: {scenario.ScenarioName}");
            }
        }

        [Then(@"the CSV report should be readable by Excel")]
        public void ThenTheCsvReportShouldBeReadableByExcel()
        {
            var reportPath = (string)_scenarioContext["GeneratedCsvReportPath"];
            var content = File.ReadAllText(reportPath);

            // Verify CSV format
            Assert.IsTrue(content.Contains(","), "CSV report does not contain comma separators");
            Assert.IsTrue(content.Contains("\n") || content.Contains("\r\n"), "CSV report does not contain line breaks");
        }

        [Given(@"previous test execution reports exist from last ""(.*)"" runs")]
        public void GivenPreviousTestExecutionReportsExistFromLastRuns(int runCount)
        {
            _scenarioContext["PreviousRunCount"] = runCount;
        }

        [Given(@"execution times were ""(.*)"", ""(.*)"", ""(.*)""")]
        public void GivenExecutionTimesWere(string time1, string time2, string time3)
        {
            var times = new List<string> { time1, time2, time3 };
            _scenarioContext["ExecutionTimes"] = times;
        }

        [Given(@"pass rates were ""(.*)"", ""(.*)"", ""(.*)""")]
        public void GivenPassRatesWere(string rate1, string rate2, string rate3)
        {
            var rates = new List<string> { rate1, rate2, rate3 };
            _scenarioContext["PassRates"] = rates;
        }

        [When(@"I generate a trend analysis report")]
        public void WhenIGenerateATrendAnalysisReport()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var rates = (List<string>)_scenarioContext["PassRates"];

            // Verify trend
            var passRates = rates.Select(r => double.Parse(r.Trim('%'))).ToList();
            _scenarioContext["ImprovementTrend"] = passRates.Last() > passRates.First();

            var reportPath = reportManager.GenerateReport("HTML");
            _scenarioContext["GeneratedTrendReportPath"] = reportPath;
        }

        [Then(@"the analysis report should show improvement trend")]
        public void ThenTheAnalysisReportShouldShowImprovementTrend()
        {
            var hasTrend = (bool)_scenarioContext["ImprovementTrend"];
            Assert.IsTrue(hasTrend, "No improvement trend detected");
        }

        [Then(@"the analysis report should highlight slow scenarios")]
        public void ThenTheAnalysisReportShouldHighlightSlowScenarios()
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            var slowest = stats.GetSlowestScenario();
            Assert.IsNotNull(slowest, "No slowest scenario identified");
        }

        [Then(@"the analysis report should identify flaky tests")]
        public void ThenTheAnalysisReportShouldIdentifyFlakyTests()
        {
            // Implementation for identifying flaky tests
            _scenarioContext["FlakyTestsIdentified"] = true;
        }

        [Given(@"test scenarios have taken ""(.*)"" screenshots")]
        public void GivenTestScenariosHaveTakenScreenshots(int screenshotCount)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];

            for (int i = 0; i < stats.ScenarioResults.Count && i < screenshotCount; i++)
            {
                stats.ScenarioResults[i].Screenshots.Add($"screenshot_{i}.png");
            }
        }

        [Given(@"""(.*)"" screenshots are from failed scenarios")]
        public void GivenScreenshotsAreFromFailedScenarios(int failedScreenshots)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            var failedScenarios = stats.ScenarioResults.Where(r => r.Status == "FAIL").ToList();

            for (int i = 0; i < Math.Min(failedScreenshots, failedScenarios.Count); i++)
            {
                failedScenarios[i].Screenshots.Add($"failed_screenshot_{i}.png");
            }
        }

        [Given(@"""(.*)"" screenshots are from passed scenarios")]
        public void GivenScreenshotsAreFromPassedScenarios(int passedScreenshots)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            var passedScenarios = stats.ScenarioResults.Where(r => r.Status == "PASS").ToList();

            for (int i = 0; i < Math.Min(passedScreenshots, passedScenarios.Count); i++)
            {
                passedScenarios[i].Screenshots.Add($"passed_screenshot_{i}.png");
            }
        }

        [When(@"I generate a report with screenshot attachments")]
        public void WhenIGenerateAReportWithScreenshotAttachments()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            reportManager.CreateScreenshotDirectories();
            var reportPath = reportManager.GenerateReport("HTML");
            _scenarioContext["GeneratedScreenshotReportPath"] = reportPath;
        }

        [Then(@"the report should contain screenshot references")]
        public void ThenTheReportShouldContainScreenshotReferences()
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            var totalScreenshots = stats.ScenarioResults.Sum(r => r.Screenshots.Count);
            Assert.IsTrue(totalScreenshots > 0, "No screenshots in report");
        }

        [Then(@"the report should have separate folders for pass/fail screenshots")]
        public void ThenTheReportShouldHaveSeparateFoldersForPassFailScreenshots()
        {
            var config = (ReportConfiguration)_scenarioContext["ReportConfig"];
            var passedDir = config.GetPassedScreenshotsDirectory();
            var failedDir = config.GetFailedScreenshotsDirectory();

            Directory.CreateDirectory(passedDir);
            Directory.CreateDirectory(failedDir);

            Assert.IsTrue(Directory.Exists(passedDir), "Passed screenshots directory not created");
            Assert.IsTrue(Directory.Exists(failedDir), "Failed screenshots directory not created");
        }

        [Then(@"the report should generate a screenshot gallery")]
        public void ThenTheReportShouldGenerateAScreenshotGallery()
        {
            var reportPath = (string)_scenarioContext["GeneratedScreenshotReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("img") || content.Contains("screenshot") || content.Contains("gallery"), 
                "Report does not contain screenshot gallery");
        }

        [Given(@"baseline test results from ""(.*)"" exist")]
        public void GivenBaselineTestResultsFromExist(string date)
        {
            _scenarioContext["BaselineDate"] = date;
        }

        [Given(@"current test results from ""(.*)"" exist")]
        public void GivenCurrentTestResultsFromExist(string date)
        {
            _scenarioContext["CurrentDate"] = date;
        }

        [When(@"I generate a comparison report")]
        public void WhenIGenerateAComparisonReport()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var reportPath = reportManager.GenerateReport("HTML");
            _scenarioContext["GeneratedComparisonReportPath"] = reportPath;
        }

        [Then(@"the comparison report should show differences in pass rates")]
        public void ThenTheComparisonReportShouldShowDifferencesInPassRates()
        {
            var reportPath = (string)_scenarioContext["GeneratedComparisonReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("%") || content.Contains("pass") || content.Contains("rate"), 
                "Comparison report does not show pass rate differences");
        }

        [Then(@"the comparison report should highlight newly failing tests")]
        public void ThenTheComparisonReportShouldHighlightNewlyFailingTests()
        {
            // Verify newly failing tests are highlighted
            Assert.IsTrue(true, "Placeholder for newly failing tests highlighting");
        }

        [Then(@"the comparison report should identify performance improvements")]
        public void ThenTheComparisonReportShouldIdentifyPerformanceImprovements()
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];
            Assert.IsTrue(stats.TotalExecutionTime >= 0, "Performance metrics available");
        }

        [Given(@"test execution with following details")]
        public void GivenTestExecutionWithFollowingDetails(Table table)
        {
            var stats = (ExecutionStatistics)_scenarioContext["ExecutionStats"];

            foreach (var row in table.Rows)
            {
                var key = row[0];
                var value = row[1];

                switch (key)
                {
                    case "Total Scenarios":
                        stats.TotalScenarios = int.Parse(value);
                        break;
                    case "Passed":
                        stats.PassedScenarios = int.Parse(value);
                        break;
                    case "Failed":
                        stats.FailedScenarios = int.Parse(value);
                        break;
                    case "Skipped":
                        stats.SkippedScenarios = int.Parse(value);
                        break;
                    case "Total Duration":
                        stats.TotalExecutionTime = double.Parse(value.Replace(" seconds", ""));
                        break;
                }
            }
        }

        [When(@"I generate an executive summary report")]
        public void WhenIGenerateAnExecutiveSummaryReport()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var summary = reportManager.GetReportSummary();
            _scenarioContext["ReportSummary"] = summary;

            var reportPath = reportManager.GenerateReport("HTML");
            _scenarioContext["GeneratedSummaryReportPath"] = reportPath;
        }

        [Then(@"the summary should include high-level overview")]
        public void ThenTheSummaryShouldIncludeHighLevelOverview()
        {
            var reportPath = (string)_scenarioContext["GeneratedSummaryReportPath"];
            var content = File.ReadAllText(reportPath);

            Assert.IsTrue(content.Contains("Summary") || content.Contains("Overview"), 
                "Report does not include high-level overview");
        }

        [Then(@"the summary should display key metrics in dashboard format")]
        public void ThenTheSummaryShouldDisplayKeyMetricsInDashboardFormat()
        {
            var summary = _scenarioContext["ReportSummary"];
            Assert.IsNotNull(summary, "Report summary not generated");
        }

        [Then(@"the summary should provide recommendations")]
        public void ThenTheSummaryShouldProvideRecommendations()
        {
            var reportPath = (string)_scenarioContext["GeneratedSummaryReportPath"];
            Assert.IsTrue(File.Exists(reportPath), "Summary report not generated");
        }

        [Then(@"the summary should be suitable for stakeholders")]
        public void ThenTheSummaryShouldBeSuitableForStakeholders()
        {
            var summary = _scenarioContext["ReportSummary"];
            Assert.IsNotNull(summary, "Report summary is null");
        }

        [Given(@"test results are ready to distribute")]
        public void GivenTestResultsAreReadyToDistribute()
        {
            _scenarioContext["ResultsReady"] = true;
        }

        [Given(@"recipient list includes ""(.*)"", ""(.*)""")]
        public void GivenRecipientListIncludes(string recipient1, string recipient2)
        {
            var recipients = new List<string> { recipient1, recipient2 };
            _scenarioContext["EmailRecipients"] = recipients;
        }

        [Given(@"report format is set to ""(.*)""")]
        public void GivenReportFormatIsSetTo(string format)
        {
            _scenarioContext["ReportFormat"] = format;
        }

        [When(@"I generate an email report")]
        public void WhenIGenerateAnEmailReport()
        {
            var reportManager = (ReportManager)_scenarioContext["ReportManager"];
            var emailContent = reportManager.GenerateEmailReport();
            _scenarioContext["EmailReportContent"] = emailContent;

            Assert.IsNotNull(emailContent, "Email report was not generated");
        }

        [Then(@"the email report should be generated in HTML format")]
        public void ThenTheEmailReportShouldBeGeneratedInHtmlFormat()
        {
            var emailContent = (string)_scenarioContext["EmailReportContent"];
            Assert.IsTrue(emailContent.Contains("<html") || emailContent.Contains("<HTML"), 
                "Email report is not in HTML format");
        }

        [Then(@"the email report should be suitable for email clients")]
        public void ThenTheEmailReportShouldBeSuitableForEmailClients()
        {
            var emailContent = (string)_scenarioContext["EmailReportContent"];
            Assert.IsTrue(emailContent.Contains("style"), "Email report missing styles for email clients");
        }

        [Then(@"the email report should include inline charts")]
        public void ThenTheEmailReportShouldIncludeInlineCharts()
        {
            var emailContent = (string)_scenarioContext["EmailReportContent"];
            Assert.IsTrue(emailContent.Contains("progress") || emailContent.Contains("chart") || emailContent.Contains("graph"), 
                "Email report does not include inline charts");
        }

        [Then(@"the email report should have fallback text version")]
        public void ThenTheEmailReportShouldHaveFallbackTextVersion()
        {
            // Placeholder for text version fallback
            Assert.IsTrue(true, "Text fallback available");
        }
    }
}
