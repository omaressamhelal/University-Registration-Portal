
ALTER DATABASE [Universityportal] COLLATE Arabic_CI_AS;


CREATE TABLE [Departments] (
	[Id]     INT IDENTITY(1,1) PRIMARY KEY ,
	[Name]    VARCHAR(100)   UNIQUE NOT NULL ,
	[Name_AR] NVARCHAR(100)  UNIQUE ,
	[Code]    VARCHAR(3)  UNIQUE NOT NULL  
);

ALTER TABLE [Departments] ALTER COLUMN [Code] VARCHAR(10);

CREATE TABLE [StudentsStatus]   (
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Description]    VARCHAR(50) UNIQUE NOT NULL  , 
	[Description_AR] NVARCHAR(50) UNIQUE

);

CREATE TABLE [InstructorsStatus]   (
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Description]    VARCHAR(50)  NOT NULL , 
	[Description_AR] NVARCHAR(50)
);

CREATE TABLE [CoursesStatus]   (
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Description]   VARCHAR(50) NOT NULL , 
	[Description_AR] NVARCHAR(50)


CREATE TABLE [EnrollmentsStatus]   (
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Description]   VARCHAR(50) NOT NULL ,
	[Description_AR] NVARCHAR(50)
);

CREATE TABLE [AdminsStatus]   (
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Description]   VARCHAR(50) NOT NULL ,
	[Description_AR] NVARCHAR(50)
);


CREATE TABLE [Students] (
	[Id]     INT IDENTITY(1,1) PRIMARY KEY ,
	[Name]     NVARCHAR(100)  NOT NULL ,
	[Name_AR] NVARCHAR(100)   ,
	[Email]     VARCHAR(100)  UNIQUE NOT NULL ,
	[Password]  VARCHAR(255)  NOT NULL ,
	[Gpa]       DECIMAL(3,2)   ,
	[Year]       INT    NOT NULL DEFAULT 1 CHECK([Year]>0 AND [Year]<7)  ,
	[National_Id]  VARCHAR(20) UNIQUE NOT NULL  ,
	[Department_Id]  INT  NOT NULL ,
	[Status_Id]    INT  NOT NULL ,

	CONSTRAINT [FK_Students_Status] 
	FOREIGN KEY ([Status_Id]) REFERENCES [StudentsStatus] ([Id]) ,

	CONSTRAINT [FK_Students_Departments] 
	FOREIGN KEY ([Department_Id]) REFERENCES [Departments] ([Id])
);


CREATE TABLE [Instructors] (
	[Id]      INT IDENTITY (1,1) PRIMARY KEY ,
	[Name]      NVARCHAR(100) NOT NULL   ,
	[Name_AR]    NVARCHAR(100)   ,
	[Email]       VARCHAR(100) UNIQUE NOT NULL ,
	[Password]    VARCHAR(255)  NOT NULL ,
	[Start_year]     INT   NOT NULL   ,
	[Department_Id]   INT  NOT NULL ,
	[National_Id]  VARCHAR(20)  UNIQUE NOT NULL,
	[Salary]        FLOAT NOT NULL CHECK([Salary]>0)   ,
	[Status_Id]    INT NOT NULL ,    
	
	CONSTRAINT [FK_Instructors_Status] 
	FOREIGN KEY ([Status_Id]) REFERENCES [InstructorsStatus] ([Id]) ,

	CONSTRAINT [FK_Instructors_Departments]
	FOREIGN KEY ([Department_Id]) REFERENCES [Departments] ([Id])
);


ALTER TABLE Instructors ADD OfficeHours NVARCHAR(100) NULL;
GO


CREATE TABLE [Admins] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Name_AR] NVARCHAR(100),
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    [National_Id] NVARCHAR(20) NULL,
    [Start_year] INT NULL,
    [Salary] FLOAT NOT NULL CHECK([Salary] > 0),
    [Status_Id] INT NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,

    CONSTRAINT [FK_Admins_Status]
    FOREIGN KEY ([Status_Id]) REFERENCES [AdminsStatus] ([Id])
);



CREATE TABLE [Courses] (
	[Id]         INT IDENTITY(1,1) PRIMARY KEY  ,
	[Name]         VARCHAR(50) UNIQUE NOT NULL  ,
	[Name_AR]      NVARCHAR(50)                   ,
	[Code]          VARCHAR(5) UNIQUE NOT NULL  ,
	[Credit_Hours]     INT   NOT NULL CHECK([Credit_Hours]>0) ,
	[Difficulty]       INT      CHECK([Difficulty]>0) ,
	[Status_Id]    INT  NOT NULL ,  
	
	CONSTRAINT [FK_Courses_Status] 
	FOREIGN KEY ([Status_Id]) REFERENCES [CoursesStatus] ([Id]) 

);



ALTER TABLE [Courses] ALTER COLUMN [Code] VARCHAR(10);

CREATE TABLE [Semesters] (
	[Id]        INT IDENTITY(1,1) PRIMARY KEY   ,
	[Name]       VARCHAR(50)  NOT NULL   ,
	[Name_AR]      NVARCHAR(50)   ,
	[Start_date]   DATE  NOT NULL,
	[End_date]     DATE    NOT NULL   ,
	[Is_Registration_Open]    BIT NOT NULL 

);



CREATE TABLE Grades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Enrollment_Id INT NOT NULL,
    
    -- Grades allow up to 999.99 (DECIMAL(5,2)) and default to NULL until entered
    Midterm_Grade DECIMAL(5,2) NULL,
    CourseWork_Grade DECIMAL(5,2) NULL,
    Final_Grade DECIMAL(5,2) NULL,
    
    -- 🌟 SMART FEATURE: This column automatically calculates the total!
    -- You never have to manually update this; the database does the math.
    Total_Grade AS (ISNULL(Midterm_Grade, 0) + ISNULL(CourseWork_Grade, 0) + ISNULL(Final_Grade, 0)),

    -- Foreign Key linking back to your Enrollments table
    CONSTRAINT FK_Grades_Enrollments FOREIGN KEY (Enrollment_Id) REFERENCES Enrollments(Id),
    
    -- Ensure we don't accidentally create two grade rows for the same enrollment
    CONSTRAINT UQ_Grades_Enrollment UNIQUE (Enrollment_Id)
);


--FIRST JUNCTION TABLE 
CREATE TABLE [Enrollments] (
	[Id]      INT IDENTITY(1,1) PRIMARY KEY    ,
	[Student_Id]   INT NOT NULL  ,
	[Course_Id]    INT NOT NULL  ,
	[Status_Id]    INT NOT NULL ,
	
	CONSTRAINT [FK_Enrollments_Status] 
	FOREIGN KEY ([Status_ID]) REFERENCES [EnrollmentsStatus] ([Id]) ,
	

	CONSTRAINT [FK_Enrollments_Students]  
	FOREIGN KEY ([Student_Id]) REFERENCES [Students] ([Id]) ,
	
	CONSTRAINT [FK_Enrollments_Courses]
	FOREIGN KEY ([Course_Id]) REFERENCES [Courses]  ([Id]) ,

	CONSTRAINT [UQ_Enrollments_Student_Course]
    UNIQUE ([Student_Id], [Course_Id])
);


-- 1. Add Semester_Id column to the Enrollments table
ALTER TABLE [Enrollments]
ADD [Semester_Id] INT NULL;
GO

-- 2. Add Foreign Key constraint linking Enrollments to Semesters
ALTER TABLE [Enrollments]
ADD CONSTRAINT [FK_Enrollments_Semesters] 
FOREIGN KEY ([Semester_Id]) REFERENCES [Semesters]([Id]);
GO

-- 1. Drop the old constraint that blocks retakes across semesters
ALTER TABLE [Enrollments] DROP CONSTRAINT UQ_Enrollments_Student_Course;
GO

-- 2. Add a new constraint that allows the same course in different semesters, 
--    but still prevents duplicate enrollment in the same semester.
ALTER TABLE [Enrollments] ADD CONSTRAINT UQ_Enrollments_Student_Course_Semester 
UNIQUE (Student_Id, Course_Id, Semester_Id);
GO

-- 3. (Optional) If you had a unique index preventing duplicate student-course rows, 
-- drop it and recreate it to include Semester_Id so retakes are allowed:
-- DROP INDEX IX_Enrollments_Student_Course ON [Enrollments];
-- CREATE UNIQUE INDEX IX_Enrollments_Student_Course_Semester 
-- ON [Enrollments]([Student_Id], [Course_Id], [Semester_Id]);



--SECOND JUNCTION TABLE 
CREATE TABLE [Instructors_Courses] (
	[Id]     INT IDENTITY(1,1) PRIMARY KEY   ,
	[Instructor_Id]  INT   NOT NULL,
	[Course_Id]      INT  NOT NULL  ,

	CONSTRAINT [FK_Instructors_Courses_Instructors]  
	FOREIGN KEY ([Instructor_Id]) REFERENCES [Instructors] ([Id]) ,

	CONSTRAINT [FK_Instructors_Courses_Courses]  
	FOREIGN KEY ([Course_Id]) REFERENCES [Courses] ([Id]) ,

	CONSTRAINT [UQ_Instructors_Courses_Combination]
    UNIQUE ([Instructor_id], [Course_id])
);

ALTER TABLE [Instructors_Courses]
ADD [Semester_Id] INT NOT NULL DEFAULT 1;

ALTER TABLE [Instructors_Courses]
ADD CONSTRAINT [FK_Instructors_Courses_Semesters] 
FOREIGN KEY ([Semester_Id]) REFERENCES [Semesters]([Id]);



--Course_Schedules Table
CREATE TABLE [Course_Schedules]  (
	[Id]    INT PRIMARY KEY IDENTITY(1,1)  ,
	[Instructors_Courses_Id] INT NOT NULL ,
	[Day]      VARCHAR(50) NOT NULL ,
	[Day_AR]   NVARCHAR(50)   ,
	[Type]     VARCHAR(50) NOT NULL  ,
	[Type_AR]   NVARCHAR(50)   ,
	[StartTime] TIME NOT NULL, 
	[EndTime]   TIME NOT NULL,

	CONSTRAINT [FK_Course_Schedules_Instructor_Courses] 
	FOREIGN KEY ([Instructors_Courses_Id]) REFERENCES [Instructors_Courses] ([Id]) 
);


--LectureEvents table
CREATE TABLE [Lecture_Events] (
	[Id]  INT PRIMARY KEY IDENTITY(1,1) ,
	[Schedule_Id] INT NOT NULL, 
    [Date_Of_Event] DATE NOT NULL,
    [Content_Covered] VARCHAR(255),

	CONSTRAINT [FK_Lecture_Events_Course_Schedules]
	FOREIGN KEY ([Schedule_Id]) REFERENCES [Course_Schedules] ([Id]) ,

	CONSTRAINT [UQ_Lecture_Events_Instance]
    UNIQUE ([Schedule_Id], [Date_Of_Event])
);



--THIRD JUNCTION TABLE 
CREATE TABLE [Courses_Departments] (
	[Id]     INT IDENTITY(1,1) PRIMARY KEY   ,
	[Course_Id]   INT NOT NULL  ,
	[Department_Id]   INT NOT NULL ,

	CONSTRAINT [FK_Courses_Department_Courses]
	FOREIGN KEY ([Course_id]) REFERENCES [Courses] ([Id])  ,

	CONSTRAINT [FK_Courses_Department_Departments]
	FOREIGN KEY ([Department_id]) REFERENCES [Departments] ([Id]) ,

	CONSTRAINT [UQ_Courses_Department_Combination]
    UNIQUE ([Course_id], [Department_id])

);

--FOURTH JUNCTION TABLE 0
CREATE TABLE [Courses_Semesters] (  
	[Id]    INT IDENTITY(1,1) PRIMARY KEY ,
	[Course_Id]   INT NOT NULL  ,
	[Semester_Id]   INT NOT NULL ,

	CONSTRAINT [UQ_Courses_Semester] 
    UNIQUE  ([Course_Id], [Semester_Id]),

	CONSTRAINT [FK_Courses_Department_Courses1]
	FOREIGN KEY ([Course_id]) REFERENCES [Courses] ([Id])  ,

	CONSTRAINT [FK_Courses_Department_Departments1]
	FOREIGN KEY ([Semester_Id]) REFERENCES [Semesters] ([Id])

);


--FIFTH JUNCTION TABLE 
CREATE TABLE [Courses_Prerequisites] (  
	[Id]   INT IDENTITY(1,1) PRIMARY KEY ,
	[Course_Id]   INT NOT NULL  ,
	[Prerequisite_Id]   INT NOT NULL ,

	CONSTRAINT [UQ_Courses_Prerequites] 
    UNIQUE ([Course_Id], [prerequisite_id]),

	CONSTRAINT [FK_Courses_Prerequisites_Courses] 
	FOREIGN KEY ([Course_Id]) REFERENCES [Courses] ([Id])  ,

	CONSTRAINT [FK_Courses_Prerequisites_Prerequisites] 
	FOREIGN KEY ([Prerequisite_id]) REFERENCES [Courses] ([Id])
);




--	Attendance JUNCTION TABLE
CREATE TABLE [Attendance] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    
    -- This single ID tells us BOTH the Date and the Course Schedule!
    [LectureEvents_Id] INT NOT NULL, 
    
    -- Who showed up?
    [Student_Id] INT NOT NULL,

    CONSTRAINT [FK_Attendance_LectureEvents] 
    FOREIGN KEY ([LectureEvents_Id]) REFERENCES [Lecture_Events]([Id]),
    
    CONSTRAINT [FK_Attendance_Students] 
    FOREIGN KEY ([Student_Id]) REFERENCES [Students]([Id]),
    
    -- CRITICAL: Ensures a student can't be marked present TWICE for the exact same lecture event!
    CONSTRAINT [UQ_Attendance_Record] 
    UNIQUE ([LectureEvents_Id], [Student_Id])
);
















