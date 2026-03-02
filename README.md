# AppTech Back-end (AppTech LMS)

Bu repository **AppTech LMS** üçün back-end hissəsidir. Layihə **.NET / ASP.NET Core** üzərində qurulub və solution daxilində klassik çoxlayihəli (multi-project) arxitektura istifadə edir:

- **AppTech.API** – HTTP API (Controllers), middleware pipeline, Swagger, Auth, RateLimit və s.
- **AppTech.Business** – biznes məntiqi (services, handlers, validation, DTO-lar)
- **AppTech.Core** – domain/əsas modellər, base abstraksiyalar, ümumi contract-lar
- **AppTech.DAL** – data access layer (EF Core / persistence, migration, repository/query məntiqi)
- **AppTech.Shared** – cross-cutting util-lər, shared DTO/enum/helper yanaşmaları
- **AppTech.MVC** – (əgər istifadə olunursa) MVC UI və ya admin panel üçün ayrı lay.

> Repo adı: `ilkoninn/apptech-back`  
> Solution: `src/AppTechBack.sln`

---

## 1) Texnologiyalar və imkanlar

`src/AppTech.API/Program.cs` faylından görünən əsas imkanlar:

- **ASP.NET Core minimal hosting** (`WebApplication.CreateBuilder`)
- **Swagger/OpenAPI** (development mühitində aktivdir, root `/` avtomatik `/swagger`-a yönləndirilir)
- **JWT Authentication** (`AddJwt(...)`)
- **CORS** policy: `AllowReactApp`
- **Rate Limiting** (`AddRateLimiter`, `UseRateLimiter`)
- **SignalR** (hazırda `AddSignalR()` var, Hub mapping comment olunub)
- **Static files** (`UseStaticFiles`)
- **Automated DB Migration** (`AutomatedMigration.MigrateAsync(...)`)
- **HttpClient**: `BankClient` üçün typed client

---

## 2) Repository strukturu

Root:
- `.editorconfig`, `.gitignore`
- `AppTech Writting.txt` – daxili qeydlər / development log
- `request.xml` – ehtimal ki test request nümunəsi
- `src/` – bütün solution buradadır

`src/`:
- `AppTechBack.sln`
- `AppTech.API/`
- `AppTech.Business/`
- `AppTech.Core/`
- `AppTech.DAL/`
- `AppTech.Shared/`
- `AppTech.MVC/`

---

## 3) Lokal run (Development)

### Tələblər
- .NET SDK (solution-un hədəf framework-una uyğun; dəqiq versiyanı `.csproj`-dan yoxlayın)
- DB (çox ehtimal MySQL istifadə olunur; qeydlərdə MySQL və image upload mention var)
- (Opsional) Docker – əgər DB və digər servisləri container ilə qaldırmaq istəyirsiniz

### Addımlar
1. Repo-nu klonla:
   ```bash
   git clone https://github.com/ilkoninn/apptech-back.git
   cd apptech-back
   ```

2. Solution-u build et:
   ```bash
   dotnet build src/AppTechBack.sln
   ```

3. API-ni run et:
   ```bash
   dotnet run --project src/AppTech.API
   ```

4. Swagger:
   - Development mühitində API açılan kimi `/` yolu avtomatik `/swagger`-a redirect edir.
   - URL tipik olaraq: `https://localhost:<port>/swagger`

> `Program.cs`-ə görə Swagger yalnız `app.Environment.IsDevelopment()` olduqda aktivdir.

---

## 4) Konfiqurasiya (appsettings / secrets)

Repo-da `src/AppTech.API/appsettings.json` hazırda boşdur:
```json
{}
```

Bu o deməkdir ki:
- real connection string-lər və secret-lər repoya daxil edilməyib (düzgün yanaşmadır), və/və ya
- config başqa fayllardan/secret manager-dən gəlir.

### Tövsiyə olunan yanaşma
Local üçün `appsettings.Development.json` yarat (gitignore-da saxla) və nümunə kimi aşağıdakı strukturu istifadə et:

```json
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=...;User=...;Password=...;"
  },
  "Jwt": {
    "Issuer": "apptech",
    "Audience": "apptech",
    "Key": "VERY_SECRET_KEY"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:3000" ]
  }
}
```

> Dəqiq key adları (`ConnectionStrings`, `Jwt` və s.) `AddDataAccess(...)` və `AddJwt(...)` implementasiyasından asılıdır.

---

## 5) Middleware pipeline (HTTP axını)

API start olduqda `Program.cs` aşağıdakı ardıcıllıqla işləyir:

1. Services:
   - `AddDataAccess(builder.Configuration)`
   - `AddBusiness(builder.Configuration)`
   - `AddSignalR()`
   - `AddSwagger()`, `AddJwt()`, `AddRateLimiter()`, `AddMemoryCache()`
   - `AddHttpClient<BankClient>()`

2. Middlewares:
   - `app.AddMiddlewares()` (custom middlewares burada yığılır)
   - `UseHttpsRedirection()`
   - `UseStaticFiles()`
   - (Development) `UseSwagger()` + UI + `/` → `/swagger` redirect
   - `UseCors("AllowReactApp")`
   - `AutomatedMigration.MigrateAsync(...)` (startup zamanı DB migrate)
   - `UseAuthentication()`
   - `UseAuthorization()`
   - `MapControllers()`
   - `UseRateLimiter()`

---

## 6) Multi-translation məntiqi (LMS üçün əsas ideya)

Repo içi qeydlərdən (`AppTech Writting.txt`) görünən əsas arxitektura qərarı:
- Bir çox entity-də **Translation** modeli var (məs: `CompanyTranslation`, `CertificationTranslation`, `ExamTranslation`, `NewsTranslation`, `FAQ translation` və s.)
- “MultipleTranslation” məntiqi ümumi yanaşma kimi hazırlanıb.

### Yanaşmanın məqsədi
LMS platformasında kontent (News, FAQ, Exam, Company məlumatları və s.) bir neçə dildə saxlanmalı və API-lər bunu düzgün formatda front-end-ə verməlidir.

### Ümumi qayda (qeydlərə əsasən)
- **DTO-lar** translation-ları yanında saxlayır
- **Validator-lar** translation-ları yanında saxlayır
- **Model-lər** translation-ları yanında saxlayır
- CRUD-dan əvvəl **Handler** qatında yoxlamalar olmalıdır
- Hər model üçün:
  - “normal service”
  - “translation service”
  yazılması gözlənilir

Bu dizaynın nəticəsi:
- API response-lar daha “front-friendly” olur (front bir entity-ni çəkəndə translations birlikdə gəlir)
- Validation həm base entity, həm də translation-lar üçün paralel aparılır
- Business qatında vahid qayda ilə yeni modul əlavə etmək asanlaşır (Company necə yazılıbsa, News/FAQ/Exam də eyni pattern)

---

## 7) Modullar (domen hissələr)

Qeydlərdə işlənən / hazırlandığı deyilən əsas modullar:
- Company / CompanyTranslation
- Certification / CertificationTranslation
- Exam / ExamTranslation
- News / NewsTranslation
- FAQ (+ translation)
- ContactUs
- GiftCard
- Statistics
- File Manager (image upload, MySQL bağlantısı qeyd olunur)
- Google Login (Auth integration)

> Qeyd: Bu siyahı kodun tam skanına əsaslanmır, repodakı qeydlərdən çıxarılıb.

---

## 8) CORS (Front-end inteqrasiyası)

`Program.cs`-də:
```csharp
app.UseCors("AllowReactApp");
```

Deməli `AllowReactApp` policy-si DI zamanı (çox ehtimal `AddBusiness` və ya `AddDataAccess`-da) konfiqurasiya olunur və React front üçün origin whitelist nəzərdə tutulub.

---

## 9) DB migrations (Avtomatik)

Startup zamanı:
```csharp
await AutomatedMigration.MigrateAsync(scope.ServiceProvider);
```

Bu o deməkdir ki:
- tətbiq ayağa qalxan kimi migration-lar tətbiq olunur
- production-da bu davranış istənməyə bilər; deployment strategiyasına görə ayrıca idarə oluna bilər

---

## 10) Swagger / API sənədləşmə

Development-da Swagger UI aktivdir və authorization token-i UI-da saxlamaq üçün:
- `c.EnablePersistAuthorization();`

Bu, test edərkən hər refresh-də token-in itməməsinə kömək edir.

---

## 11) Təhlükəsizlik

- Secret-ləri repoya commit etmə (JWT key, DB password və s.)
- Local üçün `appsettings.Development.json` və ya `dotnet user-secrets` istifadə et
- Production üçün environment variables / secret manager istifadə et

---

## 12) Roadmap / TODO (tövsiyə)

- [ ] `README`-də “Environment variables” bölməsi dəqiqləşdirilsin (config key-lər koddan çıxarılıb yazılsın)
- [ ] `appsettings.json` üçün `appsettings.example.json` əlavə edilsin
- [ ] SignalR Hub mapping (`MapHub`) lazım olsa aktivləşdirilsin və dokumentləşdirilsin
- [ ] Automated migrations production strategiyası sənədləşdirilsin

---

## 13) Lisensiya

Bu repository üçün lisensiya ayrıca göstərilməyib. Şirkət daxili/private istifadə nəzərdə tutulursa, LICENSE əlavə edin.

---

## Əlaqə / Qeyd
`AppTech Writting.txt` faylında](#)
