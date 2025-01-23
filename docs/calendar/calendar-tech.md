# Calendar Structure Documentation

## Calendar Object Example
```json
{
    "id": "12345678-1234-1234-1234-123456789abc", // 1
    "createdAt": "2025-01-01T10:00:00Z", // Timestamp when the calendar was created
    "updatedAt": "2025-01-02T15:00:00Z", // Timestamp when the calendar was last updated
    "name": "Work Calendar", // {CalendarName}
    "description": "A calendar for managing work events", // NoteDetails
    "timezone": "UTC" // Timezone of the calendar - not included in V1
}
```

## Event Object Example
```json
{
    "id": "abc12345-def6-7890-ghij-klmnopqrstuv", // 2
    "title": "Project Kickoff Meeting", // {EventTitle}
    "startDate": "2025-01-15T09:00:00-05:00", // {StartDate}
    "endDate": "2025-01-15T10:30:00-05:00", // {EndDate}
    "recurrence": {
        "frequency": "WEEKLY", // Recurrence frequency (e.g., DAILY, WEEKLY)
        "interval": 1, // Interval for the recurrence
        "byDay": ["MO", "WE"], // Days of the week the event recurs
        "until": "2025-12-31T23:59:59-05:00" // End date for the recurrence
    },
    "location": "Conference Room 1", // {Location}
    "attendees": [
        "team.member1@example.com", // {AttendeeEmail1}
        "team.member2@example.com" // {AttendeeEmail2}
    ], // List of attendees for the event
    "description": "Discuss project scope and deliverables" // {EventDescription}
}
```

## Relationships

| Id | SourceNodeId | Type      | TargetNodeId | Parent | RootNodeId |-> | Notes |
|----|--------------|-----------|--------------|--------|------------|---|-------|
| 1  | CAL | Calendar  | CAL          | / | 1 |||
| 2  | EVENT1 | Event  |   OCCUR1     | 1 | 1 |||
| 3  | 2 | Attendee  | ATTENDEE1    | 2 | 1 |||
| 4  | 2 | Attendee  | ATTENDEE2    | 2 | 1 |||
| 5  | 2  | Location  | LOC1    | 2 | 1 |||
| 6  | EVENT2 | Event  | OCCUR2    | 1 | 1 |||
| 7  | 6       | Attendee  | ATTENDEE2    | 6 | 1 |||
| 8  | 2       | Recurrence  | OCCUR3 | 2 | 1 | |if one event is changed|


## Term Entry

| Id  | Title       | Version |
|-----|-------------|----|
| CAL | {CalendarName}  | V1, V2 |
| EVENT1 | {EventTitle} | V1, V2 |
| EVENT2 | {EventTitle} | V1, V2 |
| LOC1 | {Location}  | V1 |
| ATTENDEE1 | {AttendeeEmail1}  | V1 |
| ATTENDEE2 | {AttendeeEmail2}  | V1 |

## Flexible Records

| Id | Type             | Example             | Version |
|----|------------------|---------------------|---------|
| 1  | NoteDetails  | {CalendarDescription} | V1 |
| 2  | NoteDetails     | {EventDescription} | V1 |
| 3  | AttendeeDetails  | {"Optional": false} | V2 |
| 4  | AttendeeDetails  | {"Optional": true} | V2 |
| 8  | RecurrenceDetails  | {ReplacingRecurrenceIndex: 4} | V2 |

## Occurrence Records
| Id  | StartDate | EndDate | ... |
|-----|-------------|----|----|
| OCCUR1 | {StartDate}  | {EndDate} |  |
| OCCUR2 | {StartDate2}  | {EndDate2} |  |
| OCCUR3 | {StartDate3}  | {EndDate3} |  |

# V2
## Contact Entry
| Id  | Email       |
|-----|-------------|
| ATTENDEE1 | {AttendeeEmail1}  |
| ATTENDEE2 | {AttendeeEmail2}     |

## Location Records
| Id  | Location       |
|-----|-------------|
| LOC1 | {Location}  |
