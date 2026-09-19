GO

ALTER VIEW [vw_Students] AS
SELECT 
    [Students].[Id],
    [Students].[Name] AS [StudentName],
    [Students].[Name_AR] AS [StudentName_AR] ,
    [Students].[Email],
    [Students].[Password],
    [Students].[Gpa],
    [Students].[Year],
    [Students].[National_Id],
    [Departments].[Name] AS [DepartmentName],
    [Departments].[Code] AS [DepartmentCode],
    [StudentsStatus].[Description],
    
    -- Existing aggregates
    STRING_AGG([Courses].[Name], ', ') AS [EnrolledCourses],
    STRING_AGG([Courses].[Code], ', ') AS [CourseCodes],
    STRING_AGG([EnrollmentsStatus].[Description], ', ') AS [EnrollmentStatuses],
    
    -- NEW: Aggregate individual grade components separately (with COALESCE to fallback to '-')
    STRING_AGG(COALESCE(CAST([Grades].[Midterm_Grade] AS VARCHAR(10)), '-'), ', ') AS [MidtermGrades],
    STRING_AGG(COALESCE(CAST([Grades].[CourseWork_Grade] AS VARCHAR(10)), '-'), ', ') AS [CourseWorkGrades],
    STRING_AGG(COALESCE(CAST([Grades].[Final_Grade] AS VARCHAR(10)), '-'), ', ') AS [FinalGrades],
    STRING_AGG(COALESCE(CAST([Grades].[Total_Grade] AS VARCHAR(10)), '-'), ', ') AS [TotalGrades]

FROM [Students]
INNER JOIN [Departments] 
    ON [Students].[Department_Id] = [Departments].[Id]
INNER JOIN [StudentsStatus] 
    ON [Students].[Status_Id] = [StudentsStatus].[Id]
LEFT JOIN [Enrollments] 
    ON [Students].[Id] = [Enrollments].[Student_Id]
LEFT JOIN [Courses] 
    ON [Enrollments].[Course_Id] = [Courses].[Id]
LEFT JOIN [EnrollmentsStatus]
    ON [Enrollments].[Status_Id] = [EnrollmentsStatus].[Id]
LEFT JOIN [Grades]
    ON [Enrollments].[Id] = [Grades].[Enrollment_Id]

GROUP BY 
    [Students].[Id],
    [Students].[Name],
    [Students].[Name_AR] ,
    [Students].[Email],
    [Students].[Password],
    [Students].[Gpa],
    [Students].[Year],
    [Students].[National_Id],
    [Departments].[Name],
    [Departments].[Code],
    [StudentsStatus].[Description];
GO




ALTER VIEW vw_CourseDetails AS
SELECT 
    c.Id,
    c.Name,
    c.Name_AR,
    c.Code,
    c.Credit_Hours,
    c.Difficulty,
    c.Status_Id,
    s.Description AS StatusDescription,
    s.Description_AR AS StatusDescription_AR,
    d.Name AS DepartmentName,
    d.Code AS DepartmentCode,
    ISNULL(en.TotalStudents, 0) AS TotalStudents
FROM Courses c
LEFT JOIN Courses_Departments cd ON c.Id = cd.Course_Id
LEFT JOIN Departments d ON cd.Department_Id = d.Id
LEFT JOIN CoursesStatus s ON c.Status_Id = s.Id
LEFT JOIN (
    SELECT Course_Id, COUNT(DISTINCT Student_Id) AS TotalStudents 
    FROM Enrollments 
    GROUP BY Course_Id
) en ON c.Id = en.Course_Id;

















CREATE OR ALTER VIEW [vw_InstructorCourses] AS
SELECT 
    ic.[Id],
    ic.[Instructor_Id],
    ic.[Course_Id],
    ic.[Semester_Id],
    i.[Name] AS [InstructorName],
    c.[Name] AS [CourseName]
FROM [Instructors_Courses] ic
INNER JOIN [Instructors] i ON ic.[Instructor_Id] = i.[Id]
INNER JOIN [Courses] c ON ic.[Course_Id] = c.[Id];









CREATE VIEW [vw_InstructorsSchedule] AS
SELECT
    [Instructors].[Name] AS [InstructorsName] ,
    [Courses].[Name] AS [CoursesName] , 
    [Courses].[Credit_Hours] ,
    [Course_Schedules].[Day] ,
    [Course_Schedules].[Type] ,
    [Course_Schedules].[StartTime] ,
    [Course_Schedules].[EndTime] 

FROM [Instructors]
JOIN [Instructors_Courses] 
    ON [Instructors].[Id] = [Instructors_Courses].[Instructor_Id]
JOIN [Courses]
    ON [Instructors_Courses].[Course_Id]= [Courses].[Id]
JOIN [Course_Schedules]
    ON [Instructors_Courses].[Id] = [Course_Schedules].[Instructors_Courses_Id];




CREATE VIEW [vw_CourseRoster] AS 
SELECT
    [Courses].[Name] AS [CourseName],
    [Courses].[Credit_Hours],
    [Students].[Name] AS [StudentsName],
    [EnrollmentsStatus].[Description]
FROM [Students]
JOIN [Enrollments]
    ON [Students].[Id] = [Enrollments].[Student_Id]
JOIN [Courses]
    ON [Courses].[Id] = [Enrollments].[Course_Id]
JOIN [EnrollmentsStatus]
    ON [Enrollments].[Status_Id] =[EnrollmentsStatus].[Id];




CREATE VIEW [vw_StudentAttendance] AS
SELECT 
    [Students].[Name] AS [StudentName],
    [Courses].[Name] AS [CourseName],
    [Course_Schedules].[Type],
    [Lecture_Events].[Date_Of_Event] AS [LectureDate],
    [Lecture_Events].[Content_Covered] AS [LectureContent]
FROM [Attendance]
JOIN [Students]
    ON [Attendance].[Student_Id] = [Students].[Id]
JOIN [Lecture_Events]
    ON [Attendance].[LectureEvents_Id] = [Lecture_Events].[Id]
JOIN [Course_Schedules]
    ON [Lecture_Events].[Schedule_Id] = [Course_Schedules].[Id]
JOIN [Instructors_Courses]
    ON [Course_Schedules].[Instructors_Courses_Id] = [Instructors_Courses].[Id]
JOIN [Courses]
    ON [Instructors_Courses].[Course_Id] = [Courses].[Id];





CREATE VIEW [vw_CoursePrerequisites] AS
SELECT 
    [MainCourse].[Name] AS [CourseName],
    [MainCourse].[Code] AS [CourseCode],
    [PrereqCourse].[Name] AS [PrerequisiteName],
    [PrereqCourse].[Code] AS [PrerequisiteCode]
FROM [Courses_Prerequisites]
JOIN [Courses] AS [MainCourse] 
    ON [Courses_Prerequisites].[Course_Id] = [MainCourse].[Id]
JOIN [Courses] AS [PrereqCourse] 
    ON [Courses_Prerequisites].[Prerequisite_Id] = [PrereqCourse].[Id];



CREATE VIEW vw_EnrollmentDetails AS
SELECT
    e.Id,
    e.Student_Id AS StudentId,   -- Add if needed for filtering
    s.Name AS StudentName,
    e.Course_Id AS CourseId,     -- Add if needed for filtering
    c.Name AS CourseName,
    e.Status_Id AS StatusId,     -- Add this line so you can filter by status ID!
    es.Description AS StatusName
FROM
    Enrollments e
INNER JOIN
    Students s ON e.Student_Id = s.Id
INNER JOIN
    Courses c ON e.Course_Id = c.Id
INNER JOIN
    EnrollmentsStatus es ON e.Status_Id = es.Id;


    USE [Universityportal];
GO




ALTER VIEW [vw_Instructors] AS
SELECT 
    [Instructors].[Id],
    [Instructors].[Name] AS [InstructorName],
    [Instructors].[Name_AR] AS [InstructorName_AR],
    [Instructors].[Email],
    [Instructors].[Password],
    [Instructors].[National_Id],
    [Instructors].[Start_year],
    [Instructors].[Salary],
    [Instructors].[OfficeHours], -- 🌟 NEW FIELD
    [Departments].[Name] AS [DepartmentName],
    [InstructorsStatus].[Description] AS [StatusDescription],

    STRING_AGG([Courses].[Name], ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [TaughtCourses],
    STRING_AGG([Courses].[Code], ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [CourseCodes],
    STRING_AGG(CAST([Courses].[Id] AS VARCHAR), ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [CourseIds],
    
    STRING_AGG(ISNULL([Course_Schedules].[Day], 'TBA'), ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [ScheduleDays],
    STRING_AGG(ISNULL(CAST([Course_Schedules].[StartTime] AS VARCHAR(5)) + ' - ' + CAST([Course_Schedules].[EndTime] AS VARCHAR(5)), 'TBA'), ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [ScheduleTimes],
    STRING_AGG(ISNULL([Course_Schedules].[Type], 'Session'), ', ') WITHIN GROUP (ORDER BY [Courses].[Id]) AS [ScheduleTypes]

FROM [Instructors]
INNER JOIN [Departments] ON [Instructors].[Department_Id] = [Departments].[Id]
INNER JOIN [InstructorsStatus] ON [Instructors].[Status_Id] = [InstructorsStatus].[Id]
LEFT JOIN [Instructors_Courses] ON [Instructors].[Id] = [Instructors_Courses].[Instructor_Id]
LEFT JOIN [Courses] ON [Instructors_Courses].[Course_Id] = [Courses].[Id]
LEFT JOIN [Course_Schedules] ON [Instructors_Courses].[Id] = [Course_Schedules].[Instructors_Courses_Id]

GROUP BY 
    [Instructors].[Id], [Instructors].[Name], [Instructors].[Name_AR], [Instructors].[Email],
    [Instructors].[Password], [Instructors].[National_Id], [Instructors].[Start_year], [Instructors].[Salary],
    [Instructors].[OfficeHours], -- 🌟 NEW FIELD
    [Departments].[Name], [InstructorsStatus].[Description];
GO



CREATE OR ALTER VIEW vw_Admins
AS
SELECT 
    a.Id,
    a.Name,
    a.Email,
    a.National_id,
    a.Start_year,
    a.Salary,
    a.Role,
    a.Status_id,
    s.Description AS StatusDescription
FROM Admins a
LEFT JOIN AdminsStatus s ON a.Status_id = s.Id;



CREATE VIEW vw_Departments AS
SELECT 
    d.Id,
    d.Name,
    d.Name_AR,
    d.Code,
    COUNT(DISTINCT s.Id) AS TotalStudents,
    COUNT(DISTINCT i.Id) AS TotalInstructors,
    COUNT(DISTINCT cd.Course_id) AS TotalCourses,
    AVG(s.Gpa) AS AverageGpa
FROM Departments d
LEFT JOIN Students s ON s.Department_Id = d.Id
LEFT JOIN Instructors i ON i.Department_Id = d.Id
LEFT JOIN Courses_Departments cd ON cd.Department_Id = d.Id
GROUP BY d.Id, d.Name, d.Name_AR, d.Code;


CREATE OR ALTER VIEW vw_InstructorCourses AS
SELECT 
    ic.Id,
    i.Name AS InstructorName,
    c.Name AS CourseName
FROM Instructors_Courses ic
INNER JOIN Instructors i ON ic.Instructor_id = i.Id
INNER JOIN Courses c ON ic.Course_id = c.Id;






SELECT cs.Id AS ScheduleId, c.Name AS CourseName, cs.Type 
FROM Course_Schedules cs
JOIN Instructors_Courses ic ON cs.Instructors_Courses_Id = ic.Id
JOIN Courses c ON ic.Course_Id = c.Id
WHERE cs.Id = 21;





ALTER VIEW vw_AttendanceDetails AS
SELECT 
    a.Id AS Id,
    le.Id AS LectureEvents_Id,
    le.Schedule_Id AS Course_Schedules_Id,
    le.Date_Of_Event AS AttendanceDate,
    s.Id AS Student_Id,
    s.Name AS StudentName,
    c.Name AS CourseName,
    c.Code AS CourseCode,
    cs.Day AS ScheduleDay,
    cs.StartTime,
    cs.EndTime
FROM Lecture_Events le
JOIN Course_Schedules cs ON le.Schedule_Id = cs.Id
JOIN Instructors_Courses ic ON cs.Instructors_Courses_Id = ic.Id
JOIN Courses c ON ic.Course_Id = c.Id
JOIN Enrollments e ON e.Course_Id = c.Id AND e.Status_Id = 1
JOIN Students s ON e.Student_Id = s.Id
JOIN Attendance a ON a.LectureEvents_Id = le.Id AND a.Student_Id = s.Id;





CREATE OR ALTER VIEW vw_SemesterCourses
AS
SELECT 
    cs.Id,
    cs.Course_Id,
    cs.Semester_Id,
    s.[Name] AS SemesterName,
    c.Code AS CourseCode,
    c.[Name] AS CourseName,
    c.Credit_Hours,
    c.Difficulty,
    d.Code AS DepartmentCode
FROM Courses_Semesters cs
INNER JOIN Courses c ON cs.Course_Id = c.Id
INNER JOIN Semesters s ON cs.Semester_Id = s.Id
LEFT JOIN Courses_Departments cd ON c.Id = cd.Course_id
LEFT JOIN Departments d ON cd.Department_id = d.Id;






CREATE OR ALTER VIEW [dbo].[vw_CourseGrades] AS
SELECT 
    e.Id AS EnrollmentId,
    e.Course_Id AS CourseId,
    s.Id AS StudentId,
    s.Name AS StudentName,
    c.Name AS CourseName,
    c.Code AS CourseCode,
    c.Credit_Hours AS Credits,
    e.Semester_Id AS SemesterId,
    sem.Name AS SemesterName,
    es.Description AS StatusName,
    g.Midterm_Grade AS MidtermGrade,
    g.CourseWork_Grade AS CourseWorkGrade,
    g.Final_Grade AS FinalGrade,
    g.Total_Grade AS TotalGrade,
    
    -- 🌟 Official University Grading Rules Scale
    CASE 
        WHEN g.Total_Grade IS NULL THEN '-'
        WHEN g.Total_Grade >= 97 THEN 'A+'
        WHEN g.Total_Grade >= 93 THEN 'A'
        WHEN g.Total_Grade >= 89 THEN 'A-'
        WHEN g.Total_Grade >= 84 THEN 'B+'
        WHEN g.Total_Grade >= 80 THEN 'B'
        WHEN g.Total_Grade >= 76 THEN 'B-'
        WHEN g.Total_Grade >= 73 THEN 'C+'
        WHEN g.Total_Grade >= 70 THEN 'C'
        WHEN g.Total_Grade >= 67 THEN 'C-'
        WHEN g.Total_Grade >= 64 THEN 'D+'
        WHEN g.Total_Grade >= 60 THEN 'D'
        ELSE 'F'
    END AS LetterGrade
FROM Enrollments e
INNER JOIN Students s ON e.Student_Id = s.Id
INNER JOIN Courses c ON e.Course_Id = c.Id
LEFT JOIN Semesters sem ON e.Semester_Id = sem.Id
LEFT JOIN EnrollmentsStatus es ON e.Status_Id = es.Id
LEFT JOIN Grades g ON g.Enrollment_Id = e.Id;
GO













ALTER VIEW [dbo].[vw_EnrollmentDetails] AS
SELECT 
    e.[Id],
    e.[Student_Id] AS StudentId,
    s.[Name] AS StudentName,
    e.[Course_Id] AS CourseId,
    c.[Name] AS CourseName,
    e.[Semester_Id] AS SemesterId,
    sem.[Name] AS SemesterName,
    e.[Status_Id] AS StatusId,
    es.[Description] AS StatusName
FROM [Enrollments] e
INNER JOIN [Students] s ON e.[Student_Id] = s.[Id]
INNER JOIN [Courses] c ON e.[Course_Id] = c.[Id]
INNER JOIN [Semesters] sem ON e.[Semester_Id] = sem.[Id]
INNER JOIN [EnrollmentsStatus] es ON e.[Status_Id] = es.[Id];
GO