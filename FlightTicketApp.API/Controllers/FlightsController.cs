using FlightTicketApp.Shared.DTOs;
using FlightTicketApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightTicketApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] FlightSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
                return BadRequest("Kalkış ve varış noktaları zorunludur.");

            if (request.Origin == request.Destination)
                return BadRequest("Kalkış ve varış havalimanı aynı olamaz.");

            if (request.DepartureDate.Date < DateTime.Today)
                return BadRequest("Geçmiş bir tarihe uçuş aranamaz.");

            if (request.ReturnDate.HasValue && request.ReturnDate.Value.Date < request.DepartureDate.Date)
                return BadRequest("Dönüş tarihi, gidiş tarihinden önce olamaz.");

            var result = await _flightService.SearchFlightsAsync(request);
            return Ok(result);
        }
    }
}