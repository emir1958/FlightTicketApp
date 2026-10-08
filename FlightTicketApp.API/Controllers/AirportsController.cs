using FlightTicketApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightTicketApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirportsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public AirportsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAirports()
        {
            var airports = await _flightService.GetAirportsAsync();
            return Ok(airports);
        }
    }
}