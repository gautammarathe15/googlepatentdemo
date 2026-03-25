# Quick Start - Run REQ-1 to REQ-4 Tests Now

## ? TL;DR - Do This Now

### 1. Build the Project
```bash
cd C:\Users\gauta\source\repos\GooglePatentsAutomation
dotnet build
```

**Expected Result**: ? Build successful (no errors)

---

### 2. Run Individual Tests

#### Test REQ-1: Launch Chrome
```bash
dotnet test --filter "@REQ-1"
```
**Should**: Chrome browser opens, stays open
**Verify**: Chrome window appears with empty tab

---

#### Test REQ-2: Search Google for Patents
```bash
dotnet test --filter "@REQ-2"
```
**Should**: 
- Google.com loads
- Cookie banner handled
- "google patents" typed in search box
- Search results show patents.google link
**Time**: ~15 seconds

---

#### Test REQ-3: Search Patents for "stent"
```bash
dotnet test --filter "@REQ-3"
```
**Should**:
- patents.google.com loads
- "stent" typed in search field
- Results show multiple patents
**Time**: ~10 seconds

---

#### Test REQ-4: Download Patent PDF
```bash
dotnet test --filter "@REQ-4"
```
**Should**:
- Open first patent from results
- Trigger PDF download (if available)
- Verify file created in downloads folder
**Time**: ~15 seconds

---

#### Run All REQ-1 to REQ-4
```bash
dotnet test --filter "@REQ-1 | @REQ-2 | @REQ-3 | @REQ-4"
```
**Total Time**: ~40 seconds

---

#### Run Full E2E Test (Recommended)
```bash
dotnet test --filter "@E2E"
```
**Should**: All steps executed in sequence
**Time**: ~40 seconds

---

## ? What Should Happen

### Test Execution Flow
```
1. Chrome launches
2. Navigate to google.com
3. Accept cookies
4. Search for "google patents"
5. Click patents.google link
6. Search for "stent"
7. See patent results
8. Click first patent
9. Download PDF (if available)
10. Verify file created
```

### Success Indicators
- ? Chrome window opens
- ? Pages load and respond
- ? Search boxes accept text
- ? Results display correctly
- ? Files download to downloads folder
- ? No errors in console

---

## ?? Troubleshooting

### If Tests Fail

#### Error: "Search box not found"
```
Cause: Element not visible or page not ready
Solution: 
1. Check internet connection
2. Try again (may be timing issue)
3. Increase timeout: Change 15000 to 20000 in code
```

#### Error: "Could not locate search input field"
```
Cause: Page structure different than expected
Solution:
1. Check if Google Patents changed UI
2. Open patents.google.com manually
3. Check where search field is located
4. Report new selector if changed
```

#### Error: "No search results displayed"
```
Cause: Results not rendering or page structure changed
Solution:
1. Verify internet connection
2. Check if cloud browser is accessible
3. Try manual test on Google Patents site
4. Check Chrome version matches ChromeDriver
```

#### Error: "Could not find Google search box"
```
Cause: Google page structure might have changed
Solution:
1. Check google.com loads correctly
2. Manually search to verify it works
3. Check browser console for errors
4. May need UI update from Google
```

---

## ?? Configuration

### Enable Browser Visibility (See What's Happening)
```bash
# Set environment variable before running test
set CHROME_HEADLESS=false
dotnet test --filter "@E2E"
```

### Increase Timeouts (If Network Slow)
Edit `Steps/StepDefinitions.cs`:
```csharp
// Change:
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

// To:
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
```

### Increase Sleep Times (If Timing Issues)
```csharp
// Change:
System.Threading.Thread.Sleep(1000);

// To:
System.Threading.Thread.Sleep(2000); // Double it
```

---

## ?? What Changed (Summary)

### REQ-2: Google Search
- ? Now has 3-tier element detection
- ? Better timeout values
- ? Proper scroll into view
- ? Comprehensive error messages

### REQ-3: Patents Search
- ? Now has explicit page readiness wait
- ? Enhanced to 3-tier search field detection
- ? Better timeout handling
- ? Improved results verification

### All Steps
- ? Added proper wait conditions
- ? Implemented fallback strategies
- ? Fixed compilation errors
- ? Build now successful

---

## ?? Expected Output

### Successful Test Run
```
PASSED @REQ-1 [2s]
PASSED @REQ-2 [12s]  
PASSED @REQ-3 [10s]
PASSED @REQ-4 [15s]
---
All 4 tests passed in ~40s
```

### In Console
```
Running: Launch Chrome
Running: Navigate to Google Patents
Running: Search Google
Running: Verify Results
Running: Search Patents
Running: Open Patent
Running: Trigger Download
Running: Verify Download
? All tests completed successfully
```

---

## ?? Testing Checklist

- [ ] Build compiles: `dotnet build`
- [ ] REQ-1 passes: Chrome launches
- [ ] REQ-2 passes: Google search works
- [ ] REQ-3 passes: Patents search works
- [ ] REQ-4 passes: PDF download detected
- [ ] E2E passes: All steps execute
- [ ] No compilation errors
- [ ] No runtime errors

---

## ?? Pro Tips

1. **Run one test at a time first**
   - Helps identify which step fails
   - Easier to debug one issue at a time

2. **Watch the browser window**
   - See exactly what's happening
   - Helps understand any issues
   - Verify UI matches expectations

3. **Check Internet Connection**
   - Tests require access to Google.com and patents.google.com
   - VPN/Proxy may cause issues
   - Try from normal network if failing

4. **Check Chrome Version**
   - ChromeDriver must match your Chrome version
   - Get from: https://chromedriver.chromium.org
   - Run: `chrome://version/` to see your version

5. **Allow Time for Load**
   - Google Patents pages load dynamically
   - Timeouts are built-in (15-20 seconds)
   - Don't interrupt tests while running

---

## ?? Need Help?

### Check These Files
1. `REQ_1_4_COMPREHENSIVE_FIX_SUMMARY.md` - Detailed explanations
2. `REQ_1_4_QUICK_FIX_REFERENCE.md` - Quick reference
3. `REQ_1_4_FIXES_GUIDE.md` - Step-by-step fixes
4. `REQ_1_4_CODE_CHANGES.md` - Exact code changes

### Common Issues
- **Search not found**: Network or page load issue
- **Results not showing**: DOM structure may have changed
- **Download not detected**: PDF may not be available
- **Build errors**: Check all using statements are included

### Debug Output
If tests fail, check:
- Current URL (should be google.com or patents.google.com)
- Page source length (should be > 10000)
- Browser console for JavaScript errors
- Network tab for failed requests

---

## ? Success Criteria

### All Tests Pass When:
? Chrome launches and closes normally  
? Google.com loads and searches work  
? patents.google.com loads and searches work  
? Patent results display  
? PDF detection works  
? No console errors  
? All steps complete within timeout  

---

**You're all set! Run the tests now and let's see them pass! ??**
