using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using CryptoXchange.Models;
using CryptoXchange.Infrastructure;

namespace CryptoXchange.Controllers
{
    public class HomeController : Controller
    {

        private readonly IContextHolder _contextHolder;

        public HomeController(IContextHolder contextHolder)
        {
            _contextHolder = contextHolder;
        }

        public IActionResult Index()
        {
            TransferModel model = new TransferModel();

            if (null != _contextHolder.ExchangeRate)
                model.ExchangeRate = _contextHolder.ExchangeRate.Bid;

            model.ExchangeValue = _contextHolder.Config.ExchangeValue;

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
