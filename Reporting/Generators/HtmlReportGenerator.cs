using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GooglePatentsAutomation.Reporting;

namespace GooglePatentsAutomation.Reporting.Generators
{
    /// <summary>
    /// Generates HTML reports with styling and interactive elements
    /// </summary>
    public class HtmlReportGenerator : ReportGenerator
    {
        public HtmlReportGenerator(ExecutionStatistics statistics, ReportConfiguration configuration)
            : base(statistics, configuration)
        {
        }

        /// <summary>
        /// Generate HTML report
        /// </summary>
        public override string GenerateReport()
        {
            var html = new StringBuilder();

            html.Append("<!DOCTYPE html>");
            html.Append("<html lang=\"en\">");
            html.Append("<head>");
            html.Append("<meta charset=\"UTF-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.Append($"<title>{GetReportTitle()}</title>");
            html.Append(GetStyles());
            html.Append("</head>");
            html.Append("<body>");

            // Header
            html.Append(GetHeader());

            // Summary Section
            html.Append(GetSummarySection());

            // Statistics Section
            html.Append(GetStatisticsSection());

            // Scenarios Details
            html.Append(GetScenariosDetailsSection());

            // Failed Scenarios
            if (Statistics.FailedScenarios > 0)
            {
                html.Append(GetFailedScenariosSection());
            }

            // Metrics Section
            if (Configuration.IncludeMetrics)
            {
                html.Append(GetMetricsSection());
            }

            // Footer
            html.Append(GetFooter());

            html.Append("</body>");
            html.Append("</html>");

            return html.ToString();
        }

        private string GetStyles()
        {
            return @"
<style>
    * { margin: 0; padding: 0; box-sizing: border-box; }

    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        line-height: 1.6;
        color: #333;
        background: #f5f5f5;
        padding: 20px;
    }

    .container {
        max-width: 1200px;
        margin: 0 auto;
        background: white;
        border-radius: 8px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        overflow: hidden;
    }

    .header {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 40px 20px;
        text-align: center;
    }

    .header h1 {
        font-size: 2.5em;
        margin-bottom: 10px;
    }

    .header p {
        font-size: 1.1em;
        opacity: 0.9;
    }

    .summary {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 20px;
        padding: 30px;
        background: #f9f9f9;
    }

    .summary-card {
        background: white;
        padding: 20px;
        border-radius: 6px;
        border-left: 4px solid #667eea;
        box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }

    .summary-card.success {
        border-left-color: #27ae60;
    }

    .summary-card.warning {
        border-left-color: #f39c12;
    }

    .summary-card.error {
        border-left-color: #e74c3c;
    }

    .summary-card h3 {
        font-size: 0.9em;
        text-transform: uppercase;
        color: #666;
        margin-bottom: 10px;
    }

    .summary-card .value {
        font-size: 2.5em;
        font-weight: bold;
        color: #333;
    }

    .summary-card .percentage {
        font-size: 1.2em;
        color: #667eea;
        margin-top: 5px;
    }

    .section {
        padding: 30px;
        border-top: 1px solid #eee;
    }

    .section h2 {
        font-size: 1.8em;
        color: #333;
        margin-bottom: 20px;
        padding-bottom: 10px;
        border-bottom: 3px solid #667eea;
    }

    table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 20px;
    }

    th, td {
        padding: 12px;
        text-align: left;
        border-bottom: 1px solid #ddd;
    }

    th {
        background: #667eea;
        color: white;
        font-weight: 600;
    }

    tr:hover {
        background: #f9f9f9;
    }

    .badge {
        display: inline-block;
        padding: 4px 12px;
        border-radius: 20px;
        font-size: 0.85em;
        font-weight: 600;
    }

    .badge.pass {
        background: #d4edda;
        color: #155724;
    }

    .badge.fail {
        background: #f8d7da;
        color: #721c24;
    }

    .badge.skip {
        background: #e2e3e5;
        color: #383d41;
    }

    .metric-chart {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 20px;
        margin-top: 20px;
    }

    .chart-item {
        background: white;
        padding: 20px;
        border-radius: 6px;
        border: 1px solid #eee;
    }

    .chart-item h4 {
        margin-bottom: 15px;
        color: #333;
    }

    .progress-bar {
        background: #eee;
        height: 30px;
        border-radius: 15px;
        overflow: hidden;
    }

    .progress-fill {
        height: 100%;
        background: linear-gradient(90deg, #27ae60, #2ecc71);
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
        font-weight: bold;
        font-size: 0.9em;
        transition: width 0.3s ease;
    }

    .error-details {
        background: #fff3cd;
        border-left: 4px solid #f39c12;
        padding: 15px;
        margin-top: 10px;
        border-radius: 4px;
        font-family: monospace;
        font-size: 0.9em;
        color: #856404;
    }

    .footer {
        background: #f5f5f5;
        padding: 20px;
        text-align: center;
        color: #666;
        font-size: 0.9em;
        border-top: 1px solid #ddd;
    }

    .time-badge {
        background: #e8f4f8;
        color: #1f5e7d;
        padding: 2px 8px;
        border-radius: 4px;
        font-family: monospace;
    }

    @media print {
        body { background: white; padding: 0; }
        .container { box-shadow: none; }
    }
</style>";
        }

        private string GetHeader()
        {
            return $@"
<div class=""container"">
    <div class=""header"">
        <h1>{GetReportTitle()}</h1>
        <p>Generated on {GetTimestamp()}</p>
        <p>{Configuration.CompanyName} - {Configuration.ProjectName}</p>
    </div>";
        }

        private string GetSummarySection()
        {
            var passPercentage = Statistics.GetPassRate();
            var failPercentage = 100 - passPercentage;

            return $@"
<div class=""summary"">
    <div class=""summary-card"">
        <h3>Total Scenarios</h3>
        <div class=""value"">{Statistics.TotalScenarios}</div>
    </div>

    <div class=""summary-card success"">
        <h3>Passed</h3>
        <div class=""value"">{Statistics.PassedScenarios}</div>
        <div class=""percentage"">{passPercentage:F2}%</div>
    </div>

    <div class=""summary-card error"">
        <h3>Failed</h3>
        <div class=""value"">{Statistics.FailedScenarios}</div>
        <div class=""percentage"">{failPercentage:F2}%</div>
    </div>

    <div class=""summary-card warning"">
        <h3>Skipped</h3>
        <div class=""value"">{Statistics.SkippedScenarios}</div>
    </div>
</div>";
        }

        private string GetStatisticsSection()
        {
            var avgTime = Statistics.GetAverageExecutionTime();
            var totalTime = Statistics.TotalExecutionTime;

            return $@"
<div class=""section"">
    <h2>Execution Statistics</h2>
    <div class=""summary"">
        <div class=""summary-card"">
            <h3>Total Duration</h3>
            <div class=""value"">{totalTime:F2}s</div>
        </div>

        <div class=""summary-card"">
            <h3>Average Duration</h3>
            <div class=""value"">{avgTime:F2}s</div>
        </div>

        <div class=""summary-card"">
            <h3>Peak Memory</h3>
            <div class=""value"">{Statistics.PeakMemoryUsage:F0}MB</div>
        </div>

        <div class=""summary-card"">
            <h3>Success Rate</h3>
            <div class=""value"">{Statistics.GetPassRate():F2}%</div>
        </div>
    </div>
</div>";
        }

        private string GetScenariosDetailsSection()
        {
            var tableRows = new StringBuilder();

            foreach (var result in Statistics.ScenarioResults)
            {
                var statusBadge = result.Status.ToUpper() switch
                {
                    "PASS" => $"<span class=\"badge pass\">PASS</span>",
                    "FAIL" => $"<span class=\"badge fail\">FAIL</span>",
                    "SKIP" => $"<span class=\"badge skip\">SKIP</span>",
                    _ => $"<span class=\"badge\">{result.Status}</span>"
                };

                tableRows.Append($@"
<tr>
    <td>{result.ScenarioName}</td>
    <td>{statusBadge}</td>
    <td><span class=""time-badge"">{result.Duration:F2}s</span></td>
    <td>{result.Browser}</td>
</tr>");
            }

            return $@"
<div class=""section"">
    <h2>Scenario Details</h2>
    <table>
        <thead>
            <tr>
                <th>Scenario</th>
                <th>Status</th>
                <th>Duration</th>
                <th>Browser</th>
            </tr>
        </thead>
        <tbody>
            {tableRows}
        </tbody>
    </table>
</div>";
        }

        private string GetFailedScenariosSection()
        {
            var failedCount = Statistics.FailedScenarios;
            if (failedCount == 0)
            {
                return string.Empty;
            }

            var failedScenarios = new StringBuilder();

            foreach (var result in Statistics.GetFailedScenarios())
            {
                failedScenarios.Append($@"
<div style=""margin-bottom: 20px;"">
    <h4>{result.ScenarioName}</h4>
    <div class=""error-details"">{result.ErrorMessage}</div>
</div>");
            }

            return $@"
<div class=""section"">
    <h2>Failed Scenarios Details</h2>
    {failedScenarios}
</div>";
        }

        private string GetMetricsSection()
        {
            var slowest = Statistics.GetSlowestScenario();

            return $@"
<div class=""section"">
    <h2>Performance Metrics</h2>
    <div class=""metric-chart"">
        <div class=""chart-item"">
            <h4>Pass Rate</h4>
            <div class=""progress-bar"">
                <div class=""progress-fill"" style=""width: {Statistics.GetPassRate()}%"">
                    {Statistics.GetPassRate():F1}%
                </div>
            </div>
        </div>

        <div class=""chart-item"">
            <h4>Slowest Scenario</h4>
            <p><strong>{slowest?.ScenarioName}</strong></p>
            <p>Duration: <span class=""time-badge"">{slowest?.Duration:F2}s</span></p>
        </div>

        <div class=""chart-item"">
            <h4>Downloaded Files</h4>
            <p>Total: <strong>{Statistics.DownloadedFilesCount.Values.Sum()}</strong> files</p>
        </div>
    </div>
</div>";
        }

        private string GetFooter()
        {
            return $@"
    <div class=""footer"">
        <p>Report generated on {GetTimestamp()} | Google Patents Automation Test Suite</p>
    </div>
</div>";
        }
    }
}
