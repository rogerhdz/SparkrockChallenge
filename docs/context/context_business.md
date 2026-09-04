# Core Business Rules
- **School Year Logic:** Starts in September. If `AttendDate.Month >= 9`, Year is "YYYY-(YYYY+1)". Else, "(YYYY-1)-YYYY".
- **Chronic Absenteeism:** Recalculated on every ingestion. Query `COUNT(IsAbsent == true)` for the Student/Year. Threshold is `10`.
- **Alert Generation:** If absences >= threshold, create a 'CHRONIC_ABSENCE' alert ONLY if an active one doesn't exist for the year.