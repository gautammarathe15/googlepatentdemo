from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.by import By

class BasePage:
    def __init__(self, driver, timeout: int = 20):
        self.driver = driver
        self.wait = WebDriverWait(driver, timeout)

    def open(self, url: str):
        self.driver.get(url)

    def find(self, by, locator):
        return self.wait.until(EC.presence_of_element_located((by, locator)))

    def click(self, by, locator):
        el = self.wait.until(EC.element_to_be_clickable((by, locator)))
        el.click()
        return el

    def send_keys(self, by, locator, text: str, clear_first: bool = True):
        el = self.find(by, locator)
        if clear_first:
            el.clear()
        el.send_keys(text)
        return el

    def find_all(self, by, locator):
        return self.driver.find_elements(by, locator)