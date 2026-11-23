# Запуск frontend через Docker

Требуются Docker Desktop с Linux containers и backend, опубликованный на порту **5003**. Проверка backend: откройте `http://localhost:5003/swagger/v1/swagger.json`. Backend поднимается отдельно; frontend не запускает базу данных и не изменяет backend.

Из каталога фронтенда выполните команды:

```powershell
docker compose up --build -d
docker compose ps
docker compose logs --tail 50 frontend
```

Откройте **http://localhost:8080**. Имя контейнера — **tutor-frontend**. Создайте аккаунт менти либо наставника, войдите, заполните и сохраните анкету. Обновите страницу и убедитесь, что данные загружаются. Для проверки постоянного входа закройте и снова откройте браузер с тем же профилем. В приватном режиме хранение ограничивается настройками браузера.

```powershell
docker compose down
```

Эта команда останавливает только frontend из данного compose project; контейнеры backend и база не затрагиваются. После изменения кода повторите `docker compose up --build -d`.

## Подключение API

Nginx в frontend-контейнере пересылает `/api/...` на `http://host.docker.internal:5003/api/...`. Браузер обращается к своему origin localhost:8080, поэтому ему не нужны Docker DNS names/CORS-настройки. На Linux compose добавляет host-gateway mapping.

Для другого опубликованного порта/хоста:

```powershell
$env:API_UPSTREAM = 'http://host.docker.internal:5003'
docker compose up -d --force-recreate
```

Upstream задаётся при старте контейнера, без пересборки bundle. Для связи по общей Docker-сети добавьте в compose external network с фактическим именем сети backend и включите её в networks сервиса frontend **до запуска**. Затем задайте HTTP URL сервиса и его **внутренний** порт (в Tutor compose — `http://tutor.api:8080`) как API_UPSTREAM. Имя существующей сети найдите через `docker network ls` / `docker inspect` своего backend. При стандартном подключении через опубликованный порт 5003 это не требуется. Не используйте localhost:5003 внутри контейнера — это адрес самого frontend-контейнера.

## Разработка без Docker

Node 26.13.0, pnpm 11.25.0:

```powershell
pnpm install --frozen-lockfile
pnpm dev --host 127.0.0.1
```

Откройте http://localhost:5173. Vite proxy отправляет `/api` на localhost:5003. Не задавайте VITE_API_BASE_URL для стандартного same-origin режима. Переменная предназначена только для явно согласованного прямого API origin; значения VITE_* публичны.

## Диагностика

- **502**: backend не слушает 5003 либо недоступен из Docker. Проверьте опубликованные порты, Swagger с host и `docker compose logs frontend`.
- **401**: вход/refresh отклонён; войдите заново. Выход локальный: backend не предоставляет endpoint отзыва токена.
- **403**: роль не имеет прав на анкету (только Student/Mentor).
- **404 анкеты** при первом входе означает новую анкету; frontend показывает пустую форму.
- Порт 8080 занят: измените левую часть `8080:80` в compose. Данные входа привязаны к browser origin: другой порт — другое хранилище.
- Невозможно сохранить вход: разрешите localStorage. Refresh token хранится там, access token — в памяти. Production deployment требует HTTPS; cookie/BFF не реализован.

## OpenAPI и проверки

```powershell
$env:OPENAPI_SCHEMA_URL = 'http://localhost:5003/swagger/v1/swagger.json'
pnpm api:contract:check
pnpm verify
```

Известные расхождения Swagger с ответами API перечислены в `openapi/README.md`. Страница [«Подбор и поиск»](http://localhost:8080/search) после входа получает карточки из `GET /api/mentors`. Профиль и сравнение получают анкеты из GET /api/comparison. Календарь, цели, награды и очередь куратора используют данные Figma. Разделы без макетов и операции без подключённого API недоступны.
