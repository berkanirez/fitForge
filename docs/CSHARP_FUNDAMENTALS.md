# C# / .NET Temelleri — FitForge İçin Ön Hazırlık

> Bu dosya, FitForge'un günlük çalışmalarını (`docs/daily-logs/`) rahatça takip edebilmen için gereken C#/.NET dil ve ekosistem temellerini biriktiriyor. Sen SQL, Git, genel backend kavramlarını ve Node.js/KeystoneJS/GraphQL ekosistemini biliyorsun ama C#/.NET'e tamamen yenisin — bu dosyadaki her kavram, hiçbir ön bilgi varsayılmadan, sıfırdan anlatılıyor.
>
> Her kavram: (1) neden öğrenmen gerektiği, (2) küçük, çalıştırılabilir bir örnek (`learning-sandbox/` adlı ayrı, FitForge'un gerçek koduna hiç karışmayan bir deneme projesinde), (3) FitForge'un gerçek kodunda bunu nerede kullandığımız.
>
> Bu dosya **büyüyen bir referans** — yeni bir kavram işledikçe altına yeni bir bölüm ekleniyor, var olanlar değişmiyor. 40 günlük roadmap'in tamamını tarayan arka plan analizi tamamlandı (bkz. "Roadmap Analizi Sonucu" bölümü, en altta) — buradaki 9 kavram şimdi öğrenilecekler, geri kalanı roadmap'te ilgili güne geldiğimizde, o günün kendi "kavramı öğret" adımında işlenecek.

## Kavram listesi ve durumu

| # | Kavram | Durum |
|---|---|---|
| 1 | Class, object, constructor | ✅ İşlendi |
| 2 | Namespace & `using` | ✅ İşlendi |
| 3 | Static vs instance, `static class` | ✅ İşlendi |
| 4 | Generics (`<T>`) | ✅ İşlendi |
| 5 | Interface (`I` ile başlayan tipler) | ✅ İşlendi |
| 6 | Inheritance (kalıtım, `:` işareti) | ✅ İşlendi |
| 7 | Extension method (`this` parametresi) | ✅ İşlendi |
| 8 | Dependency Injection / IoC container | ✅ İşlendi |
| 9 | Async/await ve `Task<T>` | ✅ İşlendi |
| 10 | Abstract class, method overriding (`virtual`/`override`) | ✅ İşlendi |
| 11 | `ChangeTracker` / `EntityState` | ✅ İşlendi |
| — | *(kalan ~80 kavram)* | 📋 Roadmap'te ilgili güne geldiğimizde işlenecek — bkz. en alttaki tablo |

---

## Kavram 1 — Class, Object, Constructor

### Neden öğrenmen lazım?

FitForge'daki hemen her C# dosyası bir "class" tanımıyla başlıyor (`FitForgeDbContext`, ileride `Exercise`, `WorkoutPlan` vb.). Bunun ne olduğunu anlamadan hiçbir dosyayı gerçekten çözemezsin — bu yüzden ilk taş bu.

### En basit tanım

- **Class (sınıf)** = bir **şablon/kalıp**. "Bir Exercise'ın bir ismi ve set sayısı olacak" diye bir plan çiziyorsun, ama bu plan tek başına hiçbir gerçek egzersiz değil.
- **Object (nesne)** = o şablondan **üretilmiş gerçek bir şey**. Aynı şablondan birden fazla, birbirinden bağımsız nesne üretebilirsin.
- **Constructor (yapıcı metod)** = "yeni bir nesne oluşturulurken ilk çalışacak kod". `new Exercise("Push Up", 3)` dediğinde, C# senin için boş bir `Exercise` kutusu açar ve hemen constructor'ı çalıştırır — bu kutunun içini doldurmak için.

Eğer JS'te class yazdıysan (KeystoneJS config'inde görmüş olabilirsin), bu neredeyse birebir aynı:
```js
class Exercise {
  constructor(name, sets) {
    this.name = name;
    this.sets = sets;
  }
}
```

### Örnek (`learning-sandbox/Program.cs`)

```csharp
var pushUps = new Exercise("Push Up", 3);
var squats = new Exercise("Squat", 4);

Console.WriteLine($"{pushUps.Name} - {pushUps.Sets} sets");
Console.WriteLine($"{squats.Name} - {squats.Sets} sets");

public class Exercise
{
    public string Name;
    public int Sets;

    public Exercise(string name, int sets)
    {
        Name = name;
        Sets = sets;
    }
}
```

Çalıştırınca (`dotnet run`):
```
Push Up - 3 sets
Squat - 4 sets
```

### Satır satır

```csharp
var pushUps = new Exercise("Push Up", 3);
```
- `new Exercise(...)` → "Exercise şablonundan yeni bir nesne üret, constructor'a bu iki değeri (`"Push Up"`, `3`) ver."
- `var pushUps = ...` → sonucu `pushUps` adlı bir değişkende sakla. `var`, JS'teki `let`/`const` gibi düşünülebilir — C# aslında tipini biliyor (`Exercise` tipinde olduğunu anlıyor), sadece sana o tipi tekrar yazdırmıyor.

```csharp
public class Exercise
{
    public string Name;
    public int Sets;

    public Exercise(string name, int sets)
    {
        Name = name;
        Sets = sets;
    }
}
```
- `public class Exercise` → "Exercise adında, projenin her yerinden görülebilir (`public`) bir şablon tanımlıyorum."
- `public string Name;` ve `public int Sets;` → bu şablonun **alanları (field)** — her `Exercise` nesnesinin taşıyacağı veri. `string` = yazı, `int` = tam sayı. C#'ta her değişkenin tipini önceden söylersin (**statik tipleme**) — JS'in tip belirtmeden çalışan **dinamik tipleme**sinin tam tersi.
- `public Exercise(string name, int sets)` → constructor'ın kendisi. Dikkat: **sınıfla aynı isimde**, dönüş tipi yok — bir constructor'ı böyle tanırsın.
- `Name = name; Sets = sets;` → dışarıdan gelen `name`/`sets` parametrelerini, nesnenin kendi `Name`/`Sets` alanlarına kopyalıyoruz. Parametre adı küçük harfle (`name`), alan adı büyük harfle (`Name`) başladığı için C# ikisini karıştırmıyor, `this.` yazmaya gerek kalmıyor.

### Karşılaştığımız bir hata (bilerek not ediyorum, gerçek bir C# kuralı)

İlk denemede `class Exercise` tanımını dosyanın **en üstüne**, çalıştırılacak kodu (`var pushUps = ...`) altına yazmıştım. Şu hatayı aldık:

> `error CS8803: Üst düzey deyimler ad alanı ve tür bildirimlerinden önce gelmelidir.`

Anlamı: Bu tarz bir dosyada (buna **top-level statements** deniyor, C# 9'dan beri var), **çalıştırılacak kod her zaman class tanımlarından önce gelmeli**. Bu yüzden sırayı değiştirdik — önce çalışan kod, en altta class tanımı.

### FitForge'da nerede gördük / göreceğiz?

`backend/src/FitForge.Infrastructure/Persistence/FitForgeDbContext.cs`:
```csharp
public class FitForgeDbContext : DbContext
{
    public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options) : base(options) { }
}
```
`public class FitForgeDbContext` → bir şablon tanımı, tıpkı `Exercise` gibi. `public FitForgeDbContext(...)` → onun constructor'ı. Tek fark, bu constructor kendi içinde `Name = name` gibi basit atamalar yapmıyor, aldığı `options`'ı doğrudan üst sınıfa iletiyor (`: base(options)`) — bunu **Kavram 6 (inheritance)**'da tam olarak açacağız.

Ayrıca `FitForge.Api/Program.cs`'te hiç class tanımı **yok**, sadece çalışan kod var — çünkü orada bir şablon tanımlamaya ihtiyacımız olmadı, doğrudan üst düzey kod yeterliydi.

Aynı mantıkla ilerideki günlerde `Exercise`, `WorkoutPlan`, `UserProfile` gibi gerçek FitForge sınıflarını da tam olarak bu şekilde (`public class X { ... }`, kendi constructor'larıyla) yazacağız.

---

## Kavram 2 — Namespace & `using`

### Neden öğrenmen lazım?

Şu ana kadar gördüğün **her** C# dosyasının en tepesinde bir sürü `using X;` satırı var (`FitForgeDbContext.cs`, `Program.cs`, `DependencyInjection.cs`...). Bunların ne işe yaradığını bilmeden dosyaların en üstü senin için anlamsız bir "boilerplate" (kalıp/doldurma metin) yığını gibi kalır.

### En basit tanım

- **Namespace (ad alanı)** = kodun "adres"i / bulunduğu mantıksal klasör. Aynı isimde iki class farklı namespace'lerde var olabilir, birbirine karışmaz — tıpkı `Desktop/rapor.txt` ile `Documents/rapor.txt`'nin aynı isimde ama farklı dosyalar olması gibi.
- **`using X;`** = "ben bu dosyada `X` adres/namespace'indeki şeyleri kullanacağım, onları her seferinde uzun uzun yazmama gerek kalmasın." Bu, JS/Node'daki `import { Something } from 'somewhere'` ile aynı işi görüyor — sadece C#'ta biraz farklı yazılıyor ve "hangi dosyadan" değil "hangi mantıksal grup"tan aldığını söylüyorsun.

### Örnek — önce hatasını görelim, sonra düzeltelim

`learning-sandbox/Fitness/Exercise.cs` (yeni dosya, ayrı bir namespace'te):
```csharp
namespace Fitness;

public class Exercise
{
    public string Name;
    public int Sets;

    public Exercise(string name, int sets)
    {
        Name = name;
        Sets = sets;
    }
}
```

`Exercise` class'ını Kavram 1'deki `Program.cs`'ten çıkarıp buraya, `Fitness` adlı bir namespace'in içine taşıdık.

**İlk denemede `Program.cs`'e `using Fitness;` eklemeyi unuttum (bilerek):**
```csharp
var pushUps = new Exercise("Push Up", 3);   // using Fitness; YOK
```
Hata:
```
error CS0246: 'Exercise' türü veya ad alanı adı bulunamadı (bir using yönergeniz veya derleme başvurunuz mu eksik?)
```
C#, `Exercise` diye bir şey gördü ama "bu nerede tanımlı, hangi adres altında?" sorusuna cevap bulamadı — çünkü ona hiç söylemedik.

**Düzeltilmiş hâli:**
```csharp
using Fitness;

var pushUps = new Exercise("Push Up", 3);
var squats = new Exercise("Squat", 4);

Console.WriteLine($"{pushUps.Name} - {pushUps.Sets} sets");
Console.WriteLine($"{squats.Name} - {squats.Sets} sets");
```
Çıktı (aynı, çünkü sadece dosya organizasyonu değişti, mantık aynı kaldı):
```
Push Up - 3 sets
Squat - 4 sets
```

### İki farklı namespace yazım şekli (ikisini de göreceksin)

```csharp
// Yeni stil (C# 10+), tek satır, süslü parantez yok — biz bunu kullandık
namespace Fitness;

public class Exercise { ... }
```
```csharp
// Eski stil, süslü parantez içinde — Migration dosyalarında (Designer.cs, ModelSnapshot.cs) bunu görmüştün
namespace FitForge.Infrastructure.Persistence.Migrations
{
    partial class InitialCreate { ... }
}
```
İkisi de aynı işi yapıyor, sadece yazım farkı. Yeni projelerde genelde ilk (tek satırlık) stil tercih ediliyor; EF Core'un otomatik ürettiği dosyalarda ise eski stil kullanılıyor — bunu sen seçmiyorsun, araç öyle üretiyor.

### FitForge'da nerede gördük?

`FitForge.Infrastructure/DependencyInjection.cs`'in en tepesinde:
```csharp
using FitForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitForge.Infrastructure;
```
- İlk satır: "bir önceki dosyada yazdığımız `FitForgeDbContext`'i kullanacağım" (`FitForge.Infrastructure.Persistence` namespace'inde tanımlıydı).
- Sonraki üç satır: Microsoft'un NuGet paketleriyle gelen hazır araçları kullanacağımızı söylüyor.
- `namespace FitForge.Infrastructure;` — bu dosyanın kendisinin adresi. Proje adıyla (`FitForge.Infrastructure`) birebir aynı olması tesadüf değil — .NET projelerinde namespace'ler genelde proje/klasör yapısını birebir yansıtır.

`FitForge.Api/Program.cs`'teki `using FitForge.Infrastructure;` satırı da tam olarak bu yüzden gerekliydi — `AddInfrastructure` extension metodunu (Kavram 7'de göreceğiz) görebilmek için.

---

## Kavram 3 — Static vs Instance, `static class`

### Neden öğrenmen lazım?

`FitForge.Infrastructure/DependencyInjection.cs` dosyasında `public static class DependencyInjection` yazmıştık. Bunu neden `static` yaptığımızı, `static` olmasaydı ne fark ederdi'yi anlamadan bu dosyayı gerçekten çözmüş sayılmazsın.

### En basit tanım

- **Instance (örnek) üye** = her nesnenin **kendine ait, ayrı** bir kopyası olan alan/metod. Kavram 1'deki `Exercise` böyleydi — `pushUps.Name` ile `squats.Name` birbirinden tamamen bağımsız.
- **Static üye** = nesneye değil, **sınıfın kendisine** ait, tek ve paylaşılan bir alan/metod. `new` ile hiç nesne üretmeden, doğrudan sınıf adı üzerinden çağrılır.
- **`static class`** = içindeki **her şeyin** static olduğu, hiç nesne üretilemeyen sınıflar. Genelde "bir sürü ilgili yardımcı fonksiyonu bir arada tutan araç kutusu" için kullanılır — kendi içinde saklayacak bir "durumu/verisi" yoktur.

### Örnek 1 — Instance: her nesnenin kendi verisi var

```csharp
public class RepCounter
{
    public int Count;

    public void Increment()
    {
        Count++;
    }
}
```
```csharp
var counterA = new RepCounter();
var counterB = new RepCounter();
counterA.Increment();
counterA.Increment();
counterB.Increment();
Console.WriteLine($"A: {counterA.Count}, B: {counterB.Count}");
```
Çıktı:
```
A: 2, B: 1
```
`counterA` ve `counterB`, aynı şablondan üretilmiş ama **birbirinden habersiz, ayrı** iki nesne. Birini artırmak diğerini etkilemiyor.

### Örnek 2 — Static: paylaşılan, nesnesiz araç

```csharp
public static class FitnessMath
{
    public static double EstimateOneRepMax(double weight, int reps)
    {
        return weight * (1 + reps / 30.0);
    }
}
```

**İlk denemede bilerek şunu yazdım:**
```csharp
var badAttempt = new FitnessMath();
```
Hata:
```
error CS0712: 'FitnessMath' statik sınıfının bir örneğini oluşturamaz
```
C# burada net: "Bu sınıf `static`, yani hiç nesnesi olamaz — `new` ile bir tane üretmeyi deneme bile."

**Doğru kullanım:**
```csharp
var oneRepMax = FitnessMath.EstimateOneRepMax(weight: 100, reps: 5);
Console.WriteLine($"Estimated 1RM: {oneRepMax}");
```
Çıktı:
```
Estimated 1RM: 116,66666666666667
```
(Virgül, ondalık ayracı için Windows'un Türkçe bölge ayarından geliyor — İngilizce ayarlarda nokta görürdün, sayının kendisiyle ilgisi yok.)

Dikkat et: `FitnessMath.EstimateOneRepMax(...)` — hiçbir yerde `new FitnessMath()` yok. Doğrudan **sınıf adı üzerinden** çağırdık, çünkü `EstimateOneRepMax`'ın çalışması için hiçbir nesneye özel veriye ihtiyacı yok — sadece verdiğin `weight` ve `reps`'i kullanıp bir hesap yapıyor.

### Ne zaman hangisi?

- Bir şeyin **kendine ait, zamanla değişen bir durumu/verisi** varsa (bir egzersiz kaydı, bir kullanıcı profili) → normal (instance) class.
- Bir şey sadece **girdi alıp çıktı üreten, kendi başına hiçbir veri saklamayan bir araç/yardımcı fonksiyon** ise → `static class`.

### FitForge'da nerede gördük?

`FitForge.Infrastructure/DependencyInjection.cs`:
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ...
    }
}
```
Bu sınıfın hiçbir "kendine ait verisi" yok — sadece dışarıdan `services` ve `configuration` alıp, bir kurulum işlemi yapan tek bir fonksiyonu var. Tam da `FitnessMath` gibi: bir araç kutusu. Bu yüzden `static class` olarak yazıldı, ve kodun hiçbir yerinde `new DependencyInjection()` görmeyeceksin — tıpkı `FitnessMath.EstimateOneRepMax(...)` gibi, doğrudan `DependencyInjection.AddInfrastructure(...)` şeklinde çağrılır (extension method sözdizimi sayesinde bunu `services.AddInfrastructure(...)` gibi daha da kısa yazabiliyoruz — bunu **Kavram 7**'de tam açacağız).

---

## Kavram 4 — Generics (`<T>`)

### Neden öğrenmen lazım?

`DbContextOptions<FitForgeDbContext>` (Kavram 1'de gördüğün constructor parametresi) ve ileride göreceğin `DbSet<Exercise>`, `List<Exercise>` gibi köşeli parantezli tipler — hepsi generic. Bu köşeli parantezleri "garip bir süsleme" gibi görmek yerine, ne anlama geldiğini bilmen lazım.

### En basit tanım

**Generic**, bir sınıfı **"içindeki tip ne olursa olsun aynı şekilde çalışacak şekilde"** yazabilmene yarıyor. `<T>` şu demek: "bu sınıf, senin belirteceğin herhangi bir tiple çalışabilir — ben şimdiden hangi tip olduğunu bilmiyorum, kullanan kişi söyleyecek."

**Kutunun üstündeki etiket** analojisi: Aynı boş kutu tasarımını (Box) düşün. Kutunun üstüne "İçinde Exercise var" ya da "İçinde sayı var" diye bir etiket yapıştırıyorsun — kutunun kendisi (Box) tek bir tasarım, ama her kutunun üstünde farklı bir etiket (`<Exercise>`, `<int>`) olabilir.

### Örnek

```csharp
public class Box<T>
{
    public T Content;

    public Box(T content)
    {
        Content = content;
    }
}
```
`Box<T>` — `T`, "tip parametresi" (Type). Sınıfı yazarken `T`'nin ne olacağını bilmiyoruz, kullanılırken belirleniyor.

```csharp
var exerciseBox = new Box<Exercise>(pushUps);
var numberBox = new Box<int>(42);

Console.WriteLine(exerciseBox.Content.Name);
Console.WriteLine(numberBox.Content);
```
Çıktı:
```
Push Up
42
```
`Box<Exercise>` dediğimizde, o kutunun `Content`'i **kesinlikle** bir `Exercise` olacak — `Box<int>` dediğimizde ise **kesinlikle** bir tam sayı olacak. Aynı `Box` şablonunu iki farklı "etiketle" kullandık.

### Neden bu işe yarıyor, JS'te bu problem nasıl olurdu?

**Bilerek yanlış tip deneyelim:**
```csharp
var brokenBox = new Box<int>("bu bir sayı değil");
```
Hata (kod çalışmadan, derleme anında):
```
error CS1503: 1 bağımsız değişkeni: 'string' öğesinden 'int' öğesine dönüştürülemiyor
```
C#, `Box<int>` dediğin an "bu kutunun içi sadece `int` olabilir" diye kesin bir kural koyuyor — yanlış tip vermeye çalışırsan, kod **çalışmadan önce**, derleme sırasında hatayı yakalıyor.

JS'te (dinamik tipleme nedeniyle) böyle bir kutu yazsan, içine istediğin tipi koyabilirsin, hata almazsın — ama kodun ilerleyen bir yerinde "sayı bekliyordum ama string geldi" diye **çalışma zamanında** (runtime'da), belki üretimde, kullanıcı karşısında patlayabilirsin. Generics, bu tür hataları en erken (yazarken/derlerken) yakalamanı sağlıyor.

### FitForge'da nerede gördük / göreceğiz?

`FitForgeDbContext.cs`:
```csharp
public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options) : base(options)
```
`DbContextOptions<FitForgeDbContext>` — "bu ayarlar (options) kutusu, özellikle `FitForgeDbContext` için hazırlanmış ayarları içeriyor" demek. Eğer ileride `FitForge.Infrastructure`'a başka bir `DbContext` daha eklersek (nadiren olur ama mümkün), her biri kendi `DbContextOptions<KendiDbContexti>` tipini kullanır — birbirine karışmaz.

`DependencyInjection.cs`:
```csharp
services.AddDbContext<FitForgeDbContext>(options => options.UseSqlServer(connectionString));
```
`AddDbContext<FitForgeDbContext>` — "servis konteynerine, `FitForgeDbContext` tipinde bir DbContext kaydet" demek. `<FitForgeDbContext>` kısmı olmasaydı, .NET hangi DbContext'i kaydettiğimizi bilemezdi.

İlerideki günlerde de sık göreceğin `DbSet<Exercise>` (Day 13'ten sonra), `List<Exercise>` gibi tipler hep aynı mantık: "bu koleksiyonun/kutunun içinde kesinlikle `Exercise` tipinde şeyler olacak."

---

## Kavram 5 — Interface (`I` ile başlayan tipler)

### Neden öğrenmen lazım?

`DependencyInjection.cs`'te iki parametre vardı: `IServiceCollection services` ve `IConfiguration configuration`. Bu ikisi de **somut bir sınıf değil, interface**. Bunun ne anlama geldiğini bilmeden, "neden `ServiceCollection` değil de `IServiceCollection`?" sorusu hep havada kalır.

### En basit tanım

**Interface (arabirim)** = bir **sözleşme/kontrat**. "Bu sözleşmeyi imzalayan her sınıf, şu metotlara sahip olmak **zorunda**" der — ama o metotların **içini nasıl dolduracağını hiç söylemez**, sadece isim ve imzasını (parametreleri, dönüş tipini) belirler. İsimlerinin başına `I` konması bir .NET geleneği/yazım kuralı (`IServiceCollection`, `IConfiguration`, ileride yazacağımız `IExerciseRepository` gibi) — sadece bir isimlendirme alışkanlığı, dilin zorunlu kıldığı bir kural değil.

### Örnek

```csharp
public interface ITrainable
{
    void Train();
}
```
Bu, "bir şey `ITrainable` olmak istiyorsa, **mutlaka** parametresiz bir `Train()` metodu olmalı" diyor — `Train()`'in içinde ne olacağını hiç söylemiyor.

```csharp
public class Runner : ITrainable
{
    public void Train() => Console.WriteLine("Running 5k...");
}

public class Lifter : ITrainable
{
    public void Train() => Console.WriteLine("Lifting weights...");
}
```
İki farklı sınıf, aynı sözleşmeyi (`ITrainable`) imzalıyor ama `Train()`'i **tamamen farklı** şekillerde dolduruyor.

**Asıl güç burada:**
```csharp
void RunTrainingSession(ITrainable athlete)
{
    athlete.Train();
}

RunTrainingSession(new Runner());
RunTrainingSession(new Lifter());
RunTrainingSession(new Swimmer());
```
Çıktı:
```
Running 5k...
Lifting weights...
Swimming laps...
```
`RunTrainingSession` metodu, **hangi somut sınıfla çalıştığını hiç bilmiyor/umursamıyor** — "bana `ITrainable` sözleşmesini imzalamış herhangi bir şey ver, ben `.Train()`'ini çağırırım" diyor. Bu sayede yarın yeni bir `Swimmer`, `Climber` sınıfı eklesek bile `RunTrainingSession`'ı hiç değiştirmemize gerek kalmaz — yeter ki `ITrainable`'ı imzalasın.

### Sözleşmeyi bozarsan ne olur?

**Bilerek eksik bıraktım:**
```csharp
public class Swimmer : ITrainable
{
    // Train() metodu YOK
}
```
Hata:
```
error CS0535: 'Swimmer', 'ITrainable.Train()' arabirim üyesini uygulamaz
```
C#, derleme anında "sen bu sözleşmeyi imzaladığını söyledin ama gerektirdiği şeyi yapmadın" diye seni durduruyor — çalışma zamanına kadar beklemiyor.

### FitForge'da nerede gördük?

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
```
- `IServiceCollection` — ASP.NET Core'un "servis kaydı listesi" sözleşmesi. Gerçekte arka planda somut bir sınıf (`ServiceCollection`) bu sözleşmeyi uyguluyor, ama bizim kodumuz o somut sınıfı hiç bilmek zorunda değil — tıpkı `RunTrainingSession`'ın `Runner` mı `Lifter` mı geldiğini bilmemesi gibi. Bu, ASP.NET Core'un ileride (örneğin testlerde) gerçek servis konteyneri yerine sahte/test amaçlı bir tane vermesine de izin veriyor — bizim `AddInfrastructure` metodumuz hiç fark etmez.
- `IConfiguration` — aynı mantık, "ayarlara erişim" sözleşmesi. Ayarlar appsettings.json'dan mı, User Secrets'tan mı, ortam değişkenlerinden mi geliyor — `AddInfrastructure` metodumuz hiç bilmiyor, sadece "bana `GetConnectionString(...)` çağırabileceğim bir şey ver" diyor.

İleride (Day 21 civarı) kendi interface'lerimizi de yazacağız, örneğin `IExerciseRepository` — "egzersizleri okuyup yazabilen herhangi bir şey" sözleşmesi. `FitForge.Application` bu sözleşmeyi tanımlayacak, `FitForge.Infrastructure` ise gerçek EF Core koduyla bu sözleşmeyi imzalayacak (`Runner : ITrainable` gibi) — Application katmanı, Infrastructure'ın EF Core kullandığını hiç bilmeyecek.

---

## Kavram 6 — Inheritance (Kalıtım, `:` işareti)

### Neden öğrenmen lazım?

`FitForgeDbContext.cs`'te `public class FitForgeDbContext : DbContext` ve `: base(options)` yazmıştık. Bu `:` işaretinin ne anlama geldiğini bilmeden bu satırı gerçekten çözemezsin — ve Day 4'te yazacağımız `BaseEntity`/`AuditableEntity` de tamamen bu kavrama dayanıyor.

### En basit tanım

**Inheritance (kalıtım)**: bir sınıf (**alt sınıf**), başka bir sınıfın (**üst/temel sınıf**) tüm alanlarını ve metotlarını **otomatik olarak** devralır — hiç yeniden yazmana gerek kalmadan. `: X` yazmak, "ben `X`'in bir alt sınıfıyım, onun her şeyine sahibim, üstüne kendi özelliklerimi de ekliyorum" demek.

### Örnek

```csharp
public class Athlete
{
    public string Name;

    public Athlete(string name)
    {
        Name = name;
    }

    public void Greet()
    {
        Console.WriteLine($"Hi, I'm {Name}.");
    }
}
```

Şimdi `Runner`'ı `Athlete`'ten türetelim (Kavram 5'teki `Runner` sınıfını güncelledik — hem `Athlete`'ten miras alıyor hem `ITrainable` sözleşmesini imzalıyor, ikisi bir arada olabiliyor):

**İlk denemede bilerek `: base(name)` yazmadım:**
```csharp
public class Runner : Athlete, ITrainable
{
    public double WeeklyDistanceKm;

    public Runner(string name, double weeklyDistanceKm)   // base(name) YOK
    {
        WeeklyDistanceKm = weeklyDistanceKm;
    }
    ...
}
```
Hata:
```
error CS7036: 'Athlete.Athlete(string)'nin gerekli 'name' parametresine karşılık gelen herhangi bir argüman yok
```
Sebebi: `Athlete`'in **parametresiz** bir constructor'ı yok — her `Athlete` (ve dolayısıyla her `Athlete`'ten türeyen şey) mutlaka bir `name` ile kurulmak zorunda. `Runner`, kendi constructor'ında bu `name`'i **üst sınıfa iletmezse**, C# "peki `Athlete` kısmı nasıl kurulacak?" diye soruyor ve hata veriyor.

**Düzeltilmiş hâli:**
```csharp
public class Runner : Athlete, ITrainable
{
    public double WeeklyDistanceKm;

    public Runner(string name, double weeklyDistanceKm) : base(name)
    {
        WeeklyDistanceKm = weeklyDistanceKm;
    }

    public void Train()
    {
        Console.WriteLine("Running 5k...");
    }
}
```
`: base(name)` → "gelen `name`'i doğrudan üst sınıfın (`Athlete`'in) constructor'ına ilet, o `Name` alanını doldursun."

**Kullanımı:**
```csharp
var berkan = new Runner("Berkan", 20);
berkan.Greet();
Console.WriteLine($"{berkan.Name} runs {berkan.WeeklyDistanceKm} km/week");
```
Çıktı:
```
Hi, I'm Berkan.
Berkan runs 20 km/week
```
Dikkat et: `Runner` sınıfının içinde **hiçbir yerde `Greet()` tanımlı değil**, ama `berkan.Greet()` çalışıyor — çünkü `Runner`, `Athlete`'ten miras aldığı için `Athlete`'in tüm yeteneklerine otomatik sahip. Aynı şekilde `berkan.Name` de `Athlete`'ten gelen bir alan.

### Bir sınıf hem miras alabilir hem sözleşme imzalayabilir

`public class Runner : Athlete, ITrainable` — virgülle ayırdık: `Athlete` bir **sınıf** (miras/inheritance), `ITrainable` bir **interface** (sözleşme). C#'ta bir sınıf **en fazla bir** sınıftan miras alabilir ama **istediği kadar** interface imzalayabilir.

### FitForge'da nerede gördük / göreceğiz?

```csharp
public class FitForgeDbContext : DbContext
{
    public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options)
        : base(options)
    {
    }
}
```
Şimdi bu satırı tam çözebiliyorsun: `FitForgeDbContext`, EF Core'un verdiği `DbContext` sınıfından **miras alıyor** — yani `DbContext`'in "veritabanına bağlan, sorgu çalıştır, kaydet" gibi tüm hazır yeteneklerine otomatik sahip oluyor, biz onları sıfırdan yazmıyoruz. `: base(options)` da tam olarak `Runner`'daki `: base(name)` ile aynı iş: gelen `options`'ı doğrudan `DbContext`'in kendi constructor'ına iletiyoruz, o da bağlantıyı bu bilgiyle kuruyor.

Day 4'te yazacağımız `BaseEntity` de bu mantığın ta kendisi: `Id`, `CreatedAt`, `UpdatedAt` gibi ortak alanları **bir kere** `BaseEntity`'de tanımlayacağız, sonra `Exercise : BaseEntity`, `WorkoutLog : BaseEntity` gibi onlarca sınıf bu alanları **hiç yeniden yazmadan** otomatik alacak.

---

## Kavram 7 — Extension Method (`this` parametresi)

### Neden öğrenmen lazım?

Bunu sen özellikle istemiştin — `DependencyInjection.cs`'teki `AddInfrastructure(this IServiceCollection services, ...)` satırındaki `this` kelimesi, `Program.cs`'te `builder.Services.AddInfrastructure(...)` yazabilmemizin **tüm sırrı**. Bu tek kelimeyi anlamadan o satır büyülü/açıklanamaz görünür.

### En basit tanım

**Extension method**, var olan bir tipe (senin yazmadığın, hatta kaynağına hiç erişemediğin bir tip olsa bile — mesela C#'ın kendi `int` tipi), **dışarıdan, o tipin orijinal koduna hiç dokunmadan**, yeni bir metod "yapıştırmana" izin veren bir C# özelliği. Bunu yapmanın tek şartı: bir `static class` içinde, bir `static` metod yaz, **ilk parametresinin önüne `this` koy**. O andan itibaren, o tipin her örneğinde (nesnesinde) o metodu, sanki tipin doğal bir parçasıymış gibi **noktayla** çağırabilirsin.

### Örnek 1 — C#'ın kendi `int` tipine yeni bir yetenek ekleyelim

```csharp
public static class IntExtensions
{
    public static bool IsValidRepCount(this int reps)
    {
        return reps > 0 && reps <= 100;
    }
}
```
```csharp
int reps = 12;
Console.WriteLine(reps.IsValidRepCount());
```
Çıktı:
```
True
```
Dikkat: `int`, Microsoft'un yazdığı, bizim hiç dokunamadığımız bir tip. Ama `this int reps` yazarak, sanki `int`'in üzerine `.IsValidRepCount()` diye yeni bir metod "yapıştırmışız" gibi, her `int` değişkeninde bu metodu çağırabiliyoruz.

### Örnek 2 — `this`'i unutursak ne olur?

**Bilerek `this` yazmadım:**
```csharp
public static class AthleteExtensions
{
    public static void PrintProfile(Athlete athlete)   // "this" YOK
    {
        Console.WriteLine($"=== {athlete.Name} ===");
    }
}
```
```csharp
berkan.PrintProfile();
```
Hata:
```
error CS1061: 'Runner' bir 'PrintProfile' tanımı içermiyor ve 'Runner' türünde bir ilk bağımsız değişken kabul eden hiçbir erişilebilir 'PrintProfile' genişletme yöntemi bulunamadı
```
C# çok net: bu bir "genişletme yöntemi" (extension method) **değil**, sıradan bir static metod. `this` olmadan, bunu ancak `AthleteExtensions.PrintProfile(berkan)` şeklinde, sınıf adı üzerinden çağırabilirdik (Kavram 3'teki `FitnessMath.EstimateOneRepMax(...)` gibi) — `berkan.PrintProfile()` şeklinde **noktayla** çağıramayız.

**Düzeltilmiş hâli:**
```csharp
public static void PrintProfile(this Athlete athlete)
{
    Console.WriteLine($"=== {athlete.Name} ===");
}
```
```csharp
berkan.PrintProfile();
```
Çıktı:
```
=== Berkan ===
```
Tek fark `this` kelimesi — ama bu tek kelime, metodun çağrılış şeklini tamamen değiştiriyor.

### FitForge'da nerede gördük?

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ...
    }
}
```
Bu, tam olarak `IntExtensions`/`AthleteExtensions` ile aynı desen: `IServiceCollection`, ASP.NET Core'un kendi tipi — biz onu yazmadık, kaynağına dokunamayız. Ama `this IServiceCollection services` sayesinde, `Program.cs`'te elimizde bir `IServiceCollection` (yani `builder.Services`) olduğu her yerde, sanki doğal bir parçasıymış gibi `.AddInfrastructure(...)` çağırabiliyoruz:

```csharp
// Program.cs
builder.Services.AddInfrastructure(builder.Configuration);
```

`this` olmasaydı, bunu `DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration)` şeklinde, çok daha uzun ve daha az "doğal" yazmamız gerekirdi. ASP.NET Core'un kendi `AddControllers()`, `AddOpenApi()` gibi gördüğün her metodu da aslında birer extension method — Microsoft, kendi `IServiceCollection`'ına bu şekilde "dışarıdan" yeni yetenekler ekliyor.

---

## Kavram 8 — Dependency Injection (DI) / IoC Container

### Neden öğrenmen lazım?

Bu, en soyut ama **en can alıcı** kavram. `Program.cs`'teki `builder.Services.AddInfrastructure(...)` satırının, ve `FitForgeDbContext`'in constructor'ında `DbContextOptions` parametresini nereden aldığının **gerçek cevabı** burada. Diğer 7 kavram bu tek kavramı anlayabilmek için gerekli altyapıydı.

### Önce hiç kod yok — sadece bir benzetme

Bir mutfak düşün. Aşçının (bir class) bıçağa (bir dependency/bağımlılığa) ihtiyacı var.

- **Kötü yöntem:** Her aşçı kendi bıçağını kendi cebinden getirir. Restoran bıçak markasını değiştirmek istese, her aşçıyla tek tek konuşup bıçağını değiştirmesi gerekir.
- **İyi yöntem:** Mutfakta bir **bıçak çekmecesi** var. Aşçı hiç kendi bıçağını getirmez, sadece "bana bir bıçak ver" der, çekmeceden alır. Mutfak müdürü çekmeceye hangi marka bıçağı koyacağına **bir kere** karar verir. Marka değişince sadece çekmece yeniden doldurulur, hiçbir aşçının bir şey yapması gerekmez.

Bu benzetmede: **aşçı** = `TrainingService` sınıfı, **bıçak** = `ILogger`, **çekmece** = DI container, **mutfak müdürü** = kurulum kodu (`Program.cs`).

### Önce sorunu görelim: DI olmadan nasıl olurdu?

```csharp
public class TrainingService
{
    private readonly ILogger _logger;

    public TrainingService()
    {
        _logger = new ConsoleLogger();   // dependency'yi KENDİMİZ, elle üretiyoruz
    }

    public void RunWorkout()
    {
        _logger.Log("Workout started");
    }
}
```
Bu çalışır ama bir sorunu var: `TrainingService`, `ConsoleLogger`'a **sıkı sıkıya bağlı (hardcoded)** — aşçı kendi bıçağını kendi cebinden getiriyor. Yarın "log'ları dosyaya yazan bir `FileLogger` da olsun" dersen, `TrainingService`'in **içine girip** `new ConsoleLogger()` satırını değiştirmen gerekir. Projede `TrainingService` gibi 50 sınıf olsa, hepsinin içine tek tek girip değiştirmen gerekirdi.

### Çözüm: Dependency Injection (constructor'a "bana bunu ver" demek)

**En basit tanım:** Bir sınıf, ihtiyaç duyduğu şeyi (dependency/bağımlılık) **kendisi üretmez** — bunun yerine, constructor'ında "bana böyle bir şey lazım" der, ve o şeyi **dışarıdan biri** ona verir (enjekte eder — "inject").

```csharp
public class TrainingService
{
    private readonly ILogger _logger;

    public TrainingService(ILogger logger)   // dependency DIŞARIDAN geliyor
    {
        _logger = logger;
    }

    public void RunWorkout()
    {
        _logger.Log("Workout started");
    }
}
```
Artık `TrainingService`, `ConsoleLogger`'ı hiç tanımıyor bile — sadece "bana `ILogger` sözleşmesini imzalayan bir şey ver" diyor (Kavram 5: interface = sözleşme). `ConsoleLogger` mi, `FileLogger` mı geldiği `TrainingService`'i hiç ilgilendirmiyor. Bu, aşçının "bana bir bıçak ver" demesi — hangi markanın geldiğine aşçı karışmıyor.

Şimdi kritik soru: **bunu elle de yapabiliriz**, hiç container kullanmadan:
```csharp
var manualLogger = new ConsoleLogger();
var manualService = new TrainingService(manualLogger);
manualService.RunWorkout();
```
Burada hiçbir "sihir" yok — önce `ConsoleLogger`'ı elinle üretip, sonra `TrainingService`'e veriyorsun. **Container'ın yaptığı iş, tam olarak bunu senin yerine otomatik yapmak.** Peki neden container'a ihtiyacımız var, madem elle de yapılabiliyor? Aşağıda göreceksin: container, bunu **her yerde, kaç bağımlılık olursa olsun** otomatik yapıyor — sen "elle" versiyonunu her sınıf için tekrar tekrar yazmak zorunda kalmıyorsun.

### DI Container'ı canlı görelim — aynı sonucu iki yoldan üretelim

```csharp
// --- ELLE (container YOK) ---
var manualLogger = new ConsoleLogger();
var manualService = new TrainingService(manualLogger);
manualService.RunWorkout();
```
Çıktı: `[LOG] Workout started`

```csharp
// --- CONTAINER İLE (aynı iş, otomatik) ---
var services = new ServiceCollection();
services.AddSingleton<ILogger, ConsoleLogger>();
services.AddTransient<TrainingService>();

var provider = services.BuildServiceProvider();
var trainingService = provider.GetRequiredService<TrainingService>();
trainingService.RunWorkout();
```
Çıktı: **birebir aynı**, `[LOG] Workout started`. İkisi de aynı sonucu üretti — biri elle, biri container ile.

Şimdi container satırlarını, çekmece benzetmesiyle, tek tek:
- `new ServiceCollection()` → boş bir **çekmece** aç.
- `services.AddSingleton<ILogger, ConsoleLogger>();` → çekmeceye not bırak: **"biri `ILogger` isterse, ona bir `ConsoleLogger` ver."** Bu satır henüz hiçbir nesne üretmiyor, sadece not ediyor. (`Singleton` = "hep aynısını kullan" demek — detayına şimdi girmiyoruz, önemli değil.)
- `services.AddTransient<TrainingService>();` → aynı şekilde: "biri `TrainingService` isterse, üretebilirsin" notu.
- `services.BuildServiceProvider();` → not defterini kapat, artık **gerçekten nesne üretebilen** bir çekmeceye çevir.
- `provider.GetRequiredService<TrainingService>();` → çekmeceye git: **"bana bir `TrainingService` ver."** Çekmece kendi kendine düşünüyor: *"`TrainingService`'in constructor'ı bir `ILogger` istiyor... notlarıma bakayım... `ILogger` istenirse `ConsoleLogger` vereceğim demiştim. Önce bir `ConsoleLogger` üreteyim, sonra onu `TrainingService`'e verip bir `TrainingService` üreteyim."* Bunu, senin "elle" yaptığın o iki satırı, tek satırda ve otomatik olarak yaptı.

**Bunun asıl faydası şurada:** `TrainingService`'in 5 bağımlılığı olsaydı, her birinin de kendi bağımlılıkları olsaydı, "elle" versiyonu yazmak gittikçe uzardı. Container bunu kaç bağımlılık olursa olsun otomatik çözer. Ve yarın `ConsoleLogger` yerine `FileLogger` istersen, tek değiştirmen gereken yer çekmeceye not bıraktığın **o tek satır** (`AddSingleton<ILogger, FileLogger>()`) — `TrainingService`'in içine hiç dokunmuyorsun.

Bu, "kontrolün tersine çevrilmesi" (**IoC = Inversion of Control**) dediğimiz şey: normalde sen `new` ile nesne üretirsin (kontrol sende), DI'da bu kontrolü çekmeceye devrediyorsun. Eğer KeystoneJS/GraphQL'de bir resolver'a `context.db`'nin **hazır, kurulu** gelmesini görmüşsen (sen onu elle `new` ile kurmadan) — orada da birisi (framework'ün başlangıç kodu) bu kurulumu bir kere yapmış. DI container'ın yaptığı iş, kavramsal olarak tam olarak bu.

### Kayıt unutulursa ne olur? (gerçek bir hata, birçok yeni başlayanın düştüğü tuzak)

**Bilerek `ILogger`'ı deftere yazmadan denedim:**
```csharp
var services = new ServiceCollection();
services.AddTransient<TrainingService>();   // ILogger kaydı YOK

var provider = services.BuildServiceProvider();
var trainingService = provider.GetRequiredService<TrainingService>();   // burada patlıyor
```
Hata (**bu bir derleme hatası değil, çalışma zamanı hatası** — kodun kendisi tamamen geçerli C#, `dotnet build` başarıyla derler, ama `dotnet run` sırasında patlar):
```
Unhandled exception. System.InvalidOperationException: Unable to resolve service for type 'Fitness.ILogger' while attempting to activate 'Fitness.TrainingService'.
```
Container, `TrainingService`'i üretmeye çalışırken "bir `ILogger` lazım ama defterimde böyle bir kayıt yok" deyip pes ediyor. Bu, gerçek ASP.NET Core projelerinde **en sık karşılaşılan** hatalardan biri — "yeni bir servis yazdım ama `Program.cs`'e kaydetmeyi unuttum" hatası. Şimdi bu hatayı gördüğünde ne anlama geldiğini bileceksin.

**Düzeltilmiş hâli (`AddSingleton<ILogger, ConsoleLogger>()` satırını geri ekledik):**
```
[LOG] Workout started
```

### Neden derleme zamanında değil de çalışma zamanında patlıyor?

Kavram 2-7'deki tüm hatalar (`using` eksikliği, yanlış generic tipi, eksik interface metodu, eksik base constructor, `this` eksikliği) **derleme anında** yakalandı — kodu çalıştırmadan önce. DI kayıtları ise **string/tip eşleştirmesi çalışma zamanında** yapıldığı için, container "bu tip için kayıt var mı?" sorusunu ancak gerçekten o tip istendiğinde soruyor. Bu yüzden DI hatalarını genelde **uygulamayı çalıştırıp** o kod yoluna gelince fark edersin — tıpkı Day 3'te `AddInfrastructure` içine yazdığımız `?? throw new InvalidOperationException(...)` gibi, "en azından hatayı olabildiğince erken, anlamlı bir mesajla" fark etmeye çalışıyoruz.

### FitForge'da nerede gördük?

`Program.cs`:
```csharp
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
```
`builder.Services`, tam olarak bizim `ServiceCollection`'ımızın ASP.NET Core'daki hâli — "tarif defteri". Her `AddXxx(...)` çağrısı, deftere yeni bir kayıt ekliyor.

`DependencyInjection.cs`:
```csharp
services.AddDbContext<FitForgeDbContext>(options => options.UseSqlServer(connectionString));
```
Bu satır tam olarak bizim `services.AddSingleton<ILogger, ConsoleLogger>()` satırımızla aynı iş: "biri `FitForgeDbContext` isterse, ona böyle kurulmuş bir tane ver" diye deftere yazıyoruz.

`FitForgeDbContext.cs`:
```csharp
public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options) : base(options) { }
```
Artık bu satırın **tam hikayesini** biliyorsun: `FitForgeDbContext`'in kendisi, `DbContextOptions`'ı **hiç kendi üretmiyor** (`new DbContextOptions(...)` diye bir şey yazmadık) — tıpkı `TrainingService`'in `ConsoleLogger`'ı kendi üretmemesi gibi, bu bilgiyi dışarıdan (ASP.NET Core'un DI container'ından) alıyor. API her ayağa kalktığında, container arka planda şunu yapıyor: *"birisi `FitForgeDbContext` istedi, önce onun ihtiyacı olan `DbContextOptions<FitForgeDbContext>`'i (connection string'i User Secrets'tan okuyarak) hazırlayayım, sonra `FitForgeDbContext`'in constructor'ına verip bir tane üreteyim."* Bunların hiçbirini biz elle yazmadık — `AddDbContext<FitForgeDbContext>(...)` satırı, container'a bunu nasıl yapacağını öğretti, gerisini container hallediyor.

---

**Bu 8 kavramla, Day 1-3'te yazdığımız her satırın "altındaki mantığı" artık gerçekten biliyorsun.** Ama roadmap'in tamamını tarayan analiz bir şeyi net söyledi: **9. bir kavram olmadan Day 4'e geçemeyiz** — aşağıda.

---

## Kavram 9 — Async/Await ve `Task<T>`

### Neden öğrenmen lazım, ve neden acil?

Roadmap'in tamamını tarayan analiz şunu buldu: Day 4'ten itibaren **hemen her satır** — her veritabanı sorgusu, her controller endpoint'i — `async`/`await` ile yazılacak. Bu kavram olmadan Day 4'ten sonraki hiçbir kodu okuyamazsın. Bu yüzden diğer 82 kavramın aksine (onlar ilgili güne kadar bekleyecek), bunu şimdi, Day 4'ten önce öğreniyoruz.

### En basit tanım

- **`Task`** = "ileride bitecek bir işin makbuzu/fişi". Bir işi başlattığında hemen elinde sonucu olmaz, ama elinde "iş bitince sonucu buradan alacaksın" diyen bir makbuz (`Task`) olur. `Task<string>` = "ileride bir `string` sonuç verecek makbuz".
- **`async`** = bir metodun başına yazılan işaret: "bu metodun içinde `await` kullanabilirim, çünkü bu metot bir `Task` döndürüyor."
- **`await`** = "bu makbuzun (Task'ın) sonuçlanmasını bekle — ama beklerken programın tamamını kilitleme, sadece bu noktada duraklat."

Eğer JS/Node'da `async function` ve `await` kullandıysan (KeystoneJS/GraphQL resolver'larında görmüş olabilirsin), bu **neredeyse birebir aynı**: JS'teki `Promise`, C#'taki `Task`'ın karşılığı, sözdizimi de (`async`, `await`) aynı kelimeler.

### Örnek

```csharp
public static class WorkoutApi
{
    // Gerçek bir veritabanı/network çağrısını simüle ediyoruz: 1 saniye "bekliyor".
    public static async Task<string> FetchWorkoutNameAsync()
    {
        await Task.Delay(1000);
        return "Push Day";
    }
}
```
`async Task<string>` → "bu metot asenkron çalışır ve sonunda bir `string` verecek." `await Task.Delay(1000)` → "1000 milisaniye (1 saniye) bekle" — gerçek hayatta burada bir SQL Server sorgusu ya da network çağrısı olurdu, biz onu `Task.Delay` ile simüle ettik.

### `await` unutulursa ne olur? (gerçek bir hata, çok sık yapılan bir hata)

**Bilerek `await` yazmadım:**
```csharp
Console.WriteLine("Fetching...");
WorkoutApi.FetchWorkoutNameAsync();   // await YOK
Console.WriteLine("Done");
```
Bu, **derleme hatası değil, sadece bir uyarı** (kod yine çalışır!):
```
warning CS4014: Bu çağrı beklenmediğinden, çağrı tamamlanmadan geçerli yöntemin yürütülmesi devam eder. Çağrının sonucuna 'await' işlecini eklemeyi düşünün.
```
Çıktı:
```
Fetching...
Done
```
Dikkat et: **`"Push Day"` sonucu hiçbir yerde görünmüyor!** Çünkü `FetchWorkoutNameAsync()`'i çağırdık ama sonucunu hiç beklemedik/almadık — program, bu işin bitmesini beklemeden bir sonraki satıra (`Console.WriteLine("Done")`) geçti. Buna **"fire and forget"** (ateşle ve unut) denir — bazı durumlarda bilerek yapılır ama çoğu zaman bir hatadır: "bu işin bittiğinden ve sonucundan haberim olsun istiyordum" derken, hiç haberin olmaz.

**Düzeltilmiş hâli:**
```csharp
Console.WriteLine("Fetching...");
var workoutName = await WorkoutApi.FetchWorkoutNameAsync();
Console.WriteLine($"Got: {workoutName}");
Console.WriteLine("Done");
```
Çıktı (1 saniyelik gerçek bir bekleme sonrası):
```
Fetching...
Got: Push Day
Done
```
Şimdi doğru sırada: "Fetching" yazılır, **1 saniye gerçekten beklenir** (`Task.Delay` bitene kadar), sonuç gelir, sonra "Done" yazılır.

**Not:** `Program.cs`'in en üst seviyesinde (top-level statements, Kavram 1'i hatırla) doğrudan `await` yazabiliyoruz — normalde `await` sadece `async` bir metodun içinde kullanılabilir, ama üst düzey deyimler için derleyici bunu otomatik olarak `async Task Main()` gibi bir şeye çeviriyor, biz bunu elle yazmak zorunda değiliz.

### FitForge'da nerede göreceğiz?

Day 1-3'te henüz kendi yazdığımız kodda `async`/`await` kullanmadık (sadece `dotnet ef` komutlarını terminalden çalıştırdık, onlar ayrı). Ama **Day 4'ten itibaren her yerde** olacak:
- Day 4: `DbContext.SaveChangesAsync()`'i override edeceğiz (üstüne yazıp genişleteceğiz) — bu metodun kendisi `async Task<int>` dönüyor.
- Day 6 ve sonrası: her controller endpoint'i `async Task<IActionResult>` (ya da `ActionResult<T>`) döndürecek, çünkü veritabanına her erişim (`.ToListAsync()`, `.FirstOrDefaultAsync()`, `.SaveChangesAsync()`) bir `Task` döndürür ve `await` ile beklenir.

Kısacası: SQL Server gibi "yavaş" (disk/network) bir şeyle her konuştuğumuzda, o çağrı `async`/`await` ile yazılacak — tıpkı bugünkü `WorkoutApi.FetchWorkoutNameAsync()` örneğimiz gibi.

---

## Kavram 10 — Abstract Class ve Method Overriding (`virtual`/`override`)

### Neden öğrenmen lazım?

Day 4'te yazacağımız `BaseEntity` ve `AuditableEntity`, **abstract** sınıflar olacak — yani kendilerinden doğrudan nesne üretilemeyecek, sadece onlardan türeyen gerçek entity'ler (Exercise, WorkoutLog...) üretilebilecek. Ayrıca `FitForgeDbContext`'te `SaveChangesAsync`'i **override** edeceğiz — EF Core'un hazır davranışının üstüne kendi mantığımızı ekleyeceğiz. Bu iki kelimeyi (`abstract`, `override`) bilmeden Day 4'ün kodunu okuyamazsın.

### En basit tanım

- **`abstract class`** = "bu sınıftan asla doğrudan `new` ile nesne üretilemez, sadece bir şablon/temel olarak var, ondan türeyen somut (concrete) sınıflar üretilebilir." Kavram 6'daki inheritance'ın bir adım ötesi: `Athlete` sınıfından doğrudan nesne üretebiliyorduk, ama `abstract` bir sınıftan üretemeyiz.
- **`virtual`** = bir üst sınıfın metoduna konan işaret: "alt sınıflar isterse bu metodu değiştirebilir (override edebilir)."
- **`override`** = alt sınıfın, üst sınıftaki `virtual` bir metodu **kendi versiyonuyla değiştirmesi**.

### Örnek

```csharp
public abstract class Equipment
{
    public virtual string Describe() => "Generic equipment";
}

public class Dumbbell : Equipment
{
    public override string Describe() => "A pair of dumbbells";
}
```

**Bilerek abstract sınıftan nesne üretmeyi denedim:**
```csharp
var badEquipment = new Equipment();
```
Hata:
```
error CS0144: 'Equipment' soyut türünün veya arabiriminin örneği oluşturulamıyor
```
C# net bir şekilde reddediyor — `Equipment` sadece bir şablon, gerçek bir "equipment" değil (hangi equipment olduğu belirsiz — dumbbell mı, barbell mı?).

**Doğru kullanım:**
```csharp
var dumbbell = new Dumbbell();
Console.WriteLine(dumbbell.Describe());
```
Çıktı:
```
A pair of dumbbells
```
`Dumbbell`, `Equipment`'ın genel `Describe()`'ını **kendi versiyonuyla değiştirdi** (override etti). `override` yazmadan `Describe()`'ı tekrar tanımlamaya çalışsan, C# hata verirdi çünkü üst sınıftaki metot `virtual` işaretlenmemiş olsaydı override edilemezdi.

### FitForge'da nerede göreceğiz?

Day 4'te:
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```
`BaseEntity`'den asla doğrudan nesne üretmeyeceğiz — sadece `Exercise : AuditableEntity`, `WorkoutLog : AuditableEntity` gibi gerçek entity'ler üzerinden.

`FitForgeDbContext`'te de şunu yazacağız:
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // ... audit alanlarını doldur ...
    return await base.SaveChangesAsync(cancellationToken);
}
```
`SaveChangesAsync`, EF Core'un `DbContext` sınıfında zaten `virtual` olarak tanımlı — biz onu `override` ederek "kaydetmeden önce şu ekstra işi de yap" diyoruz, sonra `base.SaveChangesAsync(...)` ile asıl EF Core'un yapması gereken işi de tetikliyoruz (tıpkı Kavram 6'daki `: base(name)` gibi — üst sınıfın kendi işini de çağırıyoruz, sadece constructor'da değil, bir metotta).

---

## Kavram 11 — `ChangeTracker` ve `EntityState`

### Neden öğrenmen lazım?

`SaveChangesAsync`'i override ettiğimizde, içine **ne yazacağımızı** bilmemiz gerekiyor — "hangi kayıt yeni eklendi, hangisi güncellendi, hangisi silinmeye çalışılıyor?" sorusunun cevabı `ChangeTracker` ve `EntityState`'te. Bu kavram olmadan Day 4'ün `SaveChangesAsync` override'ının içini yazamayız.

### En basit tanım

- **`ChangeTracker`** = `DbContext`'in, o ana kadar kendisine tanıttığın (okuduğun, eklediğin, sildiğin) **tüm nesneleri hafızasında not tuttuğu** yer. "Şu anda hangi nesnelerle ilgileniyorum, her birine ne oldu?" listesi.
- **`EntityState`** = ChangeTracker'ın her nesne için tuttuğu **durum etiketi**: `Added` (yeni eklendi, henüz kaydedilmedi), `Modified` (var olan bir alanı değiştirildi), `Deleted` (silinmek üzere işaretlendi), `Unchanged` (hiçbir değişiklik yok).

### Örnek — gerçek bir `DbContext` ile, ama SQL Server olmadan

Bunu göstermek için gerçek bir EF Core `DbContext` kurduk, ama SQL Server'a bağlanmak yerine **bellekte** çalışan bir "sahte" veritabanı kullandık (`UseInMemoryDatabase`) — sadece bu kavramı göstermek için, gerçek projede kullanmayacağız.

```csharp
public class Note
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
}

public class SandboxDbContext : DbContext
{
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseInMemoryDatabase("SandboxDb");
}
```

```csharp
using var sandboxContext = new SandboxDbContext();
sandboxContext.Notes.Add(new Note { Id = 1, Text = "First" });
sandboxContext.Notes.Add(new Note { Id = 2, Text = "Second" });
await sandboxContext.SaveChangesAsync();   // ikisi de kaydedildi

var first = sandboxContext.Notes.First(n => n.Id == 1);
first.Text = "First (edited)";             // -> Modified

var second = sandboxContext.Notes.First(n => n.Id == 2);
sandboxContext.Notes.Remove(second);       // -> Deleted

sandboxContext.Notes.Add(new Note { Id = 3, Text = "Third" });   // -> Added

foreach (var entry in sandboxContext.ChangeTracker.Entries<Note>())
{
    Console.WriteLine($"{entry.Entity.Text} -> {entry.State}");
}
```
Çıktı:
```
Third -> Added
First (edited) -> Modified
Second -> Deleted
```
`sandboxContext.ChangeTracker.Entries<Note>()` bize, o an takip edilen **her `Note` nesnesini ve durumunu** veriyor. `entry.Entity` gerçek nesnenin kendisi, `entry.State` ise `EntityState` enum değeri.

### FitForge'da nerede göreceğiz?

Day 4'te `SaveChangesAsync`'in içinde tam olarak bunu yapacağız:
```csharp
foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
{
    if (entry.State == EntityState.Added)
        entry.Entity.CreatedAt = DateTime.UtcNow;
    else if (entry.State == EntityState.Modified)
        entry.Entity.UpdatedAt = DateTime.UtcNow;
    else if (entry.State == EntityState.Deleted)
    {
        entry.State = EntityState.Modified;   // gerçekten silme, "soft delete" yap
        entry.Entity.DeletedAt = DateTime.UtcNow;
    }
}
```
Dikkat et: `Deleted` durumundaki bir kaydı `Modified`'a **çevirebiliyoruz** — bu, "soft delete"in kalbi. EF Core, sen `Remove()` desen bile, biz `entry.State`'i elle `Modified`'a çevirirsek, veritabanından gerçekten silmez, sadece `DeletedAt` alanını günceller.

---

## Roadmap Analizi Sonucu

40 günlük roadmap'in tamamını tarayan arka plan analizi tamamlandı. Sonuç: yukarıdaki 11 kavram (9. ve 11.'si tam da bu analiz sayesinde eklendi) sağlam bir temel, ama roadmap'in tamamı için toplamda **92 kavram** gerekiyor. Hepsini şimdi öğrenmek yerine, geri kalanların **roadmap'te ilgili güne geldiğimizde**, o günün kendi "kodlamadan önce kavramı öğret" adımında işlenmesine karar verdik — CLAUDE.md'nin "gereksiz teoriyle boğma" kuralına uygun.

Aşağıda, geri kalan kavramların roadmap'in hangi gününde gerekeceğinin tam listesi (referans için — şimdi okuman gerekmiyor, ilgili gün geldiğinde buraya bakabiliriz):

| Gün | Kavramlar |
|---|---|
| 4 | Guid, DateTime/DateOnly/TimeSpan, EF Core Fluent API, global query filters, data annotation attribute'ları |
| 5 | Delegate (Func/Action), try/catch & özel exception sınıfları, global exception middleware, ProblemDetails, pattern matching/switch expression, record'lar |
| 6 | ASP.NET Identity (UserManager/RoleManager/SignInManager), IServiceScope ile başlangıç seed'i, primary constructor, DTO deseni |
| 7 | JWT yapısı, Claims/ClaimsPrincipal, [Authorize], Options pattern, FluentValidation |
| 8 | Navigation property'ler, collection tipleri, Include()/eager loading, IQueryable vs IEnumerable, LINQ, güvenli rastgele üretim |
| 9-10 | Enum'lar, rol/policy bazlı authorization |
| 11-16 | Attribute routing, ActionResult, value object, EF Core seed data, projection/Select(), aggregate root, cascade delete |
| 17-20 | HashSet & LINQ set işlemleri, xUnit [Fact]/[Theory], FluentAssertions, Arrange-Act-Assert |
| 21-25 | decimal/double/float farkı, AsNoTracking(), ILogger&lt;T&gt;, Task.WhenAll |
| 26-30 | Action filter'lar, System.Text.Json, IDistributedCache, rate limiting middleware, IHealthCheck, Swagger/Swashbuckle |
| 31-35 | Unit/integration test farkı, Moq ile mocking, WebApplicationFactory, xUnit fixture'ları, IAsyncLifetime, Testcontainers, GitHub Actions YAML, multi-stage Dockerfile |
| 36-40 | CORS middleware (frontend'e geçince) |

Bu tabloyu şimdi ezberlemene gerek yok — sadece "hiçbir şey kaybolmadı, hepsi not edildi" diye burada duruyor.
