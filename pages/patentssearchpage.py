from selenium.webdriver.common.by import By
from .base_page import BasePage

class PatentsSearchPage(BasePage):
    SEARCH_INPUT = (By.NAME, "q")
    PATENT_RESULT_XPATH = "//a[contains(@href, '/patent/')]"

    def search(self, query: str):
        self.send_keys(*self.SEARCH_INPUT, text=query)
        self.find(*self.SEARCH_INPUT).submit()

    def open_first_result(self):
        self.click(By.XPATH, self.PATENT_RESULT_XPATH)