# Day 7 — Login ve JWT Access Token

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Yeni C# deseni (Options pattern / `IOptions<T>`) için `docs/CSHARP_FUNDAMENTALS.md`'deki **Kavram 16**'ya bakabilirsin.

---

## 1. Bugün çözdüğümüz problem neydi?

Day 6'da kullanıcılar kayıt olabiliyordu ama **giriş yapamıyordu** — ve giriş yapsalar bile, API'nin "sen kimsin, sana bu endpoint'e erişim var mı" diye kontrol edeceği hiçbir mekanizma yoktu. Bugün: email/şifreyle giriş yapılabiliyor, API buna karşılık bir **JWT (JSON Web Token)** veriyor, ve bu token'ı taşıyan istekler "korumalı" endpoint'lere erişebiliyor.

**Bunu yapmasaydık ne olurdu?** Herkes, kimliğini kanıtlamadan her endpoint'e erişebilirdi — "bu benim profilim" gibi bir kavramın hiçbir anlamı olmazdı, ileride (Day 11+) yazacağımız "sadece kendi verine erişebilirsin" kuralları uygulanamazdı.

---

## 2. Ana kavramlar

| Kavram | Basit tanım |
|---|---|
| **JWT (JSON Web Token)** | `header.payload.signature` şeklinde üç parçalı bir metin. `payload` kullanıcı bilgisini taşır (şifreli değil, sadece Base64 ile kodlanmış — herkes okuyabilir), `signature` ise sunucunun gizli anahtarıyla imzalanmış — biri payload'u değiştirirse imza uyuşmaz, sahtecilik anlaşılır. |
| **Access token** | Kullanıcının kimliğini kanıtlayan, kısa ömürlü bir JWT. |
| **Stateless (durumsuz) authentication** | Sunucu "kim giriş yaptı" bilgisini kendi belleğinde tutmuyor — token'ın kendisi kanıt. Her istekte sunucu sadece imzayı doğruluyor. |
| **Token neden süresi dolmalı?** | Çalınırsa zararı sınırlı olsun diye. Kullanıcı sürekli şifre girmesin diye "refresh token" (Day 8) devreye girecek. |
| **Claims / `ClaimsPrincipal`** | Claim = kullanıcı hakkında tek bir bilgi (`sub`=kimlik, `email`, `role`). Token doğrulanınca, içindeki claim'ler `User` (bir `ClaimsPrincipal`) üzerinden koda erişilebilir hâle geliyor. |
| **`[Authorize]` / `[AllowAnonymous]`** | `[Authorize]`: geçerli token yoksa 401. `[AllowAnonymous]`: token gerekmez (bugün kullanmadık ama Register/Login zaten `[Authorize]` içermiyor, varsayılan olarak açık). |
| **Options pattern (`IOptions<T>`)** | appsettings/User Secrets'taki bir ayar bölümünü, tip-güvenli bir C# sınıfına otomatik bağlama deseni. Bkz. `CSHARP_FUNDAMENTALS.md` Kavram 16. |
| **FluentValidation** | CLAUDE.md'nin zorunlu kıldığı doğrulama kütüphanesi — `RuleFor(x => x.Alan).KuralAdı()` şeklinde, okunaklı, zincirlenebilir doğrulama kuralları yazmanı sağlıyor. |

---

## 3. Neden bu şekilde yaptık?

### Neden `IJwtTokenGenerator`, `Application`'da; somut `JwtTokenGenerator`, `Infrastructure`'da?

Aynı Day 6'daki mantık: `Application`, JWT kütüphanesini (`System.IdentityModel.Tokens.Jwt`) hiç bilmemeli. Sadece "bana bir token üretme yeteneği ver" (arayüz/sözleşme, Kavram 5) diyor, gerçek JWT üretim detayı `Infrastructure`'da.

### Neden `IJwtTokenGenerator.CreateToken`, `ApplicationUser` almıyor, sade `string userId, string email` alıyor?

`ApplicationUser`, `Infrastructure`'a ait bir tip (Day 6). `Application` katmanı bunu **hiç bilmiyor** — bu yüzden arayüzü, herhangi bir kullanıcı tipine bağlı olmayan, sade veri tipleriyle (`string`, `IEnumerable<string>`) tasarladık. `AuthController` (Api), her iki tipi de görebildiği için, `ApplicationUser`'dan bu sade değerleri çıkarıp arayüze veriyor.

### Neden JWT `Secret`'ı User Secrets'ta, ama `Issuer`/`Audience`/`ExpiryMinutes`'i `appsettings.json`'da?

Sadece `Secret` gerçekten "sır" — biri onu bilse sahte token üretebilir. `Issuer`/`Audience`/`ExpiryMinutes` ise sıradan ayar, sızsa bile tek başına bir güvenlik riski oluşturmuyor. Bu yüzden sadece gerçekten hassas olanı gizli tuttuk (Day 3'teki connection string mantığıyla aynı).

### Neden şifre kontrolü için `SignInManager`, `UserManager` değil?

`UserManager.CheckPasswordAsync` de işi görürdü, ama `SignInManager.CheckPasswordSignInAsync`, üstüne otomatik olarak **hesap kilitleme** (lockout) sayacını da işletiyor — art arda yanlış şifre denemelerini fark edip hesabı geçici kilitleyebiliyor. Bu, brute-force (deneme-yanılma) saldırılarına karşı ek bir koruma; Day 29'daki rate limiting'i tamamlıyor.

### Neden `AuthController`'a `SignInManager`, `IJwtTokenGenerator`, `IValidator<LoginRequest>` gibi 4 farklı bağımlılık ekledik?

Hepsi primary constructor'da (Kavram 8 + Day 6'daki primary constructor) toplandı. Gerçek dünyada bir controller'ın birden fazla bağımlılığı olması normal — önemli olan, bu bağımlılıkların hepsinin **arayüz/soyutlama** olması (test edilebilir kalması), somut sınıflara sıkı sıkıya bağlı olmaması.

---

## 4. Dosya dosya, satır satır

### `FitForge.Application/Auth/LoginRequest.cs` ve `LoginResponse.cs` (yeni)

```csharp
public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
```
İki `record` (Kavram 14). `LoginResponse`, hem token'ı hem de "ne zaman bitecek" bilgisini taşıyor — frontend (Day 38), bu bilgiyle "token'ın süresi dolmadan yenile" mantığını kurabilecek.

### `FitForge.Application/Auth/LoginRequestValidator.cs` (yeni)

```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
```
- `AbstractValidator<LoginRequest>`'ten miras alıyoruz (Kavram 6) — FluentValidation'ın istediği şey bu.
- `RuleFor(x => x.Email)` — bir lambda (Kavram 12'deki gibi, "hangi alanı kontrol edeceğim" diyoruz) ile başlıyoruz, sonra zincirleme kurallar ekliyoruz: `.NotEmpty()` (boş olmasın), `.EmailAddress()` (geçerli bir email formatı olsun).
- Bunu test ettiğimizde, `"not-an-email"` gönderince FluentValidation otomatik olarak (sistem dilini algılayıp) `"'Email' geçerli bir e-posta adresi değil."` gibi bir Türkçe mesaj üretti — biz bu mesajı hiç yazmadık, kütüphane hazır sağlıyor.

### `FitForge.Application/Auth/IJwtTokenGenerator.cs` (yeni)

```csharp
public interface IJwtTokenGenerator
{
    LoginResponse CreateToken(string userId, string email, IEnumerable<string> roles);
}
```
Sözleşme (Kavram 5): "bana kullanıcı id'si, email'i ve rolleri ver, ben sana bir `LoginResponse` (token + bitiş zamanı) döneyim."

### `FitForge.Infrastructure/Authentication/JwtOptions.cs` (yeni)

```csharp
public class JwtOptions
{
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpiryMinutes { get; set; }
}
```
appsettings/User Secrets'taki `"Jwt"` bölümünün eşleneceği sınıf (Kavram 16).

### `FitForge.Infrastructure/Authentication/JwtTokenGenerator.cs` (yeni)

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public LoginResponse CreateToken(string userId, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(accessToken, expiresAtUtc);
    }
}
```
- Constructor'da `IOptions<JwtOptions> options` alıp `_options = options.Value;` — Options pattern (Kavram 16), `.Value`'yu bir kere burada açıp, metod içinde tekrar tekrar `.Value` yazmamak için bir alana kaydediyoruz.
- `claims` listesi — token'ın içine koyacağımız bilgiler. `JwtRegisteredClaimNames.Sub` = `"sub"` (JWT standardının "bu token kime ait" alanı), `JwtRegisteredClaimNames.Email` = `"email"`.
- `claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));` — her rol için bir claim ekliyoruz (kullanıcı birden fazla role sahip olabilir).
- `SymmetricSecurityKey` + `SigningCredentials` — "bu anahtarla, bu algoritmayla (HMAC-SHA256) imzala" diyoruz.
- `new JwtSecurityToken(...)` — token'ın kendisini oluşturuyoruz (henüz metin değil, bir nesne).
- `new JwtSecurityTokenHandler().WriteToken(token)` — nesneyi, gerçek `"eyJhbGc..."` şeklindeki metin token'a çeviriyor.

**Önemli, gizli bir detay (bilerek not ediyorum):** Token'ı `"sub"` ve `"email"` (kısa JWT isimleriyle) oluşturduk, ama Controller'da (aşağıda) onu `ClaimTypes.NameIdentifier` ve `ClaimTypes.Email` (uzun, farklı görünen isimlerle) okuyoruz. Bu bilerek yaptığımız bir hata değil — .NET'in JWT doğrulama mekanizması, gelen token'ı işlerken **varsayılan olarak** bazı kısa JWT claim isimlerini (`sub`, `email` gibi) otomatik olarak bu uzun `ClaimTypes` isimlerine çeviriyor (buna "inbound claim mapping" deniyor). Biz bunu test ederken (aşağıda) doğruladık — çalıştı, çünkü bu varsayılan eşleme zaten var.

### `FitForge.Infrastructure/DependencyInjection.cs` (değişti)

Yeni eklenen satırlar:
```csharp
services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
```
- `Configure<JwtOptions>(...)` — Kavram 16'daki "bu bölümü bu sınıfa bağla" kaydı.
- `AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();` — DI kaydı (Kavram 8). `Singleton` seçtik çünkü `JwtTokenGenerator`'ın kendine ait, isteğe özel bir "durumu" yok — sadece ayarları okuyup token üretiyor, güvenle tek bir örnek paylaşılabilir.
- `AddValidatorsFromAssemblyContaining<LoginRequestValidator>();` — FluentValidation'a "bu projedeki (Application) **tüm** validator sınıflarını otomatik bul ve DI'ya kaydet" diyoruz — ileride yeni validator'lar eklediğimizde, bu satırı değiştirmemize gerek kalmayacak.

### `FitForge.Api/Program.cs` (değişti — JWT authentication kurulumu)

```csharp
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section was not found.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
```
- `builder.Configuration.GetSection("Jwt").Get<JwtOptions>()` — burada Options pattern'i (DI üzerinden `IOptions<T>` ile) kullanmadık, çünkü bu satır `Program.cs`'in en üstünde, uygulama henüz kurulurken çalışıyor — henüz DI container'dan bir şey isteyemeyeceğimiz bir an. Bu yüzden ayarları **doğrudan** `Configuration`'dan okuyup bir `JwtOptions` nesnesine çeviriyoruz (`.Get<T>()`), DI'yı hiç karıştırmadan. (`JwtTokenGenerator` içindeyse gerçek DI/`IOptions<T>` kullandık, çünkü o zaman DI container zaten hazır.)
- `AddAuthentication(options => { ... })` — burada **karşılaştığımız gerçek bir soruna** çözüm var, aşağıda anlatıyorum.
- `AddJwtBearer(options => { options.TokenValidationParameters = ... })` — gelen bir token'ı doğrularken nelere bakılacağını söylüyoruz: doğru `Issuer`/`Audience` mi, imza doğru anahtarla mı yapılmış, süresi dolmuş mu.
- `AddAuthorization()` — `[Authorize]` attribute'unun çalışabilmesi için gereken temel servis kaydı.

**Karşılaştığımız gerçek bir sorun:** İlk denemede `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` (tek parametre) yazmıştım. Login çalıştı, token geldi, ama `/api/auth/me`'ye o token'la gittiğimde **401 aldım** — ve cevapta gizli bir `Location: .../Account/Login?ReturnUrl=...` başlığı vardı. Sebep: Day 6'da eklediğimiz `AddIdentity(...)`, arka planda **kendi cookie tabanlı** authentication şemasını da kuruyor ve varsayılan yapıyor. `[Authorize]` bir hata (challenge) üretince, .NET hangi şemanın "varsayılan" olduğuna bakıp, JWT yerine bu cookie şemasını kullanıp "giriş sayfasına yönlendir" davranışı sergiledi. Çözüm: `AddAuthentication` çağrısında `DefaultAuthenticateScheme` ve `DefaultChallengeScheme`'i **açıkça** JWT olarak belirttim — artık `[Authorize]`, token yoksa/geçersizse düz bir 401 dönüyor, yönlendirme yapmıyor.

### `FitForge.Api/Program.cs` (değişti — pipeline sıralaması)

```csharp
app.UseAuthentication();
app.UseAuthorization();
```
**Sıra kritik.** `UseAuthentication`, "bu istekte bir token var mı, geçerli mi, varsa `User`'ı doldur" işini yapıyor. `UseAuthorization`, "`User` dolu mu, bu endpoint'e izni var mı" diye kontrol ediyor. Önce kimin geldiğini bilmeden, izin kontrolü yapamazsın — bu yüzden `UseAuthentication` her zaman `UseAuthorization`'dan **önce** gelmeli.

### `FitForge.Api/Controllers/AuthController.cs` (değişti — Login ve Me eklendi)

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
{
    var validationResult = await loginRequestValidator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
        return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
        return Unauthorized();
    }

    var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    if (!signInResult.Succeeded)
    {
        return Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var loginResponse = jwtTokenGenerator.CreateToken(user.Id, user.Email!, roles);

    return Ok(loginResponse);
}
```
- `loginRequestValidator.ValidateAsync(request, cancellationToken)` — FluentValidation'ı **elle** çağırıyoruz (otomatik bir "her isteği doğrula" mekanizması kurmadık, sadece bu endpoint'te elle çağırdık — küçük başlangıç, ileride genişletebiliriz).
- `userManager.FindByEmailAsync(request.Email)` — kullanıcıyı email'e göre bul. `user is null` — Kavram 5/6'dan hatırlayacağın gibi, bulunamazsa **hangi sebeple** bulunamadığını (yanlış email mi, yanlış şifre mi) **söylemiyoruz** — ikisi için de aynı `Unauthorized()` (401) dönüyoruz. Bu bilinçli: "email yok" demek, bir saldırgana "bu email kayıtlı değil, farklı email'ler dene" bilgisini verirdi — güvenlik açısından, ikisini ayırt etmemek daha doğru.
- `signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true)` — şifreyi kontrol et, yanlışsa (ve `lockoutOnFailure: true` sayesinde) başarısız girişleri say.
- `userManager.GetRolesAsync(user)` — bu kullanıcının hangi rollere sahip olduğunu getir.
- `jwtTokenGenerator.CreateToken(user.Id, user.Email!, roles)` — `user.Email!` — sonundaki `!` işareti, C#'a "bunun `null` olmayacağını biliyorum, uyarı vermeyi kes" demek (nullable reference types, ileride daha detaylı göreceğiz). `ApplicationUser.Email`'in tipi `string?` (null olabilir) ama biz bu noktada zaten `FindByEmailAsync` ile bulduğumuz için `Email`'in dolu olduğunu biliyoruz.

```csharp
[Authorize]
[HttpGet("me")]
public IActionResult Me()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = User.FindFirstValue(ClaimTypes.Email);
    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    return Ok(new { userId, email, roles });
}
```
- `[Authorize]` — bu tek satır, bu endpoint'i koruyor. Token yoksa/geçersizse, metodun içi **hiç çalışmaz**, doğrudan 401 döner.
- `User` — `ControllerBase`'den miras aldığımız (Kavram 6) bir özellik, tam olarak Kavram 2'de bahsettiğimiz `ClaimsPrincipal`. Token doğrulandıktan sonra, .NET bunu otomatik dolduruyor.
- `User.FindFirstValue(ClaimTypes.NameIdentifier)` — token içindeki `"sub"` claim'ini (yukarıda bahsettiğimiz otomatik eşleme sayesinde `NameIdentifier` olarak) okuyoruz.
- `User.FindAll(ClaimTypes.Role)` — birden fazla rol claim'i olabileceği için `FindFirstValue` değil `FindAll` kullanıyoruz, `.Select(c => c.Value)` ile sadece değerlerini (rol isimlerini) alıyoruz.

---

## 5. Ne test ettik, nasıl doğruladık?

1. `dotnet build` — 0 hata (iki küçük paket eksikliğini yol boyunca düzelttik: `FluentValidation` paketinin `Application`'a da eklenmesi gerekiyordu).
2. `POST /api/auth/login` (Day 6'daki test kullanıcısıyla, doğru şifre) → `{"accessToken":"eyJ...", "expiresAtUtc":"..."}`, HTTP 200.
3. `GET /api/auth/me`, geçerli token ile → `{"userId":"...", "email":"test@fitforge.dev", "roles":["User"]}`, HTTP 200. **İlk denemede 401 aldık** (yukarıdaki "AddIdentity cookie şeması" sorunu) — düzelttik, sonra doğru çalıştı.
4. `GET /api/auth/me`, token olmadan → 401.
5. `GET /api/auth/me`, sahte/geçersiz bir token ile → 401.
6. `POST /api/auth/login`, yanlış şifre ile → 401 (ve Day 5'in `AddProblemDetails()`'i sayesinde otomatik olarak düzgün `ProblemDetails` şekilli bir cevap).
7. `POST /api/auth/login`, geçersiz email formatıyla (`"not-an-email"`) → 400 + FluentValidation'ın ürettiği okunabilir hata mesajı.

### Sen nasıl test edersin?

```powershell
cd backend
dotnet build
dotnet run --project src/FitForge.Api
```
Sonra:
```
POST http://localhost:5053/api/auth/login
{"email": "test@fitforge.dev", "password": "..."}
```
Dönen `accessToken`'ı kopyala, sonra:
```
GET http://localhost:5053/api/auth/me
Authorization: Bearer <accessToken>
```
Kullanıcı bilgilerini dönmeli.

### Olası hata

`/api/auth/me`'ye geçerli bir token'la gittiğinde hâlâ 401 alıyorsan (ve cevapta gizli bir `Location` başlığı varsa), `Program.cs`'teki `AddAuthentication` çağrısında `DefaultAuthenticateScheme`/`DefaultChallengeScheme`'in JWT olarak açıkça ayarlandığından emin ol — bu, Identity + JWT birlikte kullanılırken çok sık karşılaşılan bir tuzak.

---

## 6. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| Token üretimini `IJwtTokenGenerator` arayüzü arkasına koyduk | Application katmanı JWT kütüphanesini bilmesin, test edilebilir kalsın | Application, Infrastructure'a sızan bir bağımlılık kazanırdı |
| `Secret`'ı User Secrets'ta, diğer JWT ayarlarını appsettings'te tuttuk | Sadece gerçekten hassas olanı gizli tutmak | İmzalama anahtarı GitHub'da herkese açık dururdu |
| `SignInManager.CheckPasswordSignInAsync` kullandık | Otomatik hesap kilitleme (brute-force koruması) için | Art arda yanlış şifre denemelerine karşı korumasız kalırdık |
| "Email yok" ile "şifre yanlış" için aynı 401'i döndük | Saldırgana hangi email'lerin kayıtlı olduğunu sızdırmamak için | Bir saldırgan, kayıtlı email'leri tek tek tespit edebilirdi |
| `AddAuthentication`'da varsayılan şemaları JWT'ye sabitledik | `AddIdentity`'nin cookie şemasıyla çakışmayı önlemek için | `[Authorize]` her zaman login sayfasına yönlendirmeye çalışırdı, API için anlamsız olurdu |

---

**Sıradaki gün:** Day 8 — Refresh Token Rotation.
