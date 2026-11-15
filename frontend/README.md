# Куратор · frontend

React-приложение платформы наставничества «Куратор». Tutor API обслуживает регистрацию, вход, анкеты менти и наставника, подбор и поиск наставников. Остальные экраны используют демонстрационные данные, пока их API не подключён.

**Запуск через Docker:** `docker compose up --build -d`, затем http://localhost:8080. Backend должен работать отдельно на опубликованном порту 5003. Полная [инструкция Docker](docs/deployment/docker.md) содержит настройку upstream, остановку и диагностику. Контейнер frontend — `tutor-frontend`.

## Начало работы

Нужны Git, Node.js **26.13.0** (Active LTS, `.nvmrc`) и pnpm **11.25.0** (`packageManager`). В средах с nvm: `nvm install` и `nvm use`; pnpm установите указанной версии. Зависимости фиксируются exact, lockfile обязателен.

```sh
pnpm install --frozen-lockfile
pnpm exec playwright install chromium
pnpm dev
```

По желанию скопируйте `.env.example` в `.env.local`. По умолчанию API same-origin: Vite proxy → localhost:5003, Docker Nginx proxy → host.docker.internal:5003. `VITE_ENABLE_MOCKS=true` включает MSW только в dev. Mock accounts: menti@example.test и mentor@example.test, пароль любой длиной от 3 символов. Это изолированные демонстрационные данные, не реальная авторизация. `VITE_API_BASE_URL` — публичный URL, не место для секретов; для стандартного запуска оставьте пустым. Значения Vite попадают в bundle.

## Команды

| Команда                             | Назначение                                             |
| ----------------------------------- | ------------------------------------------------------ |
| `pnpm dev` / `pnpm preview`         | Dev server / просмотр готовой сборки                   |
| `pnpm build`                        | Проверка TypeScript и production build                 |
| `pnpm format`                       | Форматирование Prettier                                |
| `pnpm lint` / `pnpm typecheck`      | Правила кода, архитектура / строгие типы               |
| `pnpm test` / `pnpm test:coverage`  | Watch / однократный прогон с порогами                  |
| `pnpm test:smoke` / `pnpm test:e2e` | Chromium smoke / полный suite                          |
| `pnpm api:generate`                 | Типы из локального snapshot                            |
| `pnpm api:generated:check`          | Проверка актуальности типов без изменения файлов       |
| `pnpm api:contract:check`           | Сравнение реального backend-контракта                  |
| `pnpm api:contract:update`          | Предпросмотр изменения контракта                       |
| `pnpm api:contract:update --apply`  | Явное применение после анализа diff                    |
| `pnpm verify`                       | Линтер, типы, покрытие, сборка, типы API и smoke-тесты |

## Архитектура

`app` собирает providers/router; `routes` компонует экраны; `features/auth` и `features/questionnaire` содержат формы; `entities/session` — lifecycle сессии; `shared` — независимые API/config/lib/ui/styles; `test` — MSW/setup/fixtures. Внешние импорты идут через public index.ts, границы проверяет ESLint.

Подробности: [обзор](docs/architecture/overview.md), [границы](docs/architecture/boundaries.md), [состояние](docs/architecture/state-management.md).

## OpenAPI

**Реальный контракт Tutor.Api** импортирован с localhost:5003/swagger/v1/swagger.json. Типы генерируются из snapshot; runtime adapters учитывают расхождения Swagger с реальными enum/status, перечисленные в [openapi/README.md](openapi/README.md). Remote check требует `OPENAPI_SCHEMA_URL` в shell/CI. [Workflow](docs/api/openapi-workflow.md).

## Проверки и CI

[Стратегия тестирования](docs/testing/strategy.md): поведение через Testing Library, HTTP boundary через MSW, coverage 80/80/75/70, два Chromium smoke. CI устанавливает frozen lockfile и запускает `verify`. Remote check запускается только при заданном repository variable `OPENAPI_SCHEMA_URL`; иначе явно пропускается. Полный E2E доступен вручную. CI в GitHub необходимо проверить после подключения remote; локальный прогон не подтверждает запуск hosted workflow.

## VS Code

Установите рекомендуемые workspace extensions: ESLint (`dbaeumer.vscode-eslint`), Prettier (`esbenp.prettier-vscode`), Tailwind CSS IntelliSense (`bradlc.vscode-tailwindcss`), Vitest (`vitest.explorer`), Playwright (`ms-playwright.playwright`), EditorConfig (`editorconfig.editorconfig`). CLI/CI остаются источником проверок.

Правила разработки описаны в [CONTRIBUTING.md](CONTRIBUTING.md).
