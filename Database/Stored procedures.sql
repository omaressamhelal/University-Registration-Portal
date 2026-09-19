
--Modify procedure for (Students)
CREATE PROCEDURE usp_ManageStudent
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @Name_AR NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Password VARCHAR(255) = NULL,
    @Gpa DECIMAL(3,2) = NULL,
    @Year INT = NULL,
    @National_Id VARCHAR(20) = NULL,
    @DepartmentCode VARCHAR(50) = NULL,
    @StatusDescription VARCHAR(50) = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50013, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Department Code exists
            IF @DepartmentCode IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Departments] WHERE [Code] = @DepartmentCode)
            BEGIN
                ;THROW 50001, 'Validation Failed: The provided Department Code does not exist.', 1;
            END

            -- Validate Status Desciption exists (Only needed if provided)
            IF @StatusDescription IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [StudentsStatus] WHERE LOWER(TRIM(Description)) = LOWER(TRIM(@StatusDescription)))
                BEGIN
            ;THROW 50002, 'Validation Failed: The provided Student Status does not exist.', 1;
             END

            -- Validate Email Uniqueness
            IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM [Students] WHERE [Email] = @Email AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50003, 'Validation Failed: This Email is already registered to another student.', 1;
            END
            
            -- Validate Email Format
            IF @Email IS NOT NULL AND @Email NOT LIKE '%_@__%.__%'
            BEGIN
                ;THROW 50004, 'Validation Failed: The provided Email format is invalid.', 1;
            END

            -- Validate National ID Uniqueness
            IF @National_Id IS NOT NULL AND EXISTS (SELECT 1 FROM [Students] WHERE [National_Id] = @National_Id AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50005, 'Validation Failed: This National ID is already registered.', 1;
            END

            -- Validate Year
            IF @Year IS NOT NULL AND (@Year <= 0 OR @Year >= 7)
            BEGIN
                ;THROW 50006, 'Validation Failed: Academic Year must be between 1 and 6.', 1;
            END

            -- Validate GPA
            IF @Gpa IS NOT NULL AND (@Gpa < 0.00 OR @Gpa > 4.00)
            BEGIN
                ;THROW 50007, 'Validation Failed: GPA must be a valid number between 0.00 and 4.00.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = ''
            BEGIN
                ;THROW 50010, 'Validation Failed: Student Name is required.', 1;
            END

            IF @Email IS NULL OR LTRIM(RTRIM(@Email)) = ''
            BEGIN
                ;THROW 50011, 'Validation Failed: Email is required.', 1;
            END

            IF @Password IS NULL OR LTRIM(RTRIM(@Password)) = ''
            BEGIN
                ;THROW 50012, 'Validation Failed: Password is required.', 1;
            END

            IF @Year IS NULL
            BEGIN
                ;THROW 50013, 'Validation Failed: Year is required.', 1;
            END

            IF @DepartmentCode IS NULL OR LTRIM(RTRIM(@DepartmentCode)) = ''
            BEGIN
                ;THROW 50014, 'Validation Failed: Department Code is required.', 1;
            END

            IF @National_Id IS NULL OR LTRIM(RTRIM(@National_Id)) = ''
            BEGIN
                ;THROW 50015, 'Validation Failed: National ID is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Students] (
                [Name], [Name_AR], [Email], [Password], [Gpa],
                [Year], [National_Id], [Department_Id], [Status_Id]
            )
            VALUES (
                @Name, @Name_AR, @Email, @Password, @Gpa,
                @Year, @National_Id, 
                
                -- Translate Department Code to ID
                (SELECT Id FROM Departments WHERE Code = @DepartmentCode), 
                
                -- Hardcode default status ID for new students (e.g., 1 for Active/Pending)
                1 
            );
    
            SET @ProcessingMessage = 'Student registered successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Students]
            SET 
                [Name] = @Name,
                [Name_AR] = @Name_AR,
                [Email] = @Email,
                [Password] = @Password,
                [Gpa] = @Gpa,
                [Year] = @Year,
                [National_Id] = @National_Id,
                [Department_Id] = (SELECT Id FROM Departments WHERE Code = @DepartmentCode),
                [Status_Id] = (SELECT Id FROM StudentsStatus WHERE Description = @StatusDescription)
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50016, 'No student found to update.', 1;
            END
                
            SET @ProcessingMessage = 'Student updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Students WHERE Id = @Id)
            BEGIN
                ;THROW 50017, 'Delete Failed: Student ID not found.', 1;
            END
                
            DELETE FROM [Students] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50018, 'No student found to delete.', 1;
            END
                
            SET @ProcessingMessage = 'Student deleted successfully.';
        END
        
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50019, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;









----Modify procedure for (Instructor)

ALTER PROCEDURE usp_ManageInstructor
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @Name_AR NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Password VARCHAR(255) = NULL,
    @Start_year INT = NULL,
    @National_Id VARCHAR(20) = NULL,
    @Salary FLOAT = NULL,
    @Department_Id INT = NULL,
    @Status_Id INT = NULL,
    @OfficeHours NVARCHAR(100) = NULL, -- 🌟 ADDED PARAMETER
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50013, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Department
            IF @Department_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Departments] WHERE [Id] = @Department_Id)
            BEGIN
                ;THROW 50001, 'Validation Failed: The provided Department does not exist.', 1;
            END

            -- Validate Instructor Status
            IF @Status_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [InstructorsStatus] WHERE [Id] = @Status_Id)
            BEGIN
                ;THROW 50002, 'Validation Failed: The provided Instructor Status does not exist.', 1;
            END

            -- Validate Email Uniqueness
            IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM [Instructors] WHERE [Email] = @Email AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50003, 'Validation Failed: This Email is already registered to another instructor.', 1;
            End
            
            -- Validate Email Format
            IF @Email IS NOT NULL AND @Email NOT LIKE '%_@__%.__%'
            BEGIN
                ;THROW 50004, 'Validation Failed: The provided Email format is invalid.', 1;
            END

            -- Validate National ID Uniqueness
            IF @National_Id IS NOT NULL AND EXISTS (SELECT 1 FROM [Instructors] WHERE [National_Id] = @National_Id AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50005, 'Validation Failed: This National ID is already registered.', 1;
            END

            -- Validate Salary
            IF @Salary IS NOT NULL AND @Salary <= 0
            BEGIN
                ;THROW 50006, 'Validation Failed: Salary must be greater than zero.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = ''
            BEGIN
                ;THROW 50010, 'Validation Failed: Instructor Name is required.', 1;
            END

            IF @Email IS NULL OR LTRIM(RTRIM(@Email)) = ''
            BEGIN
                ;THROW 50011, 'Validation Failed: Email is required.', 1;
            END

            IF @Password IS NULL OR LTRIM(RTRIM(@Password)) = ''
            BEGIN
                ;THROW 50012, 'Validation Failed: Password is required.', 1;
            END

            IF @Start_year IS NULL
            BEGIN
                ;THROW 50013, 'Validation Failed: Start Year is required.', 1;
            END

            IF @Department_Id IS NULL
            BEGIN
                ;THROW 50014, 'Validation Failed: Department ID is required.', 1;
            END

            IF @National_Id IS NULL OR LTRIM(RTRIM(@National_Id)) = ''
            BEGIN
                ;THROW 50015, 'Validation Failed: National ID is required.', 1;
            END

            IF @Salary IS NULL
            BEGIN
                ;THROW 50016, 'Validation Failed: Salary is required.', 1;
            END

            IF @Status_Id IS NULL
            BEGIN
                ;THROW 50017, 'Validation Failed: Status ID is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Instructors] (
                [Name], [Name_AR], [Email], [Password], [Start_year], 
                [Department_Id], [National_Id], [Salary], [Status_Id], [OfficeHours]
            )
            VALUES (
                @Name, @Name_AR, @Email, @Password, @Start_year, 
                @Department_Id, @National_Id, @Salary, @Status_Id, @OfficeHours
            );
            SET @ProcessingMessage = 'Instructor registered successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Instructors]
            SET 
                [Name] = COALESCE(@Name, [Name]),
                [Name_AR] = COALESCE(@Name_AR, [Name_AR]),
                [Email] = COALESCE(@Email, [Email]),
                [Password] = COALESCE(@Password, [Password]),
                [Start_year] = COALESCE(@Start_year, [Start_year]),
                [Department_Id] = COALESCE(@Department_Id, [Department_Id]),
                [National_Id] = COALESCE(@National_Id, [National_Id]),
                [Salary] = COALESCE(@Salary, [Salary]),
                [Status_Id] = COALESCE(@Status_Id, [Status_Id]),
                [OfficeHours] = COALESCE(@OfficeHours, [OfficeHours]) -- 🌟 ADDED UPDATE MAPPING
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50007, 'No instructor found to update.', 1;
            END
            SET @ProcessingMessage = 'Instructor updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Instructors WHERE Id = @Id)
            BEGIN
                ;THROW 50018, 'Delete Failed: Instructor ID not found.', 1;
            END
            
            DELETE FROM [Instructors] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50008, 'No instructor found to delete.', 1;
            END
            SET @ProcessingMessage = 'Instructor deleted successfully.';
        END

        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50009, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;
GO









--Modify procedure for (Courses)

CREATE PROCEDURE usp_ManageCourse
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name VARCHAR(50) = NULL,
    @Name_AR NVARCHAR(50) = NULL,
    @Code VARCHAR(5) = NULL,
    @Credit_Hours INT = NULL,
    @Difficulty INT = NULL,
    @Status_Id INT = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50009, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Course Status
            IF @Status_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [CoursesStatus] WHERE [Id] = @Status_Id)
            BEGIN
                ;THROW 50001, 'Validation Failed: The provided Course Status does not exist.', 1;
            END

            -- Validate Course Code Uniqueness
            IF @Code IS NOT NULL AND EXISTS (SELECT 1 FROM [Courses] WHERE [Code] = @Code AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50002, 'Validation Failed: This Course Code is already registered.', 1;
            END

            -- Validate Course Name Uniqueness
            IF @Name IS NOT NULL AND EXISTS (SELECT 1 FROM [Courses] WHERE [Name] = @Name AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50003, 'Validation Failed: This Course Name is already registered.', 1;
            END

            -- Validate Credit Hours
            IF @Credit_Hours IS NOT NULL AND (@Credit_Hours <= 0 OR @Credit_Hours > 4)
            BEGIN
                ;THROW 50004, 'Validation Failed: Credit Hours must be between 1 and 4.', 1;
            END

            -- Validate Difficulty Level
            IF @Difficulty IS NOT NULL AND (@Difficulty < 1 OR @Difficulty > 5)
            BEGIN
                ;THROW 50005, 'Validation Failed: Difficulty level must be between 1 and 5.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = ''
            BEGIN
                ;THROW 50006, 'Validation Failed: Course Name is required.', 1;
            END

            IF @Code IS NULL OR LTRIM(RTRIM(@Code)) = ''
            BEGIN
                ;THROW 50007, 'Validation Failed: Course Code is required.', 1;
            END

            IF @Credit_Hours IS NULL
            BEGIN
                ;THROW 50008, 'Validation Failed: Credit Hours are required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Courses] (
                [Name], [Name_AR], [Code], [Credit_Hours], [Difficulty], [Status_Id]
            )
            VALUES (
                @Name, @Name_AR, @Code, @Credit_Hours, @Difficulty, @Status_Id
            );
            SET @ProcessingMessage = 'Course created successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Courses]
            SET 
                [Name] = COALESCE(@Name, [Name]),
                [Name_AR] = COALESCE(@Name_AR, [Name_AR]),
                [Code] = COALESCE(@Code, [Code]),
                [Credit_Hours] = COALESCE(@Credit_Hours, [Credit_Hours]),
                [Difficulty] = COALESCE(@Difficulty, [Difficulty]),
                [Status_Id] = COALESCE(@Status_Id, [Status_Id])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50010, 'No course found to update.', 1;
            END
            SET @ProcessingMessage = 'Course updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Courses WHERE Id = @Id)
            BEGIN
                ;THROW 50018, 'Delete Failed: Course ID not found.', 1;
            END
            
            DELETE FROM [Courses] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50011, 'No course found to delete.', 1;
            END
            SET @ProcessingMessage = 'Course deleted successfully.';
        END

        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50012, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;







--Modiry Procedure for (Departments)

CREATE PROCEDURE usp_ManageDepartment
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name VARCHAR(100) = NULL,
    @Name_AR NVARCHAR(100) = NULL,
    @Code VARCHAR(3) = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50006, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Department Name Uniqueness
            IF @Name IS NOT NULL AND EXISTS (SELECT 1 FROM [Departments] WHERE [Name] = @Name AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50001, 'Validation Failed: This Department Name is already registered.', 1;
            END

            -- Validate Department Code Uniqueness
            IF @Code IS NOT NULL AND EXISTS (SELECT 1 FROM [Departments] WHERE [Code] = @Code AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50002, 'Validation Failed: This Department Code is already registered.', 1;
            END

            -- Validate Arabic Name Uniqueness (if provided)
            IF @Name_AR IS NOT NULL AND EXISTS (SELECT 1 FROM [Departments] WHERE [Name_AR] = @Name_AR AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50003, 'Validation Failed: This Arabic Department Name is already registered.', 1;
            END
        END

        -- Validate Required Fields on INSERT specifically
        IF @Action = 'INSERT'
        BEGIN
            IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = ''
            BEGIN
                ;THROW 50004, 'Validation Failed: Department Name is required.', 1;
            END

            IF @Code IS NULL OR LTRIM(RTRIM(@Code)) = ''
            BEGIN
                ;THROW 50005, 'Validation Failed: Department Code is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Departments] (
                [Name], [Name_AR], [Code]
            )
            VALUES (
                @Name, @Name_AR, @Code
            );
            SET @ProcessingMessage = 'Department created successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Departments]
            SET 
                [Name] = COALESCE(@Name, [Name]),
                [Name_AR] = COALESCE(@Name_AR, [Name_AR]),
                [Code] = COALESCE(@Code, [Code])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50007, 'No department found to update.', 1;
            END
            SET @ProcessingMessage = 'Department updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Departments WHERE Id = @Id)
            BEGIN
                ;THROW 50018, 'Delete Failed: Department ID not found.', 1;
            END
            
            DELETE FROM [Departments] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50008, 'No department found to delete.', 1;
            END
            SET @ProcessingMessage = 'Department deleted successfully.';
        END

        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50009, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;












--Modify Procedure for (Semester)

CREATE OR ALTER PROCEDURE usp_ManageSemester
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name VARCHAR(50) = NULL,
    @Name_AR NVARCHAR(50) = NULL,
    @Start_date DATE = NULL,
    @End_date DATE = NULL,
    @Is_Registration_Open BIT = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ADDED 'CLOSE_REG' to allowed actions
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE', 'OPEN_REG', 'CLOSE_REG')
        BEGIN
            ;THROW 50010, 'Invalid Action specified.', 1;
        END

        IF @Action IN ('UPDATE', 'DELETE', 'OPEN_REG', 'CLOSE_REG') AND @Id IS NULL
        BEGIN
            ;THROW 50007, 'Validation Failed: An ID must be provided.', 1;
        END

        -- ==========================================
        -- THE MAGIC AUTO-CLOSE RULE
        -- ==========================================
        IF (@Action IN ('INSERT', 'UPDATE') AND @Is_Registration_Open = 1) OR (@Action = 'OPEN_REG')
        BEGIN
            UPDATE [Semesters] SET [Is_Registration_Open] = 0;
        END

        -- ==========================================
        -- EXECUTE ACTIONS
        -- ==========================================
        IF @Action = 'OPEN_REG'
        BEGIN
            UPDATE [Semesters] SET [Is_Registration_Open] = 1 WHERE [Id] = @Id;
            SET @ProcessingMessage = 'Registration opened successfully.';
        END
        ELSE IF @Action = 'CLOSE_REG'
        BEGIN
            -- NEW: Closes the specific semester
            UPDATE [Semesters] SET [Is_Registration_Open] = 0 WHERE [Id] = @Id;
            SET @ProcessingMessage = 'Registration closed successfully.';
        END
        ELSE IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Semesters] ([Name], [Name_AR], [Start_date], [End_date], [Is_Registration_Open])
            VALUES (@Name, @Name_AR, @Start_date, @End_date, @Is_Registration_Open);
            SET @ProcessingMessage = 'Semester created successfully.';
        END
        ELSE IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Semesters]
            SET [Name] = COALESCE(@Name, [Name]),
                [Name_AR] = COALESCE(@Name_AR, [Name_AR]),
                [Start_date] = COALESCE(@Start_date, [Start_date]),
                [End_date] = COALESCE(@End_date, [End_date]),
                [Is_Registration_Open] = COALESCE(@Is_Registration_Open, [Is_Registration_Open])
            WHERE [Id] = @Id;
            SET @ProcessingMessage = 'Semester updated successfully.';
        END
        ELSE IF @Action = 'DELETE'
        BEGIN
            DELETE FROM [Semesters] WHERE [Id] = @Id;
            SET @ProcessingMessage = 'Semester deleted successfully.';
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;







--Modify procedure for (Enrollments)

ALTER PROCEDURE usp_ManageEnrollment
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Student_Id INT = NULL,
    @Course_Id INT = NULL,
    @Semester_Id INT = NULL, -- Added Semester parameter
    @Status_Id INT = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50020, 'Validation Failed: An ID must be provided to update or delete an enrollment.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Student exists
            IF @Student_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Students] WHERE [Id] = @Student_Id)
            BEGIN
                ;THROW 50021, 'Validation Failed: The provided Student ID does not exist.', 1;
            END

            -- Validate Course exists
            IF @Course_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = @Course_Id)
            BEGIN
                ;THROW 50022, 'Validation Failed: The provided Course ID does not exist.', 1;
            END

            -- Validate Semester exists (if provided)
            IF @Semester_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Semesters] WHERE [Id] = @Semester_Id)
            BEGIN
                ;THROW 50031, 'Validation Failed: The provided Semester ID does not exist.', 1;
            END

            -- Validate Status exists
            IF @Status_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [EnrollmentsStatus] WHERE [Id] = @Status_Id)
            BEGIN
                ;THROW 50023, 'Validation Failed: The provided Enrollment Status ID does not exist.', 1;
            END

            -- Prevent duplicate active enrollments on INSERT for the SAME semester (allows retakes in future semesters)
            IF @Action = 'INSERT' AND EXISTS (SELECT 1 FROM [Enrollments] WHERE [Student_Id] = @Student_Id AND [Course_Id] = @Course_Id AND [Semester_Id] = @Semester_Id)
            BEGIN
                ;THROW 50024, 'Validation Failed: This student is already enrolled in this course for this semester.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Student_Id IS NULL
            BEGIN
                ;THROW 50025, 'Validation Failed: Student ID is required.', 1;
            END

            IF @Course_Id IS NULL
            BEGIN
                ;THROW 50026, 'Validation Failed: Course ID is required.', 1;
            END

            IF @Semester_Id IS NULL
            BEGIN
                ;THROW 50032, 'Validation Failed: Semester ID is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            -- Default to Pending (9) if Status_Id is not provided
            DECLARE @DefaultStatus INT = ISNULL(@Status_Id, 9);

            INSERT INTO [Enrollments] (
                [Student_Id], [Course_Id], [Semester_Id], [Status_Id]
            )
            VALUES (
                @Student_Id, @Course_Id, @Semester_Id, @DefaultStatus
            );
    
            SET @ProcessingMessage = 'Enrollment request submitted successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Enrollments]
            SET 
                [Student_Id] = COALESCE(@Student_Id, [Student_Id]),
                [Course_Id] = COALESCE(@Course_Id, [Course_Id]),
                [Semester_Id] = COALESCE(@Semester_Id, [Semester_Id]),
                [Status_Id] = COALESCE(@Status_Id, [Status_Id])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50027, 'No enrollment found to update.', 1;
            END
                
            SET @ProcessingMessage = 'Enrollment updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Enrollments WHERE Id = @Id)
            BEGIN
                ;THROW 50028, 'Delete Failed: Enrollment ID not found.', 1;
            END
                
            DELETE FROM [Enrollments] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50029, 'No enrollment found to delete.', 1;
            END
                
            SET @ProcessingMessage = 'Enrollment deleted successfully.';
        END
        
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50030, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;













--Modify procedure for (InstructorCourse)

CREATE OR ALTER PROCEDURE [dbo].[usp_ManageInstructorCourse]
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Instructor_id INT = NULL,
    @Course_id INT = NULL,
    @Semester_Id INT = NULL, -- 🌟 Added Semester parameter
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50031, 'Validation Failed: An ID must be provided to update or delete an instructor course assignment.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Instructor exists
            IF @Instructor_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = @Instructor_id)
            BEGIN
                ;THROW 50032, 'Validation Failed: The provided Instructor ID does not exist.', 1;
            END

            -- Validate Course exists
            IF @Course_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = @Course_id)
            BEGIN
                ;THROW 50033, 'Validation Failed: The provided Course ID does not exist.', 1;
            END

            -- Validate Semester exists
            IF @Semester_Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Semesters] WHERE [Id] = @Semester_Id)
            BEGIN
                ;THROW 50041, 'Validation Failed: The provided Semester ID does not exist.', 1;
            END

            -- Prevent duplicate assignments on INSERT for the *same* semester
            IF @Action = 'INSERT' AND EXISTS (SELECT 1 FROM [Instructors_Courses] WHERE [Instructor_id] = @Instructor_id AND [Course_id] = @Course_id AND [Semester_Id] = @Semester_Id)
            BEGIN
                ;THROW 50034, 'Validation Failed: This instructor is already assigned to this course for this semester.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Instructor_id IS NULL
            BEGIN
                ;THROW 50035, 'Validation Failed: Instructor ID is required.', 1;
            END

            IF @Course_id IS NULL
            BEGIN
                ;THROW 50036, 'Validation Failed: Course ID is required.', 1;
            END

            IF @Semester_Id IS NULL
            BEGIN
                ;THROW 50042, 'Validation Failed: Semester ID is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Instructors_Courses] (
                [Instructor_id], [Course_id], [Semester_Id]
            )
            VALUES (
                @Instructor_id, @Course_id, @Semester_Id
            );
    
            SET @ProcessingMessage = 'Instructor course assignment created successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Instructors_Courses]
            SET 
                [Instructor_id] = COALESCE(@Instructor_id, [Instructor_id]),
                [Course_id] = COALESCE(@Course_id, [Course_id]),
                [Semester_Id] = COALESCE(@Semester_Id, [Semester_Id])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50037, 'No instructor course assignment found to update.', 1;
            END
                
            SET @ProcessingMessage = 'Instructor course assignment updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Instructors_Courses WHERE Id = @Id)
            BEGIN
                ;THROW 50038, 'Delete Failed: Instructor course assignment ID not found.', 1;
            END
                
            DELETE FROM [Instructors_Courses] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50039, 'No instructor course assignment found to delete.', 1;
            END
                
            SET @ProcessingMessage = 'Instructor course assignment deleted successfully.';
        END
        
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50040, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;












--Modify Procedure for (Attendacne) 


CREATE PROCEDURE usp_ManageAttendance
    @Action VARCHAR(20),
    @Id INT = NULL,
    @Course_Schedules_Id INT = NULL,
    @AttendanceDate DATE = NULL,
    @Student_Id INT = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for single record Deletes
        IF @Action = 'DELETE' AND @Id IS NULL
        BEGIN
            ;THROW 50040, 'Validation Failed: An ID must be provided to delete a specific attendance record.', 1;
        END

        -- Validate required fields for CLEAR_DAY
        IF @Action = 'CLEAR_DAY' AND (@Course_Schedules_Id IS NULL OR @AttendanceDate IS NULL)
        BEGIN
            ;THROW 50041, 'Validation Failed: Course Schedule ID and Attendance Date are required to clear a day.', 1;
        END

        IF @Action = 'INSERT'
        BEGIN
            -- Validate Required Fields on INSERT
            IF @Course_Schedules_Id IS NULL OR @AttendanceDate IS NULL OR @Student_Id IS NULL
            BEGIN
                ;THROW 50042, 'Validation Failed: Course Schedule ID, Attendance Date, and Student ID are all required.', 1;
            END

            -- Validate Student exists
            IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [Id] = @Student_Id)
            BEGIN
                ;THROW 50043, 'Validation Failed: The provided Student ID does not exist.', 1;
            END

            -- Validate Course Schedule exists
            IF NOT EXISTS (SELECT 1 FROM [Course_Schedules] WHERE [Id] = @Course_Schedules_Id)
            BEGIN
                ;THROW 50044, 'Validation Failed: The provided Course Schedule ID does not exist.', 1;
            END

            -- Prevent duplicate attendance records
            IF EXISTS (SELECT 1 FROM [Attendance] WHERE [Course_Schedules_Id] = @Course_Schedules_Id AND [AttendanceDate] = @AttendanceDate AND [Student_Id] = @Student_Id)
            BEGIN
                ;THROW 50045, 'Validation Failed: This student is already marked present for this specific date and schedule.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Attendance] (
                [Course_Schedules_Id], [AttendanceDate], [Student_Id]
            )
            VALUES (
                @Course_Schedules_Id, @AttendanceDate, @Student_Id
            );
    
            SET @ProcessingMessage = 'Student marked present successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [Attendance] WHERE [Id] = @Id)
            BEGIN
                ;THROW 50046, 'Delete Failed: Attendance record ID not found.', 1;
            END
                
            DELETE FROM [Attendance] WHERE [Id] = @Id;

            SET @ProcessingMessage = 'Attendance record removed (student marked absent).';
        END

        IF @Action = 'CLEAR_DAY'
        BEGIN
            -- Bulk delete for saving a fresh attendance roster
            DELETE FROM [Attendance] 
            WHERE [Course_Schedules_Id] = @Course_Schedules_Id 
              AND [AttendanceDate] = @AttendanceDate;
            
            SET @ProcessingMessage = 'Cleared all attendance records for the specified day.';
        END
        
        IF @Action NOT IN ('INSERT', 'DELETE', 'CLEAR_DAY')
        BEGIN
            ;THROW 50047, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;
GO






--Modify proedure for (Course_Semster) 

CREATE OR ALTER PROCEDURE usp_ManageSemesterCourses
    @Action VARCHAR(20),
    @Id INT = NULL,              
    @Semester_Id INT = NULL,
    @Course_Id INT = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Valid Actions
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50010, 'Invalid Action specified.', 1;
        END

        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50007, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        -- Guard Clause: Prevent duplicates on INSERT or UPDATE
        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            IF EXISTS (
                SELECT 1 FROM [Courses_Semesters] 
                WHERE [Semester_Id] = COALESCE(@Semester_Id, [Semester_Id]) 
                  AND [Course_Id] = COALESCE(@Course_Id, [Course_Id])
                  AND (@Id IS NULL OR [Id] <> @Id)
            )
            BEGIN
                ;THROW 50001, 'Validation Failed: This course is already added to this semester.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            IF @Semester_Id IS NULL OR @Course_Id IS NULL
            BEGIN
                ;THROW 50035, 'Validation Failed: Both Semester ID and Course ID are required.', 1;
            END

            INSERT INTO [Courses_Semesters] ([Semester_Id], [Course_Id])
            VALUES (@Semester_Id, @Course_Id);
            
            SET @ProcessingMessage = 'Course successfully added to the semester.';
        END

        ELSE IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Courses_Semesters]
            SET 
                [Semester_Id] = COALESCE(@Semester_Id, [Semester_Id]),
                [Course_Id] = COALESCE(@Course_Id, [Course_Id])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50037, 'No semester course assignment found to update.', 1;
            END
                
            SET @ProcessingMessage = 'Semester course assignment updated successfully.';
        END

        ELSE IF @Action = 'DELETE'
        BEGIN
            DELETE FROM [Courses_Semesters] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50039, 'No semester course assignment found to delete.', 1;
            END
                
            SET @ProcessingMessage = 'Course removed from the semester.';
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;







--Modify Procedure for (Admins) 

CREATE OR ALTER PROCEDURE usp_ManageAdmins
    @Action VARCHAR(10),
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @Password VARCHAR(255) = NULL,
    @National_id VARCHAR(20) = NULL,
    @Start_year INT = NULL,
    @Salary FLOAT = NULL,
    @Status_id INT = NULL,
    @Role NVARCHAR(50) = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF @Action IN ('UPDATE', 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50013, 'Validation Failed: An ID must be provided to update or delete a record.', 1;
        END

        IF @Action IN ('INSERT', 'UPDATE')
        BEGIN
            -- Validate Admin Status
            IF @Status_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [AdminsStatus] WHERE [Id] = @Status_id)
            BEGIN
                ;THROW 50001, 'Validation Failed: The provided Admin Status does not exist.', 1;
            END

            -- Validate Email Uniqueness
            IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM [Admins] WHERE [Email] = @Email AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50003, 'Validation Failed: This Email is already registered to another admin.', 1;
            END
            
            -- Validate Email Format
            IF @Email IS NOT NULL AND @Email NOT LIKE '%_@__%.__%'
            BEGIN
                ;THROW 50004, 'Validation Failed: The provided Email format is invalid.', 1;
            END

            -- Validate National ID Uniqueness
            IF @National_id IS NOT NULL AND EXISTS (SELECT 1 FROM [Admins] WHERE [National_id] = @National_id AND (@Id IS NULL OR [Id] <> @Id))
            BEGIN
                ;THROW 50005, 'Validation Failed: This National ID is already registered.', 1;
            END

            -- Validate Salary
            IF @Salary IS NOT NULL AND @Salary <= 0
            BEGIN
                ;THROW 50006, 'Validation Failed: Salary must be greater than zero.', 1;
            END
        END

        -- Validate Required Fields on INSERT
        IF @Action = 'INSERT'
        BEGIN
            IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = ''
            BEGIN
                ;THROW 50010, 'Validation Failed: Admin Name is required.', 1;
            END

            IF @Email IS NULL OR LTRIM(RTRIM(@Email)) = ''
            BEGIN
                ;THROW 50011, 'Validation Failed: Email is required.', 1;
            END

            IF @Password IS NULL OR LTRIM(RTRIM(@Password)) = ''
            BEGIN
                ;THROW 50012, 'Validation Failed: Password is required.', 1;
            END

            IF @Start_year IS NULL
            BEGIN
                ;THROW 50013, 'Validation Failed: Start Year is required.', 1;
            END

            IF @National_id IS NULL OR LTRIM(RTRIM(@National_id)) = ''
            BEGIN
                ;THROW 50015, 'Validation Failed: National ID is required.', 1;
            END

            IF @Salary IS NULL
            BEGIN
                ;THROW 50016, 'Validation Failed: Salary is required.', 1;
            END

            IF @Status_id IS NULL
            BEGIN
                ;THROW 50017, 'Validation Failed: Status ID is required.', 1;
            END

            IF @Role IS NULL OR LTRIM(RTRIM(@Role)) = ''
            BEGIN
                ;THROW 50019, 'Validation Failed: Role is required.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Admins] (
                [Name], [Email], [Password], [National_id], [Start_year], 
                [Salary], [Status_id], [Role]
            )
            VALUES (
                @Name, @Email, @Password, @National_id, @Start_year, 
                @Salary, @Status_id, @Role
            );
            SET @ProcessingMessage = 'Admin registered successfully.';
        END

        ELSE IF @Action = 'UPDATE'
        BEGIN
            UPDATE [Admins]
            SET 
                [Name] = COALESCE(@Name, [Name]),
                [Email] = COALESCE(@Email, [Email]),
                [Password] = COALESCE(@Password, [Password]),
                [Start_year] = COALESCE(@Start_year, [Start_year]),
                [National_id] = COALESCE(@National_id, [National_id]),
                [Salary] = COALESCE(@Salary, [Salary]),
                [Status_id] = COALESCE(@Status_id, [Status_id]),
                [Role] = COALESCE(@Role, [Role])
            WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50007, 'No admin found to update.', 1;
            END
            SET @ProcessingMessage = 'Admin updated successfully.';
        END

        ELSE IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Admins WHERE Id = @Id)
            BEGIN
                ;THROW 50018, 'Delete Failed: Admin ID not found.', 1;
            END
            
            DELETE FROM [Admins] WHERE [Id] = @Id;

            IF @@ROWCOUNT = 0
            BEGIN
                ;THROW 50008, 'No admin found to delete.', 1;
            END
            SET @ProcessingMessage = 'Admin deleted successfully.';
        END
        ELSE
        BEGIN
            ;THROW 50009, 'Invalid Action specified.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;















--Modify Procedure For (Lecture_Event) 


CREATE PROCEDURE usp_ManageLectureEvent
    @Action VARCHAR(20),
    @Id INT = NULL,
    @Schedule_Id INT = NULL,
    @Date_Of_Event DATE = NULL,
    @Content_Covered NVARCHAR(255) = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Require ID for Updates and Deletes
        IF (@Action = 'UPDATE' OR @Action = 'DELETE') AND @Id IS NULL
        BEGIN
            ;THROW 50050, 'Validation Failed: An ID must be provided to update or delete a lecture event.', 1;
        END

        -- Validate required fields for INSERT or UPDATE
        IF @Action = 'INSERT' OR @Action = 'UPDATE'
        BEGIN
            IF @Schedule_Id IS NULL OR @Date_Of_Event IS NULL
            BEGIN
                ;THROW 50051, 'Validation Failed: Schedule ID and Date of Event are required fields.', 1;
            END

            -- Validate Course Schedule exists
            IF NOT EXISTS (SELECT 1 FROM [Course_Schedules] WHERE [Id] = @Schedule_Id)
            BEGIN
                ;THROW 50052, 'Validation Failed: The provided Course Schedule ID does not exist.', 1;
            END

            -- Prevent duplicate lecture events on the exact same schedule and date
            IF @Action = 'INSERT' AND EXISTS (SELECT 1 FROM [Lecture_Events] WHERE [Schedule_Id] = @Schedule_Id AND [Date_Of_Event] = @Date_Of_Event)
            BEGIN
                ;THROW 50053, 'Validation Failed: A lecture event for this schedule already exists on this date.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'INSERT'
        BEGIN
            INSERT INTO [Lecture_Events] (
                [Schedule_Id], [Date_Of_Event], [Content_Covered]
            )
            VALUES (
                @Schedule_Id, @Date_Of_Event, @Content_Covered
            );
    
            SET @ProcessingMessage = 'Lecture event created successfully.';
        END

        IF @Action = 'UPDATE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [Lecture_Events] WHERE [Id] = @Id)
            BEGIN
                ;THROW 50054, 'Update Failed: Lecture event ID not found.', 1;
            END

            UPDATE [Lecture_Events]
            SET [Schedule_Id] = @Schedule_Id,
                [Date_Of_Event] = @Date_Of_Event,
                [Content_Covered] = @Content_Covered
            WHERE [Id] = @Id;

            SET @ProcessingMessage = 'Lecture event updated successfully.';
        END

        IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [Lecture_Events] WHERE [Id] = @Id)
            BEGIN
                ;THROW 50055, 'Delete Failed: Lecture event ID not found.', 1;
            END
                
            DELETE FROM [Lecture_Events] WHERE [Id] = @Id;

            SET @ProcessingMessage = 'Lecture event deleted successfully.';
        END
        
        IF @Action NOT IN ('INSERT', 'UPDATE', 'DELETE')
        BEGIN
            ;THROW 50056, 'Invalid Action specified for Lecture Event.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;
GO





CREATE PROCEDURE usp_ManageGrade
    @Action VARCHAR(10),                 -- 'UPSERT' or 'DELETE'
    @Enrollment_Id INT = NULL,           -- Links directly to Enrollments table
    @Midterm_Grade DECIMAL(5,2) = NULL,
    @CourseWork_Grade DECIMAL(5,2) = NULL,
    @Final_Grade DECIMAL(5,2) = NULL,
    @ProcessingStatus INT OUTPUT,
    @ProcessingMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET XACT_ABORT ON;
    SET @ProcessingStatus = 1;
    SET @ProcessingMessage = 'Operation completed successfully.';

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. VALIDATIONS
        -- ==========================================
        
        -- Guard Clause: Enrollment ID is required for all actions
        IF @Enrollment_Id IS NULL OR @Enrollment_Id <= 0
        BEGIN
            ;THROW 50020, 'Validation Failed: A valid Enrollment ID must be provided.', 1;
        END

        -- Validate Enrollment exists in the database
        IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = @Enrollment_Id)
        BEGIN
            ;THROW 50021, 'Validation Failed: The specified Enrollment ID does not exist.', 1;
        END

        IF @Action IN ('UPSERT', 'INSERT', 'UPDATE')
        BEGIN
            -- Validate Midterm Grade range (e.g., 0.00 to 30.00 or whatever your max is)
            IF @Midterm_Grade IS NOT NULL AND (@Midterm_Grade < 0.00)
            BEGIN
                ;THROW 50022, 'Validation Failed: Midterm grade cannot be negative.', 1;
            END

            -- Validate CourseWork Grade range
            IF @CourseWork_Grade IS NOT NULL AND (@CourseWork_Grade < 0.00)
            BEGIN
                ;THROW 50023, 'Validation Failed: Coursework grade cannot be negative.', 1;
            END

            -- Validate Final Grade range
            IF @Final_Grade IS NOT NULL AND (@Final_Grade < 0.00)
            BEGIN
                ;THROW 50024, 'Validation Failed: Final grade cannot be negative.', 1;
            END
        END

        -- ==========================================
        -- 2. EXECUTE ACTIONS
        -- ==========================================
        
        IF @Action = 'UPSERT' OR @Action = 'INSERT' OR @Action = 'UPDATE'
        BEGIN
            IF EXISTS (SELECT 1 FROM [Grades] WHERE [Enrollment_Id] = @Enrollment_Id)
            BEGIN
                -- UPDATE existing grade record
                UPDATE [Grades]
                SET 
                    [Midterm_Grade] = @Midterm_Grade,
                    [CourseWork_Grade] = @CourseWork_Grade,
                    [Final_Grade] = @Final_Grade
                WHERE [Enrollment_Id] = @Enrollment_Id;

                SET @ProcessingMessage = 'Grades updated successfully.';
            END
            ELSE
            BEGIN
                -- INSERT new grade record
                INSERT INTO [Grades] (
                    [Enrollment_Id], [Midterm_Grade], [CourseWork_Grade], [Final_Grade]
                )
                VALUES (
                    @Enrollment_Id, @Midterm_Grade, @CourseWork_Grade, @Final_Grade
                );

                SET @ProcessingMessage = 'Grades recorded successfully.';
            END
        END
        ELSE IF @Action = 'DELETE'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM [Grades] WHERE [Enrollment_Id] = @Enrollment_Id)
            BEGIN
                ;THROW 50025, 'Delete Failed: No grade record found for this enrollment.', 1;
            END
                
            DELETE FROM [Grades] WHERE [Enrollment_Id] = @Enrollment_Id;

            SET @ProcessingMessage = 'Grade record deleted successfully.';
        END
        ELSE
        BEGIN
            ;THROW 50026, 'Invalid Action specified. Use UPSERT or DELETE.', 1;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ProcessingStatus = 0;
        SET @ProcessingMessage = ERROR_MESSAGE(); 
    END CATCH
END;