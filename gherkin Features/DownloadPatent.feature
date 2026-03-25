# Feature: Google Patents PDF download
# RTM (Requirements Traceability Matrix)
# REQ-1  Launch Google Chrome                     -> Scenario: Launch Chrome
# REQ-2  Search for Google Patents (Google search) -> Scenario: Navigate to Google Patents
# REQ-3  Search for 'stent' on Google Patents      -> Scenario: Search for "stent"
# REQ-4  Download PDF of a patent (if available)   -> Scenario: Download patent PDF
# REQ-5  Entire process automated with Selenium    -> Tag all scenarios with @REQ-5 and provide an End-to-End scenario
# Notes:
# - Each scenario is written to be implemented by step definitions in your chosen Cucumber runner (behave / pytest-bdd / cucumber).
# - Steps include data/paths in quotes to allow parameterized step definitions.

Feature: Download patent PDF from Google Patents
  In order to obtain patent documents programmatically
  As an automation engineer
  I want to search Google for Google Patents, search for "stent", download a patent PDF and verify the download completes

  Background:
    Given a clean download directory at "downloads"

  @REQ-1
  Scenario: Launch Chrome
    Given the Chrome browser is launched using Selenium WebDriver

  @REQ-2
  Scenario: Navigate to Google Patents via Google search
    Given the Chrome browser is launched using Selenium WebDriver
    When I open "https://www.google.com"
    And I accept cookies if presented
    And I search Google for "google patents"
    Then I should see a search result linking to "patents.google"

  @REQ-3
  Scenario: Search for "stent" on Google Patents
    Given I have navigated to the Google Patents page
    When I enter "stent" into the patents search field and submit
    Then search results containing patent listings should be displayed

  @REQ-4
  Scenario: Download patent PDF from first result
    Given search results for "stent" are displayed
    When I open the first patent result
    And I trigger the PDF download for the opened patent (if a PDF is available)
    Then a PDF file should be created in "downloads"
    And the PDF download should complete successfully within 100 seconds

  @REQ-5 @E2E
  Scenario: End-to-end - Search and download patent PDF (executable)
    Given a clean download directory at "downloads"
    And the Chrome browser is launched using Selenium WebDriver
    When I open "https://www.google.com"
    And I accept cookies if presented
    And I search Google for "google patents"
    And I follow the official Google Patents result
    And I search for "stent" on the Google Patents site
    And I open the first patent in the results
    And I download the patent PDF (or attempt to) and wait for completion
    Then the download directory "downloads" should contain at least one completed .pdf file
    And the downloaded file size should be greater than 100 bytes

  # Additional negative/edge-case scenarios (optional)
  @REQ-4
  Scenario: Handle patent pages without a direct PDF
    Given a patent page that does not expose a direct PDF link
    When I attempt to download the PDF using fallback strategies
    Then the tool should either save a viewer PDF or report "PDF not available" in the test result