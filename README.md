# Learner Information System Administrator (LISA)

## User Types
 - System Administrator
 - Principal
 - School Management
 - Administrator
 - Teacher
 - GateOfficer

Roles per Page:
Home – All
Schools – SA
Care Groups – SA, Principal
Staff – SA, Principal
Grades – SA, Principal
Register Class – SA, Principal
Subjects – SA
Assessment Types – SA
Sign in / out -  SA, GateOfficer
Daily Attendance – SA, Principal, SchoolManagement, Administrator, Teacher
Leave Early – SA, Principal, Administrator
Combinations – SA, Principal
Learners – SA, Principal, SchoolManagement, Administrator
Class Lists – SA, Principal, SchoolManagement, Administrator, Teacher
Capture Results – SA, Principal, SchoolManagement, Administrator, Teacher
Results – SA, Principal, SchoolManagement, Administrator, Teacher
Progress Feedback – SA, Principal, SchoolManagement, Administrator, Teacher
Communication – SA, Principal, SchoolManagement
Email Campaigns – SA, Principal, SchoolManagement


## User Type Privileges
### Super Admin
 - Do what Principals does at every school.
 - Add or Remove Other Super Admins.
 - Configure Communication Templates
 - Change School Information
   - Add Curriculum Information
   - Add School Color
   - Add Care Groups to School
 - Add Teachers

### Principal
 - Do what School Management can do at a specific school.

### School Management Team
 - Do what Administrator at a specific school does
 - Can query results
 - Can query communication

### Administrator
 - Can Add Edit or Deactivate Learners
 - Can Add Edit or Remove Parents on Learners
 - Can send Progress Feedback
   - `The Progress Feedback must be scheduled at a certain time`
   - `Ensure that the emails are staggered so that you do not hit limit`
   - `Microsoft 365 limit is 30 mails per minute`
   - `Error handling when SMTP details are not configured for a school.`
 - Can setup Subject Combinations
 - Can Add or Edit Grades
 - Move Learner Between Care Groups

### Teacher
 - Can capture marks for their subjects
 - Can monitor period behaviour
 - Can manage period attendance

# Workflow
 1. Seed the Database (Roles, Admin User, School Types)
 1. Setup the System Administrator (When app runs with DB Seed)
 2. Setup the School
 

## SchoolGrade
Link to School with OnDelete.Cascade → if a School is deleted, all related Grades are deleted.
RegisterClass links to SchoolGrade with OnDelete.Restrict → you must remove or update RegisterClass entries before you can delete the SchoolGrade.

## Learner
Link to School with OnDelete.Cascade → if the School is deleted, Learners are also deleted.
Link to RegisterClass with OnDelete.Restrict → you cannot delete a RegisterClass while Learners are still linked to it.
Link to Combination with OnDelete.Restrict → you cannot delete a Combination while Learners are still linked to it.
Has multiple Parents → OnDelete.Cascade from the Learner side: if a Learner is deleted, their Parents are also deleted.
Has multiple LearnerSubjects → OnDelete.Cascade from the Learner side: if a Learner is deleted, their LearnerSubject rows are also deleted.

## Parent
Link to Learner with OnDelete.Cascade → if the Learner is deleted, the Parents are also deleted.

## RegisterClass
Link to SchoolGrade with OnDelete.Restrict → you cannot delete a SchoolGrade if there are RegisterClasses linked to it.
Link to Teacher with OnDelete.Restrict → you cannot delete a Teacher if there are RegisterClasses linked to them.
Has many Learners with OnDelete.Restrict → when a RegisterClass is deleted it leaves the Learners "unparented" (they remain but lose the RegisterClass link).

## RegisterClassSubject
OnDelete.Restrict on the SubjectId side: deleting a RegisterClassSubject does not delete the Subject.
OnDelete.Cascade on the RegisterClassId side: deleting a RegisterClass deletes its RegisterClassSubjects.

## Combination
Link to SchoolGrade with OnDelete.Restrict → you cannot delete a SchoolGrade if Combinations link to it.
Many-to-many with Subject via CombinationSubject:
OnDelete.Restrict on the SubjectId side: deleting a CombinationSubject will not delete the Subject.
OnDelete.Cascade on the CombinationId side: deleting a Combination will delete its CombinationSubjects.

## Subject
Deleting a Subject is restricted if it appears in any of the following:
- Linked to LearnerSubject (OnDelete.Restrict on SubjectId)
- Linked to RegisterClassSubject (Restrict)
- Linked to CombinationSubject (Restrict).

## LearnerSubject
Link to Learner with OnDelete.Cascade → if a Learner is deleted, their LearnerSubject entries are deleted.
Link to Subject with OnDelete.Restrict → you cannot delete a Subject if LearnerSubject entries still reference it.

## Period
Links to Teacher, Subject and SchoolGrade with OnDelete.Restrict → you must remove or update the Period before deleting those entities.
The database enforces that a Teacher can only have one Period at a time between a StartTime and EndTime.
The system checks that StartTime is less than EndTime.

## Result
A Result is a record. If we delete a Learner or a Subject, nothing automatically happens to Results. We need to decide the policy for Results when a Learner is deleted because Result records reference the Learner; if the Learner is deleted we will no longer be able to tell whose result it was.

Roles per page:
 - Home – All users
 - Schools – System Administrator (SA)
 - Care Groups – SA, Principal
 - Staff – SA, Principal
 - Grades – SA, Principal
 - Register Class – SA, Principal
 - Subjects – SA
 - Assessment Types – SA
 - Sign in / Sign out – SA, Gate Officer
 - Daily Attendance – SA, Principal, School Management, Administrator, Teacher
 - Leave Early – SA, Principal, Administrator
 - Combinations – SA, Principal
 - Learners – SA, Principal, School Management, Administrator
 - Class Lists – SA, Principal, School Management, Administrator, Teacher
 - Capture Results – SA, Principal, School Management, Administrator, Teacher
 - Results – SA, Principal, School Management, Administrator, Teacher
 - Progress Feedback – SA, Principal, School Management, Administrator, Teacher
 - Communication – SA, Principal, School Management
 - Email Campaigns – SA, Principal, School Management


## User Type Privileges
### Super Admin
 - Can perform the actions a Principal can at every school.
 - Add or remove other Super Admins.
 - Configure communication templates.
 - Change school information:
   - Add curriculum information
   - Add school color
   - Add care groups to a school
 - Add teachers

### Principal
 - Can perform the actions the School Management team can do at a specific school.

### School Management Team
 - Can perform the actions an Administrator can do at a specific school.
 - Can query results.
 - Can query communications.

### Administrator
 - Can add, edit or deactivate learners.
 - Can add, edit or remove parents linked to learners.
 - Can send progress feedback.
   - Progress feedback must be scheduled to send at a specific time.
   - Ensure that emails are staggered so you don't hit provider limits.
   - Microsoft 365 limit: 30 emails per minute.
   - Proper error handling when SMTP details are not configured for a school.
 - Can set up subject combinations.
 - Can add or edit grades.
 - Move learners between care groups.

### Teacher
 - Can capture marks for their subjects.
 - Can monitor behaviour per period.
 - Can manage attendance for their periods.

# Workflow
 1. Seed the database (roles, admin user, school types).
 2. Set up the System Administrator account (the app will seed data on first run if configured).
 3. Set up the school(s).


## Database relations and delete behaviour

### SchoolGrade
 - Link to School with OnDelete.Cascade → if a School is deleted, all related Grades are deleted.
 - RegisterClass links to SchoolGrade with OnDelete.Restrict → you must remove or update RegisterClass entries before you can delete the SchoolGrade.

### Learner
 - Link to School with OnDelete.Cascade → if the School is deleted, Learners are also deleted.
 - Link to RegisterClass with OnDelete.Restrict → you cannot delete a RegisterClass while Learners are still linked to it.
 - Link to Combination with OnDelete.Restrict → you cannot delete a Combination while Learners are still linked to it.
 - Has multiple Parents → OnDelete.Cascade from the Learner side: if a Learner is deleted, their Parents are also deleted.
 - Has multiple LearnerSubjects → OnDelete.Cascade from the Learner side: if a Learner is deleted, their LearnerSubject rows are also deleted.

### Parent
 - Link to Learner with OnDelete.Cascade → if the Learner is deleted, the Parents are also deleted.

### RegisterClass
 - Link to SchoolGrade with OnDelete.Restrict → you cannot delete a SchoolGrade if there are RegisterClasses linked to it.
 - Link to Teacher with OnDelete.Restrict → you cannot delete a Teacher if there are RegisterClasses linked to them.
 - Has many Learners with OnDelete.Restrict → when a RegisterClass is deleted it leaves the Learners "unparented" (they remain but lose the RegisterClass link).

### RegisterClassSubject
 - OnDelete.Restrict on the SubjectId side: deleting a RegisterClassSubject does not delete the Subject.
 - OnDelete.Cascade on the RegisterClassId side: deleting a RegisterClass deletes its RegisterClassSubjects.

### Combination
 - Link to SchoolGrade with OnDelete.Restrict → you cannot delete a SchoolGrade if Combinations link to it.
 - Many-to-many with Subject via CombinationSubject:
   - OnDelete.Restrict on the SubjectId side: deleting a CombinationSubject will not delete the Subject.
   - OnDelete.Cascade on the CombinationId side: deleting a Combination will delete its CombinationSubjects.

### Subject
 Deleting a Subject is restricted if it appears in any of the following:
 - Linked to LearnerSubject (OnDelete.Restrict on SubjectId)
 - Linked to RegisterClassSubject (Restrict)
 - Linked to CombinationSubject (Restrict)

### LearnerSubject
 - Link to Learner with OnDelete.Cascade → if a Learner is deleted, their LearnerSubject entries are deleted.
 - Link to Subject with OnDelete.Restrict → you cannot delete a Subject if LearnerSubject entries still reference it.

### Period
 - Links to Teacher, Subject and SchoolGrade with OnDelete.Restrict → you must remove or update the Period before deleting those entities.
 - The database enforces that a Teacher can only have one Period at a time between a StartTime and EndTime.
 - The system checks that StartTime is less than EndTime.

### Result
 A Result is a record. If we delete a Learner or a Subject, nothing automatically happens to Results. We need to decide the policy for Results when a Learner is deleted because Result records reference the Learner; if the Learner is deleted we will no longer be able to tell whose result it was.

### CareGroup
 - When a School is deleted, all CareGroups for that school are deleted.
 - When a CareGroup is deleted it unlinks all Learners from that CareGroup.


## Audience per campaign
 - CareGroup <--> Teacher mapping exists for targeting campaigns.

## Subject applicability in the UI
 - Show which grades a Subject is applicable for (e.g., public List<SystemGrade>? GradesApplicable { get; set; }).

Select one (Mathematics or Mathematical Literacy):
 - Mathematics
 - Mathematical Literacy

Combinations: validation must ensure at least one subject is selected for each combination.

Extra subject options are available only for grades 10, 11 and 12.

## Class Lists
 - Register Class (e.g., 8A) -> Export
 - Combination Class List (grades 10 - 12):
   - by SchoolGrade
   - by subject (e.g., Physical Science) -> Export
 - Combination Class List reports

## Leave Early
This integrates with the sign-out module. If a learner is leaving early and no sign-out record exists for the day, the system will confirm the school's end time before continuing. Flow:
 1. Select a school.
 2. Check if a sign-out record exists for the day.
 3. If no sign-out record exists, create one with the confirmed end-of-day time — this will then be available when navigating to the sign-out module.
 4. If a sign-out record was already created (either via Leave Early or the Sign Out module), the confirmation step is skipped in the Leave Early flow.
 5. Select a learner to continue.
 6. Once a learner is selected, the main flow appears.

When selecting "Telephonic", notes must be mandatory.
When selecting "Letter", "Email" or "WhatsApp", expose a drag-and-drop / paste box where the user must attach evidence of permission.

## Sign In / Sign Out
 - Configure the school's start time (e.g., 07:30) at the school level; it will be used in confirmations.
 - Attendance roles cannot sign out learners before the configured school end time.
 - If a learner left early, mark them as 'Left Early' instead of 'Absent'.
 - If a cellphone is handed in at sign-in, it should be returned at sign-out or when the learner is marked as leaving early.

## Daily Register
The daily register page checks that a school is selected (like most pages). The user selects a Register Class; after selecting the Register Class a list of learners will load with the default status of "Absent". Clicking the "Absent" pill toggles it to "Present".

## Communication
If the email account is not successfully authenticated, an error must be shown to the user.

## Per-Period Attendance
 - If a learner left early, mark them as 'Left Early' instead of 'Absent'.
 - Clicking a learner shows the time they arrived.
 - "Lock in" finalizes attendance; if a learner arrives after lock-in they will be shown as late (e.g., with a red border).

Logic examples:
 - !SignedIn && !Registered => Show Absent [Not Signed In]
 - SignedIn && !Registered => Show Absent [Not Registered]
 - SignedIn && Registered => Show Present

After lock-in:
 - All those marked Present on the Daily Register but not in class show as Not Attending.
 - All those marked Absent on the Daily Register remain Absent.
 - Learners marked Not Attending after lock-in show as Late with a timestamp (example: 11:33).

## Misc
 - MAT (context-specific)


## Changelog
 - Fixed learner code clashes between schools.
 - Fixed historical subjects not pulling through.
 - Fixed inactive learners being sent progress feedback.
 - Fixed date ordering on progress feedbacks.

### Progress / TODO
 - [X] State Management (Results)
 - [X] Progress Feedback (Printing): smaller font to fit on one page
 - [X] Attendance when not signed in: create a special record that marks "not signed in but present" (active not at school but present)
 - [ ] ADI (Attendance Module + Ad-hoc Learners): build Per-Period support
 - [ ] Reporting and Communication (WhatsApp + Email + Print) for Attendance
 - [ ] Clean up and refactor

Planned attendance improvements:
 - Daily Register -> Daily Attendance
 - Add Period 1 - 12 to per-period attendance
 - Automatically stop a Period after 30 minutes
 - Start each period with a fresh list
 - Stop Period acts like a submit button

Acronyms:
 - (ADI) Academic Development Improvement
 - (MAP) Matric Achievement Project
```