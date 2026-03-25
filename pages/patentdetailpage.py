from selenium.webdriver.common.by import By
from .base_page import BasePage
import time

class PatentDetailPage(BasePage):
    PDF_LINK_XPATH = "//a[contains(@href, '.pdf')]"
    DOWNLOAD_BUTTON_XPATH = "//button[contains(., 'Download') or contains(., 'PDF')]"

    def download_pdf(self):
        # 1) Try direct PDF link
        try:
            link = self.find(By.XPATH, self.PDF_LINK_XPATH)
            href = link.get_attribute("href")
            link.click()
            return href
        except Exception:
            pass

        # 2) Try download button
        try:
            self.click(By.XPATH, self.DOWNLOAD_BUTTON_XPATH)
            time.sleep(0.5)
            # maybe a new link appears
            try:
                link = self.find(By.XPATH, self.PDF_LINK_XPATH)
                href = link.get_attribute("href")
                link.click()
                return href
            except Exception:
                return None
        except Exception:
            pass

        # 3) Fallback: open /pdf path
        current = self.driver.current_url
        if "/patent/" in current:
            maybe_pdf = current.rstrip("/") + "/pdf"
            self.open(maybe_pdf)
            return maybe_pdf

        return None