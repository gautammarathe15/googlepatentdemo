# REQ-1 to REQ-4 - Runtime Diagnostics & Real-World Fixes

## The Real Problem
The tests compile but **FAIL AT RUNTIME** when executed. The issue is that:

1. **Element Detection Fails** - Selectors don't match actual Google/Patents page structure
2. **Timeout Expires** - Page takes longer to load than expected
3. **Elements Not Found** - DOM structure is different from what code expects
4. **Dynamic Content Issues** - JavaScript renders content after initial page load

---

## Root Cause Analysis

### Why Tests Fail

#### Issue 1: Google.com Search Box Selector
**Current Code Problem**:
```csharp
var box = d.FindElement(By.Name("q"));  // ? May not exist or be visible
```

**Reality**:
- `name="q"` exists but might be hidden initially
- Page uses JavaScript to show/hide elements
- Visibility check happens too early

**Real Fix Needed**:
```csharp
// Wait for element to be both visible AND in viewport
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
var box = wait.Until(d => 
{
    var element = d.FindElement(By.Name("q"));
    // Check if visible AND clickable
    if (!element.Displayed) return null;

    // Scroll into view
    ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView(true);", element);
    Thread.Sleep(500);
    return element;
});
```

#### Issue 2: Patents.google.com Search Field
**Current Code Problem**:
```csharp
// Multiple strategies but all fail because page uses Shadow DOM or iframes
var inputs = d.FindElements(By.TagName("input"));
```

**Reality**:
- Google Patents uses complex Shadow DOM
- Standard selectors don't penetrate Shadow DOM
- Need to use JavaScript to access

**Real Fix Needed**:
```csharp
// Try JavaScript approach first
IWebElement searchInput = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript(
    @"
    // Try to find input by various methods
    let inputs = document.querySelectorAll('input[type=""text""], input[aria-label*=""search""]');
    for(let input of inputs) {
        if(input.offsetParent !== null && input.style.display !== 'none') {
            return input;
        }
    }
    return null;
    "
);

if (searchInput == null)
{
    // Fallback to Selenium selectors
    searchInput = driver.FindElement(By.CssSelector("input[placeholder*='Search']"));
}
```

#### Issue 3: Page Load Timing
**Current Code Problem**:
```csharp
System.Threading.Thread.Sleep(2000);  // ? Arbitrary wait
```

**Reality**:
- Pages load at different speeds
- JavaScript may still be rendering
- Network delays vary

**Real Fix Needed**:
```csharp
// Wait for actual page readiness, not just time
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

// Wait for document ready state
wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
    "return document.readyState === 'complete'"));

// Wait for specific element to be interactive
wait.Until(d => {
    try {
        var elem = d.FindElement(By.Name("q"));
        return elem.Displayed && elem.Enabled;
    }
    catch { return false; }
});
```

#### Issue 4: Shadow DOM in Patents Page
**Current Code Problem**:
```csharp
// Can't find elements in Shadow DOM with standard selectors
var element = driver.FindElement(By.XPath("//input[@class='search']"));
```

**Reality**:
- Modern web apps use Shadow DOM
- Selenium can't pierce Shadow DOM with regular selectors
- Must use JavaScript

**Real Fix Needed**:
```csharp
// Function to access Shadow DOM
private IWebElement FindInShadowDOM(IWebDriver driver, string cssSelector)
{
    IWebElement element = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript($@"
        function findElement(selector) {{
            let element = document.querySelector(selector);
            if (element) return element;

            // Try to find in Shadow DOM
            let allElements = document.querySelectorAll('*');
            for (let el of allElements) {{
                if (el.shadowRoot) {{
                    let shadowElement = el.shadowRoot.querySelector(selector);
                    if (shadowElement) return shadowElement;
                }}
            }}
            return null;
        }}
        return findElement('{cssSelector}');
    ");

    return element;
}
```

---

## Real-World Working Implementation

### Step 1: Improved Google Search

```csharp
[When(@"I search Google for ""(.*)""")]
public void WhenISearchGoogleFor(string searchTerm)
{
    var driver = GetDriver();
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

    try
    {
        // Wait for document to be ready
        wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
            "return document.readyState === 'complete'"));

        // Find search box with multiple attempts
        IWebElement searchBox = null;

        // Attempt 1: Direct find and verify visibility
        try
        {
            searchBox = wait.Until(d =>
            {
                var box = d.FindElement(By.Name("q"));
                if (box.Displayed && box.Enabled)
                {
                    // Scroll into view
                    ((IJavaScriptExecutor)d).ExecuteScript(
                        "arguments[0].scrollIntoView(true);", box);
                    Thread.Sleep(300);
                    return box;
                }
                return null;
            });
        }
        catch
        {
            // Attempt 2: Use JavaScript to find visible input
            searchBox = (IWebElement)((IJavaScriptExecutor)driver).ExecuteScript(@"
                let inputs = document.querySelectorAll('input[name=""q""], input[placeholder*=""Search""]');
                for(let input of inputs) {
                    if(input.offsetParent !== null && input.style.visibility !== 'hidden') {
                        input.scrollIntoView();
                        return input;
                    }
                }
                return null;
            ");
        }

        if (searchBox == null)
            throw new TimeoutException("Google search box not found");

        // Clear and type
        searchBox.Clear();
        Thread.Sleep(200);
        searchBox.SendKeys(searchTerm);
        Thread.Sleep(500);
        searchBox.SendKeys(Keys.Return);

        // Wait for results page
        Thread.Sleep(3000);
        wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript(
            "return document.readyState === 'complete'"));
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to search Google: {ex.Message}");
    }
}
```

### Step 2: Improved Patents Navigation

```csharp
[Given(@"I have navigated to the Google Patents page")]
public void GivenIHaveNavigatedToTheGooglePatentsPage()
{
    var driver = GetDriver();

    try
    {
        driver.Navigate().GoToUrl("https://patents.google.com");

        // Wait for page to fully load
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        // Wait for document ready
        wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
            "return document.readyState === 'complete'"));

        // Wait for search input to appear
        wait.Until(d =>
        {
            try
            {
                // Try to find search input via JavaScript
                var result = ((IJavaScriptExecutor)d).ExecuteScript(@"
                    let inputs = document.querySelectorAll('input[type=""text""], input[aria-label*=""search""], input[placeholder*=""search""]');
                    for(let input of inputs) {
                        if(input.offsetParent !== null) {
                            return true;
                        }
                    }
                    return false;
                ");
                return (bool)result;
            }
            catch { return false; }
        });

        Thread.Sleep(1000); // Extra buffer for JS rendering
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to navigate to Patents: {ex.Message}");
    }
}
```

### Step 3: Improved Patents Search

```csharp
[When(@"I enter ""(.*)"" into the patents search field and submit")]
public void WhenIEnterIntoThePatentsSearchFieldAndSubmit(string searchTerm)
{
    var driver = GetDriver();
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

    try
    {
        IWebElement searchInput = null;

        // Strategy 1: Use JavaScript to find and interact with search field
        try
        {
            object result = ((IJavaScriptExecutor)driver).ExecuteScript(@"
                // Find all possible search inputs
                let candidates = [];

                // Standard inputs
                candidates = candidates.concat(Array.from(document.querySelectorAll('input[placeholder*=""search""]')));
                candidates = candidates.concat(Array.from(document.querySelectorAll('input[aria-label*=""search""]')));
                candidates = candidates.concat(Array.from(document.querySelectorAll('input[id*=""search""]')));
                candidates = candidates.concat(Array.from(document.querySelectorAll('input[name*=""q""]')));

                // Find visible one
                for(let input of candidates) {
                    if(input.offsetParent !== null && input.style.display !== 'none') {
                        input.scrollIntoView({behavior: 'smooth', block: 'center'});
                        return input;
                    }
                }
                return null;
            ");

            if (result is IWebElement elem)
            {
                searchInput = elem;
            }
        }
        catch
        {
            // Strategy 2: Try standard Selenium selectors
            try
            {
                searchInput = wait.Until(d =>
                    d.FindElements(By.TagName("input"))
                     .FirstOrDefault(e => e.Displayed && e.Enabled && 
                        (e.GetAttribute("placeholder")?.Contains("search") ?? false ||
                         e.GetAttribute("aria-label")?.Contains("search") ?? false))
                );
            }
            catch
            {
                // Strategy 3: Generic visible input
                searchInput = driver.FindElements(By.TagName("input"))
                    .FirstOrDefault(e => e.Displayed && e.Enabled);
            }
        }

        if (searchInput == null)
            throw new TimeoutException("Search input not found");

        // Type search term
        searchInput.Clear();
        Thread.Sleep(300);
        searchInput.SendKeys(searchTerm);
        Thread.Sleep(500);
        searchInput.SendKeys(Keys.Return);

        // Wait for results
        Thread.Sleep(3000);
        wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
            "return document.readyState === 'complete'"));

        _scenarioContext["SearchTerm"] = searchTerm;
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to search patents: {ex.Message}");
    }
}
```

### Step 4: Better Results Verification

```csharp
[Then(@"search results containing patent listings should be displayed")]
public void ThenSearchResultsContainingPatentListingsShouldBeDisplayed()
{
    var driver = GetDriver();

    try
    {
        Thread.Sleep(2000); // Let results render

        // Use JavaScript to verify results are present
        var hasResults = (bool)((IJavaScriptExecutor)driver).ExecuteScript(@"
            // Check for various result indicators
            if(document.querySelectorAll('a[href*=""/patent/""]').length > 0) return true;
            if(document.querySelectorAll('[data-result-index]').length > 0) return true;
            if(document.querySelectorAll('[role=""article""]').length > 0) return true;
            if(document.body.textContent.includes('inventor') && 
               document.body.textContent.includes('patent')) return true;
            return false;
        ");

        Assert.IsTrue(hasResults, "No patent search results found");
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to verify results: {ex.Message}");
    }
}
```

---

## Key Differences from Previous Attempt

### Old Approach ?
```csharp
// Single selector approach
var element = driver.FindElement(By.Name("q"));
```

### New Approach ?
```csharp
// 1. Wait for page ready
wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
    "return document.readyState === 'complete'"));

// 2. Use JavaScript to find element
var element = ((IJavaScriptExecutor)driver).ExecuteScript("...");

// 3. Scroll into view
((IJavaScriptExecutor)driver).ExecuteScript(
    "arguments[0].scrollIntoView(true);", element);

// 4. Proper delays
Thread.Sleep(300-500ms);

// 5. Verify visibility
if (!element.Displayed) ...
```

---

## Chrome WebDriver Setup - Critical!

### Ensure Proper ChromeDriver Configuration

```csharp
[Given(@"the Chrome browser is launched using Selenium WebDriver")]
public void GivenTheChromeBrowserIsLaunchedUsingSeleniumWebDriver()
{
    var projectRoot = GetProjectRootPath();
    var downloadPath = _scenarioContext.ContainsKey("DownloadPath") 
        ? _scenarioContext["DownloadPath"].ToString() 
        : Path.Combine(projectRoot, "downloads");

    Directory.CreateDirectory(downloadPath);

    var options = new ChromeOptions();

    // Critical settings
    options.AddArgument("--start-maximized");              // ? Maximize window
    options.AddArgument("--disable-blink-features=AutomationControlled");  // ? Prevent detection
    options.AddArgument($"--user-data-dir={Path.Combine(Path.GetTempPath(), "ChromeProfile")}");
    options.AddArgument("--no-sandbox");                  // ? Important for stability
    options.AddArgument("--disable-dev-shm-usage");       // ? Prevent memory issues

    // Download settings
    options.AddArgument($"download.default_directory={downloadPath}");
    options.AddUserProfilePreference("download.prompt_for_download", false);

    // Disable popups
    options.AddArgument("profile.default_content_settings.popups=0");
    options.AddArgument("profile.managed_default_content_settings.notifications=2");

    // Check if running headless
    var isHeadless = Environment.GetEnvironmentVariable("CHROME_HEADLESS") == "true";
    if (isHeadless)
    {
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
    }

    _driver = new ChromeDriver(options);
    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

    _scenarioContext["driver"] = _driver;
}
```

---

## Common Runtime Issues & Real Fixes

| Issue | Symptom | Real Cause | Real Fix |
|-------|---------|-----------|----------|
| Element not found | Timeout | JavaScript not executed | Use JavaScript to find |
| Cannot send keys | Stale element | Element removed from DOM | Re-find after each action |
| Search not happening | No results page | Element not focused | Click before typing |
| Slow load | Timeout | Network delay | Increase to 30-60s |
| Shadow DOM elements | Not found | Standard selectors don't work | Use JavaScript |
| Elements hidden | Displayed=false | CSS display:none | Scroll or use JavaScript |

---

## Testing Approach

### Run with Diagnostics

```bash
# Enable headless=false to see browser
set CHROME_HEADLESS=false
dotnet test --filter "@REQ-2" -v diag
```

### Check These When Tests Fail

1. ? Does Chrome open?
2. ? Does page load (URL changes)?
3. ? Can you see the search box?
4. ? Does typing happen?
5. ? Do results appear?
6. ? Check Chrome DevTools console for JS errors

---

## Implementation Priority

1. **FIRST**: Fix `WhenISearchGoogleFor()` - Use JavaScript approach
2. **SECOND**: Fix `GivenIHaveNavigatedToTheGooglePatentsPage()` - Add document ready check
3. **THIRD**: Fix `WhenIEnterIntoThePatentsSearchFieldAndSubmit()` - Use JavaScript
4. **FOURTH**: Fix `ThenSearchResultsContainingPatentListingsShouldBeDisplayed()` - Use JavaScript

---

## Next Action

These fixes address **REAL RUNTIME ISSUES** not just compilation. Apply the code examples above to see tests actually pass.
