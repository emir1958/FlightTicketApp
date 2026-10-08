using CoreWCF;
using System.Runtime.Serialization;

namespace FlightProvider
{
    [DataContract]
    public class SearchRequest
    {
        [DataMember]
        public string Origin { get; set; } = string.Empty;
        [DataMember]
        public string Destination { get; set; } = string.Empty;
        [DataMember]
        public DateTime DepartureDate { get; set; }

    }


    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public bool HasError { get; set; }
        [DataMember]
        public List<FlightOption> FlightOptions { get; set; } = new List<FlightOption>();

    }

    public class FlightOption
    {
        [DataMember]
        public string FlightNumber { get; set; } = string.Empty;
        [DataMember]
        public DateTime DepartureDateTime { get; set; }
        [DataMember]
        public DateTime ArrivalDateTime { get; set; }
        [DataMember]
        public decimal Price { get; set; }

    }


    [ServiceContract]
    public interface IAirSearch
    {
        [OperationContract]
        SearchResult AvailabilitySearch(SearchRequest request);
    }


    public class AirSearch : IAirSearch
    {
        public SearchResult AvailabilitySearch(SearchRequest request)
        {
            int hour = 1;
            return new SearchResult
            {
                HasError = false,
                FlightOptions = new List<FlightOption>
                {
                    new FlightOption {
                        ArrivalDateTime= request.DepartureDate.AddHours(hour++),
                        DepartureDateTime =request.DepartureDate.AddHours(hour++),
                        FlightNumber = $"TK{hour}",
                        Price = 100.0M + hour*10,
                    },
                    new FlightOption {
                        ArrivalDateTime= request.DepartureDate.AddHours(hour++),
                        DepartureDateTime =request.DepartureDate.AddHours(hour++),
                        FlightNumber = $"TK{hour}",
                        Price = 100.0M + hour*10,
                    },
                    new FlightOption {
                        ArrivalDateTime= request.DepartureDate.AddHours(hour++),
                        DepartureDateTime =request.DepartureDate.AddHours(hour++),
                        FlightNumber = $"TK{hour}",
                        Price = 100.0M + hour*10,
                    },
                }
            };
        }
    }
}
