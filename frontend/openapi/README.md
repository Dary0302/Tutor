# Контракт

`backend.openapi.yaml` — реальный OpenAPI 3.0.4 snapshot Tutor.Api, полученный из `http://localhost:5003/swagger/v1/swagger.json` при применении change desktop-ui-auth-questionnaires. Файл содержит JSON, допустимый в YAML. Generated schema создана генератором; обе проверки local generated и live remote comparison прошли при импорте.

Ограничения экспорта: Swagger описывает 200 для register/save, тогда как controllers возвращают 201/204. Enum schema числовые; property JsonStringEnumConverter сериализует некоторые enum строками. ValidationProblemDetails и часть error statuses не отражены в responses. Snapshot не исправляется вручную; frontend runtime adapters должны проверять и нормализовать реальные ответы. Backend не изменяется этим change.

[Порядок генерации, сравнения и явного обновления](../docs/api/openapi-workflow.md). Проверка snapshot не означает, что пользовательские сценарии уже реализованы или проверены.
