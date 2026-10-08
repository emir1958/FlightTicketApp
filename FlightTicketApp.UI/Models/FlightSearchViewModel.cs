using FlightTicketApp.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace FlightTicketApp.UI.Models
{
    public class FlightSearchViewModel
    {
        [Required(ErrorMessage = "Lütfen kalkış havalimanı seçiniz.")]
        public string? Origin { get; set; }

        [Required(ErrorMessage = "Lütfen varış havalimanı seçiniz.")]
        public string? Destination { get; set; }

        [Required(ErrorMessage = "Lütfen gidiş tarihi seçiniz.")]
        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string TripType { get; set; } = "round";

        public List<AirportDto> Airports { get; set; } = new();

        public FlightSearchResponseDto? SearchResults { get; set; }
    }
}