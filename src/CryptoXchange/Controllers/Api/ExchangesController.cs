using Microsoft.AspNetCore.Mvc;

using CryptoXchange.Models;
using CryptoXchange.Services;

namespace CryptoXchange.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/Exchanges")]
    public class ExchangesController : Controller
    {
        private readonly ExchangeService _exchangeService;

        public ExchangesController(ExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        // GET: 
        [HttpGet("{symbol}")]
        public IActionResult Get(string symbol)
        {
            return Ok(_exchangeService.GetTransferRequestForSymbol(symbol).Result);
        }

        // GET: 
        [HttpPost("transfer")]
        public IActionResult Transfer(TransferModel request)
        {
            //ExchangeModel model = GenerateExchangeModel();
            return Ok(_exchangeService.HandleTransfer(request).Result);
        }
    }
}