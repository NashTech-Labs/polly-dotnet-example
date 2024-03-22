using Microsoft.AspNetCore.Mvc;
using PollyExample.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PollyExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        readonly APIService _service;

        public APIController(APIService service)
        {
            _service = service;
        }

        [HttpGet("/test/retry")]
        public async Task<IEnumerable<string>> Get()
        {
            var data = await _service.GetDataFromTestApi();
            return new string[] { data.ToString() };
        }

        [HttpGet("/test/circuitbreaker")]
        public async Task<string> TestCircuitBreaker()
        {
            var data = await _service.GetDataForCercuitBreaker();
            return data;
        }
    }
}
