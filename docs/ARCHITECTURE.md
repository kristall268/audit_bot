# Архитектура EnergyAudit Bot

---

## Обзор системы

EnergyAudit Bot построен по принципам **Clean Architecture** с явным разделением на слои. Входная точка — Webhook от Telegram. Бизнес-логика изолирована от инфраструктуры и фреймворков.

```
┌──────────────────────────────────────────────────────┐
│                   Telegram                            │
│              (пользователь / кнопки)                  │
└────────────────────────┬─────────────────────────────┘
                         │ HTTPS Webhook
                         ▼
┌──────────────────────────────────────────────────────┐
│               EnergyAuditBot.Api                      │
│         POST /api/webhook  ←  Telegram.Bot            │
│         UpdateRouter → Handler                        │
└────────────────────────┬─────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────┐
│            EnergyAuditBot.Application                 │
│    Use Cases / Services / Interfaces                  │
│  ┌─────────────┐ ┌──────────────┐ ┌───────────────┐  │
│  │ AuditService│ │ StatsService │ │   AiService   │  │
│  └─────────────┘ └──────────────┘ └───────────────┘  │
└──────────┬──────────────────────────────┬────────────┘
           │                              │
           ▼                              ▼
┌────────────────────┐       ┌────────────────────────┐
│  EnergyAuditBot    │       │  EnergyAuditBot         │
│  .Domain           │       │  .Infrastructure        │
│                    │       │                         │
│  Entities:         │       │  - EF Core + PostgreSQL │
│  - User            │       │  - Semantic Kernel      │
│  - HousingProfile  │       │  - OpenAI / YandexGPT   │
│  - DailyCheckin    │       │  - Repositories         │
│  - Appliance       │       │  - Background Services  │
│  - AuditResult     │       └────────────────────────┘
└────────────────────┘
```

---

## Слои приложения

### `EnergyAuditBot.Domain`

Чистые бизнес-сущности без зависимостей от фреймворков.

**Модели:**

```
User
  id (long)               — Telegram chat_id
  username (string)
  createdAt (DateTime)
  isOnboarded (bool)

HousingProfile
  id (int)
  userId (long)
  housingType (enum)      — Apartment / House / Office
  areaSqM (float)
  regionCode (string)
  electricityRate (decimal) — тариф ₸/кВт·ч
  residentsCount (int)
  createdAt (DateTime)

Appliance
  id (int)
  profileId (int)
  name (string)           — "Кондиционер", "Стиральная машина"
  powerKw (float)         — потребляемая мощность
  category (enum)         — Heating / Cooling / Washing / Lighting / Other

DailyCheckin
  id (int)
  userId (long)
  date (DateOnly)
  applianceUsages (JSON)  — [{ applianceId, hoursUsed }]
  totalKwh (float)        — рассчитывается при сохранении
  totalCost (decimal)     — totalKwh * тариф

AuditResult
  id (int)
  userId (long)
  generatedAt (DateTime)
  periodDays (int)
  totalKwh (float)
  totalCost (decimal)
  savingPercent (float)   — сравнение с предыдущим периодом
  aiRecommendation (string)
```

**Бизнес-правила (Domain Services):**

- `EnergyCalculator` — расчёт кВт·ч на основе мощности и часов
- `SavingsEstimator` — сравнение периодов, расчёт потенциальной экономии
- `CheckinValidator` — не более одного чек-ина в день

---

### `EnergyAuditBot.Application`

Use Cases и интерфейсы. Не знает о Telegram, EF Core, OpenAI.

**Сервисы:**

| Сервис | Ответственность |
|---|---|
| `OnboardingService` | Создание пользователя и профиля жилья |
| `CheckinService` | Обработка ежедневного чек-ина |
| `StatsService` | Агрегация статистики за период |
| `AiRecommendationService` | Формирование контекста + вызов ИИ |
| `NotificationService` | Планирование вечерних напоминаний |

**Интерфейсы:**

```csharp
IUserRepository
ICheckinRepository
IApplianceRepository
IAiProvider          // реализуется в Infrastructure
INotificationSender  // реализуется в Infrastructure
```

---

### `EnergyAuditBot.Infrastructure`

Реализация всех внешних зависимостей.

**База данных:**
- EF Core Code First
- PostgreSQL в production, SQLite в разработке
- Репозитории реализуют интерфейсы из Application

**ИИ-интеграция (Semantic Kernel):**

```
AiProvider
  ├── SemanticKernelProvider (основной)
  │     ├── OpenAI connector
  │     └── YandexGPT connector (через HTTP)
  └── MockAiProvider (для тестов)
```

Промпт строится из:
- Профиль жилья пользователя
- История чек-инов за 7 дней
- Самый энергоёмкий прибор
- Вопрос / контекст

**Background Services:**
- `DailyReminderService` — в 20:00 по часовому поясу пользователя отправляет напоминание о чек-ине

---

### `EnergyAuditBot.Api`

Единственная точка входа — Webhook.

```
POST /api/webhook
  ↓
UpdateRouter
  ├── CommandHandler      (/start, /audit, /stats, /tips, /history, /profile, /help)
  ├── CallbackHandler     (Inline Keyboard — кнопки чек-ина)
  └── MessageHandler      (текстовые ответы в диалоге)
```

**Состояние диалога:**

Диалог хранится в памяти через `ConversationStateService` (словарь `chatId → State`). При рестарте сервиса диалог сбрасывается — некритично, пользователь просто начинает заново.

---

## Жизненный цикл чек-ина

```
Пользователь нажимает /audit
        ↓
Бот показывает список приборов (Inline Keyboard)
        ↓
Пользователь отмечает что работало (мультивыбор)
        ↓
Для каждого прибора — кнопки с часами (0.5 / 1 / 2 / 4 / 8)
        ↓
CheckinService.SaveAsync()
  → EnergyCalculator.Calculate()
  → Сохранение DailyCheckin в БД
        ↓
Бот показывает итог дня:
  "Сегодня: 7.4 кВт·ч = 59 ₸
   Кондиционер — 65% потребления
   💡 Вчера было 9.1 кВт·ч — молодец!"
```

---

## Жизненный цикл ИИ-рекомендации

```
Пользователь нажимает /tips
        ↓
AiRecommendationService.GenerateAsync(userId)
  → Загружает профиль + последние 7 чек-инов
  → Строит промпт через Semantic Kernel
  → Вызывает LLM (OpenAI / YandexGPT)
  → Сохраняет результат в AuditResult
        ↓
Бот отправляет персональный совет
```

---

## База данных — схема

```
users
  chat_id (PK, bigint)
  username
  created_at
  is_onboarded

housing_profiles
  id (PK)
  user_id (FK → users)
  housing_type
  area_sq_m
  region_code
  electricity_rate
  residents_count
  created_at

appliances
  id (PK)
  profile_id (FK → housing_profiles)
  name
  power_kw
  category

daily_checkins
  id (PK)
  user_id (FK → users)
  date (DateOnly, unique per user)
  appliance_usages (jsonb)
  total_kwh
  total_cost

audit_results
  id (PK)
  user_id (FK → users)
  generated_at
  period_days
  total_kwh
  total_cost
  saving_percent
  ai_recommendation (text)
```

---

## Решения, принятые сознательно

**Webhook вместо Polling** — надёжнее в production, масштабируется горизонтально.

**Inline Keyboard вместо текстового ввода** — пользователь не печатает цифры, просто тапает. Исключает ошибки валидации и ускоряет чек-ин до 1 минуты.

**JSON для appliance_usages** — структура может меняться (новые поля), при этом не нужны дополнительные таблицы для каждого чек-ина.

**Semantic Kernel вместо прямых HTTP-запросов к LLM** — абстракция позволяет менять провайдера (OpenAI → YandexGPT) без изменения бизнес-логики.

**Без анализа фото** — распознавание розеток и приборов по фото даёт ненадёжные результаты. Структурированный чек-ин точнее и быстрее для пользователя.
