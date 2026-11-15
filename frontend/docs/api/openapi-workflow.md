# OpenAPI workflow

`openapi/backend.openapi.yaml` — реальный snapshot Tutor.Api OpenAPI 3.0.4. `src/shared/api/client.ts` создаёт `openapi-fetch<paths>`; вызов несуществующего endpoint не проходит TypeScript. Известные различия между Swagger и runtime описаны в openapi/README.md, нормализуются в runtime.ts и не исправляются вручную в generated schema. Продуктовые endpoints нельзя придумывать.

1. `pnpm api:generate` генерирует декларации локально. Не редактировать generated schema руками.
2. `pnpm api:generated:check` генерирует типы во временный файл и сравнивает с сохранённым schema.d.ts; рабочее дерево не меняет.
3. Задайте `OPENAPI_SCHEMA_URL` в shell/CI. `pnpm api:contract:check` проверяет HTTP status, OpenAPI структуру/refs и семантически сравнивает snapshot (порядок ключей/формат YAML не важен). Нет URL — явная ошибка, не ложный успех.
4. `pnpm api:contract:update` скачивает схему во временный файл, валидирует и выводит различия. Согласуйте несовместимые изменения с командой. Для обновления снимка и типов запустите `pnpm api:contract:update --apply` и проверьте изменения в Git.

Не вызывайте update автоматически в install/build/test/verify. Обычный verify проверяет только local consistency; remote check зависит от URL и выполняется в CI условно. Анализ исходников Tutor — источник вопросов, а не доказательство backend parity. Внешние refs контракта должны быть доступны для валидации; предпочтителен самодостаточный экспорт.
