# ⚡ EnergyAudit Bot

> Telegram-бот для ежедневного мониторинга энергопотребления с персональными ИИ-рекомендациями.

---

## О проекте

EnergyAudit Bot помогает владельцам жилья и офисов **понять и снизить** расходы на электроэнергию. В отличие от классических энергоаудитов — никаких сложных форм и выездов специалистов. Просто ежедневный чек-ин в Telegram за 1 минуту.

**Как это работает:**

1. Один раз заполняешь профиль жилья (площадь, тариф, приборы)
2. Каждый вечер бот спрашивает что работало сегодня — отвечаешь кнопками
3. Бот считает потребление и показывает динамику
4. ИИ анализирует твою историю и даёт персональный совет

---

## Стек технологий

| Компонент | Технология |
|---|---|
| Backend | ASP.NET Core 8, Minimal API |
| Telegram | Telegram.Bot + Webhook |
| База данных | EF Core + PostgreSQL |
| ИИ | Semantic Kernel + OpenAI / YandexGPT |
| Логирование | Serilog |
| Валидация | FluentValidation |
| Тесты | xUnit + Moq |
| Деплой | Docker + Docker Compose |
| Документация API | Scalar |

---

## Структура репозитория

```
EnergyAuditBot/
├── src/
│   ├── EnergyAuditBot.Api/              # Web API + Webhook + Middleware
│   ├── EnergyAuditBot.Application/      # Сервисы, Use Cases, интерфейсы
│   ├── EnergyAuditBot.Domain/           # Модели, сущности, бизнес-логика
│   ├── EnergyAuditBot.Infrastructure/   # EF Core, внешние сервисы, ИИ
│   └── EnergyAuditBot.Common/           # Утилиты, константы, расширения
├── tests/
│   ├── EnergyAuditBot.UnitTests/
│   └── EnergyAuditBot.IntegrationTests/
├── docker-compose.yml
├── .env.example
├── README.md
├── ARCHITECTURE.md
├── API.md
└── SPECIFICATION.md
```

---

## Быстрый старт

### Требования

- .NET 8 SDK
- Docker и Docker Compose
- Telegram Bot Token (получить у [@BotFather](https://t.me/BotFather))
- OpenAI API Key или YandexGPT API Key

### 1. Клонировать репозиторий

```bash
git clone https://github.com/your-org/energy-audit-bot.git
cd energy-audit-bot
```

### 2. Настроить окружение

```bash
cp .env.example .env
# Заполните .env — токены, строку подключения к БД
```

### 3. Запустить через Docker Compose

```bash
docker compose up -d
```

### 4. Настроить Webhook

```bash
curl -X POST https://api.telegram.org/bot<TOKEN>/setWebhook \
     -d "url=https://your-domain.com/api/webhook"
```

### 5. Применить миграции

```bash
docker compose exec api dotnet ef database update
```

---

## Переменные окружения

```env
# Telegram
TELEGRAM_BOT_TOKEN=           # Токен от @BotFather

# База данных
DATABASE_URL=                 # postgresql://user:pass@host:5432/dbname

# ИИ (выбрать один или оба)
OPENAI_API_KEY=               # OpenAI API ключ
YANDEX_GPT_API_KEY=           # YandexGPT API ключ
YANDEX_FOLDER_ID=             # Folder ID для Yandex Cloud

# Приложение
WEBHOOK_URL=                  # Публичный URL вашего сервера
APP_ENV=                      # development / production
```

---

## Команды бота

| Команда | Описание |
|---|---|
| `/start` | Приветствие и онбординг |
| `/audit` | Ежедневный чек-ин |
| `/stats` | Статистика за неделю / месяц |
| `/tips` | Персональный совет от ИИ |
| `/profile` | Просмотр и редактирование профиля жилья |
| `/history` | История потребления по дням |
| `/help` | Справка |

---

## Тесты

```bash
# Все тесты
dotnet test

# Только unit-тесты
dotnet test tests/EnergyAuditBot.UnitTests

# С покрытием
dotnet test --collect:"XPlat Code Coverage"
```

---

## Лицензия

MIT License. См. [LICENSE](LICENSE).
