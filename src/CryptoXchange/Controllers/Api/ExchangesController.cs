using Microsoft.AspNetCore.Mvc;

using CryptoXchange.Models;
using CryptoXchange.Services;

namespace CryptoXchange.Controllers.Api
{
    [Route("api/[controller]")]
    public class ExchangesController : Controller
    {
        private readonly ExchangeService _exchangeService;

        public ExchangesController(ExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return Ok(_exchangeService.GetTransferRequestForSymbol(id).Result);
        }

        [HttpPost]
        public IActionResult Transfer([FromBody]TransferModel request)
        {
            return Ok(_exchangeService.HandleTransfer(request).Result);
        }
    }
}