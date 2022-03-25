// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

const string URL = "https://www.rezerwacje.lodzkie.eu/";
var DELAY = TimeSpan.FromMinutes(10);
const string SOUND_FILE = "/Users/marcin/Music/Not-That-Weak-4.mp3";

using var driver = GetDriver();
try
{
    Console.WriteLine($"Trying to find a free slot at {URL}");
    
    while (true)
    {
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
        await Task.Delay(DELAY);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
finally
{
    driver?.Quit();
}

WebDriver GetDriver()
{
    new DriverManager().SetUpDriver(new ChromeConfig());
    
    var options = new ChromeOptions();
    return new ChromeDriver(options);
}

bool LookForAvailability(WebDriver webDriver)
{
    webDriver.Navigate().GoToUrl(URL);
    Wait(() => webDriver.FindElement(By.ClassName("queue-button")));
    var button1 = FindButton(webDriver, "queue-button",
        "POBYT CZASOWY I STAŁY (STUDENCI, MAŁŻEŃSTWA, INNE OKOLICZNOŚCI) - WGLĄD W SPRAWĘ");
    button1.Click();

    Wait(() =>
    {
        var buttons = webDriver.FindElements(By.ClassName("queue-button"));
        return buttons.Count == 2 ? buttons.First() : null;
    });

    var button2 = FindButton(webDriver, "queue-button",
        "WYDZIAŁ SPRAW OBYWATELSKICH");
    button2.Click();

    Wait(() => webDriver.FindElement(By.TagName("td")));

    var availableDate = GetAvailableDate(webDriver);

    if (availableDate is null)
    {
        SwitchMonth(webDriver);
        Thread.Sleep(2000); // calendar animation
        availableDate = GetAvailableDate(webDriver);
    }

    return availableDate is not null;
}

void Wait(Func<IWebElement> func)
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
}

IWebElement FindButton(WebDriver driver, string className, string text)
{
    return driver
        .FindElements(By.ClassName(className))
        .First(b => b.Text.Contains(text));
}

IWebElement GetAvailableDate(WebDriver driver)
{
    return driver
        .FindElements(By.TagName("td"))
        .FirstOrDefault(b => b.FindElement(By.TagName("button")).Enabled);
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
