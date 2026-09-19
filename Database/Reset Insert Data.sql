-- =======================================================
-- 1. DELETE DATA (Child tables first, Parents last)
-- =======================================================

-- Level 4 (Deepest Dependencies)
DELETE FROM [Attendance];

-- Level 3
DELETE FROM [Lecture_Events];
DELETE FROM [Enrollments];
DELETE FROM [Courses_Prerequisites];
DELETE FROM [Courses_Departments];
DELETE FROM [Courses_Semesters];

-- Level 2
DELETE FROM [Course_Schedules];
DELETE FROM [Instructors_Courses];

-- Level 1 (Main entities)
DELETE FROM [Students];
DELETE FROM [Instructors];
DELETE FROM [Admins];
DELETE FROM [Courses];

-- Level 0 (Base / Lookup tables)
DELETE FROM [Semesters];
DELETE FROM [StudentsStatus];
DELETE FROM [InstructorsStatus];
DELETE FROM [CoursesStatus];
DELETE FROM [EnrollmentsStatus];
DELETE FROM [AdminsStatus];

-- =======================================================
-- 2. RESET IDENTITY COUNTERS BACK TO 0
-- =======================================================

DBCC CHECKIDENT ('[Attendance]', RESEED, 0);
DBCC CHECKIDENT ('[Lecture_Events]', RESEED, 0);
DBCC CHECKIDENT ('[Enrollments]', RESEED, 0);
DBCC CHECKIDENT ('[Courses_Prerequisites]', RESEED, 0);
DBCC CHECKIDENT ('[Courses_Departments]', RESEED, 0);
DBCC CHECKIDENT ('[Courses_Semesters]', RESEED, 0);
DBCC CHECKIDENT ('[Course_Schedules]', RESEED, 0);
DBCC CHECKIDENT ('[Instructors_Courses]', RESEED, 0);
DBCC CHECKIDENT ('[Students]', RESEED, 0);
DBCC CHECKIDENT ('[Instructors]', RESEED, 0);
DBCC CHECKIDENT ('[Admins]', RESEED, 0);
DBCC CHECKIDENT ('[Courses]', RESEED, 0);
DBCC CHECKIDENT ('[Departments]', RESEED, 0);
DBCC CHECKIDENT ('[Semesters]', RESEED, 0);
DBCC CHECKIDENT ('[StudentsStatus]', RESEED, 0);
DBCC CHECKIDENT ('[InstructorsStatus]', RESEED, 0);
DBCC CHECKIDENT ('[CoursesStatus]', RESEED, 0);
DBCC CHECKIDENT ('[EnrollmentsStatus]', RESEED, 0);
DBCC CHECKIDENT ('[AdminsStatus]', RESEED, 0);


-- 1. Temporarily disable the safety trigger
DISABLE TRIGGER [trg_PreventDepartmentDelete] ON [Departments];

-- 2. Delete the departments now that the trigger is sleeping
DELETE FROM [Departments];

-- 3. Reset the identity counter back to 0
DBCC CHECKIDENT ('[Departments]', RESEED, 0);

-- 4. Turn the safety trigger back on for the future
ENABLE TRIGGER [trg_PreventDepartmentDelete] ON [Departments];