# Uçak Bileti Satış Uygulaması

Bu proje, acente arayüzü (UI), uçuş sağlayıcısı ile haberleşen arka uç servisi (REST API) ve paylaşılan sözleşmelerden (Contracts/Shared DTO) oluşan dağıtık bir mimaride geliştirilmiştir.

## Mimari Yapı

Proje, Sorumlulukların Ayrıştırılması (SoC) ve Anti-Corruption Layer (ACL) prensiplerine uygun olarak 4 katmana bölünmüştür:

1. **`FlightTicketApp.API` (REST Backend):**
   * Verilen `FlightProvider` SOAP servisini (CoreWCF) arka planda tüketir.
   * Dış dünyaya RESTful endpoint'ler (`/api/Airports`, `/api/Flights/search`) sunar.
   * Gidiş ve dönüş uçuşlarını eşzamanlı (`Task.WhenAll`) sorgulayarak yanıt süresini (latency) optimize eder.
   * Statik referans verileri (Havalimanları) için `IMemoryCache` stratejisi uygulanmıştır.
   * WCF istemcisi `using` blokları ile güvenli şekilde kapatılarak kaynak sızıntısı önlenmiştir.

2. **`FlightTicketApp.UI` (MVC Client):**
   * İş kurallarından arındırılmış, yalnızca REST API'yi tüketen bağımsız istemcidir.
   * `IHttpClientFactory` kullanır.
   * Ağ dalgalanmaları ve servis kesintilerine karşı **Polly** ile **Retry (Tekrar Deneme)** ve **Circuit Breaker (Hata Koruması / Şalter)** desenleri uygulanmıştır.

3. **`FlightTicketApp.Shared` (Domain Contracts):**
   * UI ile API arasında ortak kullanılan DTO'ları barındırır.
   * UI'ın API katmanını doğrudan derlemesini engelleyerek bağımlılık sızıntısını ortadan kaldırır.

4. **`FlightTicketApp.Tests` (Unit Tests):**
   * Controller düzeyinde rota kuralları (Origin != Destination), tarih mantığı (ReturnDate >= DepartureDate) ve veri akışını xUnit ve Moq ile doğrular.

---

## Kurulum ve Çalıştırma Talimatı (ÖNEMLİ)

Sistemin uçtan uca çalışabilmesi için **3 sürecin de ayakta olması gerekmektedir**:

1. **FlightProvider SOAP Servisi:**
   * Verilen SOAP servis projesinin `https://localhost:5001/Service.svc` adresinde çalıştığından emin olun.
2. **FlightTicketApp.API (REST):**
   * `https://localhost:7091/` adresinde dinler (Swagger arayüzü aktiftir).
3. **FlightTicketApp.UI (Arayüz):**
   * Kullanıcı arayüzünü sunar.

### Visual Studio Üzerinden Başlatma:
1. Solution Explorer'da **Solution**'a sağ tıklayıp **"Set Startup Projects..."** menüsünü açın.
2. **"Multiple startup projects"** seçeneğini işaretleyin.
3. `FlightTicketApp.API` ve `FlightTicketApp.UI` projelerinin eylemini **"Start"** olarak ayarlayın.
4. `F5` tuşuna basarak projeleri başlatın.