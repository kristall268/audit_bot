# API Reference — EnergyAudit Bot

Базовый URL: `https://your-domain.com/api`

Интерактивная документация: `GET /scalar`

> Большинство эндпоинтов — внутренние (вызываются из самого бота или Background Services). Внешний эндпоинт только один — Webhook от Telegram.

---

## Аутентификация

Внутренние эндпоинты защищены API-ключом:

```http
X-Api-Key: <internal_api_key>
```

Webhook от Telegram проверяется через секретный токен в заголовке:

```http
X-Telegram-Bot-Api-Secret-Token: <webhook_secret>
```

---

## Webhook

### Принять обновление от Telegram

```http
POST /api/webhook
X-Telegram-Bot-Api-Secret-Token: <secret>
Content-Type: application/json
```

Тело — стандартный объект `Update` от Telegram Bot API.

**Ответ `200 OK`** — всегда, даже при ошибке обработки (иначе Telegram будет повторять запрос).

---

## Пользователи

### Получить профиль пользователя

```http
GET /api/users/{chatId}
X-Api-Key: <key>
```

**Ответ `200 OK`**

```json
{
  "chatId": 123456789,
  "username": "ivan_petrov",
  "isOnboarded": true,
  "createdAt": "2024-03-01T10:00:00Z",
  "profile": {
    "housingType": "Apartment",
    "areaSqM": 54.0,
    "regionCode": "KZ-ALA",
    "electricityRate": 22.68,
    "residentsCount": 2
  }
}
```

**Ответ `404 Not Found`** — пользователь не найден.

---

### Создать или обновить профиль жилья

```http
PUT /api/users/{chatId}/profile
X-Api-Key: <key>
Content-Type: application/json

{
  "housingType": "Apartment",
  "areaSqM": 54.0,
  "regionCode": "KZ-ALA",
  "electricityRate": 22.68,
  "residentsCount": 2
}
```

**Ответ `200 OK`**

```json
{
  "profileId": 42,
  "updatedAt": "2024-06-01T09:00:00Z"
}
```

---

## Приборы (Appliances)

### Получить список приборов пользователя

```http
GET /api/users/{chatId}/appliances
X-Api-Key: <key>
```

**Ответ `200 OK`**

```json
{
  "count": 4,
  "items": [
    {
      "id": 1,
      "name": "Кондиционер",
      "powerKw": 1.5,
      "category": "Cooling"
    },
    {
      "id": 2,
      "name": "Стиральная машина",
      "powerKw": 2.0,
      "category": "Washing"
    },
    {
      "id": 3,
      "name": "Холодильник",
      "powerKw": 0.15,
      "category": "Other"
    },
    {
      "id": 4,
      "name": "Освещение",
      "powerKw": 0.1,
      "category": "Lighting"
    }
  ]
}
```

---

### Добавить прибор

```http
POST /api/users/{chatId}/appliances
X-Api-Key: <key>
Content-Type: application/json

{
  "name": "Духовка",
  "powerKw": 2.2,
  "category": "Other"
}
```

**Ответ `201 Created`**

```json
{
  "id": 5,
  "name": "Духовка",
  "powerKw": 2.2,
  "category": "Other"
}
```

---

### Удалить прибор

```http
DELETE /api/users/{chatId}/appliances/{applianceId}
X-Api-Key: <key>
```

**Ответ `204 No Content`**

**Ответ `404 Not Found`** — прибор не найден или не принадлежит пользователю.

---

## Чек-ины (Daily Check-ins)

### Сохранить ежедневный чек-ин

```http
POST /api/checkins
X-Api-Key: <key>
Content-Type: application/json

{
  "chatId": 123456789,
  "date": "2024-06-01",
  "usages": [
    { "applianceId": 1, "hoursUsed": 3.5 },
    { "applianceId": 2, "hoursUsed": 1.0 }
  ]
}
```

**Ответ `201 Created`**

```json
{
  "checkinId": 201,
  "date": "2024-06-01",
  "totalKwh": 7.25,
  "totalCost": 164.33,
  "breakdown": [
    {
      "applianceName": "Кондиционер",
      "kwh": 5.25,
      "percent": 72.4
    },
    {
      "applianceName": "Стиральная машина",
      "kwh": 2.0,
      "percent": 27.6
    }
  ],
  "comparedToYesterday": {
    "diffKwh": -1.85,
    "diffPercent": -20.3
  }
}
```

**Ответ `409 Conflict`** — чек-ин за эту дату уже существует.

---

### Получить историю чек-инов

```http
GET /api/users/{chatId}/checkins?from=2024-05-01&to=2024-06-01
X-Api-Key: <key>
```

**Query параметры**

| Параметр | Тип | Описание |
|---|---|---|
| `from` | date | Начало периода (YYYY-MM-DD) |
| `to` | date | Конец периода (YYYY-MM-DD) |
| `limit` | int | Кол-во записей (по умолчанию 30) |

**Ответ `200 OK`**

```json
{
  "count": 28,
  "totalKwh": 198.4,
  "totalCost": 4495.27,
  "avgKwhPerDay": 7.09,
  "items": [
    {
      "date": "2024-06-01",
      "totalKwh": 7.25,
      "totalCost": 164.33
    }
  ]
}
```

---

## Статистика

### Получить сводную статистику

```http
GET /api/users/{chatId}/stats?period=week
X-Api-Key: <key>
```

**Query параметры**

| Параметр | Значения | Описание |
|---|---|---|
| `period` | `week` / `month` | Период сравнения |

**Ответ `200 OK`**

```json
{
  "period": "week",
  "current": {
    "totalKwh": 49.3,
    "totalCost": 1118.12,
    "avgKwhPerDay": 7.04
  },
  "previous": {
    "totalKwh": 61.8,
    "totalCost": 1400.51,
    "avgKwhPerDay": 8.83
  },
  "saving": {
    "kwh": 12.5,
    "cost": 283.39,
    "percent": 20.2
  },
  "topConsumer": {
    "applianceName": "Кондиционер",
    "kwhShare": 58.3
  }
}
```

---

## ИИ-рекомендации

### Сгенерировать персональный совет

```http
POST /api/users/{chatId}/recommendations
X-Api-Key: <key>
```

Запрос без тела — контекст собирается автоматически из истории пользователя.

**Ответ `200 OK`**

```json
{
  "recommendationId": 88,
  "generatedAt": "2024-06-01T20:05:00Z",
  "text": "За последнюю неделю кондиционер потребил 58% всей энергии. Попробуй установить температуру на 26°C вместо 22°C — это снизит потребление примерно на 30% без заметной потери комфорта. Потенциальная экономия: ~850 ₸ в месяц.",
  "estimatedMonthlySaving": 850.00
}
```

**Ответ `429 Too Many Requests`** — рекомендация уже генерировалась сегодня.

---

### Получить последнюю рекомендацию

```http
GET /api/users/{chatId}/recommendations/latest
X-Api-Key: <key>
```

**Ответ `200 OK`** — объект рекомендации (см. выше).

**Ответ `404 Not Found`** — рекомендаций ещё нет.

---

## Уведомления

### Отправить напоминание о чек-ине (вызывается Background Service)

```http
POST /api/notifications/reminders
X-Api-Key: <key>
Content-Type: application/json

{
  "chatIds": [123456789, 987654321]
}
```

**Ответ `200 OK`**

```json
{
  "sent": 2,
  "failed": 0
}
```

---

## Коды ошибок

| HTTP-статус | Код | Описание |
|---|---|---|
| `400` | `VALIDATION_ERROR` | Ошибка валидации; детали в `errors` |
| `401` | `UNAUTHORIZED` | Неверный или отсутствующий API-ключ |
| `404` | `NOT_FOUND` | Ресурс не найден |
| `409` | `CONFLICT` | Чек-ин за эту дату уже существует |
| `429` | `RATE_LIMITED` | Превышен лимит (ИИ-запросы) |
| `500` | `INTERNAL_ERROR` | Внутренняя ошибка сервера |

**Формат ошибки:**

```json
{
  "error": "VALIDATION_ERROR",
  "message": "Поле areaSqM обязательно.",
  "errors": {
    "areaSqM": ["Значение должно быть больше 0."]
  }
}
```
