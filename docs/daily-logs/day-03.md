# Day 3 — API'yi SQL Server'a Bağlama (Entity Framework Core)

> Bu dosyayı tek başına okuyup, bugün ne yaptığımızı, neden yaptığımızı ve nasıl yaptığımızı sıfırdan anlayabilmelisin. Hiçbir C#/.NET terimi bilmediğin varsayılıyor — her terim ilk geçtiği yerde açıklanıyor.

---

## 1. Bugün çözdüğümüz problem neydi?

Day 1'de bir API iskeleti kurduk, Day 2'de SQL Server'ı Docker container'ı olarak ayağa kaldırdık. Ama bu ikisi birbirinden **tamamen habersizdi**. API çalışıyordu, SQL Server çalışıyordu, ama API'nin içindeki hiçbir kod SQL Server'a "merhaba" bile demiyordu.

Bugünün tek amacı: **API'nin SQL Server'a gerçekten bağlanabildiğini kanıtlamak.** Bunu yaparken 3 şeye ihtiyacımız vardı:

1. SQL Server'a nasıl bağlanılacağını söyleyen bir bilgi (adres, kullanıcı adı, şifre).
2. Bu bilgiyi kullanıp gerçekten bağlantı kuran bir kod parçası.
3. "Gerçekten çalışıyor mu?" sorusuna kanıt: SQL Server üzerinde, API'nin komutuyla oluşmuş, gözle görülür bir sonuç (bir veritabanı).

Bu üçünü sırayla nasıl kurduğumuzu anlatacağım.

**Bunu yapmasaydık ne olurdu?** Day 4'ten itibaren yazacağımız "kullanıcı", "egzersiz", "antrenman kaydı" gibi her şey aslında SQL Server'da bir tabloya karşılık gelecek. Bugünkü bağlantı kurulmadan, o tablolar hiçbir yere yazılamaz — API'nin verileri kalıcı olarak saklayabilmesinin ön koşulu bu.

---

## 2. Yeni terimler (sırayla, ilk kullanıldıkları yerde tanımlı)

Bu bölümü referans olarak kullan — aşağıdaki "dosya dosya" bölümünde bu terimlere tekrar tekrar döneceğiz.

| Terim | Basit tanım |
|---|---|
| **NuGet paketi** | .NET dünyasının hazır kod kütüphanesi sistemi. Node.js'teki `npm install` neyse, .NET'te `dotnet add package` de o. Başkalarının yazdığı, senin projene eklediğin hazır kod parçaları. |
| **ORM (Object-Relational Mapper)** | Veritabanı tablolarıyla, C# nesneleri (class'lar) arasında çeviri yapan araç. SQL yazmak yerine C# nesneleriyle çalışmanı sağlar. |
| **Entity Framework Core (EF Core)** | Microsoft'un resmi ORM'i — .NET'te veritabanı işlemleri için standart araç. Biz bunu NuGet paketi olarak ekledik. |
| **DbContext** | EF Core'un verdiği, "veritabanına açılan tek kapı" sınıfı. Kodun her yerinde veritabanıyla konuşmak istediğinde bu sınıf üzerinden geçersin. |
| **Connection string** | "Hangi sunucuya, hangi veritabanına, hangi kullanıcı adı/şifreyle bağlanılacak" bilgisini tek bir metin cümlesinde tutan format. Örnek: `Server=localhost,1433;Database=FitForgeDb;User Id=sa;Password=...` |
| **Dependency Injection (DI) / servis konteyneri** | ASP.NET Core'un, uygulama başlarken "bana şu araç lazım olacak, sen onu hazırla, ben istediğimde ver" dediğin sistemi. KeystoneJS/GraphQL'de bir resolver'a `context.db` gibi hazır bir nesnenin gelmesine benzer — sen onu resolver içinde elle kurmuyorsun, biri (framework) en baştan hazırlayıp sana veriyor. .NET'te bu "biri" `Program.cs`'teki `builder.Services` denen servis konteyneridir. |
| **Extension method** | C#'a özgü bir özellik: var olan bir tipe (sınıfa), o sınıfın orijinal koduna dokunmadan, dışarıdan yeni bir fonksiyon "yapıştırma" yeteneği. Bunu birazdan somut örnekle göreceğiz. |
| **appsettings.json** | Projenin ayar dosyası (ortam bazlı config). Node'daki `.env` / `config.js` dosyalarının benzeri. |
| **User Secrets** | .NET'in, **sadece senin bilgisayarında**, proje klasörünün tamamen dışında, gizli ayarları (şifre gibi) sakladığı sistem. `.env` dosyasını git'e hiç eklememek gibi düşünebilirsin — ama burada dosya proje klasöründe bile değil, tamamen ayrı bir yerde. |
| **Migration** | "Veritabanında şunu değiştir" diyen, EF Core'un otomatik ürettiği bir talimat dosyası. Veritabanı için bir git commit'i gibi düşünebilirsin — her migration, şemadaki bir değişikliği kaydeder. |
| **Startup project / migrations project** | `dotnet ef` komutlarını çalıştırırken iki proje devreye girer: migration dosyalarının **yazıldığı** proje (bizde `FitForge.Infrastructure`) ve gerçek uygulamayı **ayağa kaldırıp ayarları okuyan** proje (bizde `FitForge.Api`, buna "startup project" deniyor). |

---

## 3. Neden bu şekilde yaptık? (Kararların arkasındaki mantık)

### Neden şifreyi hiçbir dosyaya yazılı bırakmadık?

CLAUDE.md'deki kural: "secrets kod içinde/repoda olmayacak." SQL Server şifresini `appsettings.json`'a yazsaydık, bu dosya git'e commit'lenip GitHub'a gidecekti — yani şifre herkese açık olurdu. Bunun yerine **User Secrets** kullandık: gerçek connection string, senin bilgisayarında `%APPDATA%` altında, proje klasörünün tamamen dışında bir dosyada duruyor. Projede sadece "bu bilgiyi ara" diyen bir referans numarası (`UserSecretsId`) var, şifrenin kendisi yok.

**Bunu yapmasaydık ne olurdu?** Şifre GitHub'da herkese açık dururdu — bu, gerçek şirketlerde ciddi bir güvenlik ihlali sayılır (ve otomatik tarayıcı botlar GitHub'ı sürekli "sızan şifre" için tarar).

### Neden `DbContext` şu an tamamen boş?

`Domain` projemizde (Day 1'de kurduğumuz) henüz hiçbir entity (kullanıcı, egzersiz gibi bir C# sınıfı) yok. `DbContext`, entity'leri tabloya çeviren araç olduğu için, çevirecek entity yoksa şu an boş olması **doğru ve beklenen** bir durum. Day 4'te temel yapı taşları (`BaseEntity`), sonraki günlerde gerçek entity'ler eklenince, `DbContext` de dolmaya başlayacak.

### Neden `Microsoft.EntityFrameworkCore.Design` paketini iki projeye de eklemek zorunda kaldık?

İlk denemede sadece `FitForge.Infrastructure`'a eklemiştim (mantıklı geliyordu, çünkü `DbContext` orada). Ama `dotnet ef migrations add` komutu şu hatayı verdi:

> *"Your startup project 'FitForge.Api' doesn't reference Microsoft.EntityFrameworkCore.Design"*

Sebebi: migration oluşturma komutu, migration dosyasını `Infrastructure`'a yazsa da, arka planda gerçek `FitForge.Api` uygulamasını ayağa kaldırıp oradaki ayarları (connection string'i User Secrets'tan okumak dahil) kullanıyor. Yani hem "dosyanın yazılacağı yer" (Infrastructure) hem "uygulamanın gerçekten çalıştığı yer" (Api) bu paketi bilmek zorunda. Bunu ilk seferde bilmiyordum, hata alınca öğrendik — gerçek geliştirmede sık karşılaşılan, normal bir durum.

---

## 4. Dosya dosya, satır satır

### Dosya 1 — `backend/src/FitForge.Infrastructure/FitForge.Infrastructure.csproj` (değişti)

Bu dosya, `.csproj` uzantılı — her .NET projesinin "bu proje ne içeriyor, hangi paketlere/projelere bağımlı" bilgisini tuttuğu dosya. Node'daki `package.json`'ın C# karşılığı gibi düşünebilirsin.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\FitForge.Application\FitForge.Application.csproj" />
    <ProjectReference Include="..\FitForge.Domain\FitForge.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.10" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

Satır satır:
- **Satır 1:** `<Project Sdk="Microsoft.NET.Sdk">` — bu dosyanın bir .NET proje tanımı olduğunu söylüyor. Değişmedi, zaten Day 1'den beri vardı.
- **Satır 3-6:** `<ItemGroup>` içindeki `<ProjectReference>` satırları — bu projenin, kendi projemizden hangi **diğer** projelere bağımlı olduğunu söylüyor (`Application` ve `Domain`). Bunlar da Day 1'den kalma, değişmedi.
- **Satır 8-14 (bugün eklenen kısım):** İkinci bir `<ItemGroup>`, bu sefer **dış dünyadan** (NuGet'ten) indirilen paketleri listeliyor:
  - `Microsoft.EntityFrameworkCore.Design` — EF Core'un migration oluşturma gibi "geliştirme zamanı" (design-time) araçlarını içeren paket. `<IncludeAssets>` ve `<PrivateAssets>` satırları, "bu paket sadece geliştirme sırasında kullanılsın, gerçek uygulama çalışırken (production'da) gerekmesin" demenin teknik yolu — bunu genelde Visual Studio/`dotnet add package` otomatik ekler, elle yazman gerekmez.
  - `Microsoft.EntityFrameworkCore.SqlServer` — EF Core'un "SQL Server ile nasıl konuşulur" bilgisini içeren paket. EF Core'un kendisi veritabanı-bağımsız bir çekirdek; hangi veritabanını (SQL Server, PostgreSQL, vb.) kullanacağını bu ayrı paket belirliyor.
- **Satır 16-20:** `<PropertyGroup>` — projenin genel ayarları (hedef .NET sürümü vb.), Day 1'den beri aynı, değişmedi.

**Bu dosyayı sen hiç elle düzenlemedin** — `dotnet add package` komutunu çalıştırdığımda otomatik güncellendi. Node'da `npm install` dediğinde `package.json`'ın otomatik güncellenmesi gibi düşün.

---

### Dosya 2 — `backend/src/FitForge.Infrastructure/Persistence/FitForgeDbContext.cs` (yeni)

```csharp
using Microsoft.EntityFrameworkCore;

namespace FitForge.Infrastructure.Persistence;

public class FitForgeDbContext : DbContext
{
    public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options)
        : base(options)
    {
    }
}
```

Satır satır:
- **Satır 1:** `using Microsoft.EntityFrameworkCore;` — "bu dosyada `Microsoft.EntityFrameworkCore` paketindeki araçları kullanacağım" demek. Node'daki `import { DbContext } from 'something'` gibi düşün — C#'ta `import` yerine `using` kelimesi kullanılıyor.
- **Satır 3:** `namespace FitForge.Infrastructure.Persistence;` — bu dosyadaki kodun "adres"i. C#'ta her dosya bir namespace'e ait olur; bu, projenin klasör yapısına (`FitForge.Infrastructure` projesi, `Persistence` klasörü) karşılık geliyor. Aynı isimde bir sınıf başka bir namespace'te de olabilir, çakışmaz — tıpkı iki farklı klasörde aynı isimli iki dosya olabilmesi gibi.
- **Satır 5:** `public class FitForgeDbContext : DbContext` — yeni bir sınıf (class) tanımlıyoruz: `FitForgeDbContext`. `: DbContext` kısmı "bu sınıf, EF Core'un verdiği `DbContext` sınıfının **özelleştirilmiş bir versiyonu**" demek (buna "kalıtım/inheritance" denir — `FitForgeDbContext`, `DbContext`'in yapabildiği her şeyi otomatik yapabilir, üstüne kendi özelliklerimizi ekleyebiliriz). `public` kelimesi, bu sınıfın proje dışından da (örneğin `FitForge.Api`'den) görülüp kullanılabileceğini söylüyor.
- **Satır 7-10:** Bu bir **constructor** (yapıcı metod) — sınıfın "yeni bir tane oluşturulduğunda ilk çalışacak kod" bloğu. `DbContextOptions<FitForgeDbContext> options` parametresi, "bu DbContext'in hangi veritabanına, nasıl bağlanacağı" bilgisini **dışarıdan** alıyor — bağlantı bilgisi bu sınıfın içinde hiç yazılı değil, birazdan göreceğimiz `DependencyInjection.cs` dosyasından geliyor. `: base(options)` kısmı, bu bilgiyi doğrudan üst sınıfa (`DbContext`'e) iletiyor — kendimiz bir şey yapmıyoruz, sadece bilgiyi yukarı aktarıyoruz.

**Neden içi bu kadar boş?** Normalde bir `DbContext` içinde şöyle satırlar olur:
```csharp
public DbSet<Exercise> Exercises { get; set; }
```
Bu, "Exercises adında bir tablo var, C#'ta `Exercise` nesneleri olarak temsil ediliyor" demek. Ama bizim `Domain` projemizde henüz `Exercise` diye bir sınıf yok (Day 13'te gelecek) — o yüzden şu an hiç `DbSet` satırımız yok. Bu, hata değil, olması gereken hâl.

---

### Dosya 3 — `backend/src/FitForge.Infrastructure/DependencyInjection.cs` (yeni)

Bu, bugünün en önemli dosyası. Önce ne işe yaradığını anlatayım, sonra satır satır gireceğim.

**Ne işe yarıyor?** `Program.cs`'in (API'nin başlangıç dosyası) hiç EF Core bilmeden, tek satırla "veritabanı bağlantısını hazırla" diyebilmesini sağlıyor.

```csharp
using FitForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FitForgeDb")
            ?? throw new InvalidOperationException("Connection string 'FitForgeDb' was not found.");

        services.AddDbContext<FitForgeDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
```

Satır satır:
- **Satır 1-4:** `using` satırları — bu dosyanın ihtiyaç duyduğu dış araçları içeri alıyor: bir önceki dosyada yazdığımız `FitForgeDbContext`, EF Core'un kendisi, ayarları okumak için `Configuration` araçları, ve servis konteynerini (DI) yönetmek için `DependencyInjection` araçları.
- **Satır 8:** `public static class DependencyInjection` — burada **`static class`** dedik, normal `class` değil. Fark şu: normal bir class'tan `new` ile "örnek" (instance) oluşturursun (`new FitForgeDbContext(...)` gibi). `static class` ise hiç örneği olmayan, sadece içindeki hazır fonksiyonları çağırabildiğin bir "araç kutusu" gibi düşünülür — asla `new DependencyInjection()` yazmayız, direkt `DependencyInjection.AddInfrastructure(...)` gibi çağırırız (ya da birazdan göreceğin gibi daha da kısa bir yazımla).
- **Satır 10:** ```public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)``` — bu satırda iki yeni kavram var:
  - `IServiceCollection` — ASP.NET Core'un servis konteynerinin (yukarıda tanımladığımız DI) C#'taki adı. "Uygulama başlarken hangi araçların hazırlanacağının listesi" gibi düşün.
  - `this IServiceCollection services` — parametrenin başındaki **`this`** kelimesi, bu metodu bir **extension method** yapıyor (yukarıdaki terimler tablosunda tanımlamıştık). Bunun anlamı: artık elimizde her yerde bir `IServiceCollection` (yani `services`) varsa, üzerine sanki o sınıfın **doğuştan bir parçasıymış gibi** `.AddInfrastructure(...)` yazabiliriz — az sonra `Program.cs`'te tam olarak bunu yapacağız: `builder.Services.AddInfrastructure(...)`. Bu, ASP.NET Core'un kendisinin de kullandığı bir desen — `AddControllers()`, `AddOpenApi()` gibi gördüğün her şey aslında birer extension method.
  - `IConfiguration configuration` — ikinci parametre, uygulamanın tüm ayarlarına (appsettings.json + User Secrets + ortam değişkenleri, hepsi birleşik) erişim sağlayan nesne.
- **Satır 12-13:**
  ```csharp
  var connectionString = configuration.GetConnectionString("FitForgeDb")
      ?? throw new InvalidOperationException("Connection string 'FitForgeDb' was not found.");
  ```
  `configuration.GetConnectionString("FitForgeDb")` — ayarların içinden `ConnectionStrings:FitForgeDb` adlı değeri arıyor (bunu User Secrets'a kaydetmiştik). `??` işareti, "eğer solundaki değer bulunamazsa (null ise), sağındakini yap" demek — yani connection string hiçbir yerde bulunamazsa, uygulama anlamlı bir hata mesajıyla (`InvalidOperationException`) **hemen** durur. Bu bilinçli bir tercih: yanlış/eksik ayarla sessizce çalışıp ileride garip hatalar vermek yerine, en başta net bir şekilde patlaması daha güvenli.
- **Satır 15-16:**
  ```csharp
  services.AddDbContext<FitForgeDbContext>(options =>
      options.UseSqlServer(connectionString));
  ```
  Burada asıl işi yapıyoruz: "Servis konteynerine, `FitForgeDbContext` istendiğinde nasıl hazırlanacağını öğret." `UseSqlServer(connectionString)` de "bu DbContext, SQL Server'a, şu connection string ile bağlanacak" demek. Bu satırdan sonra, uygulamanın herhangi bir yerinde "bana bir `FitForgeDbContext` ver" dendiğinde, .NET bu tarifi kullanarak otomatik hazırlayıp verecek — tıpkı GraphQL resolver'ına `context.db`'nin hazır gelmesi gibi, sen elle `new FitForgeDbContext(...)` yazmak zorunda kalmıyorsun.
- **Satır 18:** `return services;` — extension method'ların yaygın bir deseni: aldığın nesneyi (`services`) sonunda geri döndürüyorsun ki, birden fazla `.AddXxx()` çağrısını zincirleme (`services.AddControllers().AddOpenApi().AddInfrastructure(...)` gibi) yazabilesin. Biz zincirlemedik ama bu yüzden metodun imzasında `IServiceCollection` dönüş tipi var.

---

### Dosya 4 — `backend/src/FitForge.Api/Program.cs` (değişti — 2 satır eklendi)

Bu dosya, API başladığında en baştan sona çalışan tek dosya. Sadece yeni eklenen kısımlara odaklanalım (gerisini Day 1'de anlatmıştık):

```csharp
using FitForge.Infrastructure;             // 1. satır — yeni

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);   // yeni satır
```

- **`using FitForge.Infrastructure;`** — bir önceki dosyada yazdığımız `DependencyInjection` sınıfının bulunduğu namespace'i içeri alıyor. Bunu eklemeden `AddInfrastructure` metodunu göremeyiz.
- **`builder.Services.AddInfrastructure(builder.Configuration);`** — işte extension method'un gerçek kullanımı burada. `builder.Services`, ASP.NET Core'un servis konteyneri (`IServiceCollection`). Üzerine `.AddInfrastructure(...)` yazabiliyoruz çünkü bir önceki dosyada `this IServiceCollection services` ile onu "genişlettik". `builder.Configuration` da uygulamanın tüm ayarlarını (appsettings + User Secrets) temsil eden nesne — bunu `DependencyInjection.cs`'teki `configuration` parametresine geçiriyoruz.

Program.cs'in geri kalanı (health endpoint, controller kaydı vb.) Day 1'den beri aynı, değişmedi.

---

### Dosya 5 — `backend/src/FitForge.Api/FitForge.Api.csproj` (değişti)

İki değişiklik oldu:

```xml
<PropertyGroup>
  ...
  <UserSecretsId>60948200-d6a5-488e-91a4-31149a339bf0</UserSecretsId>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.10" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

- **`<UserSecretsId>60948200-...</UserSecretsId>`** — `dotnet user-secrets init` komutunu çalıştırdığımda otomatik eklendi. Bu, rastgele üretilmiş bir kimlik numarası — bu numara, gerçek şifrelerin bilgisayarında `%APPDATA%\Microsoft\UserSecrets\60948200-.../secrets.json` adlı dosyada durduğunu işaret ediyor. Bu satırın kendisinde **hiçbir şifre yok**, sadece "nereye bakılacağının" adresi var — bu yüzden git'e girmesinde sakınca yok.
- **`Microsoft.EntityFrameworkCore.Design` paket referansı** — bir önceki bölümde (madde 3) anlattığım nedenle, `dotnet ef` komutlarının çalışabilmesi için bu paketin **startup project** olan `Api`'de de olması gerekiyordu.

---

### Dosya 6 — User Secrets (repo'da dosya olarak görünmez, ama önemli)

Şu komutları çalıştırdık:
```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:FitForgeDb" "Server=localhost,1433;Database=FitForgeDb;User Id=sa;Password=FitForge_Dev_2026!;TrustServerCertificate=True"
```

`init` komutu yukarıdaki `UserSecretsId`'yi oluşturdu. `set` komutu ise gerçek connection string'i, proje klasörünün **tamamen dışında**, senin kullanıcı profilinde bir JSON dosyasına yazdı. Uygulama çalışırken, `IConfiguration` (Dosya 3'te gördüğümüz) bu dosyayı da otomatik okuyup `appsettings.json`'daki ayarlarla birleştiriyor — sen hiçbir ek kod yazmadan.

**Bu dosyayı asla `git status` çıktısında göremezsin** çünkü proje klasörünün içinde bile değil. Day 2'deki `.env` dosyasıyla aynı amaca hizmet ediyor (gizli bilgiyi repo dışında tutmak), sadece .NET'in kendi standart yöntemi bu.

---

### Dosya 7, 8, 9 — Migration dosyaları (otomatik üretildi, elle yazmadık)

`dotnet ef migrations add InitialCreate` komutunu çalıştırdığımızda, EF Core şu 3 dosyayı otomatik oluşturdu:

#### 7) `20260728154418_InitialCreate.cs` — asıl migration

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
```

- Dosya adının başındaki `20260728154418` — migration'ın oluşturulduğu tarih/saat damgası (yıl-ay-gün-saat-dakika-saniye). Bu sayede birden fazla migration olduğunda, hangisinin daha önce/sonra geldiği isme bakarak bile anlaşılır.
- `public partial class InitialCreate : Migration` — bu sınıf, EF Core'un `Migration` sınıfından türetilmiş (Dosya 2'deki `DbContext` kalıtımıyla aynı mantık). `partial` kelimesi, "bu sınıfın kodu birden fazla dosyaya bölünmüş olabilir" demek — nitekim birazdan göreceğimiz `.Designer.cs` dosyası da aynı sınıfın (`InitialCreate`) başka bir parçası.
- **`Up(MigrationBuilder migrationBuilder)`** — "bu migration'ı **uygula**" dendiğinde çalışacak kod. İçi şu an **tamamen boş** çünkü oluşturacağımız hiçbir tablo yok (Domain'de entity yok).
- **`Down(MigrationBuilder migrationBuilder)`** — "bu migration'ı **geri al**" dendiğinde çalışacak kod (`Up`'ın tam tersi). O da boş.

Day 4'ten sonra ilk gerçek entity eklendiğinde, yeni bir migration'da `Up()` içinde şöyle satırlar göreceğiz:
```csharp
migrationBuilder.CreateTable(name: "Exercises", ...);
```

#### 8) `20260728154418_InitialCreate.Designer.cs` ve 9) `FitForgeDbContextModelSnapshot.cs`

Bu iki dosyanın içeriği neredeyse birebir aynı ve şunu söylüyor: *"Bu migration'dan sonra, modelin (yani DbContext'in) tam hâli budur."* Şu an model bomboş olduğu için içerikleri de neredeyse boş:

```csharp
modelBuilder
    .HasAnnotation("ProductVersion", "10.0.10")
    .HasAnnotation("Relational:MaxIdentifierLength", 128);

SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
```

Bunlar sadece "EF Core sürümü şu, SQL Server kimlik (identity/otomatik artan Id) sütunlarını şu şekilde oluşturur" gibi teknik ayar bilgileri — hiçbir tablo tanımı yok çünkü hiç tablomuz yok.

**Önemli:** Bu iki dosyayı (`.Designer.cs` ve `ModelSnapshot.cs`) **sen asla elle düzenlemeyeceksin.** Her yeni migration eklediğinde EF Core bunları otomatik günceller. Tek işin, bunları git'e commit'lemek — çünkü bunlar olmadan EF Core "bir önceki migration'dan sonra model neye benziyordu?" sorusunu cevaplayamaz ve bir sonraki migration'ı doğru üretemez.

---

## 5. Ne test ettik, nasıl doğruladık?

1. `dotnet build` ile tüm solution'ın hâlâ derlendiğini doğruladık (yeni kod bir şeyi bozmadı).
2. `dotnet ef database update` komutunu çalıştırdık — çıktıda `CREATE DATABASE [FitForgeDb]` ve `CREATE TABLE [__EFMigrationsHistory]` SQL komutlarının gerçekten çalıştığını gördük.
3. `sqlcmd` ile (Day 2'de kurduğumuz container'ın içinden) bağımsız olarak kontrol ettik:
   - `sys.databases` listesinde `FitForgeDb` gerçekten var mı? → Evet.
   - `__EFMigrationsHistory` tablosunda bizim `InitialCreate` migration'ımız kayıtlı mı? → Evet.
4. API'yi tekrar çalıştırdık — `AddInfrastructure()` çağrısı hata vermeden ayağa kalktı, `/health` hâlâ `Healthy` döndü (yani DbContext'in DI'ya kaydı bir şeyi bozmadı).

### Sen nasıl test edersin?

SSMS'i aç (Day 2'de kurduğun), `localhost,1433` sunucusuna `sa` kullanıcısıyla bağlan, sol taraftaki "Databases" klasörüne sağ tıklayıp **Refresh** de. `FitForgeDb` adında yeni bir veritabanı görmelisin. İçini açtığında `dbo.__EFMigrationsHistory` tablosunu bulacaksın.

### Olası hata

`dotnet ef` komutlarından biri "bağlantı kurulamadı" gibi bir hata verirse, ilk kontrol edeceğin şey: `docker compose ps` ile SQL Server container'ının gerçekten `Up` durumda olduğundan emin ol (Day 2). Container kapalıysa, elbette API da ona bağlanamaz.

---

## 6. Hızlı özet tablosu

| Ne yaptık | Neden | Olmasaydı ne olurdu |
|---|---|---|
| EF Core + SQL Server paketlerini ekledik | C# nesneleriyle veritabanı işlemi yapabilmek için | SQL'i elle, string olarak yazıp yönetmemiz gerekirdi — hataya çok açık, EF Core'un sağladığı güvenlik/kolaylık olmazdı |
| Boş bir `FitForgeDbContext` oluşturduk | Veritabanına açılan "kapı"yı en baştan kurmak, entity'ler geldikçe genişletmek için | İleride her entity eklediğimizde sıfırdan bağlantı kurmamız gerekirdi |
| `AddInfrastructure()` extension method'u yazdık | `Program.cs`'i EF Core'dan tamamen habersiz tutmak için (Clean Architecture kuralı) | Api projesi doğrudan EF Core'a bağımlı olurdu, katmanlar arası sınır bozulurdu |
| Connection string'i User Secrets'a yazdık, appsettings.json'a değil | Şifrenin GitHub'a gitmesini engellemek için | SQL Server şifresi herkese açık bir şekilde repo'da dururdu |
| İlk migration'ı oluşturup uyguladık | Bağlantının gerçekten çalıştığını somut, gözle görülür şekilde kanıtlamak için | "Bağlanıyor mu, bağlanmıyor mu?" sorusunun cevabını hiçbir zaman kesin bilemezdik |

---

**Sıradaki gün:** Day 4 — `BaseEntity` ve `AuditableEntity` (her tabloda tekrar edecek ortak alanların — Id, CreatedAt, UpdatedAt — temelini atacağız).
