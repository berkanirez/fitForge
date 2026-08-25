# Day 8 — Refresh Token Rotation

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Yeni C# kavramları (navigation property/`Include()`, `IQueryable` vs `IEnumerable`, güvenli rastgele üretim) için `docs/CSHARP_FUNDAMENTALS.md`'deki **Kavram 18-20**'ye bakabilirsin.

---

## 1. Bugün çözdüğümüz problem neydi?

Day 7'de access token'ı kısa ömürlü (30 dakika) yaptık — güvenlik için gerekliydi, ama kullanıcı 30 dakikada bir şifresini yeniden girmek zorunda kalırdı. Bugün: **refresh token** ekleyip, kullanıcı şifresini tekrar girmeden sessizce yeni bir access token alabilsin sağladık. Ayrıca bu refresh token'ları **"rotate"** ettik (her kullanımda eskisini geçersiz kılıp yenisini verdik) ve **çalınma tespiti** ekledik.

**Bunu yapmasaydık ne olurdu?** Ya kullanıcı sürekli şifre girmek zorunda kalırdı (kötü deneyim), ya da access token'ı çok uzun ömürlü yapardık (çalınırsa çok daha uzun süre kötüye kullanılabilirdi).

---

## 2. Ana kavramlar

| Kavram | Basit tanım |
|---|---|
| **Refresh token** | Uzun ömürlü (7 gün), sadece "bana yeni bir access token ver" için kullanılan rastgele bir metin. Korumalı endpoint'lere erişim için kullanılamaz. |
| **Refresh token rotation** | Her kullanıldığında eski token geçersiz olur, yerine yeni bir token verilir — bir token en fazla bir kez kullanılabilir. |
| **Reuse detection (çalınma tespiti)** | Zaten kullanılmış/iptal edilmiş bir token tekrar denenirse, bu "birinin çalıntı bir token kullanmaya çalıştığı" sinyali sayılır — o kullanıcının **tüm** aktif token'ları güvenlik için iptal edilir. |
| **Navigation property / `Include()`** | Bkz. Kavram 18 — `ApplicationUser`'ın birden fazla `RefreshToken`'ı olabilir, bunu okurken `Include()` unutulursa veri "yokmuş" gibi görünür. |
| **`IQueryable` vs `IEnumerable`** | Bkz. Kavram 19 — sorgu ne zaman "gerçekten" çalışıyor. |
| **Güvenli rastgele üretim** | Bkz. Kavram 20 — `RandomNumberGenerator`, tahmin edilemez token değerleri üretmek için. |

---

## 3. Neden bu şekilde yaptık?

### Neden `RefreshToken`, `Domain`'de değil `Infrastructure/Identity`'de?

`RefreshToken`, `ApplicationUser`'a (Infrastructure'a ait bir tip) doğrudan bir navigation property ile bağlı (`public ApplicationUser User { get; set; }`). Bu yüzden `Domain`'e koyamazdık (Domain, Infrastructure'ı hiç bilmez). Ama `AuditableEntity`'den (Domain) miras almasında sorun yok — çünkü Infrastructure, Domain'e bakabilir (izin verilen yön), tersi değil.

### Neden `IJwtTokenGenerator.CreateToken` artık `LoginResponse` değil `AccessTokenResult` dönüyor?

`LoginResponse`, artık **hem** access token **hem de** refresh token'ı taşıyan, HTTP cevabının tam şekli. `JwtTokenGenerator`'ın tek işi JWT üretmek — refresh token'dan hiç haberi olmamalı (tek sorumluluk ilkesi). Bu yüzden onun döndürdüğü tipi (`AccessTokenResult`) daha dar, sade tuttuk; `LoginResponse`'u (ikisini birleştiren) `AuthController` inşa ediyor.

### Neden "reuse detection" bulunca **tüm** token'ları iptal ediyoruz, sadece o token'ı değil?

Bir refresh token çalınıp kullanıldıysa, saldırgan büyük olasılıkla **yeni bir token zinciri** üretmiştir (rotation sayesinde). Sadece çalınan token'ı iptal etmek yetmez — o zincirin devamındaki (saldırganın elindeki) diğer token'lar da tehlikeli. En güvenli tepki: o kullanıcının **tüm** aktif oturumlarını sonlandırmak, gerçek kullanıcıyı yeniden giriş yapmaya zorlamak (can sıkıcı ama güvenli).

### Neden `RefreshTokenService`, `AddScoped` ile kaydedildi (Day 7'deki `JwtTokenGenerator` `AddSingleton` idi)?

`RefreshTokenService`, constructor'ında `FitForgeDbContext` alıyor — `DbContext` `Scoped` (Day 3'te öğrendik, her HTTP isteği için ayrı bir örnek). Bir `Scoped` bağımlılığı olan bir servis de en az `Scoped` olmalı — `Singleton` yaparsak, ilk isteğin `DbContext`'i sonsuza dek "yapışıp" kalırdı, bu ciddi bir hataya yol açardı.

---

## 4. Dosya dosya, satır satır

### `FitForge.Infrastructure/Identity/RefreshToken.cs` (yeni)

```csharp
public class RefreshToken : AuditableEntity
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }

    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
}
```
- `AuditableEntity`'den miras (Day 4) — `Id`, `CreatedAt`, `UpdatedAt`'i bedava alıyoruz.
- `RevokedAtUtc` — `null` ise token hâlâ aktif; bir tarih varsa iptal edilmiş.
- `ReplacedByToken` — bu token rotate edildiğinde, yerine gelen yeni token'ın değerini burada saklıyoruz (ileri seviye bir izleme/denetim izi — kim hangi token'ın yerine geçmiş, takip edilebilir).
- `UserId` + `User` — navigation property (Kavram 18): "bu token hangi kullanıcıya ait."

### `FitForge.Infrastructure/Identity/ApplicationUser.cs` (değişti)

```csharp
public List<RefreshToken> RefreshTokens { get; set; } = [];
```
İlişkinin **karşı tarafı** — "bu kullanıcının hangi token'ları var." İkisi birlikte, tam bir one-to-many (bire-çok) navigation kurulumu oluşturuyor.

### `FitForge.Application/Auth/IRefreshTokenService.cs` (yeni)

```csharp
public record RefreshTokenRotationResult(bool Succeeded, string? UserId, string? NewToken);

public interface IRefreshTokenService
{
    Task<string> CreateAsync(string userId, CancellationToken cancellationToken);
    Task<RefreshTokenRotationResult> RotateAsync(string token, CancellationToken cancellationToken);
}
```
`RefreshTokenRotationResult`'ın alanları (`UserId`, `NewToken`) `string?` (null olabilir) — çünkü rotasyon başarısız olursa (token geçersizse), bunların hiçbiri dolu olmayacak, sadece `Succeeded = false` dönecek.

### `FitForge.Infrastructure/Authentication/RefreshTokenService.cs` (yeni — asıl mantık burada)

```csharp
public async Task<RefreshTokenRotationResult> RotateAsync(string token, CancellationToken cancellationToken)
{
    var existing = await dbContext.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    if (existing is null)
    {
        return new RefreshTokenRotationResult(false, null, null);
    }

    var isExpired = existing.ExpiresAtUtc < DateTime.UtcNow;

    if (existing.RevokedAtUtc is not null || isExpired)
    {
        if (existing.RevokedAtUtc is not null)
        {
            await RevokeAllActiveTokensAsync(existing.UserId, cancellationToken);
        }

        return new RefreshTokenRotationResult(false, null, null);
    }

    var newToken = GenerateSecureToken();

    existing.RevokedAtUtc = DateTime.UtcNow;
    existing.ReplacedByToken = newToken;

    dbContext.RefreshTokens.Add(new RefreshToken
    {
        Token = newToken,
        UserId = existing.UserId,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(ExpiryDays)
    });

    await dbContext.SaveChangesAsync(cancellationToken);

    return new RefreshTokenRotationResult(true, existing.UserId, newToken);
}
```
- `dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ...)` — gerçek bir LINQ sorgusu (Kavram 19): "token'ı bu değere eşit olan ilk kaydı bul, yoksa `null` dön."
- `existing.RevokedAtUtc is not null` kontrolü — **zaten iptal edilmiş** bir token tekrar mı deneniyor? Buysa, `RevokeAllActiveTokensAsync` çağrılıyor — **çalınma tespiti** tam burada.
- Süresi dolmuş (`isExpired`) ama hiç iptal edilmemiş bir token için tüm token'ları iptal **etmiyoruz** — bu normal bir durum (kullanıcı uzun süre giriş yapmadı), çalınma şüphesi değil.
- Her şey normalse: eskisini iptal et (`RevokedAtUtc` doldur), yenisini üret ve kaydet, ikisini de tek bir `SaveChangesAsync` çağrısında veritabanına yaz.

```csharp
private static string GenerateSecureToken()
{
    var randomBytes = RandomNumberGenerator.GetBytes(32);
    return Convert.ToBase64String(randomBytes);
}
```
Kavram 20 — birebir sandbox'taki gibi.

### `FitForge.Api/Controllers/AuthController.cs` (değişti)

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
{
    var rotation = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);

    if (!rotation.Succeeded || rotation.UserId is null || rotation.NewToken is null)
    {
        return Unauthorized();
    }

    var user = await userManager.FindByIdAsync(rotation.UserId);
    if (user is null)
    {
        return Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var accessToken = jwtTokenGenerator.CreateToken(user.Id, user.Email!, roles);

    return Ok(new LoginResponse(accessToken.AccessToken, accessToken.ExpiresAtUtc, rotation.NewToken));
}
```
Akış: rotasyonu dene → başarısızsa 401 → başarılıysa kullanıcıyı bul (rollerini almak için) → yeni bir access token üret → hem yeni access token'ı hem yeni refresh token'ı (`rotation.NewToken`) tek bir `LoginResponse` olarak dön.

### Migration — `AddRefreshTokens`

`RefreshTokens` tablosunu, `AspNetUsers`'a bağlayan bir foreign key (`FK_RefreshTokens_AspNetUsers_UserId`, `ON DELETE CASCADE` — bir kullanıcı silinirse, token'ları da otomatik silinir) ile oluşturdu. `UserId` üzerinde bir index de eklendi (bir kullanıcının token'larını hızlı bulabilmek için).

---

## 5. Ne test ettik, nasıl doğruladık?

1. `dotnet build` — 0 hata.
2. Migration'ı oluşturup uyguladım — `RefreshTokens` tablosu gerçekten oluştu.
3. `POST /api/auth/login` → artık `accessToken`, `expiresAtUtc`, **ve** `refreshToken` dönüyor.
4. `POST /api/auth/refresh` (geçerli refresh token ile) → yeni bir `accessToken` **ve yeni bir** `refreshToken` döndü, HTTP 200.
5. **Aynı (eski) refresh token'ı tekrar denedim** → 401. Rotasyon çalışıyor.
6. **Rotasyondan gelen yeni (henüz kullanılmamış) token'ı da test ettim** → **o da 401 döndü** — çalınma tespiti tetiklenip o kullanıcının tüm aktif token'larını iptal ettiğini kanıtladı.
7. Tamamen sahte bir token (`"totally-fake-token"`) → 401.

### Sen nasıl test edersin?

```powershell
cd backend
dotnet build
dotnet run --project src/FitForge.Api
```
Login yap, dönen `refreshToken`'ı al, `/api/auth/refresh`'e gönder — yeni bir çift almalısın. Aynı refresh token'ı ikinci kez göndermeyi dene — 401 almalısın.

### Olası hata

Docker Desktop kapalıysa (`docker compose ps` boş dönerse), önce onu açman gerekir — container'lar `restart: unless-stopped` sayesinde Docker Desktop açılınca otomatik ayağa kalkar, veriler (named volume sayesinde) kaybolmaz.

---

## 6. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| Refresh token'ı veritabanında sakladık | İptal edilebilir olması gerekiyordu | Access token gibi "sadece imzayla doğrulanan" bir şey olsaydı, hiç iptal edemezdik |
| Her kullanımda rotate ettik (eskiyi iptal, yeni üret) | Bir token'ın kullanım ömrünü sınırlamak için | Çalınan bir token, süresi dolana kadar (7 gün) sınırsız kullanılabilirdi |
| Zaten iptal edilmiş bir token tekrar denenince TÜM token'ları iptal ettik | Çalınma şüphesinde en güvenli tepkiyi vermek için | Saldırgan, fark edilmeden erişimini sürdürebilirdi |
| `IJwtTokenGenerator`'ı sade `AccessTokenResult` dönecek şekilde tuttuk | Tek sorumluluk — JWT üretimiyle refresh token'ı ayrı tutmak için | Bir sınıf hem JWT hem refresh token mantığını taşırdı, test etmesi ve değiştirmesi zorlaşırdı |

---

**Sıradaki gün:** Day 9 — Logout ve Token İptali.
