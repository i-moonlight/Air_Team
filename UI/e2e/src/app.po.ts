import { browser, by, element } from 'protractor';

export class AppPage {
  navigateTo(): Promise<unknown> {
    return browser.get(browser.baseUrl) as Promise<unknown>;
  }

  getButtonText(): Promise<string> {
    return element(by.css('app-root .search-row button')).getText() as Promise<string>;
  }
}
