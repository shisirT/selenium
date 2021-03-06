using System;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace OpenQA.Selenium
{
    [TestFixture]
    public class ImplicitWaitTest : DriverTestFixture
    {
        [TearDown]
        public void ResetImplicitWaitTimeout()
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);
        }

        [Test]
        public void ShouldImplicitlyWaitForASingleElement()
        {
            driver.Url = dynamicPage;
            IWebElement add = driver.FindElement(By.Id("adder"));

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(3000);

            add.Click();
            driver.FindElement(By.Id("box0"));  // All is well if this doesn't throw.
        }

        [Test]
        public void ShouldStillFailToFindAnElementWhenImplicitWaitsAreEnabled()
        {
            driver.Url = dynamicPage;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
            Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("box0")));
        }

        [Test]
        [NeedsFreshDriver]
        public void ShouldReturnAfterFirstAttemptToFindOneAfterDisablingImplicitWaits()
        {
            driver.Url = dynamicPage;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(3000);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);
            Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("box0")));
        }

        [Test]
        [NeedsFreshDriver]
        public void ShouldImplicitlyWaitUntilAtLeastOneElementIsFoundWhenSearchingForMany()
        {
            driver.Url = dynamicPage;
            IWebElement add = driver.FindElement(By.Id("adder"));

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(2000);
            add.Click();
            add.Click();

            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.ClassName("redbox"));
            Assert.GreaterOrEqual(elements.Count, 1);
        }

        [Test]
        [NeedsFreshDriver]
        public void ShouldStillFailToFindElementsWhenImplicitWaitsAreEnabled()
        {
            driver.Url = dynamicPage;

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.ClassName("redbox"));
            Assert.AreEqual(0, elements.Count);
        }

        [Test]
        [NeedsFreshDriver]
        public void ShouldReturnAfterFirstAttemptToFindManyAfterDisablingImplicitWaits()
        {
            driver.Url = dynamicPage;
            IWebElement add = driver.FindElement(By.Id("adder"));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1100);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);
            add.Click();
            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.ClassName("redbox"));
            Assert.AreEqual(0, elements.Count);
        }

        [Test]
        [IgnoreBrowser(Browser.IE)]
        [IgnoreBrowser(Browser.Safari)]
        public void ShouldImplicitlyWaitForAnElementToBeVisibleBeforeInteracting()
        {
            driver.Url = dynamicPage;

            IWebElement reveal = driver.FindElement(By.Id("reveal"));
            IWebElement revealed = driver.FindElement(By.Id("revealed"));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);

            Assert.IsFalse(revealed.Displayed, "revealed should not be visible");
            reveal.Click();

            try
            {
                revealed.SendKeys("hello world");
                // This is what we want
            }
            catch (ElementNotVisibleException)
            {
                Assert.Fail("Element should have been visible");
            }
        }

        [Test]
        [IgnoreBrowser(Browser.Safari)]
        public void ShouldRetainImplicitlyWaitFromTheReturnedWebDriverOfWindowSwitchTo()
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            driver.Url = xhtmlTestPage;
            driver.FindElement(By.Name("windowOne")).Click();

            string originalHandle = driver.CurrentWindowHandle;
            WaitFor(() => driver.WindowHandles.Count == 2, "Window handle count was not 2");
            List<string> handles = new List<string>(driver.WindowHandles);
            handles.Remove(originalHandle);

            IWebDriver newWindow = driver.SwitchTo().Window(handles[0]);

            DateTime start = DateTime.Now;
            newWindow.FindElements(By.Id("this-crazy-thing-does-not-exist"));
            DateTime end = DateTime.Now;
            TimeSpan time = end - start;

            driver.Close();
            driver.SwitchTo().Window(originalHandle);
            Assert.IsTrue(time.TotalMilliseconds >= 1000);
        }
    }
}
