using FlightTicketApp.Shared.DTOs;

namespace FlightTicketApp.API.Services
{
    public interface IFlightService
    {
        Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request);
        Task<List<AirportDto>> GetAirportsAsync();
    }
}
