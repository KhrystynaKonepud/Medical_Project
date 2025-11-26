# Medical Center Source Generator

Це Source Generator для автоматичної генерації DTO класів, API контролерів з CRUD методами та маппінгу для моделей у проекті Medical Center.

## Що генерується?

Для кожної моделі, позначеної атрибутом `[GenerateApi]`, автоматично створюються:

1. **DTO клас** (`{Model}Dto.g.cs`) - спрощена версія моделі без навігаційних властивостей
2. **Mapper** (`{Model}Mapper.g.cs`) - extension метод `ToDto()` для конвертації Entity → DTO
3. **API Controller** (`{Model}sController.g.cs`) - контролер з повним CRUD функціоналом:
   - `GET /api/generated/{model}s` - отримати всі записи
   - `GET /api/generated/{model}s/{id}` - отримати запис за ID
   - `POST /api/generated/{model}s` - створити новий запис
   - `PUT /api/generated/{model}s/{id}` - оновити запис
   - `DELETE /api/generated/{model}s/{id}` - видалити запис

## Як використовувати?

### Крок 1: Позначте модель атрибутом

```csharp
using Medical_center.Attributes;

namespace Medical_center.Models
{
    [GenerateApi]
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        // Навігаційні властивості (не будуть включені в DTO)
        public ICollection<Doctor> Doctors { get; set; }
        public ICollection<Patient> Patients { get; set; }
    }
}
```

### Крок 2: Зберіть проект

```bash
dotnet build
```

### Крок 3: Використовуйте згенерований код

#### Використання DTO та Mapper:

```csharp
var clinic = await _context.Clinics.FindAsync(id);
var dto = clinic.ToDto(); // Extension метод згенерований автоматично
return Ok(dto);
```

#### Використання згенерованого контролера:

Згенерований контролер автоматично доступний за адресою:
```
GET  /api/generated/clinics
POST /api/generated/clinics
GET  /api/generated/clinics/{id}
PUT  /api/generated/clinics/{id}
DELETE /api/generated/clinics/{id}
```

## Приклад згенерованого коду

### ClinicDto.g.cs

```csharp
#nullable enable
using System;

namespace Medical_center.Models.Generated
{
    public class ClinicDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
```

### ClinicMapper.g.cs

```csharp
#nullable enable
using System;
using Medical_center.Models;
using Medical_center.Models.Generated;

namespace Medical_center.Models.Generated
{
    public static class ClinicMapper
    {
        public static ClinicDto ToDto(this Clinic entity)
        {
            if (entity == null) return null;

            return new ClinicDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                PhoneNumber = entity.PhoneNumber
            };
        }
    }
}
```

### ClinicsController.g.cs

```csharp
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medical_center.Models;
using Medical_center.Models.Generated;
using Medical_center.Data;

namespace Medical_center.Models.Generated.Controllers
{
    [ApiController]
    [Route("api/generated/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClinicDto>>> GetAll()
        {
            var items = await _context.Clinics
                .Select(x => x.ToDto())
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClinicDto>> GetById(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            return Ok(clinic.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<ClinicDto>> Create(ClinicDto dto)
        {
            var clinic = new Clinic
            {
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber
            };
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClinicDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();

            clinic.Name = dto.Name;
            clinic.Address = dto.Address;
            clinic.PhoneNumber = dto.PhoneNumber;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return NotFound();
            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
```

## Застосування до інших моделей

Щоб застосувати генерацію до інших моделей (Patient, Doctor, Appointment, тощо), просто додайте атрибут `[GenerateApi]`:

```csharp
[GenerateApi]
public class Patient { ... }

[GenerateApi]
public class Doctor { ... }

[GenerateApi]
public class Appointment { ... }
```

## Переваги Source Generator

1. **Автоматизація** - не потрібно писати boilerplate код вручну
2. **Час компіляції** - код генерується під час збірки проекту
3. **Безпека типів** - згенерований код перевіряється компілятором
4. **IntelliSense** - підтримка автодоповнення в IDE
5. **Консистентність** - всі DTO та контролери мають однакову структуру
6. **Легко підтримувати** - зміни в моделі автоматично відображаються в DTO

## Налаштування

Атрибут `[GenerateApi]` має додаткові параметри:

```csharp
[GenerateApi(Version = "2.0", IncludeNavigations = false)]
public class Clinic { ... }
```

- `Version` - версія API (за замовчуванням "1.0")
- `IncludeNavigations` - включати навігаційні властивості в DTO (за замовчуванням false)

## Технічні деталі

- **Framework**: .NET Standard 2.0
- **NuGet Packages**:
  - Microsoft.CodeAnalysis.CSharp 4.5.0
  - Microsoft.CodeAnalysis.Analyzers 3.3.4
- **Компіляція**: Source Generator виконується під час збірки проекту
- **Розташування**: Згенеровані файли доступні через `{Namespace}.Generated`

## Примітки

- Згенеровані файли мають суфікс `.g.cs`
- Файли не зберігаються фізично в проекті, вони існують в пам'яті компілятора
- Навігаційні властивості (ICollection, навігаційні посилання) автоматично виключаються з DTO
- Всі згенеровані файли мають `#nullable enable` для сумісності з проектом
