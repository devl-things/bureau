# Calendar API Documentation

This document describes the Calendar API, which adheres to RESTful principles. It supports CRUD operations for managing calendars and events, the ability to attach objects to specific dates, share calendars, and search within calendars.

## Base URL
```
https://api.yourdomain.com/calendar
```

---

## Versioning
This API uses a custom header `api-version` to specify the version of the API being requested.

### Example
```
GET /calendars HTTP/1.1
Host: api.yourdomain.com
api-version: 1.0
Authorization: Bearer <your_token>
```

---

## Standard API Response Format

All responses follow the structure of the `ApiResponse` class:

### Success Response
```json
{
    "status": "success",
    "message": "Optional description",
    "data": {}
}
```

### Error Response
```json
{
    "status": "error",
    "message": "Error description",
    "error": {
        "type": "ErrorType",
        "title": "Error Title",
        "status": 400,
        "detail": "Detailed error message",
        "instance": "/path/to/resource"
    }
}
```

---

## Endpoints Overview

| Method | Endpoint                                 | Description                                   |
|--------|-----------------------------------------|-----------------------------------------------|
| POST   | `/calendars`                            | Create a new calendar.                       |
| GET    | `/calendars`                            | Retrieve all calendars (supports pagination).|
| GET    | `/calendars/{calendarId}`               | Retrieve a specific calendar.                |
| PUT    | `/calendars/{calendarId}`               | Update a specific calendar.                  |
| DELETE | `/calendars/{calendarId}`               | Delete a specific calendar.                  |
| POST   | `/calendars/{calendarId}/events`        | Add a new event to a calendar.               |
| GET    | `/calendars/{calendarId}/events`        | Retrieve all events for a calendar (supports pagination). |
| GET    | `/calendars/{calendarId}/events/{eventId}` | Retrieve a specific event.                   |
| PUT    | `/calendars/{calendarId}/events/{eventId}` | Update a specific event.                     |
| DELETE | `/calendars/{calendarId}/events/{eventId}` | Delete a specific event.                     |
| POST   | `/calendars/{calendarId}/share`         | Share a calendar with another user.          |
| POST   | `/calendars/search`                     | Search for events within all calendars (supports pagination). |

---

## Endpoints Details

### 1. **Create a Calendar**
**POST** `/calendars`

#### Request Body
```json
{
    "name": "Work Calendar",
    "description": "A calendar for work events"
}
```

#### Response
```json
{
    "status": "success",
    "data": {
        "calendarId": "b7f0a4e2-d4d1-4a6c-8b5b-4d4e2e3ed2df",
        "name": "Work Calendar",
        "description": "A calendar for work events",
        "createdAt": "2025-01-13T12:34:56Z"
    }
}
```

---

### 2. **Retrieve All Calendars**
**GET** `/calendars`

#### Query Parameters
| Parameter | Type   | Required | Description                            |
|-----------|--------|----------|----------------------------------------|
| `page`    | int?   | No       | Page number for pagination (default: 1). |
| `limit`   | int?   | No       | Number of items per page (default: 10). |

#### Response
```json
{
    "status": "success",
    "data": [
        {
            "calendarId": "b7f0a4e2-d4d1-4a6c-8b5b-4d4e2e3ed2df",
            "name": "Work Calendar",
            "description": "A calendar for work events",
            "createdAt": "2025-01-13T12:34:56Z"
        },
        {
            "calendarId": "d9f0c9a4-e4d2-5b7d-8c6e-5e5f3f4gd3ef",
            "name": "Personal Calendar",
            "description": "A calendar for personal events",
            "createdAt": "2025-01-14T08:22:33Z"
        }
    ],
    "pagination": {
        "requestedPage": 1,
        "pageSize": 10,
        "totalItems": 2,
        "totalPages": 1,
        "hasNext": false,
        "hasPrevious": false
    }
}
```

---

### 3. **Retrieve a Specific Calendar**
**GET** `/calendars/{calendarId}`

#### Response
```json
{
    "status": "success",
    "data": {
        "calendarId": "b7f0a4e2-d4d1-4a6c-8b5b-4d4e2e3ed2df",
        "name": "Work Calendar",
        "description": "A calendar for work events",
        "createdAt": "2025-01-13T12:34:56Z"
    }
}
```

---

### 4. **Add an Event to a Calendar**
**POST** `/calendars/{calendarId}/events`

#### Request Body
```json
{
    "title": "Meeting with HR",
    "startDate": "2025-01-20T10:00:00Z",
    "endDate": "2025-01-20T11:00:00Z",
    "attachedObjects": ["12345-abcde", "67890-fghij"],
    "recurrence": {
        "frequency": "WEEKLY",
        "interval": 1,
        "daysOfWeek": ["MO", "WE"],
        "until": "2025-12-31T11:00:00Z"
    }
}
```

#### Response
```json
{
    "status": "success",
    "data": {
        "eventId": "e7f8a9b0-c9d1-4c6e-9d7a-6f6e3f5hd7gh",
        "title": "Meeting with HR",
        "startDate": "2025-01-20T10:00:00Z",
        "endDate": "2025-01-20T11:00:00Z",
        "attachedObjects": ["12345-abcde", "67890-fghij"],
        "recurrence": {
            "frequency": "WEEKLY",
            "interval": 1,
            "daysOfWeek": ["MO", "WE"],
            "until": "2025-12-31T11:00:00Z"
        }
    }
}
```

---

### 5. **Retrieve All Events for a Calendar**
**GET** `/calendars/{calendarId}/events`

#### Query Parameters
| Parameter | Type   | Required | Description                            |
|-----------|--------|----------|----------------------------------------|
| `page`    | int?   | No       | Page number for pagination (default: 1). |
| `limit`   | int?   | No       | Number of items per page (default: 10). |

#### Response
```json
{
    "status": "success",
    "data": [
        {
            "eventId": "e7f8a9b0-c9d1-4c6e-9d7a-6f6e3f5hd7gh",
            "title": "Meeting with HR",
            "startDate": "2025-01-20T10:00:00Z",
            "endDate": "2025-01-20T11:00:00Z",
            "attachedObjects": ["12345-abcde", "67890-fghij"],
            "recurrence": {
                "frequency": "WEEKLY",
                "interval": 1,
                "daysOfWeek": ["MO", "WE"],
                "until": "2025-12-31T11:00:00Z"
            }
        }
    ],
    "pagination": {
        "requestedPage": 1,
        "pageSize": 10,
        "totalItems": 1,
        "totalPages": 1,
        "hasNext": false,
        "hasPrevious": false
    }
}
```

---

### 6. **Search Events in Calendars**
**POST** `/calendars/search`

#### Query Parameters
| Parameter | Type   | Required | Description                            |
|-----------|--------|----------|----------------------------------------|
| `page`    | int?   | No       | Page number for pagination (default: 1). |
| `limit`   | int?   | No       | Number of items per page (default: 10). |

#### Request Body
```json
{
    "query": "Meeting",
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-12-31T23:59:59Z"
}
```

#### Response
```json
{
    "status": "success",
    "data": [
        {
            "calendarId": "b7f0a4e2-d4d1-4a6c-8b5b-4d4e2e3ed2df",
            "eventId": "e7f8a9b0-c9d1-4c6e-9d7a-6f6e3f5hd7gh",
            "title": "Meeting with HR",
            "startDate": "2025-01-20T10:00:00Z",
            "endDate": "2025-01-20T11:00:00Z",
            "attachedObjects": ["12345-abcde", "67890-fghij"],
            "recurrence": {
                "frequency": "WEEKLY",
                "interval": 1,
                "daysOfWeek": ["MO", "WE"],
                "until": "2025-12-31T11:00:00Z"
            }
        }
    ],
    "pagination": {
        "requestedPage": 1,
        "pageSize": 10,
        "totalItems": 1,
        "totalPages": 1,
        "hasNext": false,
        "hasPrevious": false
    }
}
```

---

## Objects Used in API

### Calendar
| Field         | Type   | Description                           |
|---------------|--------|---------------------------------------|
| `calendarId`  | UUID   | Unique identifier for the calendar.   |
| `name`        | string | Name of the calendar.                 |
| `description` | string | Description of the calendar.          |
| `createdAt`   | string | Timestamp of creation (ISO 8601).     |

### Event
| Field             | Type   | Description                                   |
|-------------------|--------|-----------------------------------------------|
| `eventId`         | UUID   | Unique identifier for the event.             |
| `title`           | string | Title of the event.                          |
| `startDate`       | string | Start date and time of the event (ISO 8601). |
| `endDate`         | string | End date and time of the event (ISO 8601).   |
| `attachedObjects` | UUID[] | Optional attached object identifiers.        |
| `recurrence`      | object | Recurrence details (if applicable).          |

### Recurrence
| Field        | Type       | Description                                           |
|--------------|------------|-------------------------------------------------------|
| `frequency`  | string     | Recurrence frequency (e.g., DAILY, WEEKLY).          |
| `interval`   | int        | Interval between occurrences (e.g., every 1 week).   |
| `daysOfWeek` | string[]   | Days of the week for recurrence (e.g., ["MO", "WE"]). |
| `until`      | string     | The end date for the recurrence pattern (ISO 8601).  |

---

## Notes
- **Authentication**: Ensure requests include proper authentication headers.
- **API Versioning**: Use the `api-version` header to specify the desired API version.
- **Rate Limiting**: Requests may be rate-limited; handle `429 Too Many Requests` responses appropriately.
- **Error Handling**: Common HTTP status codes include:
  - `400 Bad Request`: Invalid input.
  - `401 Unauthorized`: Missing or invalid authentication.
  - `404 Not Found`: Resource not found.
  - `500 Internal Server Error`: Server error.

