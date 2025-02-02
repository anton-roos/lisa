# TODO

## Backups
- Setup Database Backups in SharePoint

## Results
1. Select a SchoolGrade
2. Select a Subject
3. (System) Filters the students for that SchoolGrade and Subject
    - Use the Register Class for the student to understand what SchoolGrade they are in.
4. Show a list of students to capture results for.

# Validation
 - Ensure Result date is not past today's date.

## Subject Comb
 - Needs a button to duplicate a subject combination 
   - (After duplication it will go to add page)
   - (On Add page the SchoolGrade will be deselected)

# Tests
Deactivate a student and see if they come up in Results when filter by the SchoolGrade and subject for that student.
 - Pass: Student is not showing
 - Fail: Student is displaying

# Questions
Should we incldue [X] Active tickbox when adding a Learner?

Capture Marks
Selecting a SchoolGrade should only show subjects applicable to that SchoolGrade.

# Capture Results
State Management is broken when selecting different schools.

