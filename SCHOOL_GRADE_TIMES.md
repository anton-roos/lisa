# School Grade Start and End Time Implementation

## Overview
This implementation adds start and end time functionality to School Grades and integrates it with the Leave Early and Sign In/Out modules.

## Key Features

### 1. School Grade Time Management (`/grades`)
- **Time Input Fields**: Each enabled school grade now has editable start and end time inputs
- **Default Times**: New school grades are created with default times (8:00 AM - 2:00 PM)
- **Dynamic Updates**: Times can be updated in real-time and are immediately saved to the database
- **Visual Styling**: Custom CSS classes provide a clean, modern appearance for time inputs

### 2. Leave Early Integration (`/leave-early`)
- **Dynamic End Times**: School end times are now automatically determined based on the selected learner's grade
- **Early Leave Detection**: The system intelligently determines if a learner is leaving early based on their grade's end time
- **Improved Messaging**: Attendance records now indicate whether a departure was "early" or "normal"

### 3. Sign In/Out Module Integration (`/attendance`)
- **School-Wide Time Defaults**: Attendance session times are pre-populated based on all active school grades
- **Intelligent Time Selection**: Uses the earliest start time and latest end time from all grades as defaults
- **Visual Indicators**: Clear messaging shows that times are based on school grade settings

### 4. Attendance History Enhancement (`/attendance/details/{id}`)
- **Early Leave Badges**: Attendance records display visual badges indicating:
  - **"Left Early"** (orange badge) - for learners who left before their grade's end time
  - **"Full Day"** (green badge) - for learners who stayed until or after their grade's end time
  - **"Present"** (blue badge) - for learners who checked in but haven't checked out
  - **"No Record"** (gray badge) - for learners with incomplete records

## Technical Implementation

### New Service: `SchoolGradeTimeService`
- `GetSchoolGradeTimesForLearnerAsync(Guid learnerId)` - Gets start/end times for a specific learner's grade
- `GetSchoolGradeTimesForSchoolGradeAsync(Guid schoolGradeId)` - Gets times for a specific school grade
- `IsCurrentTimeWithinSchoolHoursAsync(Guid learnerId)` - Checks if current time is within school hours
- `IsEarlyLeaveAsync(Guid learnerId, TimeOnly leaveTime)` - Determines if a leave time constitutes early departure

### Enhanced Services
- **SchoolGradeService**: Added `UpdateAsync()` method for updating grade times
- **AttendanceComponent**: Auto-loads school default times from grade settings
- **LeaveEarly**: Dynamically loads appropriate end times per learner
- **AttendanceDetails**: Shows early leave indicators

### Database Integration
- Uses existing `SchoolGrade.StartTime` and `SchoolGrade.EndTime` properties
- All times are stored as `TimeOnly?` (nullable) values
- Default fallback times: 8:00 AM start, 2:00 PM end

### UI/UX Improvements
- **Custom CSS Classes**: 
  - `.school-grade-time-input` - Styled time input fields
  - `.grade-time-container` - Organized time input layout
  - `.grade-item` - Enhanced list item styling with hover effects
- **Visual Feedback**: Disabled inputs for inactive grades
- **Responsive Design**: Time inputs work well on mobile and desktop

## Usage Instructions

### For School Administrators
1. **Setting Up Grade Times**:
   - Navigate to `/grades`
   - Enable the desired school grades using the toggle switches
   - Set appropriate start and end times for each grade
   - Times are automatically saved when changed

2. **Managing Attendance Sessions**:
   - Go to `/attendance`
   - Times will auto-populate based on your school grades
   - Adjust if needed and confirm to start the session

3. **Processing Early Departures**:
   - Use `/leave-early` to process learner departures
   - The system automatically determines if departure is early
   - Appropriate notes are added to attendance records

### For Staff Members
1. **Viewing Attendance History**:
   - Check `/attendance/details/{sessionId}` for detailed records
   - Look for early leave indicators in the Status column
   - Use this information for reporting and follow-up

## Benefits
- **Accurate Tracking**: Proper early leave detection based on actual grade schedules
- **Reduced Manual Work**: Automatic time calculations eliminate guesswork
- **Better Reporting**: Clear visual indicators help identify patterns
- **Flexibility**: Different grades can have different schedules
- **Integration**: Seamlessly works with existing attendance workflow

## Future Enhancements
- Grade-specific break times
- Automated notifications for early departures
- Integration with parent communication systems
- Advanced reporting based on schedule adherence
