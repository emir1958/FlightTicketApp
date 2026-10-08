using FlightSearchService;
using FlightTicketApp.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace FlightTicketApp.API.Services
{
    public class FlightService : IFlightService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FlightService> _logger; 

        public FlightService(IMemoryCache cache, IConfiguration configuration, ILogger<FlightService> logger)
        {
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        private static readonly List<AirportDto> Airports = new()
        {
            new AirportDto { Code = "IST", Name = "İstanbul Havalimanı", City = "İstanbul" },
            new AirportDto { Code = "SAW", Name = "Sabiha Gökçen Havalimanı", City = "İstanbul" },
            new AirportDto { Code = "ESB", Name = "Esenboğa Havalimanı", City = "Ankara" },
            new AirportDto { Code = "ADB", Name = "Adnan Menderes Havalimanı", City = "İzmir" },
            new AirportDto { Code = "AYT", Name = "Antalya Havalimanı", City = "Antalya" },
            new AirportDto { Code = "TZX", Name = "Trabzon Havalimanı", City = "Trabzon" }
        };

        public async Task<List<AirportDto>> GetAirportsAsync()
        {
            return await _cache.GetOrCreateAsync("AirportsList", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return Task.FromResult(Airports);
            });
        }

        public async Task<FlightSearchResponseDto> SearchFlightsAsync(FlightSearchRequestDto request)
        {
            var response = new FlightSearchResponseDto();
            var soapUrl = _configuration["SoapSettings:FlightProviderUrl"] ?? "https://localhost:5001/Service.svc";

            try
            {
                var outboundSoapRequest = new SearchRequest
                {
                    Origin = request.Origin,
                    Destination = request.Destination,
                    DepartureDate = request.DepartureDate
                };

                using var outboundClient = new AirSearchClient(AirSearchClient.EndpointConfiguration.BasicHttpBinding_IAirSearch, soapUrl);
                var outboundTask = outboundClient.AvailabilitySearchAsync(outboundSoapRequest);

                Task<SearchResult>? inboundTask = null;
                AirSearchClient? inboundClient = null;

                if (request.ReturnDate.HasValue)
                {
                    var inboundSoapRequest = new SearchRequest
                    {
                        Origin = request.Destination,
                        Destination = request.Origin,
                        DepartureDate = request.ReturnDate.Value
                    };
                    inboundClient = new AirSearchClient(AirSearchClient.EndpointConfiguration.BasicHttpBinding_IAirSearch, soapUrl);
                    inboundTask = inboundClient.AvailabilitySearchAsync(inboundSoapRequest);
                }

                if (inboundTask != null)
                {
                    await Task.WhenAll(outboundTask, inboundTask);
                }
                else
                {
                    await outboundTask;
                }

                var outboundResult = await outboundTask;

                if (outboundResult.HasError)
                {
                    response.HasError = true;
                    response.ErrorMessage = "Gidiş uçuşları aranırken sağlayıcıdan bir hata döndü.";
                    return response;
                }

                if (outboundResult.FlightOptions != null)
                {
                    response.OutboundFlights = outboundResult.FlightOptions.Select(f => new FlightDto
                    {
                        FlightNumber = f.FlightNumber,
                        DepartureDateTime = f.DepartureDateTime,
                        ArrivalDateTime = f.ArrivalDateTime,
                        Price = f.Price,
                        Origin = request.Origin,
                        Destination = request.Destination
                    }).ToList();
                }

                if (inboundTask != null)
                {
                    var inboundResult = await inboundTask;
                    inboundClient?.Close();

                    if (inboundResult.HasError)
                    {
                        response.HasError = true;
                        response.ErrorMessage = "Dönüş uçuşları aranırken sağlayıcıdan bir hata döndü.";
                        return response;
                    }

                    if (inboundResult.FlightOptions != null)
                    {
                        response.InboundFlights = inboundResult.FlightOptions.Select(f => new FlightDto
                        {
                            FlightNumber = f.FlightNumber,
                            DepartureDateTime = f.DepartureDateTime,
                            ArrivalDateTime = f.ArrivalDateTime,
                            Price = f.Price,
                            Origin = request.Destination,
                            Destination = request.Origin
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP servisi ile iletişim kurulurken bir hata oluştu.");
                response.HasError = true;
                response.ErrorMessage = "Uçuş sağlayıcısı ile iletişim kurulamadı. Lütfen daha sonra tekrar deneyiniz.";
            }

            return response;
        }
    }
}