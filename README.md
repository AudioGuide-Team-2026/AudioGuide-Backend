# AudioGuide-Backend

Сервер для [Android-приложения AudioGuide](https://github.com/AudioGuide-Team-2026/AudioGuide-Android).
Хранит места в PostgreSQL и возвращает адреса, координаты и описания для приложения.
Стек: ASP.NET Core / .NET 10, Entity Framework Core + Npgsql, PostgreSQL 16.

## Что установить

- [Git](https://git-scm.com/downloads).
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) — именно SDK, одного Runtime недостаточно.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) с Docker Compose v2
  для Windows/macOS; на Linux подойдут Docker Engine и плагин Compose v2.
- Для проверки приложения — Android SDK, эмулятор и debug-сборка Android.
  Сам сервер можно проверить через браузер без Android Studio.

Запустите Docker Desktop и проверьте в терминале:

```bash
git --version
dotnet --version
docker compose version
docker info
```

`dotnet --version` должен показать `10.x`, а `docker info` — информацию о работающем Docker.
Первый запуск потребует интернета для загрузки образа PostgreSQL и зависимостей .NET.

## Первый запуск сервера

Команды ниже подходят для терминала macOS/Linux и PowerShell в Windows.
Выполняйте шаги по порядку; переходите к следующему после успешного завершения предыдущего.

### 1. Получить проект

```bash
git clone https://github.com/AudioGuide-Team-2026/AudioGuide-Backend.git
cd AudioGuide-Backend
```

Если проект уже скачан или получен архивом, откройте терминал в его корневой папке.
В ней должны находиться `Guide.slnx`, `docker-compose.yml` и папка `src`.

### 2. Запустить PostgreSQL

```bash
docker compose up -d --wait postgres
docker compose ps
```

Команда дождётся готовности базы. У сервиса `postgres` должен быть статус `healthy`.
Docker запускает только PostgreSQL; API запускается отдельно через `dotnet` на шаге 5.

Стандартные параметры уже согласованы в `docker-compose.yml` и
`src/Guide.Api/appsettings.json`; при обычном запуске менять их не нужно:

| Параметр | Значение |
| --- | --- |
| Адрес БД с компьютера | `localhost:55433` |
| Порт PostgreSQL внутри контейнера | `5432` |
| База данных | `guide` |
| Пользователь | `guide` |
| Пароль для локальной разработки | `guide` |

### 3. Восстановить зависимости и применить миграции

```bash
dotnet restore Guide.slnx
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Guide.Api --startup-project src/Guide.Api
```

Миграции создадут таблицы в новой базе или обновят существующую.
Шаг необходим до первого обращения к списку мест. При получении новых миграций
повторите команду `database update`.

### 4. Загрузить 10 мест Петербурга

Новая база пустая. Выполните обе команды из корня репозитория:

```bash
docker compose cp src/Guide.Api/Data/Seeds/saint-petersburg.sql postgres:/tmp/saint-petersburg.sql
docker compose exec -T postgres psql -U guide -d guide -v ON_ERROR_STOP=1 -f /tmp/saint-petersburg.sql
```

Копирование файла в контейнер позволяет использовать одинаковые команды в PowerShell
и в терминале macOS/Linux. Устанавливать PostgreSQL или `psql` на компьютер не требуется.

При первой загрузке ожидается `INSERT 0 10`, при повторной — `INSERT 0 0`.
Скрипт не создаёт дубли, не перезаписывает изменённые места и сохраняет другие записи.
Если база была пустой, после загрузки в ней будет 10 мест.

### 5. Запустить API

```bash
dotnet run --project src/Guide.Api --launch-profile http
```

В консоли должна появиться строка `Now listening on: http://localhost:5213`.
Оставьте этот терминал открытым: пока команда работает, сервер доступен.

### 6. Проверить результат

Откройте в браузере на компьютере:

- [Состояние API](http://localhost:5213/api/v1/health) — ожидается
  `{"status":"ok","service":"guide-api"}`.
- [Список мест](http://localhost:5213/api/v1/places) — ожидается JSON-массив с местами.
- [Liveness probe](http://localhost:5213/health) — ожидается `Healthy`.

Адрес `http://localhost:5213/` может вернуть `404`: у сервера нет главной веб-страницы
и Swagger UI. Для проверки используйте адреса выше.

## Проверка через Android-приложение

Сервер и Android Emulator должны работать на одном компьютере.
В текущем Android-проекте уже указан адрес `http://10.0.2.2:5213/`:
`10.0.2.2` позволяет стандартному эмулятору обратиться к компьютеру, где он запущен.
Для физического телефона этот адрес не подходит.

1. Откройте репозиторий `AudioGuide-Android` в Android Studio.
2. В SDK Manager установите Android SDK Platform 37 и SDK Build-Tools 36.0.0,
   если их ещё нет. Запустите существующий эмулятор или создайте устройство
   с Android API 26 или новее и образом Google APIs / Google Play для работы геолокации.
3. Запустите модуль `app` в варианте `debug`. В нём уже разрешён HTTP к локальному серверу.
4. Разрешите приложению доступ к точному местоположению.
5. Нажмите **«Обновить данные»**. Сообщение **«Данные обновлены»** означает,
   что список получен с сервера и сохранён в локальную базу приложения.

Для проверки распознавания места откройте у эмулятора **⋮ → Location**:

- широта / latitude: `59.95616`;
- долгота / longitude: `30.310011`.

Нажмите **Set location**. Это примерно в 23 метрах к югу от точки главного корпуса ИТМО.
При азимуте около `0°` / `360°` приложение покажет **«Вы смотрите на: Главный корпус ИТМО»**
и описание. При другом направлении оно предложит повернуться к корпусу.
Направление можно изменить в **Extended controls → Virtual sensors → Device Pose**.

В текущем приложении место определяется только в радиусе 50 метров и при наведении
с погрешностью до 15°. Сообщение «В радиусе 50 метров ничего нет» в другой точке
само по себе не означает ошибку сервера. Пустой ответ API `[]` заменит локальный
список Android пустым списком, поэтому сначала загрузите места на шаге 4.

### Если Android Studio не поддерживает AGP 9.3.3

Android-проект использует AGP `9.3.3`, Gradle `9.5.0` и SDK 37.
Для работы кнопки Run нужна версия Android Studio, совместимая с этим AGP:
см. [таблицу совместимости](https://developer.android.com/build/releases/about-agp).

Для проверки можно собрать APK через терминал без обновления IDE.
Потребуются установленный SDK и JDK 17 или 21; путь к SDK должен быть задан
в `local.properties` Android-проекта как `sdk.dir=...` или через `ANDROID_HOME`.
Файл `local.properties` обычно создаёт Android Studio при открытии проекта.

Из корня **Android-репозитория** на macOS/Linux:

```bash
sh ./gradlew :app:assembleDebug --no-configuration-cache
```

На macOS можно явно использовать Java из установленной Android Studio:

```bash
JAVA_HOME="/Applications/Android Studio.app/Contents/jbr/Contents/Home" sh ./gradlew :app:assembleDebug --no-configuration-cache
```

В Windows PowerShell при настроенном JDK:

```powershell
.\gradlew.bat :app:assembleDebug --no-configuration-cache
```

После `BUILD SUCCESSFUL` APK находится в `app/build/outputs/apk/debug/app-debug.apk`.
Запустите эмулятор через Device Manager и установите APK командой из папки Android-проекта:

```bash
adb install -r -g app/build/outputs/apk/debug/app-debug.apk
adb shell am start -n com.project.audioguide/.ui.MainActivity
```

Если `adb` не найден, добавьте папку `platform-tools` установленного Android SDK в `PATH`
или вызывайте `adb` по полному пути. На macOS стандартный путь:
`~/Library/Android/sdk/platform-tools/adb`.
Подробности: [сборка Android из командной строки](https://developer.android.com/build/building-cmdline).

## Повторный запуск и остановка

После первого успешного запуска обычно достаточно:

```bash
docker compose up -d --wait postgres
dotnet run --project src/Guide.Api --launch-profile http
```

При обновлении кода сначала выполните шаг 3, чтобы применить новые миграции.
Если нужны начальные места, шаг 4 можно повторить.

Чтобы остановить API, нажмите `Ctrl+C` в его терминале. Для остановки базы:

```bash
docker compose stop postgres
```

Данные хранятся в Docker volume `guide-postgres-data` и сохраняются между запусками.

## Если что-то не работает

| Симптом | Что проверить |
| --- | --- |
| `Cannot connect to the Docker daemon` | Запущен ли Docker Desktop; выполните `docker info`. |
| `dotnet` не найден или SDK не поддерживает `net10.0` | Установлен ли .NET SDK 10; проверьте `dotnet --list-sdks`. |
| Не найдены проект, Compose-файл или SQL-файл | Терминал должен быть открыт в корне актуального backend-репозитория. |
| Порт `55433` занят | Остановите другой процесс/контейнер, занявший порт. Если меняете порт в Compose, поменяйте его и в строке подключения API. |
| PostgreSQL недоступен | Проверьте `docker compose ps` и `docker compose logs postgres`; база должна быть `healthy`. |
| Ошибка об отсутствующей таблице `places` или столбце `Address` | Выполните миграции на шаге 3 для той же базы, к которой подключён API. |
| Порт `5213` занят | Возможно, сервер уже работает: откройте `/api/v1/health`. Перед новым запуском остановите предыдущий экземпляр API. |
| API отдаёт `[]` | Загрузите места на шаге 4 и повторите запрос. |
| Android не получает данные | Откройте **в браузере эмулятора** `http://10.0.2.2:5213/api/v1/health`. Проверьте работающий API на том же компьютере и сборку `debug`. |
| Ошибка `incompatible AGP` | Используйте совместимую IDE или описанную выше сборку APK из терминала. |

Строка подключения API задаётся в `src/Guide.Api/appsettings.json`.
Её также можно переопределить переменной окружения `ConnectionStrings__DefaultConnection`.
При использовании другой PostgreSQL применяйте миграции и загружайте SQL именно в неё.

## Places API

| Метод | Адрес | Действие |
| --- | --- | --- |
| `GET` | `/api/v1/places` | Получить массив мест, отсортированный по имени. |
| `GET` | `/api/v1/places/{id}` | Получить одно место. |
| `POST` | `/api/v1/places` | Создать место; ID назначает сервер. |
| `PUT` | `/api/v1/places/{id}` | Обновить место. |
| `DELETE` | `/api/v1/places/{id}` | Удалить место. |

`{id}` — UUID места из ответа API. Для отсутствующего места сервер возвращает `404`.

Тело запроса `POST` / `PUT` (`Content-Type: application/json`):

```json
{
  "name": "Главный корпус ИТМО",
  "address": "Санкт-Петербург, Кронверкский проспект, 49",
  "description": "Главный корпус Университета ИТМО на Петроградской стороне.",
  "latitude": 59.956363,
  "longitude": 30.310011
}
```

Координаты в запросах передаются на верхнем уровне. В ответах `GET`, `POST` и `PUT`
место имеет формат Android-модели `SightEntityResponse`:

```json
{
  "id": "ef4f9033-39cb-4d50-ad40-25311ef40501",
  "address": "Санкт-Петербург, Кронверкский проспект, 49",
  "location": {
    "latitude": 59.956363,
    "longitude": 30.310011
  },
  "name": "Главный корпус ИТМО",
  "description": "Главный корпус Университета ИТМО на Петроградской стороне."
}
```

Отсутствующее описание в ответе заменяется пустой строкой: Android ожидает `String`,
а не `null`. Миграция `AddPlaceAddress` сохраняет старые записи с пустым адресом.

## Места Петербурга и источники

Набор хранится в [saint-petersburg.sql](src/Guide.Api/Data/Seeds/saint-petersburg.sql):

- Главный корпус ИТМО, корпус на Ломоносова и корпус на Биржевой.
- Дворцовая площадь, Исаакиевский собор, Спас на Крови, Казанский собор,
  Медный всадник, Петропавловская крепость и Северная Ростральная колонна.

Адреса корпусов сверены с [музеем ИТМО](https://museum.itmo.ru/page/596/),
их координаты совпадают с начальными данными Android-приложения.
Координаты достопримечательностей — [© OpenStreetMap contributors, ODbL](https://www.openstreetmap.org/copyright),
для [Спаса на Крови — Tripster](https://experience.tripster.ru/sights/20189-spas-na-krovi/).
Это точки ориентиров, а не гарантированные координаты входов.

## Сборка сервера без запуска

```bash
dotnet restore Guide.slnx
dotnet build Guide.slnx --configuration Release
```
