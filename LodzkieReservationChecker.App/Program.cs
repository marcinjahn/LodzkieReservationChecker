// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

const string URL = "https://www.rezerwacje.lodzkie.eu/";
var DELAY = TimeSpan.FromMinutes(10);
const string SOUND_FILE = "C:\\Users\\plmajah\\Music\\Not-That-Weak-4.mp3";

new DriverManager().SetUpDriver(new ChromeConfig());
try
{
    Console.WriteLine($"Trying to find a free slot at {URL}");
    
    while (true)
    {
        using var driver = GetDriver();
        try
        {
            Console.WriteLine($"Starting to check at {DateTime.Now}");
            if (LookForAvailability(driver))
            {
                Console.WriteLine("Found it!");
                await Alert();
                break;
            }
            Console.WriteLine($"Check completed at {DateTime.Now}");
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception throws:");
            Console.WriteLine(e.Message);
            Console.WriteLine();
        }
        finally
        {
            driver?.Quit();
        }
        await Task.Delay(DELAY);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

WebDriver GetDriver()
{    
    var options = new ChromeOptions();
    options.AddArgument("--disable-logging");
    options.AddArgument("--disable-dev-shm-usage");
    options.AddArgument("--log-level=3");
    options.AddArgument("--output=/dev/null");
    options.AddArgument("--disable-extensions");
    options.AddArgument("--disable-crash-reporter");
    return new ChromeDriver(options);
}

bool LookForAvailability(WebDriver driver)
{
    driver.Navigate().GoToUrl(URL);
    Wait(driver, () => driver.FindElement(By.ClassName("queue-button")));
    var button1 = FindButton(driver, "queue-button",
        "POBYT CZASOWY I STAŁY (STUDENCI, MAŁŻEŃSTWA, INNE OKOLICZNOŚCI) - WGLĄD W SPRAWĘ");
    button1.Click();

    Wait(driver, () =>
    {
        var buttons = driver.FindElements(By.ClassName("queue-button"));
        return buttons.Count == 2 ? buttons.First() : null;
    });

    var button2 = FindButton(driver, "queue-button",
        "WYDZIAŁ SPRAW OBYWATELSKICH");
    button2.Click();

    Wait(driver, () => driver.FindElement(By.TagName("td")));

    var availableDate = GetAvailableDate(driver);

    if (!availableDate)
    {
        SwitchMonth(driver);
        Thread.Sleep(2000); // calendar animation
        availableDate = GetAvailableDate(driver);
    }

    return availableDate;
}

void Wait(WebDriver driver, Func<IWebElement> func)
{
    var wait = new WebDriverWait(driver, timeout: TimeSpan.FromSeconds(30))
    {
        PollingInterval = TimeSpan.FromSeconds(2)
    };
    wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

    wait.Until(drv =>
    {
        try
        {
            return func();
        }
        catch (NoSuchElementException)
        {
        }

        return null;
    });

    Thread.Sleep(2000);
}

IWebElement FindButton(WebDriver driver, string className, string text)
{
    return driver
        .FindElements(By.ClassName(className))
        .First(b => b.Text.Contains(text));
}

bool GetAvailableDate(WebDriver driver)
{
    var tds = driver
        .FindElements(By.TagName("td"));

    return tds
        .FirstOrDefault(b => 
        {
            try {
                var button = b.FindElement(By.TagName("button"));
                return button?.Enabled == true;
            }
            catch(Exception)
            {
                return false;
            }
        }) is not null;
}

void SwitchMonth(WebDriver driver)
{
    var buttons = driver
        .FindElement(By.ClassName("v-date-picker-header"))
        .FindElements(By.TagName("button"));
    buttons[2].Click();
}

async Task Alert()
{
    var services = new ServiceCollection();
    services.AddNodeServices();

    var provider = services.BuildServiceProvider();

    var service = provider.GetRequiredService<INodeServices>();
    await service.InvokeExportAsync<string>("./node/play", "play", SOUND_FILE);
}
