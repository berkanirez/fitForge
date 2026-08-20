# Day 5 — Global Exception Handling ve Standart API Response

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Bugünkü kodun temelindeki C# kavramları (delegate, pattern matching/switch expression, record'lar) için `docs/CSHARP_FUNDAMENTALS.md`'deki **Kavram 12-14**'e bakabilirsin — orada canlı hatalarla, sandbox'ta gösterilmişlerdi.

---

## 1. Bugün çözdüğümüz problem neydi?

Şu ana kadar API'de bir hata (exception) fırlatılsa ne olacağını hiç kontrol etmemiştik. Varsayılan davranış: ya API çöker, ya da .NET'in kendi hata sayfasını/stack trace'ini (kodun iç yapısını gösteren teknik detay) doğrudan istemciye döndürür. Bugün, **her türlü hatayı** tek bir merkezi yerde yakalayıp, tutarlı, temiz, güvenli bir JSON cevaba çeviren bir mekanizma kurduk.

**Bunu yapmasaydık ne olurdu?** Bir saldırgan, API'yi bilerek hata verdirip (örneğin geçersiz bir istekle) dönen stack trace'ten dosya yollarını, kullanılan kütüphaneleri, hatta bazen iç mantığı öğrenebilirdi — ciddi bir güvenlik açığı. Ayrıca frontend (Day 37+), her hatayı farklı bir şekilde parse etmek zorunda kalırdı.

---

## 2. Ana kavramlar

### Middleware (boru hattı)

İsteğin API'ye girişinden cevabın çıkışına kadar geçtiği, sıralı bir boru hattı düşün. `app.UseX()` çağrıları, bu boruya istasyon ekler — Day 1'den beri `app.UseHttpsRedirection()`, `app.UseAuthorization()` gibi satırları zaten görüyorduk. Bugün, hata yakalama mekanizmasını bu borunun **en başına** koyduk (`app.UseExceptionHandler();`), çünkü zincirde herhangi bir istasyonda (controller, EF Core, her yerde) bir hata patlarsa, en baştaki bu mekanizma onu yakalayabilsin.

### `IExceptionHandler` (modern .NET yaklaşımı)

.NET 8'den beri, global hata yakalamanın önerilen yolu, kendi middleware'ini elle yazmak değil, `IExceptionHandler` arayüzünü (Kavram 5: interface) uygulamak. ASP.NET Core, kayıtlı her `IExceptionHandler`'ı, bir hata oluştuğunda otomatik çağırıyor.

### `ProblemDetails`

Hata cevabının şeklini sıfırdan icat etmedik — ASP.NET Core'un RFC 7807 (bir internet standardı) temelli hazır tipini (`ProblemDetails`: `title`, `status` gibi standart alanlar) kullandık.

---

## 3. Neden bu şekilde yaptık?

### Neden `NotFoundException`, `FitForge.Application`'da; `GlobalExceptionHandler`, `FitForge.Api`'de?

`NotFoundException`, düz bir C# sınıfı — hiçbir ASP.NET Core'a özel şey içermiyor. İleride (Day 17+) servislerin ("bu egzersiz yok" gibi) fırlatacağı bir "beklenen hata" tipi olacak, bu yüzden Application katmanına ait.

`GlobalExceptionHandler` ise `HttpContext`, `ProblemDetails` gibi tamamen web'e özgü tipler kullanıyor — Application katmanı bunları hiç bilmemeli (Day 1'de kurduğumuz katman kuralı). Bu yüzden `FitForge.Api`'de.

### Neden bilinmeyen hatalarda `ex.Message`'ı asla göstermiyoruz?

Kendi yazdığımız `NotFoundException`'ın mesajını göstermek güvenli — çünkü mesajı biz yazdık, içinde hassas bilgi olmadığını biliyoruz. Ama beklenmeyen bir hata (örneğin veritabanı bağlantı hatası) neyin patladığını, hangi dosyanın etkilendiğini mesajında taşıyabilir — bu yüzden bilinmeyen her hatada sabit, güvenli bir mesaj (`"An unexpected error occurred."`) dönüyoruz, gerçek detayı sadece **sunucu tarafı loga** (`ILogger`) yazıyoruz.

### Neden geçici test endpoint'leri ekleyip sonra kaldırdık?

Roadmap'in "test by throwing a sample exception" görevini gerçekten yapmak için, bilerek hata fırlatan iki endpoint (`/throw-test`, `/throw-test-500`) ekledim, ikisini de `curl` ile test ettim, sonra **kaldırdım** — bunlar kalıcı ürün kodu değil, sadece bugünkü doğrulama için vardı.

---

## 4. Dosya dosya, satır satır

### Dosya 1 — `backend/src/FitForge.Application/Common/Exceptions/NotFoundException.cs` (yeni)

```csharp
namespace FitForge.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
```
- `public class NotFoundException : Exception` — .NET'in kendi `Exception` sınıfından miras alan (Kavram 6) yeni bir hata tipi. Bu, "bu belirli türde bir hata oluştu" demenin C#'taki yolu.
- `: base(message)` — gelen mesajı, `Exception`'ın kendi constructor'ına iletiyoruz (tıpkı Day 3-4'te gördüğümüz `: base(options)`/`: base(name)` gibi) — böylece `ex.Message` normal şekilde çalışır.

### Dosya 2 — `backend/src/FitForge.Api/ExceptionHandling/GlobalExceptionHandler.cs` (yeni)

```csharp
using FitForge.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitForge.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title
        }, cancellationToken);

        return true;
    }
}
```
- `public class GlobalExceptionHandler : IExceptionHandler` — bu sınıf, `IExceptionHandler` sözleşmesini (Kavram 5) imzalıyor. ASP.NET Core, bu sözleşmeyi imzalayan her sınıfı, bir hata oluştuğunda otomatik çağırabilir.
- `private readonly ILogger<GlobalExceptionHandler> _logger;` ve constructor'daki `ILogger<GlobalExceptionHandler> logger` parametresi — Dependency Injection (Kavram 8). `ILogger<T>`'yi biz `new` ile üretmiyoruz, DI container otomatik veriyor.
- `TryHandleAsync(...)` — `IExceptionHandler`'ın gerektirdiği tek metot. `ValueTask<bool>` dönüyor (Kavram 9'daki `Task<T>`'e çok benzer, sadece daha performanslı bir versiyonu — şimdilik `Task<T>` gibi düşünebilirsin), `bool` ise "bu hatayı ben hallettim mi?" cevabı.
- `_logger.LogError(exception, "...")` — gerçek hatayı (tüm detayıyla) sunucu konsoluna/logına yazıyoruz. Bu, istemciye **gitmeyecek** bilgi.
- `var (statusCode, title) = exception switch { ... }` — pattern matching/switch expression (Kavram 13): exception'ın tipine göre durum kodu ve başlık seçiyoruz. `NotFoundException` → 404, mesajını güvenle gösteriyoruz. Her şey (`_`) → 500, **sabit** güvenli mesaj.
- `httpContext.Response.StatusCode = statusCode;` — HTTP cevabının durum kodunu ayarlıyoruz.
- `await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { ... }, cancellationToken);` — standart `ProblemDetails` şeklinde JSON cevap yazıyoruz.
- `return true;` — "bu hatayı ben hallettim, başka bir handler'a gerek yok."

### Dosya 3 — `backend/src/FitForge.Api/Program.cs` (değişti)

Yeni eklenen satırlar:
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
...
app.UseExceptionHandler();
```
- `AddExceptionHandler<GlobalExceptionHandler>();` — servis konteynerine (Kavram 8) "hata olduğunda `GlobalExceptionHandler`'ı kullan" diye kaydediyoruz.
- `AddProblemDetails();` — ASP.NET Core'un `ProblemDetails` ile ilgili yerleşik desteğini açıyoruz (örneğin framework'ün kendi ürettiği hatalarda da bu şekli kullanması için).
- `app.UseExceptionHandler();` — az önce anlattığımız boru hattına, hata yakalama istasyonunu **en başa** ekliyoruz — `app.UseHttpsRedirection()`'dan, `app.MapControllers()`'dan **önce**, ki zincirdeki her şeyi kapsayabilsin.

**Karşılaştığımız bir hata (bilerek not ediyorum):** İlk denemede test endpoint'ini `() => throw new NotFoundException(...)` şeklinde yazdım. Derleyici şu hatayı verdi: *"Temsilci 'RequestDelegate', 0 bağımsız değişkenleri almaz"* — yani C#, bu lambda'nın hangi delegate tipine (Kavram 12) dönüştürüleceğini tek başına çıkaramadı (çünkü `throw` ifadesinin kendisi bir "dönüş tipi" belirtmiyor). Çözüm: `(Func<IResult>)(() => throw ...)` şeklinde, hangi delegate tipini istediğimizi **açıkça** belirttim.

---

## 5. Ne test ettik, nasıl doğruladık?

Geçici olarak iki test endpoint'i ekleyip `curl` ile denedim, sonra kaldırdım:

1. `GET /throw-test` → `NotFoundException` fırlattı → cevap: `{"title":"Test exercise not found","status":404}`, HTTP 404. ✅ Beklenen hata mesajı güvenle gösterildi.
2. `GET /throw-test-500` → içinde `"SECRET internal detail..."` diye bilerek hassas bir mesaj olan `InvalidOperationException` fırlattı → cevap: `{"title":"An unexpected error occurred.","status":500}`, HTTP 500. ✅ **"SECRET" kelimesi hiçbir yerde görünmedi** — güvenlik davranışı çalışıyor.
3. Sunucu logunu kontrol ettim: gerçek hata (`"SECRET internal detail..."` mesajı ve tam stack trace) loga yazılmıştı — yani biz hâlâ neyin patladığını görebiliyoruz, sadece istemci göremiyor.

### Sen nasıl test edersin?

```powershell
cd backend
dotnet build
dotnet run --project src/FitForge.Api
```
`/health` hâlâ `Healthy` dönmeli. Kalıcı bir hata endpoint'i bırakmadık, bu yüzden hata davranışını görmek istersen, geçici olarak `Program.cs`'e benzer bir `throw` endpoint'i ekleyip deneyebilirsin.

### Olası hata

`() => throw ...` şeklinde bir minimal API endpoint'i yazarsan ve "hangi delege" hatası alırsan, bugün gördüğümüz gibi `(Func<IResult>)(...)` ile açıkça tip belirt.

---

## 6. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| `IExceptionHandler` uygulayan `GlobalExceptionHandler` yazdık | Tüm hataları tek, merkezi bir yerde yakalamak için | Her controller'da tekrar tekrar try/catch yazmamız gerekirdi |
| Bilinmeyen hatalarda sabit, güvenli mesaj döndük | Stack trace/iç detay sızmasın | Saldırgan, hata mesajlarından sistem hakkında bilgi toplayabilirdi |
| Gerçek hatayı `ILogger` ile logladık | Biz (geliştiriciler) hâlâ neyin patladığını görebilelim | Hata sessizce yutulur, sorunu asla fark edemezdik |
| `ProblemDetails` (RFC 7807) kullandık | Sıfırdan format icat etmemek, .NET/Swagger standardına uymak | Kendi özel, tutarsız bir hata formatımız olurdu |
| Test endpoint'lerini kullanıp sonra kaldırdık | Gerçekten doğrulamak, ama kalıcı kod kirliliği bırakmamak | Ya hiç test etmezdik ya da gereksiz kod ürün koduna sızardı |

---

**Sıradaki gün:** Day 6 — ASP.NET Core Identity Kurulumu (kullanıcı yönetimi başlıyor).
