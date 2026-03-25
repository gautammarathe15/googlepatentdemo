from selenium.webdriver.common.by import By
from .base_page import BasePage

class GoogleHomePage(BasePage):
    SEARCH_INPUT = (By.NAME, "q")
    COOKIE_BUTTON_XPATH = "//button[contains(., 'I agree') or contains(., 'Accept') or contains(., 'Agree')]"

    def open_home(self):
        self.open("https://www.google.com")

    def accept_cookies(self):
        try:
            self.click(By.XPATH, self.COOKIE_BUTTON_XPATH)
        except Exception:
            pass

    def search(self, query: str):
        self.send_keys(*self.SEARCH_INPUT, text=query)
        self.find(*self.SEARCH_INPUT).submit()