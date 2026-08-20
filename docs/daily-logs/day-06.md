# Day 6 — ASP.NET Core Identity Kurulumu

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Bugünkü kodun temelindeki C# kavramı (`IServiceScope`/scoped servisler) için `docs/CSHARP_FUNDAMENTALS.md`'deki **Kavram 15**'e bakabilirsin.

---

## 1. Bugün çözdüğümüz problem neydi?

Şu ana kadar API'nin kimin kullandığını hiç bilmiyorduk — kimlik doğrulama (authentication) sistemi yoktu. Bugün, kullanıcıların kayıt olabileceği, şifrelerin güvenli şekilde saklandığı, ve "User/Coach/Admin" gibi rollerin tanımlı olduğu bir temel kurduk. Bunu **sıfırdan yazmadık** — ASP.NET Core Identity'yi (Microsoft'un hazır, test edilmiş kullanıcı yönetim sistemi) kullandık.

**Bunu yapmasaydık ne olurdu?** Şifre hash'lemeyi kendimiz yazsaydık (zayıf algoritma, "salt" eksikliği gibi hatalarla), ciddi bir güvenlik açığı riski olurdu. Identity, bunu endüstri standardı bir algoritmayla (PBKDF2) otomatik yapıyor.

---

## 2. Ana kavramlar

| Kavram | Basit tanım |
|---|---|
| **Authentication** | "Sen kimsin?" sorusunun cevabı (authorization — "sana izin var mı?" — ayrı, Day 10'da). |
| **Identity** | ASP.NET Core'un hazır kullanıcı/rol/şifre yönetim sistemi. |
| **Password hashing** | Şifreyi geri döndürülemez bir "iz"e çevirme — veritabanında asla düz metin şifre tutulmaz. |
| **User / Role / Claim** | User = giriş yapabilen kişi. Role = isimli bir grup ("Admin"). Claim = kullanıcı hakkında tek bir bilgi parçası (Role'ler de arka planda bir tür Claim). |
| **`IdentityResult`** | Identity metotlarının (örn. `CreateAsync`) hata fırlatmak yerine döndürdüğü, `Succeeded`/`Errors` içeren sonuç nesnesi — "şifre zayıf" gibi beklenen durumlar için exception yerine bir "sonuç" kullanılıyor (Day 5'teki exception yaklaşımından farklı, bilinçli bir tasarım tercihi). |
| **Controller (ilk kez!)** | Day 1-5'te hep `app.MapGet(...)` (minimal API) kullanmıştık. Bugün ilk kez `[ApiController]`, `[Route]`, `[HttpPost]` attribute'larıyla tanımlanan gerçek bir Controller sınıfı (`AuthController`) yazdık. `ControllerBase`'den miras alınca `Ok()`/`BadRequest()` gibi hazır yardımcılar gelir. |
| **Primary constructor** | `public class AuthController(UserManager<ApplicationUser> userManager) : ControllerBase` — constructor'ı sınıf başlığında tanımlayan, C# 12'nin kısa yazım şekli. Aynı DI (Kavram 8) mekanizması, daha az kod. |

---

## 3. Neden bu şekilde yaptık?

### Neden `ApplicationUser`, `FitForge.Infrastructure`'da; `Domain`'de değil?

`ApplicationUser : IdentityUser` yazmak, `Microsoft.AspNetCore.Identity` paketine bağımlı olmak demek. Day 1'de "Domain hiçbir dış pakete bağımlı olmayacak" kuralını koymuştuk — bu yüzden `ApplicationUser`, kendisi zaten EF Core'a bağımlı olan `Infrastructure`'a ait.

### Neden `FitForgeDbContext` artık `DbContext` yerine `IdentityDbContext<ApplicationUser>`'dan türüyor?

`IdentityDbContext<TUser>`, kendisi bir `DbContext` alt sınıfı (Kavram 6) — üstüne `DbSet<TUser> Users`, `DbSet<IdentityRole> Roles` gibi Identity'nin ihtiyaç duyduğu tabloları otomatik tanımlıyor. Biz bunları elle yazmadık, miras yoluyla "bedava" aldık.

### Neden `Roles` sabitlerini `Domain`'de tanımladık?

"User", "Coach", "Admin" gibi metinleri (magic string) kodun birden fazla yerine (bugün `Program.cs`ve `AuthController`, ileride Day 10'daki `[Authorize(Roles = ...)]`) elle yazsaydık, bir yerde yazım hatası yapma riski olurdu. Tek bir yerde tanımlayıp her yerde onu kullanmak, hem C#'ın kendi derleme-zamanı kontrolünden (yanlış yazarsan derlenmez) faydalanmamızı sağlıyor hem de tekrarı önlüyor.

### Neden `RegisterRequest`, `FitForge.Application`'da?

PROJECT_BRIEF, DTO'ları Application katmanının sorumluluğu olarak tanımlıyor. `RegisterRequest`, veritabanı entity'si değil, sadece "bir HTTP isteğinin şekli" — bu yüzden Application'da, `record` olarak (Kavram 14).

---

## 4. Dosya dosya, satır satır

### Dosya 1 — `backend/src/FitForge.Domain/Common/Roles.cs` (yeni)

```csharp
namespace FitForge.Domain.Common;

public static class Roles
{
    public const string User = "User";
    public const string Coach = "Coach";
    public const string Admin = "Admin";
}
```
`static class` (Kavram 3) — hiç nesnesi olmayan, sadece sabit değerleri bir arada tutan bir araç kutusu. `const` — bu değerler asla değişmez, derleme zamanında sabitlenir.

### Dosya 2 — `backend/src/FitForge.Infrastructure/Identity/ApplicationUser.cs` (yeni)

```csharp
using Microsoft.AspNetCore.Identity;

namespace FitForge.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
}
```
`IdentityUser`'dan miras alıyoruz (Kavram 6) — bu bize otomatik olarak `Email`, `PasswordHash`, `UserName` gibi alanları veriyor. İçi şu an boş çünkü FitForge'a özel ekstra bir kullanıcı alanına (henüz) ihtiyacımız yok — ama ileride (örneğin "kullanıcının adı soyadı" gibi) eklemek istersek, bu sınıfın içine ekleyeceğiz.

### Dosya 3 — `backend/src/FitForge.Infrastructure/Persistence/FitForgeDbContext.cs` (değişti)

```csharp
public class FitForgeDbContext : IdentityDbContext<ApplicationUser>
{
    // ... (SaveChangesAsync override'ı aynı kaldı, Day 4'ten)
}
```
Tek değişiklik: `DbContext` yerine `IdentityDbContext<ApplicationUser>`. Bu, generic bir tip (Kavram 4) — `<ApplicationUser>` diyerek, "Identity tablolarını, benim `ApplicationUser` sınıfımla eşleştir" demiş oluyoruz.

### Dosya 4 — `backend/src/FitForge.Infrastructure/DependencyInjection.cs` (değişti)

```csharp
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FitForgeDbContext>()
    .AddDefaultTokenProviders();
```
- `AddIdentity<ApplicationUser, IdentityRole>()` — servis konteynerine (Kavram 8), `UserManager`, `RoleManager`, `SignInManager` gibi Identity'nin tüm hazır servislerini kaydediyor.
- `.AddEntityFrameworkStores<FitForgeDbContext>()` — "bu servisler, verileri `FitForgeDbContext` üzerinden SQL Server'da saklasın" diyoruz.
- `.AddDefaultTokenProviders()` — ileride (şifre sıfırlama, email doğrulama gibi) token üretmek için gereken altyapıyı ekliyor.

### Dosya 5 — `backend/src/FitForge.Infrastructure/FitForge.Infrastructure.csproj` (değişti)

```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```
**Karşılaştığımız bir hata:** `AddIdentity` metodunu eklediğimizde derleyici "böyle bir metot yok" dedi. Sebebi: `Infrastructure`, düz bir sınıf kütüphanesi (`Microsoft.NET.Sdk`) — `FitForge.Api` gibi bir web SDK'sı değil, bu yüzden ASP.NET Core'un bazı temel parçalarına (bu ikisi dahil) otomatik erişimi yok. `FrameworkReference`, "bu projeye ASP.NET Core'un paylaşılan çalışma zamanına erişim ver" diyor — sorunu çözdü.

### Dosya 6 — `backend/src/FitForge.Application/Auth/RegisterRequest.cs` (yeni)

```csharp
namespace FitForge.Application.Auth;

public record RegisterRequest(string Email, string Password);
```
Bir `record` (Kavram 14) — kayıt isteğinin şekli, sadece iki alan.

### Dosya 7 — `backend/src/FitForge.Api/Controllers/AuthController.cs` (yeni)

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        await userManager.AddToRoleAsync(user, Roles.User);

        return Ok(new { user.Id, user.Email });
    }
}
```
- `[ApiController]` — "bu sınıf bir API controller'ı, otomatik model doğrulama gibi özellikleri aç."
- `[Route("api/auth")]` — bu controller'ın tüm endpoint'lerinin başına gelecek yol.
- `public class AuthController(UserManager<ApplicationUser> userManager) : ControllerBase` — primary constructor: `UserManager<ApplicationUser>`'ı DI'dan (Kavram 8) alıyoruz, ayrıca bir constructor gövdesi yazmadan.
- `[HttpPost("register")]` — bu metodu `POST api/auth/register` isteğine bağlıyor.
- `userManager.CreateAsync(user, request.Password)` — Identity, şifreyi burada hash'liyor, kullanıcıyı kaydediyor. `result.Succeeded` false ise, `result.Errors`'ı (her biri okunabilir bir açıklama) döndürüyoruz.
- `userManager.AddToRoleAsync(user, Roles.User)` — her yeni kullanıcıya varsayılan olarak "User" rolünü veriyoruz.

### Dosya 8 — `backend/src/FitForge.Api/Program.cs` (değişti — rol seed etme)

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = [Roles.User, Roles.Coach, Roles.Admin];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
```
Kavram 15'te tam olarak bunu görmüştük: `RoleManager<IdentityRole>` "scoped" bir servis, uygulama başlarken (HTTP isteği olmadan) kullanmak için elle bir `scope` açmamız gerekiyor. `RoleExistsAsync` kontrolü, uygulamayı her yeniden başlattığımızda rolleri tekrar tekrar eklemeye çalışmamızı (ve hata almamızı) önlüyor.

### Migration — `AddIdentityTables` (3 dosya, otomatik üretildi)

Bu, Day 3'teki "boş" migration'dan farklı olarak **gerçek tablolar** oluşturan ilk migration'ımız: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`. Bunları biz tasarlamadık — Identity'nin kendi standart şeması, `IdentityDbContext`'ten miras almamız sayesinde otomatik geldi.

---

## 5. Karşılaştığımız büyük bir sorun: Windows "Smart App Control"

Kod tamamen doğru derlendi ama API'yi çalıştırmaya çalışınca şu hatayı aldık:
```
System.IO.FileLoadException: Could not load file or assembly '...FitForge.Application.dll'.
Uygulama Denetimi ilkesi bu dosyayı engelledi.
```
Bu bir **kod hatası değildi** — Windows 11'in "Smart App Control" güvenlik özelliği, imzasız/yeni derlenmiş DLL'leri (reputation'ı olmadığı için) engelliyordu. Bugüne kadar sorun çıkmamıştı çünkü ilk kez gerçek bir Controller yazdık — bu, ASP.NET Core'un `FitForge.Application.dll`'i derinlemesine incelemesini gerektirdi, ve o dosya yeni derlendiği için "tanınmıyor" sayıldı.

Windows Olay Günlüğü'nde (`Microsoft-Windows-CodeIntegrity/Operational`, Event ID 3077/3118) bunu doğruladık. Çözüm: Smart App Control'ü (Windows Ayarları > Gizlilik ve Güvenlik > Windows Güvenliği > App ve tarayıcı denetimi) kapattın. **Önemli not:** bu tek yönlü bir karar — Windows'u temiz kurmadan tekrar açılamıyor. Yerel .NET geliştirmesi için bu özellik pratikte uyumsuz, bu yüzden kapatmak makul bir tercih.

---

## 6. Ne test ettik, nasıl doğruladık?

1. `dotnet build` — 0 hata.
2. `dotnet ef migrations add AddIdentityTables` + `dotnet ef database update` — Identity tabloları gerçekten oluştu.
3. API'yi çalıştırdım, startup loglarında rollerin (`INSERT INTO [AspNetRoles] ...`) gerçekten eklendiğini gördüm.
4. `POST /api/auth/register` — geçerli bilgiyle: `{"id":"...","email":"test@fitforge.dev"}`, HTTP 200.
5. `POST /api/auth/register` — zayıf şifreyle: 4 farklı, okunabilir hata mesajı, HTTP 400. `IdentityResult` deseni çalışıyor.
6. SSMS/`sqlcmd` ile veritabanına bakıp doğruladım:
   - `PasswordHash` sütunu, düz metin değil, uzun bir hash (`AQAAAAIAAYag...`) içeriyor.
   - `AspNetUserRoles` tablosunda, kullanıcının "User" rolüne doğru şekilde bağlandığını gördüm.

### Sen nasıl test edersin?

```powershell
cd backend
dotnet build
dotnet run --project src/FitForge.Api
```
Sonra (Postman/curl ile):
```
POST http://localhost:5053/api/auth/register
Content-Type: application/json

{"email": "sen@fitforge.dev", "password": "GucluBirSifre1!"}
```
`200 OK` ve kullanıcı bilgisi dönmeli. SSMS'te `AspNetUsers` tablosunda kaydı görebilirsin.

### Olası hata

`AddIdentity` metodunu bulamama hatası alırsan, `Infrastructure.csproj`'de `FrameworkReference Include="Microsoft.AspNetCore.App"` satırının olduğundan emin ol. API çalışmayı reddedip "Uygulama Denetimi" hatası verirse, Smart App Control'ün açık olup olmadığını kontrol et.

---

## 7. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| `IdentityDbContext<ApplicationUser>`'a geçtik | Identity tablolarını elle tasarlamamak için | Users/Roles/Claims tablolarını sıfırdan, hatalara açık şekilde tasarlamamız gerekirdi |
| Şifre hash'lemeyi Identity'ye bıraktık | Güvenli, test edilmiş bir algoritma (PBKDF2) kullanmak için | Kendi yazdığımız hash'leme, güvenlik açığına yol açabilirdi |
| Rolleri `Domain`'de sabit olarak tanımladık | Magic string tekrarını ve yazım hatalarını önlemek için | "Admin" yerine "admin" yazıp sessizce bozulan bir kod olabilirdi |
| İlk gerçek Controller'ı yazdık | Minimal API'nin yetersiz kaldığı, daha yapılandırılmış endpoint'ler için | Tüm endpoint'leri `app.MapGet/MapPost` ile yönetmek, büyüyünce dağınıklaşırdı |
| Smart App Control'ü kapattık | Yerel derlenmiş, imzasız DLL'lerin engellenmesini önlemek için | Her yeni controller/assembly değişikliğinde rastgele engellenme riski sürerdi |

---

**Sıradaki gün:** Day 7 — Login ve JWT Access Token.
