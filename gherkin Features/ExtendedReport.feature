# Extended Report Feature
# This feature provides comprehensive reporting capabilities for test execution
# Generates detailed HTML, JSON, and CSV reports with metrics and analytics

Feature: Generate Extended Test Reports
  In order to analyze test execution in detail
  As a QA engineer
  I want to generate comprehensive reports with metrics, screenshots, and analytics

  Background:
    Given the report generation system is initialized
    And the reports directory "reports" exists

  @Report @ExtendedReport
  Scenario: Generate HTML report with execution summary
    Given a test execution has completed with "5" total scenarios
    And "4" scenarios passed successfully
    And "1" scenario failed
    When I generate an HTML report with execution summary
    Then the HTML report file should be created in "reports"
    And the report should contain pass/fail summary
    And the report should display scenario execution times
    And the report should include error details for failed scenarios

  @Report @ExtendedReport @Detailed
  Scenario: Generate JSON report with detailed metrics
    Given a test execution completed in "120" seconds
    And memory usage peaked at "450" MB
    And downloaded "3" PDF files successfully
    When I generate a JSON report with detailed metrics
    Then the JSON report should contain execution statistics
    And the JSON report should include performance metrics
    And the JSON report should list downloaded files
    And the JSON report should be valid JSON format

  @Report @ExtendedReport @CSV
  Scenario: Generate CSV report for test results
    Given the following test scenarios have executed
      | Scenario Name | Status | Duration | Browser |
      | Launch Chrome | PASS | 5 | Chrome |
      | Google Search | PASS | 12 | Chrome |
      | Patent Search | PASS | 15 | Chrome |
      | PDF Download | FAIL | 45 | Chrome |
      | E2E Workflow | SKIP | 0 | Chrome |
    When I generate a CSV report from scenario results
    Then the CSV report should be created in "reports"
    And the CSV report should contain all scenario data
    And the CSV report should be readable by Excel

  @Report @ExtendedReport @Analysis
  Scenario: Generate analysis report with trends
    Given previous test execution reports exist from last "3" runs
    And execution times were "120s", "115s", "125s"
    And pass rates were "80%", "85%", "90%"
    When I generate a trend analysis report
    Then the analysis report should show improvement trend
    And the analysis report should highlight slow scenarios
    And the analysis report should identify flaky tests

  @Report @ExtendedReport @Screenshot
  Scenario: Generate report with screenshot attachments
    Given test scenarios have taken "8" screenshots
    And "2" screenshots are from failed scenarios
    And "6" screenshots are from passed scenarios
    When I generate a report with screenshot attachments
    Then the report should contain screenshot references
    And the report should have separate folders for pass/fail screenshots
    And the report should generate a screenshot gallery

  @Report @ExtendedReport @Comparison
  Scenario: Generate comparison report between test runs
    Given baseline test results from "2026-03-24" exist
    And current test results from "2026-03-25" exist
    When I generate a comparison report
    Then the comparison report should show differences in pass rates
    And the comparison report should highlight newly failing tests
    And the comparison report should identify performance improvements

  @Report @ExtendedReport @Summary
  Scenario: Generate executive summary report
    Given test execution with following details
      | Total Scenarios | 6 |
      | Passed | 5 |
      | Failed | 1 |
      | Skipped | 0 |
      | Total Duration | 145 seconds |
      | Success Rate | 83.33% |
    When I generate an executive summary report
    Then the summary should include high-level overview
    And the summary should display key metrics in dashboard format
    And the summary should provide recommendations
    And the summary should be suitable for stakeholders

  @Report @ExtendedReport @Email
  Scenario: Generate email report for distribution
    Given test results are ready to distribute
    And recipient list includes "qa@company.com", "dev@company.com"
    And report format is set to "HTML_EMAIL"
    When I generate an email report
    Then the email report should be generated in HTML format
    And the email report should be suitable for email clients
    And the email report should include inline charts
    And the email report should have fallback text version
