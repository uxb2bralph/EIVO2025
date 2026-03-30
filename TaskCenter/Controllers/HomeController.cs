using System.Diagnostics;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc;
using TaskCenter.Models;
using TaskCenter.Properties;

namespace TaskCenter.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public ActionResult ReloadSettings()
    {
        AppSettings.Reload();
        ModelCore.Properties.AppSettings.Reload();
        ModelExtension.Properties.AppSettings.Reload();
        CommonLib.Core.Properties.AppSettings.Reload();
        CommonLib.Logger.Properties.AppSettings.Reload();

        return Content(AppSettings.AllSettings.JsonStringify(), "application/json");
    }

    public ActionResult SaveSettings()
    {
        //AppSettings.SaveAll();
        AppSettings.Default.Save();
        ModelCore.Properties.AppSettings.Default.Save();
        ModelExtension.Properties.AppSettings.Default.Save();
        CommonLib.Core.Properties.AppSettings.Default.Save();
        CommonLib.Logger.Properties.AppSettings.Default.Save();
        return Content(AppSettings.AllSettings!.ToString(), "application/json");
    }
}
