# Состояние

TanStack Query владеет загрузкой, cache, stale state, retries, mutations и invalidation server data. Не дублируйте query data в Context или local state без причины в design.

Параметры URL хранят применённые фильтры, сортировку и номер страницы. React state хранит локальное состояние интерфейса. Context предоставляет стабильные зависимости, например сервис сессии. Формы используют React Hook Form и Zod.

Добавление глобального хранилища требует архитектурного решения.
