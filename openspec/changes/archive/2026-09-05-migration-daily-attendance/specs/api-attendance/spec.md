## Purpose

This capability defines the behavior for migrating the legacy daily attendance workflow to a modern API that supports batch ingestion, attendance history, chronic status evaluation, and alert generation while preserving the legacy business rules.

## ADDED Requirements

### Requirement: Bulk attendance ingestion accepts JSON records
The system SHALL accept a batch payload as a JSON array of attendance records, each containing `studentId`, `attendDate`, `attendanceCode`, `minutesLate`, and `notes`.

#### Scenario: Valid bulk submission
- **WHEN** a client submits a valid attendance batch containing multiple student records for the same school day
- **THEN** the system SHALL process each record according to the student-date upsert rules and return a successful response indicating the number of records accepted or updated

#### Scenario: Missing required field
- **WHEN** a batch record is missing a required property such as `studentId` or `attendDate`
- **THEN** the system SHALL reject the request with a validation error that identifies the invalid record

### Requirement: Batch duplicate records resolve by last entry wins
The system SHALL treat multiple entries in the same request for the same `studentId` and `attendDate` as a single logical record, with the last entry in the batch taking precedence.

#### Scenario: Duplicate entries in a single request
- **WHEN** a batch contains more than one record for the same student and date
- **THEN** the system SHALL persist only the final record for that student-date pair and SHALL ignore earlier duplicates for the purpose of attendance state and absence counting

### Requirement: School year is calculated from September
The system SHALL calculate the school year using the September-based rule defined by the legacy system: dates from September through December belong to `YYYY-(YYYY+1)`, and dates from January through August belong to `(YYYY-1)-YYYY`.

#### Scenario: September boundary
- **WHEN** an attendance record is dated in September of a given year
- **THEN** the system SHALL assign it to the school year starting in that September

#### Scenario: August boundary
- **WHEN** an attendance record is dated in August of a given year
- **THEN** the system SHALL assign it to the previous academic year

### Requirement: Attendance data is upserted by student and date
The system SHALL upsert attendance records by `studentId` and `attendDate`, preserving the legacy update-or-insert behavior of the stored procedure.

#### Scenario: Existing record is updated
- **WHEN** a student already has attendance recorded for a given date
- **THEN** the system SHALL update the existing attendance row instead of creating a duplicate row

#### Scenario: New record is inserted
- **WHEN** a student has no attendance recorded for a given date
- **THEN** the system SHALL insert a new attendance row for that student and date

### Requirement: Chronic absenteeism is recalculated after each ingestion
The system SHALL recalculate chronic absenteeism for each student after each successful attendance ingestion by counting absences for the active school year and comparing that count to the configured threshold.

#### Scenario: Absence threshold reached
- **WHEN** a student’s total absences in the active school year reaches or exceeds the configured threshold
- **THEN** the system SHALL evaluate the chronic absence rule and create or update the alert state for that student and year

#### Scenario: Absence threshold not reached
- **WHEN** a student’s total absences remains below the configured threshold
- **THEN** the system SHALL not mark the student as chronically absent and SHALL not create a chronic absence alert

### Requirement: Chronic absence alerts are generated idempotently per unresolved year
The system SHALL generate a `CHRONIC_ABSENCE` alert only when the student’s active school year absences are at or above the threshold and no unresolved alert already exists for that student and school year.

#### Scenario: Alert is created once
- **WHEN** a student crosses the threshold for the first time in a school year and no unresolved alert exists
- **THEN** the system SHALL create a chronic absence alert for that student and year

#### Scenario: Active alert prevents duplicates
- **WHEN** an unresolved chronic absence alert for the same student and school year already exists
- **THEN** the system SHALL not create a duplicate unresolved alert for that year

#### Scenario: Resolved alert can recur
- **WHEN** a chronic absence alert for a student and year has been resolved and the student later crosses the threshold again in the same school year
- **THEN** the system SHALL create a new alert because the earlier alert is no longer active

### Requirement: Student attendance history is queryable by school year
The system SHALL provide attendance history for a student within a selected school year, including the attendance date, attendance code, absence flag, excused flag, minutes late, notes, and term information.

#### Scenario: Query history for a student and school year
- **WHEN** a client requests the attendance history for a student and school year
- **THEN** the system SHALL return the matching attendance rows ordered by date descending

### Requirement: Chronic status is queryable
The system SHALL provide a chronic absence status response for a student, including the total absences for the relevant school year and whether the threshold has been reached.

#### Scenario: Chronic status evaluation
- **WHEN** a client requests the chronic absence status for a student and school year
- **THEN** the system SHALL return the total absences and a chronic status indicator derived from the configured threshold
