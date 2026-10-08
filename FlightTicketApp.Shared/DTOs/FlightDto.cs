namespace FlightTicketApp.Shared.DTOs
{
    public class FlightDto
    {
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public decimal Price { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
    }

    public class FlightSearchResponseDto
    {
        public List<FlightDto> OutboundFlights { get; set; } = new(); 
        public List<FlightDto> InboundFlights { get; set; } = new();  
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
