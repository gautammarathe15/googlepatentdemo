import os
import tempfile
import shutil
import pytest
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from webdriver_manager.chrome import ChromeDriverManager

@pytest.fixture(scope="session")
def download_dir():
    d = os.path.join(os.getcwd(), "downloads")
    os.makedirs(d, exist_ok=True)
    yield d
    # Cleanup optional: keep files for inspection
    # shutil.rmtree(d, ignore_errors=True)

@pytest.fixture
def driver(download_dir, request):
    chrome_options = Options()
    prefs = {
        "download.default_directory": download_dir,
        "download.prompt_for_download": False,
        "plugins.always_open_pdf_externally": True,
        "profile.default_content_settings.popups": 0,
        "safebrowsing.enabled": True,
    }
    chrome_options.add_experimental_option("prefs", prefs)
    # Optionally enable headless via pytest - add CLI flag and check here
    # chrome_options.add_argument("--headless=new")
    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)
    driver.maximize_window()
    yield driver
    driver.quit()