# Day 4 — BaseEntity, AuditableEntity ve Soft Delete Temeli

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Yeni C# terimleri burada da tanımlı, ama daha derin örnekler/canlı hata gösterimleri için `docs/CSHARP_FUNDAMENTALS.md`'deki **Kavram 10** (abstract class, `virtual`/`override`) ve **Kavram 11** (`ChangeTracker`/`EntityState`) bölümlerine bakabilirsin — bugünkü kod tam olarak o iki kavramın üzerine kuruldu.

---

## 1. Bugün çözdüğümüz problem neydi?

İleride `Exercise`, `WorkoutLog`, `UserProfile` gibi onlarca entity (veritabanı tablosuna karşılık gelen C# sınıfı) yazacağız. Hepsinde tekrar tekrar aynı üç şey olacak:
1. Bir kimlik (`Id`).
2. "Ne zaman oluşturuldu, ne zaman değişti" bilgisi (audit/denetim izi).
3. "Silinmiş mi" bilgisi — ama **gerçekten silinmeden**, geri alınabilir şekilde (soft delete).

Bugün bu üçünü, her entity'de tekrar tekrar yazmak yerine **bir kere**, iki temel sınıfta (`BaseEntity`, `AuditableEntity`) topladık. İleride yazacağımız her entity bunlardan miras alacak.

**Bunu yapmasaydık ne olurdu?** Her entity'ye elle `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt` eklemek gerekirdi — 30 entity yazınca bu alanları 30 kere kopyala-yapıştır yapmış olurduk, biri unutulursa tutarsızlık oluşurdu. Soft delete olmasaydı, biri yanlışlıkla bir kaydı silse, o veri **sonsuza dek** giderdi.

---

## 2. Yeni terimler (kısa hatırlatma — derinlemesine anlatımı `CSHARP_FUNDAMENTALS.md`'de)

| Terim | Basit tanım |
|---|---|
| **abstract class** | Kendisinden asla doğrudan nesne (`new`) üretilemeyen, sadece başka sınıfların türeyebileceği bir şablon sınıf. |
| **virtual / override** | Üst sınıf bir metodu `virtual` işaretler ("alt sınıflar bunu değiştirebilir"), alt sınıf `override` ile kendi versiyonunu yazar. |
| **`ChangeTracker`** | `DbContext`'in, o an ilgilendiği tüm nesneleri ve her birine ne olduğunu (eklendi/değişti/silindi) not tuttuğu yer. |
| **`EntityState`** | Bir nesnenin `ChangeTracker`'daki durumu: `Added`, `Modified`, `Deleted`, `Unchanged`. |
| **Soft delete** | Bir kaydı veritabanından gerçekten silmek yerine, "silinme tarihi" alanını doldurup kaydı olduğu gibi bırakmak. |

---

## 3. Neden bu şekilde yaptık?

### Neden `BaseEntity`/`AuditableEntity` `FitForge.Domain`'de, `FitForge.Infrastructure`'da değil?

Day 1'de kurduğumuz kurala göre `Domain` hiçbir dış pakete (EF Core dahil) bağımlı olmayacaktı. `BaseEntity`/`AuditableEntity` içinde EF Core'a özel hiçbir şey yok (sadece `Guid`, `DateTime` gibi saf C# tipleri) — bu yüzden Domain'e koymak doğru: yarın EF Core'u başka bir ORM ile değiştirsek bile, bu iki sınıf hiç değişmeden kalır.

### Neden `CreatedBy`/`UpdatedBy` eklemedik?

Roadmap bunu "opsiyonel" olarak işaretlemişti. Şu an "kim yaptı" sorusunu cevaplayacak bir kullanıcı/kimlik sistemimiz yok (Identity, Day 6'da gelecek). Şimdi eklesek, hep boş kalan, hiçbir işe yaramayan bir alan olurdu — bu yüzden Day 6-7'den sonra, gerçekten dolduracak bir mekanizmamız olduğunda ekleyeceğiz.

### Neden global query filter (soft-delete'li kayıtları otomatik gizleme) eklemedik?

Bu filtreyi yazmak için "filtrelenecek gerçek bir tablo" gerekiyor — ama Domain'de henüz hiç entity yok. İlk gerçek entity (Day 13, `Exercise`) geldiğinde ekleyeceğiz.

### Neden migration oluşturmadık?

`BaseEntity`/`AuditableEntity` `abstract` oldukları ve hiçbir `DbSet<T>` onlara doğrudan işaret etmediği için, EF Core bunları henüz "model"in bir parçası saymıyor — yani veritabanında karşılıklarında hiçbir tablo yok, oluşturulacak bir şey de yok. Day 3'teki gibi: gerçek tablolar, gerçek entity'ler (Day 13+) geldiğinde migration'la gelecek.

---

## 4. Dosya dosya, satır satır

### Dosya 1 — `backend/src/FitForge.Domain/Common/BaseEntity.cs` (yeni)

```csharp
namespace FitForge.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```
- `namespace FitForge.Domain.Common;` — bu sınıfın "adresi". `Common` klasörü, birden fazla entity'nin paylaşacağı ortak/genel şeyler için kullanılan standart bir isim.
- `public abstract class BaseEntity` — `abstract` kelimesi, "bu sınıftan asla doğrudan `new BaseEntity()` yazılamaz" demek. Sadece ondan türeyen gerçek sınıflar (`Exercise : AuditableEntity` gibi) nesne olarak üretilebilecek.
- `public Guid Id { get; set; } = Guid.NewGuid();` — her entity'nin bir kimliği olacak. `Guid` (Globally Unique Identifier), 128-bit'lik, pratikte çakışma ihtimali olmayan bir kimlik türü — SQL Server'ın otomatik artan sayılarına (`1, 2, 3...`) alternatif. `= Guid.NewGuid()` demek, "bu alan hiç değer verilmezse, otomatik olarak rastgele benzersiz bir kimlik üret" — yani bir nesne oluşturduğun an, kimliği zaten hazır olur, veritabanına gitmeyi beklemene gerek kalmaz.

### Dosya 2 — `backend/src/FitForge.Domain/Common/AuditableEntity.cs` (yeni)

```csharp
namespace FitForge.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```
- `public abstract class AuditableEntity : BaseEntity` — `AuditableEntity`, `BaseEntity`'den miras alıyor (Kavram 6), yani otomatik olarak bir `Id`'ye de sahip. Bu da kendisi `abstract` — doğrudan üretilemez, sadece ondan türeyen gerçek entity'ler üretilebilir.
- `public DateTime CreatedAt { get; set; }` — kaydın ne zaman oluşturulduğu. `?` işareti **yok** çünkü her kayıt kesinlikle bir oluşturulma anına sahip olacak, boş olamaz.
- `public DateTime? UpdatedAt { get; set; }` — `?` işareti **var** çünkü bir kayıt hiç güncellenmemiş olabilir (yeni oluşturulmuş, henüz değişmemiş) — bu durumda `null` (boş) kalır.
- `public DateTime? DeletedAt { get; set; }` — `null` ise kayıt hâlâ "aktif" demek; bir tarih değeri varsa, kayıt o tarihte "soft delete" edilmiş demek — ama satır veritabanında hâlâ duruyor.

### Dosya 3 — `backend/src/FitForge.Infrastructure/Persistence/FitForgeDbContext.cs` (değişti)

```csharp
using FitForge.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FitForge.Infrastructure.Persistence;

public class FitForgeDbContext : DbContext
{
    public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options)
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```
Satır satır (sadece yeni eklenen kısım):
- `using FitForge.Domain.Common;` — bir önceki iki dosyada tanımladığımız `AuditableEntity`'yi görebilmek için.
- `public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)` — EF Core'un `DbContext` sınıfında zaten `virtual` olan `SaveChangesAsync` metodunu **override** ediyoruz (Kavram 10). `async Task<int>` (Kavram 9): asenkron çalışır, sonunda "kaç satır etkilendi" bilgisini (`int`) verir. `CancellationToken cancellationToken = default` — CLAUDE.md'nin "cancellation token kullan" kuralı gereği, isteğin iptal edilebilmesini sağlayan parametre.
- `foreach (var entry in ChangeTracker.Entries<AuditableEntity>())` — o an takip edilen, `AuditableEntity`'den türeyen **her** nesneyi tek tek geziyoruz (Kavram 11). `Entries<AuditableEntity>()` generic (Kavram 4) — "bana sadece `AuditableEntity` tipindeki takip edilen nesneleri ver" diyor.
- `switch (entry.State)` — her nesnenin durumuna (`EntityState`) bakıp dallanıyoruz:
  - `EntityState.Added` → yeni eklenmiş, henüz kaydedilmemiş → `CreatedAt`'i şimdiki zamana (`DateTime.UtcNow`) ayarla. (`UtcNow` kullanıyoruz, yerel saat dilimi yerine — sunucular farklı saat dilimlerinde olabilir, UTC herkes için tek referans noktası.)
  - `EntityState.Modified` → var olan bir kayıt değişmiş → `UpdatedAt`'i güncelle.
  - `EntityState.Deleted` → biri bu kaydı silmeye çalışmış → **`entry.State = EntityState.Modified;`** ile durumu "değişti"ye çeviriyoruz (yani EF Core artık bunu bir `DELETE` SQL komutu olarak değil, `UPDATE` olarak gönderecek) ve `DeletedAt`'i dolduruyoruz. **Bu, soft delete'in tam olarak nasıl çalıştığı.**
- `return await base.SaveChangesAsync(cancellationToken);` — kendi ek işimizi yaptıktan sonra, asıl kaydetme işini (gerçek SQL komutlarını veritabanına gönderme) EF Core'un orijinal `SaveChangesAsync`'ine (`base.`) devrediyoruz — Kavram 6'daki `: base(name)` ile aynı mantık, sadece bir metotta.

---

## 5. Ne test ettik, nasıl doğruladık?

1. `dotnet build` — tüm solution 0 hata ile derlendi.
2. API'yi tekrar çalıştırdım — `FitForgeDbContext`'teki değişiklik hiçbir şeyi bozmadı, `/health` hâlâ `Healthy` döndü.
3. Henüz gerçek bir entity olmadığı için, soft delete davranışını **gerçek veritabanında** bugün test edemedik — bunu ilk gerçek entity geldiğinde (Day 13 sonrası) canlı olarak göreceğiz. Ama mantığın kendisini `learning-sandbox`'ta (Kavram 11), bellek içi sahte bir veritabanıyla, `Added`/`Modified`/`Deleted` durumlarını gerçekten üreterek doğruladık.

### Sen nasıl test edersin?

```powershell
cd backend
dotnet build
```
"Oluşturma başarılı oldu" ve "0 Hata" görmelisin.

### Olası hata

`AuditableEntity` içindeki `Guid`/`DateTime` tiplerini yanlış yazarsan (örneğin `Guid` yerine `guid` küçük harfle), C# derleyicisi "tür veya ad alanı bulunamadı" hatası verir — büyük/küçük harf C#'ta önemlidir.

---

## 6. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| `BaseEntity` (Id) oluşturduk | Her entity'nin ortak bir kimliği olsun, tekrar yazılmasın | Her entity'de elle `Id` tanımlamak gerekirdi |
| `AuditableEntity` (CreatedAt/UpdatedAt/DeletedAt) oluşturduk | Audit izi ve soft delete altyapısını bir kerede kurmak için | Her entity'de bu 3 alanı elle kopyalamak, birini unutma riski |
| `SaveChangesAsync`'i override ettik | Kaydetmeden hemen önce, otomatik olarak bu alanları doldurmak için | Her yerde elle `entity.CreatedAt = DateTime.UtcNow` yazmayı unutma riski |
| `CreatedBy`/`UpdatedBy` eklemedik | Henüz "kim yaptı" bilgisini dolduracak bir kimlik sistemi yok | Hep boş kalan, anlamsız bir alan olurdu |
| Global query filter eklemedik | Filtrelenecek gerçek bir entity henüz yok | Test edilemeyen, doğrulanamayan bir kod parçası olurdu |

---

**Sıradaki gün:** Day 5 — Global Exception Handling ve Standart API Response.
