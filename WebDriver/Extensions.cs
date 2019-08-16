using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Threading;

namespace FunkyBDD.SxS.Selenium.WebDriver
{
    public static class Extensions
    {
        /// <summary>
        ///     Get the element matching the current by criteria
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="by">Selenium By selector</param>
        /// <param name="explicitWait">Timeout in seconds to set explicitWait to find the element, default 5</param>
        /// <returns>First matching IWebElement or Null</returns>
        public static IWebElement FindElementFirstOrDefault(this IWebDriver driver, By by, int explicitWait = 5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(explicitWait));
            IWebElement result;

            try
            {
                result = wait.Until(
                    d => {
                        try
                        {
                            return driver.FindElement(by);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                );
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        ///     Get the elements matching the current by criteria
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="by">Selenium By selector</param>
        /// <param name="explicitWait">Timeout in seconds to set explicitWait to find the element, default 5</param>
        /// <returns>ReadOnlyCollection<IWebElement> or Null</returns>
        public static ReadOnlyCollection<IWebElement> FindElementsOrDefault(this IWebDriver driver, By by, int explicitWait = 5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(explicitWait));
            ReadOnlyCollection<IWebElement> result;

            try
            {
                result = wait.Until(
                    d => {
                        try
                        {
                            var tResult = driver.FindElements(by);
                            if (tResult.Count == 0)
                            {
                                return null;
                            }
                            else
                            {
                                return tResult;
                            }
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                );
            }
            catch (Exception)
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        ///     Get current ScrollYPosition
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <returns>Offset Y as integer</returns>
        public static int GetScrollPosition(this IWebDriver driver)
        {
            var scriptExecutor = (IJavaScriptExecutor)driver;
            return Convert.ToInt32(scriptExecutor.ExecuteScript("return window.pageYOffset;"));
        }

        /// <summary>
        ///     Navigates to the relative path based on the current URL.
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="path">path relative to URL</param>
        public static void NavigateToPath(this IWebDriver driver, string path)
        {
            var baseUrl = new Uri(driver.Url).GetLeftPart(UriPartial.Authority);
            var newUrl = baseUrl + path;
            driver.Navigate().GoToUrl(newUrl);
        }

        /// <summary>
        ///     Returns a partial screenshot of the passed element 
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="element">The IWebElement from which a screenshot is to be taken.</param>
        /// <param name="withScrolling">normally you want to scroll to the element and take a screenshot from there... but maybe you dont want it...</param>
        /// <returns>A bitmap with the partial screenshot</returns>
        public static Bitmap GetElementScreenshot(this IWebDriver driver, IWebElement element, bool withScrolling = true)
        {
            Bitmap returnImage;
            var platform = (string)((RemoteWebDriver)driver).Capabilities.GetCapability("platformName");
            var browserName = (string)((RemoteWebDriver)driver).Capabilities.GetCapability("browserName");
            var executor = (IJavaScriptExecutor)driver;

            #region Element location with Safari Hack
            int x;
            int y;
            try
            {
                x = element.Location.X;
            }
            catch (Exception)
            {
                x = ((RemoteWebElement)element).LocationOnScreenOnceScrolledIntoView.X;
            }
            try
            {
                y = element.Location.Y;
            }
            catch (Exception)
            {
                y = ((RemoteWebElement)element).LocationOnScreenOnceScrolledIntoView.Y;
            }
            #endregion

            var yOffset = 0;
            /* IE generate FullPageScreenshots so no offset and no scrolling will needed */
            if (browserName != "internet explorer")
            {
                if (withScrolling)
                {
                    executor.ExecuteScript($"window.scroll(0, {y}); ");
                }

                Thread.Sleep(1000);
                yOffset = Convert.ToInt32(executor.ExecuteScript($"var doc = document.documentElement; return (window.pageYOffset || doc.scrollTop)  - (doc.clientTop || 0);"));
            }

            Screenshot sc = ((ITakesScreenshot)driver).GetScreenshot();
            var img = Image.FromStream(new MemoryStream(sc.AsByteArray)) as Bitmap;

            #region Fixes for mobile devices
            /* The screenshots of Browserstack are much too big and have to be reduced. */
            if (platform == "iOS" || platform == "Android")
            {
                var screenWidth = Convert.ToInt32(executor.ExecuteScript($"return window.innerWidth;"));
                var screenHeight = Convert.ToInt32(executor.ExecuteScript($"return window.innerHeight;"));
                img = new Bitmap(img, new Size(screenWidth, screenHeight));
            }
            /* iOS and Android needs different offsets */
            switch (platform)
            {
                case "iOS":
                    yOffset = yOffset - 50;
                    break;
                case "Android":
                    yOffset = yOffset + 20;
                    break;
            }
            #endregion

            try
            {
                returnImage = img.Clone(new Rectangle(x, (y - yOffset), element.Size.Width - 5, element.Size.Height - 5), img.PixelFormat);
            }
            catch (Exception)
            {
                if (platform == "iOS")
                {
                    returnImage = img.Clone(new Rectangle(0, 80, img.Size.Width, img.Size.Height - 140), img.PixelFormat);
                }
                else
                {
                    returnImage = img;
                }
            }
            return returnImage;
        }

        /// <summary>
        ///     Set the browser size for responsive testing
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="width">Width in pixels. Default 750</param>
        /// <param name="height">Height in pixels. Default 750</param>
        public static void SetMobileSize(this IWebDriver driver, int width = 640, int height = 1024)
        {
            try
            {
                driver.Manage().Window.Size = new Size(width, height);
            }
            catch (Exception)
            {
                // catch the exception on mobile devices
            }
        }

        /// <summary>
        ///     Make a screenshot and safe it into the folder "SeleniumResults"
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="fileName">Filename of the screenshot</param>
        /// <param name="artifactDirectory">Path of the artifact directory</param>
        public static void MakeScreenshot(this IWebDriver driver, string fileName, string artifactDirectory = null)
        {
            Thread.Sleep(1000);
            if(artifactDirectory == null)
            {
                artifactDirectory = Directory.GetCurrentDirectory(); 
            }
            artifactDirectory = Path.GetFullPath(Path.Combine(artifactDirectory, $"TestResults"));

            if (!Directory.Exists(artifactDirectory))
            {
                Directory.CreateDirectory(artifactDirectory);
            }

            var screenshotFilePath = Path.Combine(artifactDirectory, fileName);

            try
            {
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotFilePath, ScreenshotImageFormat.Png);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        ///     Execute a JavaScript into the actual browser
        /// </summary>
        /// <param name="driver">Selenium IWebDriver reference</param>
        /// <param name="script">The javascript</param>
        public static void ExecuteScript(this IWebDriver driver, string script, IWebElement element = null)
        {
            var scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(script, element);
        }

        /// <summary>
        ///     Set a JavaScript property selenium = true
        /// </summary>
        /// <param name="driver"></param>
        public static void SetSeleniumFlag(this IWebDriver driver)
        {
            driver.ExecuteScript("selenium = true;");
        }
    }
}
