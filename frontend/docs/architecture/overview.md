# Архитектура

Одно клиентское React SPA, Vite + strict TypeScript; отдельный REST backend. `src/app/main.tsx` запускает bootstrap, при dev opt-in ожидает MSW, затем создаёт QueryClient и browser router. RootLayout восстанавливает session, содержит header/main/footer; route errorElement — общий fallback. Home/auth/questionnaire маршруты загружаются сразу, презентационные экраны — отдельным lazy chunk.

Feature-модули auth/questionnaire используют RHF + Zod. Session entity содержит service с lifecycle/token coordination; Context хранит стабильный service, Query — identity и server data. Shared содержит типизированный API-клиент, runtime adapters, environment validation, UI primitives и CSS custom properties. App связывает service и Query; shared не импортирует entities. Tailwind подключён через Vite plugin, стили БЭМ/CSS tokens находятся в shared/styles. Backend и контракты публично не меняются; runtime расхождения Swagger документированы отдельно.

React baseline 19.2.7 без Compiler/RSC/Server Actions. Простые документированные React-паттерны; memoization только при измеримой причине. URL должен обслуживаться сервером как SPA fallback на index.html. `pnpm preview` не production hosting. Настройка deployment/CSP/production routing — отдельное решение после выбора хостинга.
