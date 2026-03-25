import os
import time
import glob
from pathlib import Path

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager


def wait_for_download(download_dir: str, timeout: int = 60) -> str:
    """
    Wait until a .pdf appears in download_dir and any .crdownload files disappear.
    Returns the path to the downloaded file.
    Raises TimeoutError on timeout.
    """
    download_dir_path = Path(download_dir)
    end_time = time.time() + timeout
    last_size = -1
    candidate = None

    while time.time() < end_time:
        # find any pdfs
        pdfs = list(download_dir_path.glob("*.pdf"))
        crdownloads = list(download_dir_path.glob("*.crdownload"))

        if pdfs and not crdownloads:
            # if multiple, pick the most recent
            candidate = max(pdfs, key=lambda p: p.stat().st_mtime)
            # ensure stable size
            size = candidate.stat().st_size
            if size == last_size and size > 0:
                return str(candidate.resolve())
            last_size = size

        time.sleep(0.5)

    raise TimeoutError(f"No completed PDF found in '{download_dir}' after {timeout}s")


def main():
    download_dir = os.path.join(os.getcwd(), "downloads")
    os.makedirs(download_dir, exist_ok=True)

    prefs = {
        "download.default_directory": download_dir,
        "download.prompt_for_download": False,
        "plugins.always_open_pdf_externally": True,
        "profile.default_content_settings.popups": 0,
        "safebrowsing.enabled": True,
    }

    chrome_options = Options()
    chrome_options.add_experimental_option("prefs", prefs)
    # Do not run headless because headless Chrome has historically required extra setup for downloads.
    # chrome_options.add_argument("--headless=new")  # optional: only if you configure downloads for headless

    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)
    wait = WebDriverWait(driver, 20)

    try:
        # 1) Open Google
        driver.get("https://www.google.com")
        # accept cookie prompt if present (best-effort)
        try:
            accept = wait.until(EC.element_to_be_clickable((By.XPATH, "//button[contains(., 'I agree') or contains(., 'Accept')]")))
            accept.click()
        except Exception:
            pass

        # 2) Search for "google patents"
        q = wait.until(EC.presence_of_element_located((By.NAME, "q")))
        q.clear()
        q.send_keys("google patents")
        q.send_keys(Keys.ENTER)

        # 3) Click the official Google Patents result (href contains "patents.google")
        patents_link = wait.until(EC.element_to_be_clickable((By.XPATH, "//a[contains(@href, 'patents.google')]")))
        patents_link.click()

        # 4) Wait for Google Patents to load and search for "stent"
        # The patents search input typically has name 'q' as well
        search_input = wait.until(EC.presence_of_element_located((By.NAME, "q")))
        search_input.clear()
        search_input.send_keys("stent")
        search_input.send_keys(Keys.ENTER)

        # 5) Wait for search results and open the first patent result
        # Patent result links usually contain "/patent/" in the URL
        first_result = wait.until(EC.element_to_be_clickable((By.XPATH, "//a[contains(@href, '/patent/')]")))
        first_result.click()

        # 6) On the patent page, try to find a direct PDF link (href contains ".pdf")
        # Attempt multiple strategies, best-effort:
        pdf_url = None
        try:
            link = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//a[contains(@href, '.pdf')]"))
            )
            pdf_url = link.get_attribute("href")
            # click the link to trigger download (some links open in new tab)
            link.click()
        except Exception:
            # fallback: look for a button or menu labeled "Download PDF"
            try:
                download_button = driver.find_element(By.XPATH, "//button[contains(., 'Download') or contains(., 'PDF')]")
                download_button.click()
            except Exception:
                # last resort: try to open the viewer PDF by adding /pdf at end of URL
                current = driver.current_url
                if "/patent/" in current:
                    # try a common pattern: append "/pdf" (best-effort)
                    maybe_pdf = current.rstrip("/") + "/pdf"
                    driver.get(maybe_pdf)
                else:
                    raise RuntimeError("Could not locate a PDF link on the patent page")

        # 7) Wait for the download to complete and verify
        print(f"Waiting for PDF to appear in '{download_dir}' ...")
        downloaded_path = wait_for_download(download_dir, timeout=60)
        print(f"Download complete: {downloaded_path}")

    finally:
        driver.quit()


if __name__ == "__main__":
    main()

# ensure browser is visible (not headless) while debugging
from selenium import webdriver
from selenium.webdriver.chrome.options import Options

options = Options()
# comment out any headless flags while debugging:
# options.add_argument("--headless")
driver = webdriver.Chrome(options=options)
driver.get("https://example.com")
print("current_url:", driver.current_url)
driver.save_screenshot("screenshots/example.png")
driver.quit()