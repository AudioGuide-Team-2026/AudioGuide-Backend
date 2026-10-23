# AudioGuide-Backend

Минимальный серверный проект для мобильного приложения гида.

## Стек

- ASP.NET Core Web API
- C# / .NET 10
- PostgreSQL 16
- Entity Framework Core + Npgsql

## Запуск

```bash
docker compose up -d postgres
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Guide.Api --startup-project src/Guide.Api
dotnet run --project src/Guide.Api
```

Проверки:

- `GET http://localhost:5000/api/v1/health` — состояние API
- `GET http://localhost:5000/health` — liveness probe

Порт приложения может отличаться при запуске через `dotnet run`; актуальный адрес выводится в консоли.

## Places API

Основной ресурс — места на карте:

- `GET /api/v1/places` — список мест;
- `GET /api/v1/places/{id}` — одно место;
- `POST /api/v1/places` — создать место;
- `PUT /api/v1/places/{id}` — обновить место;
- `DELETE /api/v1/places/{id}` — удалить место.

Пример тела для создания или обновления места:

```json
{
  "name": "Красная площадь",
  "description": "Центральная площадь Москвы",
  "latitude": 55.7539,
  "longitude": 37.6208
}
```

## Сборка

```bash
dotnet restore Guide.slnx
dotnet build Guide.slnx --configuration Release
```

Строка подключения к PostgreSQL задаётся через `ConnectionStrings__DefaultConnection`. В Docker PostgreSQL доступен на локальном порту `55433`.
