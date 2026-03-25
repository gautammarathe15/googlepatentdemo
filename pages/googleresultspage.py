from selenium.webdriver.common.by import By
from .base_page import BasePage

class GoogleResultsPage(BasePage):
    # result links with href containing patents.google
    PATENTS_LINK_XPATH = "//a[contains(@href, 'patents.google')]"

    def click_google_patents_result(self):
        self.click(By.XPATH, self.PATENTS_LINK_XPATH)