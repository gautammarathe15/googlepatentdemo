# Test Execution Guide - Google Patents Automation

## Overview
This document provides instructions on how to run the Google Patents automation tests using SpecFlow with .NET 6 and Selenium WebDriver.

---

## Prerequisites

### System Requirements
- **OS**: Windows 10/11 or Linux
- **.NET Version**: .NET 6.0 or later
- **Chrome Browser**: Latest version installed
- **RAM**: Minimum 2GB (4GB recommended)
- **Internet**: Active internet connection required

### Dependencies Installed
The following NuGet packages are configured in the project:

- `Selenium.WebDriver` (4.41.0) - WebDriver for browser automation
- `Selenium.WebDriver.ChromeDriver` (131.0.6778) - ChromeDriver binary
- `SpecFlow.NUnit` (3.9.40) - SpecFlow integration with NUnit
- `NUnit` (3.13.2) - Unit testing framework
- `Microsoft.Bcl.AsyncInterfaces` (8.0.0) - Async interfaces compatibility
- `FluentAssertions` (6.2.0) - Fluent assertion library

---

## Project Structure

```
GooglePatentsAutomation/
??? gherkin Features/
?   ??? DownloadPatent.feature       # Gherkin feature file with test scenarios
??? Steps/
?   ??? StepDefinitions.cs            # Step definitions for all scenarios
??? downloads/                        # Directory for downloaded PDFs
??? GooglePatentsAutomation.csproj   # Project configuration
??? GooglePatentsAutomation.sln      # Solution file
```

---

## Running Tests

### 1. **Restore Dependencies**
```powershell
cd "C:\Users\gauta\source\repos\GooglePatentsAutomation\"
dotnet restore
```

### 2. **Build the Project**
```powershell
dotnet build
```

### 3. **Run All Tests**
```powershell
dotnet test
```

### 4. **Run Tests with Verbose Output**
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### 5. **Run Specific Scenario by Tag**
```powershell
# Run only E2E tests
dotnet test --filter "E2E"

# Run only REQ-1 tests
dotnet test --filter "REQ-1"
```

### 6. **Run in Headless Mode (CI/CD)**
```powershell
$env:CHROME_HEADLESS = "true"
dotnet test
```

---

## Test Scenarios

### Scenario 1: Launch Chrome (@REQ-1)
**Purpose**: Verify that Chrome browser launches successfully

**Steps**:
1. Launch Chrome browser using Selenium WebDriver

**Expected Result**: Browser opens without errors

---

### Scenario 2: Navigate to Google Patents via Google search (@REQ-2)
**Purpose**: Verify navigation to Google Patents through Google search

**Steps**:
1. Launch Chrome
2. Open https://www.google.com
3. Accept cookies if presented
4. Search for "google patents"
5. Verify search results contain patents.google link

**Expected Result**: Patents.google appears in search results

---

### Scenario 3: Search for "stent" on Google Patents (@REQ-3)
**Purpose**: Verify patent search functionality

**Steps**:
1. Navigate to Google Patents page
2. Enter "stent" in search field
3. Submit search
4. Verify search results are displayed

**Expected Result**: Patent listings for "stent" are shown

---

### Scenario 4: Download patent PDF from first result (@REQ-4)
**Purpose**: Verify PDF download capability

**Steps**:
1. Perform search for "stent" patents
2. Open first patent result
3. Trigger PDF download
4. Verify PDF file is created
5. Verify download completes within 60 seconds

**Expected Result**: PDF file downloaded successfully

---

### Scenario 5: End-to-End - Search and download patent PDF (@REQ-5 @E2E)
**Purpose**: Complete workflow test from search to download

**Steps**:
1. Clean downloads directory
2. Launch Chrome
3. Open Google.com
4. Accept cookies
5. Search for "google patents"
6. Follow patents.google link
7. Search for "stent"
8. Open first patent
9. Download PDF
10. Verify PDF file exists and has size > 0 bytes

**Expected Result**: Complete workflow executes successfully

---

### Scenario 6: Handle patent pages without direct PDF (@REQ-4)
**Purpose**: Test fallback strategies for PDF unavailability

**Steps**:
1. Navigate to patent page without direct PDF link
2. Attempt download using fallback strategies
3. Report appropriate status

**Expected Result**: System handles gracefully or reports PDF unavailable

---

## Step Definitions Reference

### Helper Methods

#### `GetProjectRootPath()`
Returns the absolute path to the project root directory.

#### `GetDriver()`
Gets or creates a WebDriver instance, automatically initializing if needed.

### Key Steps Implemented

| Step | Type | Purpose |
|------|------|---------|
| `a clean download directory at ""` | Given | Creates/cleans downloads directory |
| `the Chrome browser is launched` | Given | Initializes ChromeDriver with options |
| `I have navigated to the Google Patents page` | Given | Direct navigation to patents.google.com |
| `I open ""` | When | Navigate to URL |
| `I accept cookies if presented` | When | Dismiss cookie banners |
| `I search Google for ""` | When | Perform Google search |
| `I enter "" into the patents search field and submit` | When | Search on Google Patents |
| `I open the first patent result` | When | Click first search result |
| `I trigger the PDF download` | When | Initiate PDF download |
| `I follow the official Google Patents result` | When | Click patents.google link |
| `I should see a search result linking to ""` | Then | Verify search results |
| `search results containing patent listings should be displayed` | Then | Verify results exist |
| `a PDF file should be created in ""` | Then | Verify file creation |
| `the PDF download should complete successfully within X seconds` | Then | Wait for download completion |
| `the download directory should contain at least one completed .pdf file` | Then | Verify downloads |
| `the downloaded file size should be greater than X bytes` | Then | Verify file size |

---

## Configuration Options

### Chrome Options Configured

- **Download Directory**: Set to project's `downloads` folder
- **Popups Handling**: Disabled
- **Notifications**: Disabled
- **Headless Mode**: Optional (set via `CHROME_HEADLESS` environment variable)

### Timeouts

- **Default WebDriver Wait**: 10 seconds
- **Cookie Banner Wait**: 5 seconds
- **Page Load Wait**: 2-3 seconds
- **Download Completion Wait**: 60 seconds maximum

---

## Troubleshooting

### Issue: Chrome Driver Not Found
**Solution**: Ensure `Selenium.WebDriver.ChromeDriver` NuGet package is installed
```powershell
dotnet add package Selenium.WebDriver.ChromeDriver
```

### Issue: Assembly Loading Error
**Solution**: Restore NuGet packages
```powershell
dotnet restore
```

### Issue: Download Directory Not Found
**Solution**: The directory will be created automatically, but verify permissions:
```powershell
Test-Path "C:\Users\gauta\source\repos\GooglePatentsAutomation\downloads"
```

### Issue: Tests Timeout
**Solution**: 
- Increase wait times in step definitions
- Check internet connection
- Verify Chrome browser is working
- Try headless mode

### Issue: Step Definition Not Found
**Solution**: Verify step text in feature file matches exactly with `[Given]`, `[When]`, `[Then]` attributes

---

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Run Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '6.0.x'
      - run: dotnet restore
      - run: dotnet build
      - run: CHROME_HEADLESS=true dotnet test
```

---

## Performance Metrics

### Expected Test Execution Times

- **Scenario 1 (Launch Chrome)**: ~3-5 seconds
- **Scenario 2 (Google Search)**: ~8-12 seconds
- **Scenario 3 (Patents Search)**: ~10-15 seconds
- **Scenario 4 (PDF Download)**: ~20-60 seconds
- **Scenario 5 (E2E)**: ~60-120 seconds
- **Scenario 6 (Fallback)**: ~5-10 seconds

**Total Suite**: Approximately 2-5 minutes

---

## Best Practices

1. **Clean Downloads Directory**: Each scenario cleans the directory first
2. **Explicit Waits**: All waits use WebDriverWait with explicit conditions
3. **Error Handling**: Steps include try-catch blocks for robustness
4. **Teardown**: Browser is properly closed after each scenario
5. **Logging**: Steps output provides clear execution trace

---

## Support & Documentation

- **Selenium Documentation**: https://www.selenium.dev/documentation/
- **SpecFlow Documentation**: https://docs.specflow.org/
- **NUnit Documentation**: https://docs.nunit.org/
- **Google Patents**: https://patents.google.com/

---

## Version Information

- **SpecFlow Version**: 3.9.40
- **.NET Version**: 6.0
- **Selenium WebDriver Version**: 4.41.0
- **ChromeDriver Version**: 131.0.6778
- **Project Created**: March 24, 2026

---

## Contact & Issues

For issues or questions, refer to the step definitions in `Steps/StepDefinitions.cs` or the feature file at `gherkin Features/DownloadPatent.feature`.
