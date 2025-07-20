# Learner Information System Administrator (LISA)

![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/anton-roos/lisa?utm_source=oss&utm_medium=github&utm_campaign=anton-roos%2Flisa&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

## User Types
 - System Administrator
 - Principal
 - School Management
 - Administrator
 - Teacher
 - GateOfficer

Roles per Page:
Home – Alle gebruikers
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
Link na School met OnDelete.Cascade → as die Skool delete word, word alle Grades ook delete.
RegisterClass link na SchoolGrade met OnDelete.Restrict → jy moet eers RegisterClass verwyder of aanpas voordat jy die SchoolGrade kan uitvee.

# Learner
Link na School met OnDelete.Cascade → as die Skool delete word, word Learners ook delete.
Link na RegisterClass met OnDelete.Restrict → kan nie RegisterClass uitvee as Learners steeds daaraan gekoppel is nie.
Link na Combination met OnDelete.Restrict → kan nie Combination uitvee as Learners steeds daaraan gekoppel is nie.
Het meer as een Parents → OnDelete.Cascade vanaf die Learner-kant: As ’n Learner delete word, word sy Parents ook delete.
Het meer as een LearnerSubjects → OnDelete.Cascade vanaf die Learner kant: As ’n Learner delete word, word sy LearnerSubject-rye ook delete.

# Parent
Link na Learner met OnDelete.Cascade → as ’n Learner delete word, word sy Parents ook delete.

# RegisterClass
Link na SchoolGrade met OnDelete.Restrict → Kan nie n SchoolGrade delete as daar n RegisterClass gekoppel is nie.
Link na Teacher met OnDelete.Restrict → kan nie n Teacher delete as daar n RegisterClass gekoppel is nie.
Het baie Learners OnDelete.Restrict → Waneer n RegisterClass delete word hou hy die Learners "unparented". 

# RegisterClassSubject
OnDelete.Restrict aan die SubjectId-kant, Waneer n RegisterClassSubject delete word delete hy nie die Subject nie.
OnDelete.Cascade aan die RegisterClassId-kant, Waneer n RegisterClass delete word, delete hy al die RegisterClassSubjects.

# Combination
Link na SchoolGrade met OnDelete.Restrict → kan nie SchoolGrade delete as daar Combinations is wat daarna Link nie.
Baie-tot-baie met Subject via CombinationSubject:
OnDelete.Restrict aan die SubjectId-kant, sal nie n subject delete as die CombinationSubject delete word nie.
OnDelete.Cascade aan die CombinationId-kant. As n combination delete word delete dit al sy CombinationSubjects.

# Subject
Om ’n Subject uit te vee is beperk as dit in enige van die volgende voorkom:
Link aan LearnerSubject (OnDelete.Restrict op SubjectId),
Link aan RegisterClassSubject (Restrict),
Link aan CombinationSubject (Restrict).

# LearnerSubject
Link na Learner met OnDelete.Cascade → as ’n Learner delete word, word sy LearnerSubject ook delete.
Link na Subject met OnDelete.Restrict → kan nie Subject uitvee as ’n LearnerSubject nog daarna Link nie.

# Period
Link na Teacher, Subject en SchoolGrade met OnDelete.Restrict → jy moet eers die Period verwyder/aanpas as jy daardie entiteite wil uitvee.
Die databassis maak seker n Teacher het net een Period op n slag tussen n StartTime en EndTime. 
En die sisteem check dat StartTime kleiner is as EndTime.

# Result
n Result is n rekord so as ons n Learner of n Subject Delete dan gebeer daar niks met n resultaat nie. Ons moet besluit wat ons wil doen as die Learner delete word want die resultaat link na n Learner toe as ons die Learner delete gaan ons nie weet wie se resultaat dit is nie.

# CareGroup
Waneer n Skool Delete word word alle CareGroups Delete.
Waneer n CareGroup Delete Word Unlink hy alle Learners van daai CareGroup af.


# Audience per campaing
# Caregroup <--> Teacher

# Subject show on UI which grades it is applicable for
# public List<SystemGrade>? GradesApplicable { get; set; }

Select 1 (Mathematics or Maths Literacy)
 Mathematics
 Mathematical Literacy

Combination -> Validation for each combination at least one needs to be selected.

Extra subject for only 10, 11, 12

# Class List
--> Register Class bv 8A -> Export
--> Combination Class List (10 - 12)
  --> SchoolGrade
  --> Physical Science -> Export
--> Combination Class List

# Leave Early
It integrates with the sign out module, if a learner is leaving early before the sign out record was created for the day it will confirm the school end time before continuing, here is the flow

1. Select a school
2. Check if a sign out record was created for the day
3. If no sign out record it should create one with the confirmed end of day time -- this will pull throug when a user now goes and navigate to the sign out module
4. If a sign out record was created before either through leave early or the sign out module the confirm time will be skipped on the leave early module
5. You select a learner before you can continue
6. Once a learner is selected the main flow will appear

When selecting Telephonic, notes needs to be mandatory
When selecting Letter, email or whatsapp it should expose a dragover and paste box where the user needs to paste evidence of the permission.

# Sign In / Sign Out
Configure at school level what the start time is (7:30) will pull through on the confirmation.
Attendance roles can't sign out learners before school end time.
If a learner left early for the instead of 'Absent' 'Left Early'
Cellphone handed in on sign in cellphone retreived at sign out or leave early.

# Daily Register
The daily register opens and check if there is a selected school like most pages, then a user selects a register class, after selecting the register class a list of learners in that register class will load with the default Absent pill. When clikcing on the Absent button it tunrs to Present.

# Communication
If the email is not sucessfully authenticated, a error must go to the user.

# Per Period Attendance
If a learner left early for the instead of 'Absent' 'Left Early'
If a leaner is clicked you will see the time of when he arrived.
Lock in
Then if you come in after lock in it will show late with red border

!SignenIn && !Registered == Show Absent [Not Signed In]
SignedIn && !Registered == Show Absent [Not Registered]
SignedIn && Registered == Show Absent

After lock in all Present on Daily Register but not in class Shows Not Attending
After lock in all Abent on Daily Register shows Absent
After lock in all Marked not Attending that shows up reads Late (11:33)

MAT


Changelog
-- Fixed Learner Code clashing between schools.
-- Fixed Historical Subjects not pulling through
-- Fixed Inactive Learners getting sent Progress Feedback
-- Fixed Date Oredering on Progress Feedbacks

[X] State Management (Results)
[X] Progress Feedback (Printing) Font smaller Print on one page
[X] Attendance if not signed in. Create a special record that says not sign in but present. (Active not at school for being present)
[] ADI (Attendance Module + Adhoc Learners) Build on Per Period
[] Reporting and Communication (WA + Email + Print) for Attendance
[] Clean up and refactor

Daily Register --> Daily Attendance
Add Period 1 - 12 to the per period attendance
Stop Period automatically called after 30 mins
Start with fresh list
Stop Period functions like a submit button
(ADI) - Academic Development Improvement
(MAP) - Matric Achievement Project