using FlightTicketApp.API.Controllers;
using FlightTicketApp.API.Services;
using FlightTicketApp.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FlightTicketApp.Tests
{
    public class FlightsControllerTests
    {
        private readonly Mock<IFlightService> _mockFlightService;
        private readonly FlightsController _controller;

        public FlightsControllerTests()
        {
            _mockFlightService = new Mock<IFlightService>();
            _controller = new FlightsController(_mockFlightService.Object);
        }

        [Fact]
        public async Task Search_EmptyOriginOrDestination_ReturnsBadRequest()
        {
            var request = new FlightSearchRequestDto
            {
                Origin = "",
                Destination = "AYT",
                DepartureDate = DateTime.Today
            };

            var result = await _controller.Search(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Kalkış ve varış noktaları zorunludur.", badRequestResult.Value);
        }

        [Fact]
        public async Task Search_SameOriginAndDestination_ReturnsBadRequest()
        {
            var request = new FlightSearchRequestDto
            {
                Origin = "IST",
                Destination = "IST",
                DepartureDate = DateTime.Today
            };

            var result = await _controller.Search(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Kalkış ve varış havalimanı aynı olamaz.", badRequestResult.Value);
        }

        [Fact]
        public async Task Search_ReturnDateBeforeDepartureDate_ReturnsBadRequest()
        {
            var request = new FlightSearchRequestDto
            {
                Origin = "IST",
                Destination = "AYT",
                DepartureDate = DateTime.Today.AddDays(5),
                ReturnDate = DateTime.Today.AddDays(2) 
            };

            var result = await _controller.Search(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Dönüş tarihi, gidiş tarihinden önce olamaz.", badRequestResult.Value);
        }

        [Fact]
        public async Task Search_ValidRequest_ReturnsOkWithData()
        {
            var request = new FlightSearchRequestDto
            {
                Origin = "IST",
                Destination = "AYT",
                DepartureDate = DateTime.Today.AddDays(2)
            };

            var expectedResponse = new FlightSearchResponseDto
            {
                HasError = false,
                OutboundFlights = new List<FlightDto> { new FlightDto { FlightNumber = "TK123" } }
            };

            _mockFlightService.Setup(s => s.SearchFlightsAsync(request)).ReturnsAsync(expectedResponse);

            var result = await _controller.Search(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<FlightSearchResponseDto>(okResult.Value);

            Assert.False(returnValue.HasError);
            Assert.Single(returnValue.OutboundFlights);
            Assert.Equal("TK123", returnValue.OutboundFlights[0].FlightNumber);
        }
    }
}