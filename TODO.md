- Error Handling and validation when adding a student with the same CODE in Student Screen.
- Add Marks -> Select Subject -> Not pulling the subjects from the database.
- Add Marks -> Select Subject -> Implement Load Students.
- Subject Page is not showing the list of subjects.
- Teacher -> Add -> Email Validation is not working.
- Teacher list does not distingish between schools.
- Communication Page needs to be built.
- Make sure that all Authentication and Authorization works
- Force go to every page with every user type to see that everything is working
- Test all roles capabilities
- Setup Database Backups in SharePoint

# Bugs
State management is broken on selecting schools for grades.
State management is broken on selecting schools for register class.
Description for Subjuct is not pulling through in Index.razor
Grade is not pulling through for Combination on Index.razor
Combination details page is broken.
Edit Combinations should have the tickboxes instead of list select.
Adding Learner -> Combinations for other schools are pulling through.
Learner Grade is not pulling through on the Index Page.
System Administrators -> No System Administrators Found
State Management is an issue when you are viewing a list of principals.
Fitlering is incorrect on School Management for the schools selected.

# TODO
## Results
1. Select a Grade
2. Select a Subject
3. (System) Filters the students for that Grade and Subject
    - Use the Register Class for the student to understand what Grade they are in.
4. Show a list of students to capture results for.

# Tests
Deactivate a student and see if they come up in Results when filter by the grade and subject for that student.
 - Pass: Student is not showing
 - Fail: Student is displaying