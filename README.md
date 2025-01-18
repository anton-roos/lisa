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
 