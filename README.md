# Uçak Bileti Satış Uygulaması

Bu proje, acente arayüzü (UI), uçuş sağlayıcısı ile haberleşen arka uç servisi (REST API) ve paylaşılan sözleşmelerden (Contracts/Shared DTO) oluşan dağıtık bir mimaride geliştirilmiştir. Geliştirici deneyimini (DX) artırmak amacıyla, harici uçuş sağlayıcısı SOAP servisi (`FlightProvider`) de bağımsız bir kurulum gerektirmemesi için ana çözüm (Solution) içerisine dahil edilmiştir.

## Mimari Yapı

1. **`FlightTicketApp.API` (REST Backend):**
   * Verilen `FlightProvider` SOAP servisini (CoreWCF) arka planda tüketir.
   * Dış dünyaya RESTful endpoint'ler (`/api/Airports`, `/api/Flights/search`) sunar.
   * Gidiş ve dönüş uçuşlarını eşzamanlı (`Task.WhenAll`) sorgulayarak yanıt süresini optimize eder.
   * Statik referans verileri (Havalimanları) için `IMemoryCache` stratejisi uygulanmıştır.

2. **`FlightTicketApp.UI` (MVC Client):**
   * İş kurallarından arındırılmış, yalnızca REST API'yi tüketen bağımsız istemcidir.
   * `IHttpClientFactory` kullanır.
   * Ağ dalgalanmaları ve servis kesintilerine karşı **Polly** ile **Retry (Tekrar Deneme)** ve **Circuit Breaker (Hata Koruması / Şalter)** desenleri uygulanmıştır.
   * Kötü niyetli botlara, veri kazıma (scraping) girişimlerine ve aşırı ağ yüküne karşı .NET'in yerleşik **Rate Limiting (Hız Sınırlandırıcı)** mekanizması entegre edilmiştir.

3. **`FlightTicketApp.Shared` (Domain Contracts):**
   * UI ile API arasında ortak kullanılan DTO'ları barındırır.
   * UI'ın API katmanını doğrudan derlemesini engelleyerek bağımlılık sızıntısını ortadan kaldırır.

4. **`FlightTicketApp.Tests` (Unit Tests):**
   * Controller düzeyinde rota kuralları (Origin != Destination), tarih mantığı (ReturnDate >= DepartureDate) ve veri akışını xUnit ve Moq ile doğrular, sistemi regresyona karşı korur.

---

## Kurulum ve Çalıştırma Talimatı (ÖNEMLİ)

Sistemin uçtan uca çalışabilmesi için projeye dahil edilen SOAP servisinin, REST API'nin ve Web UI projesinin eşzamanlı çalışması gerekmektedir. Dışarıdan ekstra bir servis ayağa kaldırmanıza gerek yoktur.

### Visual Studio Üzerinden Başlatma:
1. Solution Explorer'da en üstteki **Solution 'FlightTicketApp'** öğesine sağ tıklayıp **"Set Startup Projects..."** menüsünü açın.
2. **"Multiple startup projects"** seçeneğini işaretleyin.
3. Sırasıyla aşağıdaki **3 projenin** de eylemini (Action) **"Start"** olarak ayarlayın:
   * `FlightProvider`
   * `FlightTicketApp.API`
   * `FlightTicketApp.UI`
4. `F5` tuşuna basarak projeleri başlatın.