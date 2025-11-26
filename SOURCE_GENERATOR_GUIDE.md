# Source Generator - Швидкий старт

## Що це?

Source Generator автоматично створює для ваших моделей:
- ✅ **DTO класи** (без навігаційних властивостей)
- ✅ **API контролери** з повним CRUD функціоналом
- ✅ **Маппінг** (extension методи для конвертації Entity → DTO)

## Як використовувати?

### 1. Додайте атрибут до моделі

```csharp
using Medical_center.Attributes;

[GenerateApi]
public class YourModel
{
    public int Id { get; set; }
    public string Property1 { get; set; }
    public string Property2 { get; set; }
}
```

### 2. Зберіть проект

```bash
dotnet build
```

### 3. Готово!

Автоматично згенеруються:

- **YourModelDto** - DTO клас
- **YourModelMapper** - extension метод `.ToDto()`
- **YourModelsController** - API контролер з endpoints:
  - `GET /api/generated/yourmodels` - всі записи
  - `GET /api/generated/yourmodels/{id}` - один запис
  - `POST /api/generated/yourmodels` - створити
  - `PUT /api/generated/yourmodels/{id}` - оновити
  - `DELETE /api/generated/yourmodels/{id}` - видалити

## Приклад використання

```csharp
// В вашому коді:
var model = await _context.YourModels.FindAsync(id);
var dto = model.ToDto(); // ← Згенерований extension метод
return Ok(dto);
```

## Які моделі можна використовувати?

Будь-які моделі Entity Framework! Наприклад:

```csharp
[GenerateApi] public class Patient { ... }
[GenerateApi] public class Doctor { ... }
[GenerateApi] public class Appointment { ... }
[GenerateApi] public class Clinic { ... }
```

## Що НЕ включається в DTO?

- Навігаційні властивості (`ICollection<T>`, `T Property`)
- Складні об'єкти (генеруються тільки прості властивості + Id)

## Переваги

1. **Швидкість** - не треба писати boilerplate код
2. **Консистентність** - всі API однакові
3. **Безпека** - перевіряється компілятором
4. **Легко підтримувати** - зміни в моделі → автоматично в DTO

---

**Детальна документація**: `SourceGenerator/README.md`
