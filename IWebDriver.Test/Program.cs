﻿using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;

namespace FunkyBDD.SxS.Selenium.WebDriver.Test
{
    class Program
    {
        public static IWebDriver Driver;

        static void Main(string[] args)
        {
            #region init browser
                var firefoxOptions = new FirefoxOptions();
                firefoxOptions.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Server, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Client, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                firefoxOptions.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                firefoxOptions.LogLevel = FirefoxDriverLogLevel.Error;
                firefoxOptions.AcceptInsecureCertificates = true;
                firefoxOptions.AddArguments("-purgecaches", "-private", "--disable-gpu", "--disable-direct-write", "--disable-display-color-calibration", "--allow-http-screen-capture", "--disable-accelerated-2d-canvas");
                Driver = new FirefoxDriver("./", firefoxOptions, TimeSpan.FromSeconds(120));
                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            #endregion

            Driver.Navigate().GoToUrl("https://www.swisslife.ch/de/private.html/");

            /* Find the element inside the driver safe without exception */
                var labelToScroll = Driver.FindElementFirstOrDefault(By.CssSelector("h3.a-heading--style-italic"), 5);
                Console.WriteLine(labelToScroll.Text);

            /* Scroll to the element and take a screenshot of the element */
                var img = Driver.GetElementScreenshot(labelToScroll);
                Console.WriteLine($"The screenshot is {img.Height} pixels height");

            /* Find all elementS inside the parent safe without exception */
                var labels = Driver.FindElementsOrDefault(By.TagName("h3"));
                Console.WriteLine($"{labels.Count} labels of h3 found");

            /* Real example, accept the disclaimer, if it exists */
                var disclaimerButton = Driver.FindElementFirstOrDefault(By.CssSelector("[class*='cookie-disclaimer']>button"), 1);
                if (disclaimerButton != null)
                {
                    disclaimerButton.Click();
                }

            /* Search a not existing element inside the parent, should by null after 5 seconds */
                var notFound = Driver.FindElementFirstOrDefault(By.TagName("h99"), 5);
                Console.WriteLine($"Element h99 is {notFound}");

            /* The position from top */
                var y = Driver.GetScrollPosition();
                Console.WriteLine($"You are {y} pixels from top");

            /* Navigate relative */
                Driver.NavigateToPath("/de/private/kontakt-service/persoenliche-services.html");
                Console.WriteLine($"Your are here: {Driver.Title}");

            /* Resize the browser */
                Driver.SetMobileSize();

            /* Make a screenshot of the whole browser and safe it to bin/testresults folder */
                Driver.MakeScreenshot("swisslife.png");

            /* Set Selenium flag */
                Driver.SetSeleniumFlag();

            /* Execute a JavaScript */
                Driver.ExecuteScript("alert('FunkyBDD');");

            #region console handling
                Console.WriteLine(" ");
                Console.WriteLine("Press enter to terminate...");
                Console.ReadLine();
            #endregion

            #region teardown browser
                Driver.Close();
                Driver.Dispose();
                Driver.Quit();
            #endregion
        }
    }
}
