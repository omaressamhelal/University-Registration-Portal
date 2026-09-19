CREATE TRIGGER [trg_EnforceAttendacneEnrollment]
ON [Attendance] AFTER INSERT 
AS
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM [inserted]
		JOIN [Lecture_Events] ON [inserted].[LectureEvents_Id] = [Lecture_Events].[Id]
		JOIN [Course_Schedules] ON [Course_Schedules].[Id] = [Lecture_Events].[Schedule_Id]
		JOIN [Instructors_Courses] ON [Instructors_Courses].[Id]= [Course_Schedules].[Instructors_Courses_Id]
		LEFT JOIN [Enrollments] ON [Enrollments].[Student_Id] = [inserted].[Student_Id] AND [Enrollments].[Course_Id] = [Instructors_Courses].[Course_Id]
		WHERE [Enrollments].[Id] IS NULL 
		)
		BEGIN
        -- Block the insert and throw an error back to your C# application
		;THROW 50001, 'Attendance rejected: Student is not enrolled in this course.', 1;
        ROLLBACK TRANSACTION;
    END
END;


CREATE TRIGGER [trg_PreventDepartmentDelete]
ON [Departments]
INSTEAD OF DELETE
AS
BEGIN
    -- Block the delete and send an error message to the user
    ;THROW 50002 ,'Action Blocked: You cannot delete a Department from the system.', 1 
END;

		