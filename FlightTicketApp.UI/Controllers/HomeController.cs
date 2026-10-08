using Microsoft.AspNetCore.Mvc;
using FlightTicketApp.UI.Models;
using FlightTicketApp.Shared.DTOs;
using System.Net.Http.Json;
using Polly.CircuitBreaker;
using Microsoft.AspNetCore.RateLimiting; 

namespace FlightTicketApp.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool rateLimitError = false) 
        {
            if (rateLimitError)
            {
                ViewBag.ErrorMessage = "Çok fazla arama yaptınız. Lütfen 10 saniye bekleyip tekrar deneyin.";
            }

            var client = _httpClientFactory.CreateClient("FlightApi");
            List<AirportDto> airports = new();

            try
            {
                airports = await client.GetFromJsonAsync<List<AirportDto>>("api/Airports") ?? new();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "API sunucusuna ulaşılamıyor. Lütfen uçuş servisinin ayakta olduğundan emin olun.";
            }

            var model = new FlightSearchViewModel
            {
                Airports = airports,
                DepartureDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(4)
            };

            return View(model);
        }

        [HttpPost]
        [EnableRateLimiting("SearchLimiter")]
        public async Task<IActionResult> Index(FlightSearchViewModel model)
        {
            var client = _httpClientFactory.CreateClient("FlightApi");
            client.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                model.Airports = await client.GetFromJsonAsync<List<AirportDto>>("api/Airports") ?? new();
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "API sunucusuna ulaşılamıyor. Lütfen uçuş servisinin çalıştığından emin olun.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Origin == model.Destination)
            {
                ModelState.AddModelError("", "Kalkış ve varış havalimanı aynı olamaz.");
                return View(model);
            }

            if (model.TripType == "oneWay")
            {
                model.ReturnDate = null;
            }

            var requestDto = new FlightSearchRequestDto
            {
                Origin = model.Origin,
                Destination = model.Destination,
                DepartureDate = model.DepartureDate,
                ReturnDate = model.ReturnDate
            };

            try
            {
                var response = await client.PostAsJsonAsync("api/Flights/search", requestDto);

                if (response.IsSuccessStatusCode)
                {
                    model.SearchResults = await response.Content.ReadFromJsonAsync<FlightSearchResponseDto>();

                    if (model.SearchResults != null && model.SearchResults.HasError)
                    {
                        ModelState.AddModelError("", model.SearchResults.ErrorMessage);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Arama servisi hata döndürdü: {errorContent}");
                }
            }
            catch (BrokenCircuitException)
            {
                ModelState.AddModelError("", "Sistemde yoğunluk veya bağlantı sorunu var. Lütfen 30 saniye sonra tekrar deneyin.");
            }
            catch (TaskCanceledException)
            {
                ModelState.AddModelError("", "Sunucu yanıt vermekte çok gecikti (Timeout). Lütfen tekrar deneyin.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Uçuş arama API'sine ulaşılamıyor. Sunucu bağlantısı reddedildi.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Beklenmeyen bir sistem hatası oluştu.");
            }

            return View(model);
        }
    }
}