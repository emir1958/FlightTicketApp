namespace FlightTicketApp.Shared.DTOs
{
    public class FlightSearchRequestDto
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; } 
    }
}
