# Learner Information System Administrator (LISA)

## User Types
 - System Administrator
 - Principal
 - School Management
 - Administrator
 - Teacher

## User Type Priveleges
### Super Admin
 - Do what Principals does at every school.
 - Add or Remove Other Super Admins.
 - Configure Communication Templates
 - Change School Information
   - Add Cirriculum Information
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
 - Move Students Between Care Groups

### Teacher
 - Can capture marks for their subjects
 - Can monitor period behaviour
 - Can manage period attendance

# Credentials

## Impact
portali@impactindependent.co.za
Portal@II

## Destiny
portalDISK@destiny-kemptonpark.co.za
Portal@DISK

## Greenacres
portalGA@broadlands-schools.co.za
Portal@GA

## Broadlands
portalBL@broadlands-schools.co.za
Portal@BL

## DCEG
portalDCEG@dcegroup.co.za
Portal@DCEG

# Workflow
 1. Seed the Database (Roles, Admin User, School Types)
 1. Setup the System Administrator (When app runs with DB Seed)
 2. Setup the School
 
# Deleting Entities
## Skool
As ’n Skool delete word, delete dit (cascade) ook Principals, Administrators, SchoolManagements, Teachers, Grades en Learners.
Die Skool het ’n unieke ShortName nie langer as 20 karakters nie.

## Grade
Link na School met OnDelete.Cascade → as die Skool delete word, word alle Grades ook delete.
RegisterClass link na Grade met OnDelete.Restrict → jy moet eers RegisterClass verwyder of aanpas voordat jy die Grade kan uitvee.

# Learner
Link na School met OnDelete.Cascade → as die Skool delete word, word Learners ook delete.
Link na RegisterClass met OnDelete.Restrict → kan nie RegisterClass uitvee as Learners steeds daaraan gekoppel is nie.
Link na Combination met OnDelete.Restrict → kan nie Combination uitvee as Learners steeds daaraan gekoppel is nie.
Het meer as een Parents → OnDelete.Cascade vanaf die Learner-kant: As ’n Learner delete word, word sy Parents ook delete.
Het meer as een LearnerSubjects → OnDelete.Cascade vanaf die Learner kant: As ’n Learner delete word, word sy LearnerSubject-rye ook delete.

# Parent
Link na Learner met OnDelete.Cascade → as ’n Learner delete word, word sy Parents ook delete.

# RegisterClass
Link na Grade met OnDelete.Restrict → Kan nie n Grade delete as daar n RegisterClass gekoppel is nie.
Link na Teacher met OnDelete.Restrict → kan nie n Teacher delete as daar n RegisterClass gekoppel is nie.
Het baie Learners OnDelete.Restrict → Waneer n RegisterClass delete word hou hy die Students "unparented". 

# RegisterClassSubject
OnDelete.Restrict aan die SubjectId-kant, Waneer n RegisterClassSubject delete word delete hy nie die Subject nie.
OnDelete.Cascade aan die RegisterClassId-kant, Waneer n RegisterClass delete word, delete hy al die RegisterClassSubjects.

# Combination
Link na Grade met OnDelete.Restrict → kan nie Grade delete as daar Combinations is wat daarna Link nie.
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
Link na Teacher, Subject en Grade met OnDelete.Restrict → jy moet eers die Period verwyder/aanpas as jy daardie entiteite wil uitvee.
Die databassis maak seker n Teacher het net een Period op n slag tussen n StartTime en EndTime. 
En die sisteem check dat StartTime kleiner is as EndTime.

# Result
n Result is n rekord so as ons n Learner of n Subject Delete dan gebeer daar niks met n resultaat nie. Ons moet besluit wat ons wil doen as die Learner delete word want die resultaat link na n Learner toe as ons die Learner delete gaan ons nie weet wie se resultaat dit is nie.

# CareGroup
Waneer n Skool Delete word word alle CareGroups Delete.
Waneer n CareGroup Delete Word Unlink hy alle Learners van daai CareGroup af.
