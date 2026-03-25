import os
import time
from pages.google_home_page import GoogleHomePage
from pages.google_results_page import GoogleResultsPage
from pages.patents_search_page import PatentsSearchPage
from pages.patent_detail_page import PatentDetailPage
from utils.download_helper import wait_for_download

def test_download_first_patent_pdf(driver, download_dir):
    # 1) Google home -> search "google patents"
    google = GoogleHomePage(driver)
    google.open_home()
    google.accept_cookies()
    google.search("google patents")

    # 2) Click official Google Patents result
    results = GoogleResultsPage(driver)
    results.click_google_patents_result()

    # 3) On patents site, search "stent"
    patents = PatentsSearchPage(driver)
    patents.search("stent")

    # 4) Open first patent result
    patents.open_first_result()

    # 5) Download PDF from patent detail page
    detail = PatentDetailPage(driver)
    href = detail.download_pdf()
    # optional: print or assert href exists
    assert href is not None, "Could not find a PDF download link or path"

    # 6) Wait for the PDF file to be saved
    downloaded = wait_for_download(download_dir, timeout=60)
    assert os.path.exists(downloaded), "Downloaded file not found"
    print("Downloaded file:", downloaded)
    
    url = driver.current_url
    driver.get(url)
    print("Launched URL:", driver.current_url)           # appears in console with pytest -s
    driver.save_screenshot("screenshots/last_page.png")  # capture visible browser for verification