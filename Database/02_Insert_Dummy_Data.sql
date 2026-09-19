-- ==========================================
-- 1. LOOKUP & STATUS TABLES (Fewer rows are natural here, but populated fully)
-- ==========================================

INSERT INTO [Departments] ([Name], [Name_AR], [Code]) VALUES
('Communication Engineering Program', N'برنامج هندسة الاتصالات', 'CCE_E'),
('Computer Engineering Program', N'برنامج هندسة الحاسب', 'CCE_C'),
('Biomedical And Healthcare Data Engineering', N'هندسة بيانات الرعاية الصحية والطب الحيوي', 'BDE'),
('Architectural Engineering and Technology Program', N'برنامج الهندسة المعمارية وتكنولوجيا البناء', 'AET'),
('Aeronautical Engineering and Aviation Management', N'هندسة الطيران وإدارة الملاحة الجوية', 'AEM'),
('Construction Engineering and Management Program', N'برنامج هندسة وإدارة الإنشاءات', 'CEM'),
('Electrical Energy Engineering Program', N'برنامج هندسة الطاقة الكهرببية', 'EEE'),
('Mechanical Design Engineering Program', N'برنامج هندسة التصميم الميكانيكي', 'MDE'),
('Mechatronics Engineering Program', N'برنامج هندسة الميكاترونكس', 'MEE'),
('Sustainable Energy Engineering Program', N'برنامج هندسة الطاقة المستدامة', 'SEE'),
('Chemical Engineering Program', N'برنامج الهندسة الكيميائية', 'CHE');




INSERT INTO [StudentsStatus] ([Description], [Description_AR]) VALUES
('Active', N'منتظم'),
('Suspended', N'موقوف'),
('Graduated', N'خريج'),
('Transferred', N'محول'),
('Dismissed', N'مفصول'),
('On Leave', N'إجازة دراسية'),
('Pending', N'قيد الانتظار'),
('Deferred', N'مؤجل'),
('Expelled', N'مفصول نهائياً');




INSERT INTO [InstructorsStatus] ([Description], [Description_AR]) VALUES
('Active', N'على رأس العمل'),
('Sabbatical', N'إجازة تفرغ علمي'),
('Part-Time', N'دوام جزئي'),
('Resigned', N'مستقيل'),
('Retired', N'متعاقد/متقاعد'),
('On Leave', N'إجازة'),
('Pending', N'قيد الانتظار');




INSERT INTO [CoursesStatus] ([Description], [Description_AR]) VALUES
('Available', N'متاح'),
('Closed', N'مغلق'),
('Under Revision', N'قيد المراجعة'),
('Cancelled', N'ملغي'),
('Archived', N'مؤرشف');




INSERT INTO [EnrollmentsStatus] ([Description], [Description_AR]) VALUES
('Enrolled', N'مسجل'),
('Pending', N'قيد المراجعة'),
('Dropped', N'منسحب'),
('Passed', N'ناجح'),
('Failed', N'راسب');



INSERT INTO [AdminsStatus] ([Description], [Description_AR]) VALUES
('Active', N'نشط'),
('Inactive', N'غير نشط'),
('Suspended', N'موقوف'),
('Pending', N'قيد الانتظار');



-- ==========================================
-- 2. SEMESTERS
-- ==========================================

INSERT INTO [Semesters] ([Name], [Name_AR], [Start_date], [End_date], [Is_Registration_Open]) VALUES
('Fall 2025', N'خريف 2025', '2025-09-01', '2026-01-15', 0),
('Spring 2026', N'ربيع 2026', '2026-02-01', '2026-06-15', 0),
('Fall 2026', N'خريف 2026', '2026-09-01', '2027-01-15', 1);


-- Insert historical semesters before Fall 2025
INSERT INTO [Semesters] ([Name], [Name_AR], [Start_date], [End_date], [Is_Registration_Open])
VALUES 
    ('Fall 2024', N'خريف 2024', '2024-09-01', '2025-01-15', 0),
    ('Spring 2025', N'ربيع 2025', '2025-02-01', '2025-06-15', 0);


-- ==========================================
-- 3. COURSES (20 Rows)
-- ==========================================

INSERT INTO [Courses] ([Name], [Name_AR], [Code], [Credit_Hours], [Difficulty], [Status_Id]) VALUES
-- 1. Communication Engineering Program (CCE_E)
('Analog Communications', N'اتصالات تناظرية', 'CCE301', 3, 3, 1),
('Digital Communications', N'اتصالات رقمية', 'CCE302', 3, 4, 1),
('Electromagnetic Fields', N'المجالات الكهرومغناطيسية', 'CCE201', 3, 4, 1),
('Signal Processing', N'معالجة الإشارات', 'CCE303', 3, 3, 1),
('Wireless Networks', N'الشبكات اللاسلكية', 'CCE401', 3, 4, 1),

-- 2. Computer Engineering Program (CCE_C)
('Digital Logic Design', N'تصميم المنطق الرقمي', 'CMP201', 3, 3, 1),
('Microprocessors', N'المعالجات الدقيقة', 'CMP301', 3, 4, 1),
('Computer Architecture', N'هندسة الحاسب', 'CMP302', 3, 4, 1),
('Embedded Systems', N'الأنظمة المدمجة', 'CMP401', 3, 5, 1),
('VLSI Design', N'تصميم الدوائر المتكاملة', 'CMP402', 3, 5, 1),

-- 3. Biomedical And Healthcare Data Engineering (BDE)
('Biomedical Instrumentation', N'الأجهزة الطبية الحيوية', 'BDE301', 3, 3, 1),
('Medical Image Processing', N'معالجة الصور الطبية', 'BDE302', 3, 4, 1),
('Healthcare Information Systems', N'أنظمة معلومات الرعاية الصحية', 'BDE303', 3, 3, 1),
('Biomechanics', N'الميكانيكا الحيوية', 'BDE201', 3, 3, 1),
('Bioinformatics', N'المعلوماتية الحيوية', 'BDE401', 3, 4, 1),

-- 4. Architectural Engineering and Technology Program (AET)
('Architectural Design I', N'التصميم المعماري 1', 'AET201', 4, 4, 1),
('History of Architecture', N'تاريخ العمارة', 'AET101', 2, 2, 1),
('Building Construction', N'إنشاء المباني', 'AET202', 3, 3, 1),
('Environmental Control Systems', N'أنظمة التحكم البيئي', 'AET301', 3, 3, 1),
('Computer-Aided Architectural Design', N'التصميم المعماري بالحاسب', 'AET302', 3, 3, 1),

-- 5. Aeronautical Engineering and Aviation Management (AEM)
('Aerodynamics', N'ديناميكا الهواء', 'AEM301', 3, 4, 1),
('Aircraft Structures', N'هياكل الطائرات', 'AEM302', 3, 4, 1),
('Flight Mechanics', N'ميكانيكا الطيران', 'AEM303', 3, 4, 1),
('Aviation Safety Management', N'إدارة سلامة الطيران', 'AEM401', 2, 2, 1),
('Propulsion Systems', N'أنظمة الدفع', 'AEM402', 3, 4, 1),

-- 6. Construction Engineering and Management Program (CEM)
('Construction Project Management', N'إدارة مشاريع الإنشاءات', 'CEM401', 3, 3, 1),
('Construction Methods', N'طرق الإنشاءات', 'CEM301', 3, 3, 1),
('Engineering Economics', N'اقتصاديات الهندسة', 'CEM201', 2, 2, 1),
('Contracts and Specifications', N'العقود والمواصفات', 'CEM302', 2, 2, 1),
('Structural Analysis for Construction', N'التحليل الإنشائي للتشييد', 'CEM303', 3, 4, 1),

-- 7. Electrical Energy Engineering Program (EEE)
('Electrical Power Systems', N'أنظمة القوى الكهربائية', 'EEE301', 3, 4, 1),
('Electrical Machines', N'الآلات الكهربائية', 'EEE302', 3, 4, 1),
('High Voltage Engineering', N'هندسة الجهد العالي', 'EEE401', 3, 5, 1),
('Power Electronics', N'إلكترونيات القوى', 'EEE303', 3, 4, 1),
('Electrical Protection', N'الحماية الكهربائية', 'EEE402', 3, 4, 1),

-- 8. Mechanical Design Engineering Program (MDE)
('Machine Design', N'تصميم الآلات', 'MDE301', 3, 4, 1),
('Theory of Machines', N'نظرية الآلات', 'MDE201', 3, 3, 1),
('CAD/CAM Systems', N'أنظمة التصميم والتصنيع بالحاسب', 'MDE401', 3, 3, 1),
('Mechanical Vibrations', N'الاهتزازات الميكانيكية', 'MDE302', 3, 4, 1),
('Stress Analysis', N'تحليل الإجهادات', 'MDE303', 3, 4, 1),

-- 9. Mechatronics Engineering Program (MEE)
('Mechatronics System Design', N'تصميم أنظمة الميكاترونكس', 'MEE401', 3, 4, 1),
('Robotics Engineering', N'هندسة الروبوتات', 'MEE402', 3, 4, 1),
('Sensors and Actuators', N'المستشعرات ومشغلات الحركة', 'MEE301', 3, 3, 1),
('Control Systems', N'أنظمة التحكم', 'MEE302', 3, 4, 1),
('Industrial Automation', N'الأتمتة الصناعية', 'MEE403', 3, 4, 1),

-- 10. Sustainable Energy Engineering Program (SEE)
('Renewable Energy Systems', N'أنظمة الطاقة المتجددة', 'SEE301', 3, 3, 1),
('Solar Energy Engineering', N'هندسة الطاقة الشمسية', 'SEE302', 3, 4, 1),
('Wind Energy Engineering', N'هندسة طاقة الرياح', 'SEE303', 3, 4, 1),
('Energy Storage Systems', N'أنظمة تخزين الطاقة', 'SEE401', 3, 4, 1),
('Energy Efficiency and Audit', N'كفاءة وتدقيق الطاقة', 'SEE402', 2, 3, 1),

-- 11. Chemical Engineering Program (CHE)
('Chemical Engineering Thermodynamics', N'الديناميكا الحرارية الهندسية', 'CHE201', 3, 3, 1),
('Fluid Mechanics', N'ميكانيكا الموائع', 'CHE202', 3, 4, 1),
('Heat and Mass Transfer', N'انتقال الحرارة والكتلة', 'CHE301', 3, 4, 1),
('Chemical Reaction Engineering', N'هندسة التفاعلات الكيميائية', 'CHE401', 3, 5, 1),
('Process Control and Design', N'تصميم والتحكم في العمليات', 'CHE302', 3, 3, 1);




-- ==========================================
-- 4. STUDENTS (20 Rows)
-- ==========================================

INSERT INTO [Students] ([Name], [Name_AR], [Email], [Password], [Gpa], [Year], [National_Id], [Department_Id], [Status_Id]) VALUES
('Youssef Ahmed', N'يوسف أحمد', 'youssef@univ.edu', 'hashed_pass', 3.45, 2, '30401010100111', 1, 1),
('Salma Khaled', N'سلمى خالد', 'salma@univ.edu', 'hashed_pass', 3.89, 3, '30302020200222', 2, 1),
('Karim Mahmoud', N'كريم محمود', 'karim@univ.edu', 'hashed_pass', 2.95, 2, '30503030300333', 3, 1),
('Nour El-Din', N'نور الدين', 'nour@univ.edu', 'hashed_pass', 3.60, 4, '30204040400444', 4, 1),
('Farah Hassan', N'فرح حسن', 'farah@univ.edu', 'hashed_pass', 3.20, 1, '30605050500555', 5, 1),
('Ziad Mohamed', N'زياد محمد', 'ziad@univ.edu', 'hashed_pass', 2.75, 2, '30406060600666', 6, 1),
('Hana Ali', N'هنا علي', 'hana@univ.edu', 'hashed_pass', 3.92, 4, '30207070700777', 7, 1),
('Nourhan Adel', N'نورهان عادل', 'nourhan@univ.edu', 'hashed_pass', 3.70, 3, '30321212102121', 8, 1),
('Mona Tarek', N'منى طارق', 'mona@univ.edu', 'hashed_pass', 3.40, 2, '30409090900999', 9, 1),
('Ali Ibrahim', N'علي إبراهيم', 'ali.ibrahim@univ.edu', 'hashed_pass', 2.65, 1, '30610101001010', 10, 1),
('Habiba Yasser', N'حبيبة ياسر', 'habiba@univ.edu', 'hashed_pass', 3.75, 3, '30311111101111', 11, 1),
('Tarek Zaki', N'طارق زكي', 'tarek@univ.edu', 'hashed_pass', 2.80, 4, '30212121201212', 1, 1),
('Mariam Nader', N'مريم نادر', 'mariam@univ.edu', 'hashed_pass', 3.55, 2, '30413131301313', 2, 1),
('Ahmed Samir', N'أحمد سمير', 'ahmed.samir@univ.edu', 'hashed_pass', 3.15, 1, '30614141401414', 3, 1),
('Laila Mostafa', N'ليلى مصطفى', 'laila@univ.edu', 'hashed_pass', 3.90, 4, '30215151501515', 4, 1),
('Youssef Nabil', N'يوسف نبيل', 'y.nabil@univ.edu', 'hashed_pass', 2.90, 3, '30316161601616', 5, 1),
('Dina Adel', N'دينا عادل', 'dina@univ.edu', 'hashed_pass', 3.65, 2, '30417171701717', 6, 1),
('Khaled Gamal', N'خالد جمال', 'khaled@univ.edu', 'hashed_pass', 2.50, 1, '30618181801818', 7, 1),
('Yasmin Fathy', N'ياسمين فتحي', 'yasmin@univ.edu', 'hashed_pass', 1.35, 3, '30319191901919', 8, 1),
('Ibrahim Saad', N'إبراهيم سعد', 'ibrahim@univ.edu', 'hashed_pass', 3.05, 2, '30420202002020', 9, 1);


-- ==========================================
-- 5. INSTRUCTORS (20 Rows)
-- ==========================================

INSERT INTO [Instructors] ([Name], [Name_AR], [Email], [Password], [Start_year], [Department_Id], [National_Id], [Salary], [Status_Id]) VALUES
('Dr. Ahmed Hassan', N'د. أحمد حسن', 'ahmed.hassan@univ.edu', 'hashed_pass', 2012, 1, '27201010100111', 15000.0, 1),
('Dr. Mona Ali', N'د. منى علي', 'mona.ali@univ.edu', 'hashed_pass', 2015, 2, '27502020200222', 14000.0, 1),
('Dr. Tarek Mahmoud', N'د. طارق محمود', 'tarek.mahmoud@univ.edu', 'hashed_pass', 2010, 3, '27003030300333', 16500.0, 1),
('Dr. Hoda Youssef', N'د. هدى يوسف', 'hoda.youssef@univ.edu', 'hashed_pass', 2018, 4, '27804040400444', 12500.0, 1),
('Dr. Omar Farouk', N'د. عمر فاروق', 'omar.farouk@univ.edu', 'hashed_pass', 2014, 5, '27405050500555', 13500.0, 1),
('Dr. Sherif Labib', N'د. شريف لبيب', 'sherif.labib@univ.edu', 'hashed_pass', 2008, 6, '26806060600666', 18000.0, 1),
('Dr. Rania Shawky', N'د. رانيا شوقي', 'rania.shawky@univ.edu', 'hashed_pass', 2016, 7, '27607070700777', 13000.0, 1),
('Dr. Essam Eldin', N'د. عصام الدين', 'essam.eldin@univ.edu', 'hashed_pass', 2011, 8, '27108080800888', 16000.0, 1),
('Dr. Neveen Kamel', N'د. نيفين كامل', 'neveen.kamel@univ.edu', 'hashed_pass', 2017, 9, '27709090900999', 12800.0, 1),
('Dr. Khaled Said', N'د. خالد سعيد', 'khaled.said@univ.edu', 'hashed_pass', 2013, 10, '27310101001010', 14500.0, 1),
('Dr. Amira Salah', N'د. أميرة صلاح', 'amira.salah@univ.edu', 'hashed_pass', 2019, 11, '27911111101111', 12000.0, 1),
('Dr. Mostafa Kamel', N'د. مصطفى كامل', 'mostafa.kamel@univ.edu', 'hashed_pass', 2009, 1, '26912121201212', 17500.0, 1),
('Dr. Salwa Badawy', N'د. سلوى بدوي', 'salwa.badawy@univ.edu', 'hashed_pass', 2015, 2, '27513131301313', 13800.0, 1),
('Dr. Waleed Fouad', N'د. وليد فؤاد', 'waleed.fouad@univ.edu', 'hashed_pass', 2012, 3, '27214141401414', 15200.0, 1),
('Dr. Naglaa Fathy', N'د. نجلاء فتحي', 'naglaa.fathy@univ.edu', 'hashed_pass', 2016, 4, '27615151501515', 13100.0, 1),
('Dr. Ashraf Zidan', N'د. أشرف زيدان', 'ashraf.zidan@univ.edu', 'hashed_pass', 2007, 5, '26716161601616', 19000.0, 1),
('Dr. Reham Hosny', N'د. ريهام حسني', 'reham.hosny@univ.edu', 'hashed_pass', 2018, 6, '27817171701717', 12200.0, 1),
('Dr. Fathy Mounir', N'د. فتحي منير', 'fathy.mounir@univ.edu', 'hashed_pass', 2014, 7, '27418181801818', 14200.0, 1),
('Dr. Ghada Adel', N'د. غادة عادل', 'ghada.adel@univ.edu', 'hashed_pass', 2020, 8, '28019191901919', 11500.0, 1),
('Dr. Zaki Osman', N'د. زكي عثمان', 'zaki.osman@univ.edu', 'hashed_pass', 2010, 9, '27020202002020', 16800.0, 1);


-- ==========================================
-- 6. ADMINS (5 Rows)
-- ==========================================

INSERT INTO [Admins] ([Name], [Name_AR], [Email], [Password], [National_Id], [Start_year], [Salary], [Status_Id], [Role]) VALUES
('Dr. Mostafa El-Sayed', N'د. مصطفى السيد', 'mostafa.admin@univ.edu', 'hashed_pass', '29001010100111', 2018, 12000.0, 1, 'SuperAdmin'),
('Eng. Tarek Nour', N'م. طارق نور', 'tarek.reg@univ.edu', 'hashed_pass', '29202020200222', 2019, 9500.0, 1, 'Registrar'),
('Mahmoud Reda', N'محمود رضا', 'mahmoud.it@univ.edu', 'hashed_pass', '29503030300333', 2021, 8000.0, 1, 'ITSupport'),
('Sherine Abdelrahman', N'شيرين عبد الرحمن', 'sherine.fin@univ.edu', 'hashed_pass', '28804040400444', 2015, 10500.0, 1, 'Accountant'),
('Amal Wagih', N'أمل وجيه', 'amal.hr@univ.edu', 'hashed_pass', '28505050500555', 2016, 11000.0, 1, 'HRManager'),
('Prof. Dr. Adel Sadek', N'أ.د. عادل صادق', 'adel.dean@univ.edu', 'hashed_pass', '27006060600666', 2012, 18000.0, 1, 'Faculty Dean'),
('Omar Essam', N'عمر عصام', 'omar.essam@univ.edu', 'hashed_pass', '29907070700777', 2024, 15000.0, 1, 'Lead Developer');


-- ==========================================
-- 7. INSTRUCTORS_COURSES (20 Rows linking Instructors to Courses)
-- ==========================================

INSERT INTO [Instructors_Courses] ([Instructor_Id], [Course_Id], [Semester_Id]) VALUES
-- Department 1 Instructors
(1, 1, 2), (1, 2, 2),
(12, 3, 2), (12, 4, 2),

-- Department 2 Instructors
(2, 6, 2), (2, 7, 2),
(13, 8, 2), (13, 9, 2),

-- Department 3 Instructors
(3, 11, 2), (3, 12, 2),
(14, 13, 2), (14, 14, 2),

-- Department 4 Instructors
(4, 16, 2), (4, 17, 2),
(15, 18, 2), (15, 19, 2),

-- Department 5 Instructors
(5, 21, 2), (5, 22, 2),
(16, 23, 2), (16, 24, 2),

-- Department 6 Instructors
(6, 26, 2), (6, 27, 2),
(17, 28, 2), (17, 29, 2),

-- Department 7 Instructors
(7, 31, 2), (7, 32, 2),
(18, 33, 2), (18, 34, 2),

-- Department 8 Instructors
(8, 36, 2), (8, 37, 2),
(19, 38, 2), (19, 39, 2),

-- Department 9 Instructors
(9, 41, 2), (9, 42, 2),
(20, 43, 2), (20, 44, 2),

-- Department 10 Instructor
(10, 46, 2), (10, 47, 2), (10, 48, 2),

-- Department 11 Instructor
(11, 51, 2), (11, 52, 2), (11, 53, 2);

-- ==========================================
-- 8. COURSE_SCHEDULES (20 Rows)
-- ==========================================

INSERT INTO [Course_Schedules] ([Instructors_Courses_Id], [Day], [Day_AR], [Type], [Type_AR], [StartTime], [EndTime]) VALUES
(1, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(1, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(1, 'Thursday', N'الخميس', 'Lab', N'معمل', '12:00:00', '14:00:00'),
(2, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(2, 'Wednesday', N'الأربعاء', 'Section', N'سكشن', '12:00:00', '14:00:00'),
(3, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '12:00:00', '14:00:00'),
(3, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '14:00:00', '16:00:00'),
(4, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(4, 'Thursday', N'الخميس', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(5, 'Wednesday', N'الأربعاء', 'Lecture', N'محاضرة', '09:00:00', '11:00:00'),
(5, 'Sunday', N'الأحد', 'Section', N'سكشن', '11:00:00', '13:00:00'),
(6, 'Tuesday', N'الثلاثاء', 'Lecture', N'محاضرة', '13:00:00', '15:00:00'),
(6, 'Thursday', N'الخميس', 'Section', N'سكشن', '08:00:00', '10:00:00'),
(7, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(8, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '14:00:00', '16:00:00'),
(8, 'Wednesday', N'الأربعاء', 'Section', N'سكشن', '08:00:00', '10:00:00'),
(9, 'Tuesday', N'الثلاثاء', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(10, 'Thursday', N'الخميس', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(10, 'Monday', N'الإثنين', 'Lab', N'معمل', '12:00:00', '14:00:00'),
(11, 'Wednesday', N'الأربعاء', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(12, 'Sunday', N'الأحد', 'Section', N'سكشن', '12:00:00', '14:00:00'),
(13, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '12:00:00', '14:00:00'),
(14, 'Thursday', N'الخميس', 'Section', N'سكشن', '14:00:00', '16:00:00'),
(15, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '09:00:00', '11:00:00'),
(16, 'Tuesday', N'الثلاثاء', 'Lecture', N'محاضرة', '11:00:00', '13:00:00'),
(17, 'Wednesday', N'الأربعاء', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(18, 'Monday', N'الإثنين', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(19, 'Thursday', N'الخميس', 'Lecture', N'محاضرة', '12:00:00', '14:00:00'),
(20, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(21, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '12:00:00', '14:00:00'),
(22, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '14:00:00', '16:00:00'),
(23, 'Wednesday', N'الأربعاء', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(24, 'Thursday', N'الخميس', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(25, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(26, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(27, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '11:00:00', '13:00:00'),
(28, 'Wednesday', N'الأربعاء', 'Section', N'سكشن', '13:00:00', '15:00:00'),
(29, 'Thursday', N'الخميس', 'Lecture', N'محاضرة', '09:00:00', '11:00:00'),
(30, 'Sunday', N'الأحد', 'Section', N'سكشن', '11:00:00', '13:00:00'),
(31, 'Tuesday', N'الثلاثاء', 'Lecture', N'محاضرة', '14:00:00', '16:00:00'),
(32, 'Monday', N'الإثنين', 'Section', N'سكشن', '08:00:00', '10:00:00'),
(33, 'Wednesday', N'الأربعاء', 'Lecture', N'محاضرة', '10:00:00', '12:00:00'),
(34, 'Thursday', N'الخميس', 'Section', N'سكشن', '12:00:00', '14:00:00'),
(35, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '12:00:00', '14:00:00'),
(36, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '14:00:00', '16:00:00'),
(37, 'Monday', N'الإثنين', 'Lecture', N'محاضرة', '08:00:00', '10:00:00'),
(38, 'Wednesday', N'الأربعاء', 'Section', N'سكشن', '10:00:00', '12:00:00'),
(39, 'Thursday', N'الخميس', 'Lab', N'معمل', '12:00:00', '14:00:00'),
(40, 'Sunday', N'الأحد', 'Lecture', N'محاضرة', '09:00:00', '11:00:00'),
(41, 'Tuesday', N'الثلاثاء', 'Section', N'سكشن', '11:00:00', '13:00:00'),
(42, 'Thursday', N'الخميس', 'Lab', N'معمل', '13:00:00', '15:00:00');


-- ==========================================
-- 9. LECTURE_EVENTS (20 Rows)
-- ==========================================

INSERT INTO [Lecture_Events] ([Schedule_Id], [Date_Of_Event], [Content_Covered]) VALUES
-- Schedule 1 (Analog Communications - Lecture)
(1, '2026-09-06', 'Introduction to Communication Systems and Signals'),
(1, '2026-09-13', 'Amplitude Modulation (AM) Principles'),
(1, '2026-09-20', 'Angle Modulation: Frequency and Phase Modulation'),
(1, '2026-09-27', 'Superheterodyne Receiver Architecture'),
(1, '2026-10-04', 'Noise in Analog Communication Systems'),

-- Schedule 2 (Analog Communications - Section)
(2, '2026-09-08', 'Signal Power and Energy Calculations'),
(2, '2026-09-15', 'AM Modulator Circuit Design Problems'),
(2, '2026-09-22', 'FM Bandwidth and Carson Rule Exercises'),
(2, '2026-10-06', 'Receiver Sensitivity and Noise Figure Analysis'),

-- Schedule 3 (Analog Communications - Lab)
(3, '2026-10-01', 'Function Generator and Oscilloscope Setup'),
(3, '2026-10-08', 'Simulating AM and FM Modulation in MATLAB/Simulink'),

-- Schedule 4 (Digital Communications - Lecture)
(4, '2026-09-07', 'Sampling Theorem and Pulse Code Modulation (PCM)'),
(4, '2026-09-14', 'Line Coding Techniques and Power Spectra'),
(4, '2026-09-21', 'Digital Modulation: ASK, FSK, and PSK'),
(4, '2026-09-28', 'Matched Filter and Inter-Symbol Interference (ISI)'),

-- Schedule 5 (Digital Communications - Section)
(5, '2026-09-09', 'Nyquist Rate and Quantization Error Problems'),
(5, '2026-09-16', 'Constellation Diagrams and Error Probability'),
(5, '2026-09-30', 'Equalization Techniques Workshop'),

-- Schedule 6 (Digital Logic Design - Lecture)
(6, '2026-09-08', 'Boolean Algebra and Logic Gates Minimization'),
(6, '2026-09-15', 'Combinational Logic Design: Adders and Decoders'),
(6, '2026-09-22', 'Sequential Circuits: Latches and Flip-Flops'),

-- Schedule 7 (Digital Logic Design - Section)
(7, '2026-09-10', 'Karnaugh Map Simplification Practice'),
(7, '2026-09-24', 'Designing Synchronous Counters'),

-- Schedule 8 (Microprocessors - Lecture)
(8, '2026-09-06', 'Microprocessor Architecture and Bus Structures'),
(8, '2026-09-13', 'Assembly Language Programming Fundamentals'),
(8, '2026-09-20', 'Interrupt Handling and Memory Interfacing'),

-- Schedule 9 (Microprocessors - Section)
(9, '2026-09-08', 'Writing Basic Assembly Routines'),
(9, '2026-09-22', 'Debugging Register and Stack Operations'),

-- Schedule 10 (Computer Architecture - Lecture)
(10, '2026-09-07', 'Instruction Set Architecture (ISA) Design'),
(10, '2026-09-14', 'Pipelining Hazards and Performance'),
(10, '2026-09-21', 'Memory Hierarchy and Cache Optimization'),

-- Schedule 11 (Computer Architecture - Lab)
(11, '2026-09-11', 'RISC-V Simulator Setup'),
(11, '2026-09-25', 'Implementing Pipeline Stages in Simulation'),

-- Schedule 12 (Biomedical Instrumentation - Lecture)
(12, '2026-09-06', 'Origin of Biopotentials and Electrodes'),
(12, '2026-09-13', 'Electrocardiography (ECG) Principles and Amplifiers'),
(12, '2026-09-20', 'Blood Pressure and Flow Measurement Systems'),

-- Schedule 13 (Biomedical Instrumentation - Section)
(13, '2026-09-08', 'Biopotential Amplifier Design Calculations'),
(13, '2026-09-22', 'Artifacts Filtering in Bio-signals'),

-- Schedule 14 (Medical Image Processing - Lecture)
(14, '2026-09-07', 'Introduction to Digital Image Fundamentals'),
(14, '2026-09-14', 'Spatial Filtering and Histogram Processing'),
(14, '2026-09-21', 'Image Segmentation in Medical Scans (MRI/CT)'),

-- Schedule 15 (Architectural Design I - Lecture)
(15, '2026-09-06', 'Introduction to Architectural Form and Space'),
(15, '2026-09-13', 'Site Analysis and Environmental Context'),
(15, '2026-09-20', 'Functional Zoning and Circulation Planning'),

-- Schedule 16 (History of Architecture - Lecture)
(16, '2026-09-08', 'Ancient Architecture: Mesopotamian and Egyptian'),
(16, '2026-09-15', 'Classical Antiquity: Greek and Roman Design'),
(16, '2026-09-22', 'Gothic and Renaissance Architectural Styles'),

-- Schedule 17 (Aerodynamics - Lecture)
(17, '2026-09-07', 'Fluid Dynamics Fundamentals for Aviation'),
(17, '2026-09-14', 'Airfoil Characteristics and Lift Generation'),
(17, '2026-09-21', 'Compressible Flow and Shock Waves'),

-- Schedule 18 (Aircraft Structures - Section)
(18, '2026-09-09', 'Load Distribution on Wing Structures'),
(18, '2026-09-23', 'Stress Analysis in Thin-Walled Beams'),

-- Schedule 19 (Construction Project Management - Lecture)
(19, '2026-09-06', 'Project Lifecycle and Project Delivery Systems'),
(19, '2026-09-13', 'Work Breakdown Structure (WBS) Development'),
(19, '2026-09-20', 'Critical Path Method (CPM) Network Scheduling'),

-- Schedule 20 (Electrical Power Systems - Lecture)
(20, '2026-09-07', 'Structure of Power Systems and Generation'),
(20, '2026-09-14', 'Transmission Line Parameters and Modeling'),
(20, '2026-09-21', 'Load Flow Analysis and System Stability'),

-- Schedule 21 (Electrical Machines - Lecture)
(21, '2026-09-09', 'Principles of Electromechanical Energy Conversion'),
(21, '2026-09-16', 'DC Motors and Generators Characteristics'),
(21, '2026-09-23', 'Three-Phase Induction Motors Operation'),

-- Schedule 22 (Machine Design - Lecture)
(22, '2026-09-08', 'Mechanical Engineering Design Process and Failure Theories'),
(22, '2026-09-15', 'Design of Shafts and Keys Under Torsion'),
(22, '2026-09-22', 'Spur and Helical Gear Mechanics'),

-- Schedule 23 (Theory of Machines - Section)
(23, '2026-09-10', 'Kinematic Analysis of Linkages'),
(23, '2026-09-24', 'Camm Mechanisms Displacement Diagrams'),

-- Schedule 24 (Mechatronics System Design - Lecture)
(24, '2026-09-07', 'Introduction to Mechatronic Systems Integration'),
(24, '2026-09-14', 'Microcontroller Interfacing and Embedded C'),
(24, '2026-09-21', 'Actuator Selection and Drive Circuits'),

-- Schedule 25 (Robotics Engineering - Lecture)
(25, '2026-09-09', 'Robot Classifications and Spatial Transformations'),
(25, '2026-09-16', 'Forward and Inverse Kinematics of Manipulators'),
(25, '2026-09-23', 'Trajectory Planning and Motion Control'),

-- Schedule 26 (Renewable Energy Systems - Lecture)
(26, '2026-09-06', 'Global Energy Trends and Renewable Portfolio'),
(26, '2026-09-13', 'Photovoltaic Cell Physics and Panel Characteristics'),
(26, '2026-09-20', 'Wind Turbine Aerodynamics and Power Extraction'),

-- Schedule 27 (Solar Energy Engineering - Lecture)
(27, '2026-09-08', 'Solar Geometry and Radiation Measurement'),
(27, '2026-09-15', 'PV System Sizing and Balance of System (BOS)'),
(27, '2026-09-22', 'Concentrated Solar Power (CSP) Technologies'),

-- Schedule 28 (Chemical Engineering Thermodynamics - Lecture)
(28, '2026-09-06', 'First and Second Laws of Thermodynamics for Chemical Systems'),
(28, '2026-09-13', 'PVT Behavior and Equation of State'),
(28, '2026-09-20', 'Phase Equilibria and Chemical Reaction Equilibria'),

-- Schedule 29 (Chemical Engineering Thermodynamics - Section)
(29, '2026-09-08', 'Volumetric Properties Calculations'),
(29, '2026-09-22', 'Equilibrium Constant and Fugacity Exercises'),

-- Schedule 30 (Fluid Mechanics - Lecture)
(30, '2026-09-07', 'Fluid Statics and Manometry'),
(30, '2026-09-14', 'Bernoulli Equation and Fluid Dynamics'),
(30, '2026-09-21', 'Navier-Stokes Equations and Pipe Friction'),

-- Schedule 31 (Heat and Mass Transfer - Lecture)
(31, '2026-09-06', 'Conduction, Convection, and Radiation Fundamentals'),
(31, '2026-09-13', 'Heat Exchangers Design and Analysis'),
(31, '2026-09-20', 'Fick First and Second Laws of Diffusion'),

-- Schedule 32 (Heat and Mass Transfer - Section)
(32, '2026-09-08', 'Overall Heat Transfer Coefficient Problem Solving'),
(32, '2026-09-22', 'Mass Transfer Flux Calculations'),

-- Schedule 33 (Chemical Reaction Engineering - Lecture)
(33, '2026-09-09', 'Kinetics of Homogeneous Reactions'),
(33, '2026-09-16', 'Ideal Reactor Models: CSTR, PFR, and Batch'),
(33, '2026-09-23', 'Catalysis and Catalytic Reactors Design'),

-- Schedule 34 (Process Control and Design - Lecture)
(34, '2026-09-08', 'Introduction to Process Dynamics and Laplace Transforms'),
(34, '2026-09-15', 'Feedback Controller Design (PID Controllers)'),
(34, '2026-09-22', 'Frequency Response Analysis and Stability Criteria');



-- ==========================================
-- 10. ENROLLMENTS (20 Rows)
-- ==========================================

INSERT INTO [Enrollments] ([Student_Id], [Course_Id], [Status_Id]) VALUES
-- Student 1 (Dept 1: Communication Engineering)
(1, 1, 1), (1, 2, 1), (1, 3, 1), (1, 4, 1), (1, 5, 1),
-- Student 2 (Dept 2: Computer Engineering)
(2, 6, 1), (2, 7, 1), (2, 8, 1), (2, 9, 1), (2, 10, 1),
-- Student 3 (Dept 3: Biomedical Engineering)
(3, 11, 1), (3, 12, 1), (3, 13, 1), (3, 14, 1), (3, 15, 1),
-- Student 4 (Dept 4: Architectural Engineering)
(4, 16, 1), (4, 17, 1), (4, 18, 1), (4, 19, 1), (4, 20, 1),
-- Student 5 (Dept 5: Aeronautical Engineering)
(5, 21, 1), (5, 22, 1), (5, 23, 1), (5, 24, 1), (5, 25, 1),
-- Student 6 (Dept 6: Construction Engineering)
(6, 26, 1), (6, 27, 1), (6, 28, 1), (6, 29, 1), (6, 30, 1),
-- Student 7 (Dept 7: Electrical Energy Engineering)
(7, 31, 1), (7, 32, 1), (7, 33, 1), (7, 34, 1), (7, 35, 1),
-- Student 8 (Dept 8: Mechanical Design Engineering)
(8, 36, 1), (8, 37, 1), (8, 38, 1), (8, 39, 1), (8, 40, 1),
-- Student 9 (Dept 9: Mechatronics Engineering)
(9, 41, 1), (9, 42, 1), (9, 43, 1), (9, 44, 1), (9, 45, 1),
-- Student 10 (Dept 10: Sustainable Energy Engineering)
(10, 46, 1), (10, 47, 1), (10, 48, 1), (10, 49, 1), (10, 50, 1),
-- Student 11 (Dept 11: Chemical Engineering)
(11, 51, 1), (11, 52, 1), (11, 53, 1), (11, 54, 1), (11, 55, 1),
-- Student 12 (Dept 1: Communication Engineering)
(12, 1, 1), (12, 2, 1), (12, 3, 1), (12, 4, 1), (12, 5, 1),
-- Student 13 (Dept 2: Computer Engineering)
(13, 6, 1), (13, 7, 1), (13, 8, 1), (13, 9, 1), (13, 10, 1),
-- Student 14 (Dept 3: Biomedical Engineering)
(14, 11, 1), (14, 12, 1), (14, 13, 1), (14, 14, 1), (14, 15, 1),
-- Student 15 (Dept 4: Architectural Engineering)
(15, 16, 1), (15, 17, 1), (15, 18, 1), (15, 19, 1), (15, 20, 1),
-- Student 16 (Dept 5: Aeronautical Engineering)
(16, 21, 1), (16, 22, 1), (16, 23, 1), (16, 24, 1), (16, 25, 1),
-- Student 17 (Dept 6: Construction Engineering)
(17, 26, 1), (17, 27, 1), (17, 28, 1), (17, 29, 1), (17, 30, 1),
-- Student 18 (Dept 7: Electrical Energy Engineering)
(18, 31, 1), (18, 32, 1), (18, 33, 1), (18, 34, 1), (18, 35, 1),
-- Student 19 (Dept 8: Mechanical Design Engineering)
(19, 36, 1), (19, 37, 1), (19, 38, 1), (19, 39, 1), (19, 40, 1),
-- Student 20 (Dept 9: Mechatronics Engineering)
(20, 41, 1), (20, 42, 1), (20, 43, 1), (20, 44, 1), (20, 45, 1);

-- ==========================================
-- 11. COURSES_DEPARTMENTS (20 Rows)
-- ==========================================

INSERT INTO [Courses_Departments] ([Course_Id], [Department_Id]) VALUES
-- Department 1 (Communication Engineering Program)
(1, 1), (2, 1), (3, 1), (4, 1), (5, 1),
-- Department 2 (Computer Engineering Program)
(6, 2), (7, 2), (8, 2), (9, 2), (10, 2),
-- Department 3 (Biomedical And Healthcare Data Engineering)
(11, 3), (12, 3), (13, 3), (14, 3), (15, 3),
-- Department 4 (Architectural Engineering and Technology Program)
(16, 4), (17, 4), (18, 4), (19, 4), (20, 4),
-- Department 5 (Aeronautical Engineering and Aviation Management)
(21, 5), (22, 5), (23, 5), (24, 5), (25, 5),
-- Department 6 (Construction Engineering and Management Program)
(26, 6), (27, 6), (28, 6), (29, 6), (30, 6),
-- Department 7 (Electrical Energy Engineering Program)
(31, 7), (32, 7), (33, 7), (34, 7), (35, 7),
-- Department 8 (Mechanical Design Engineering Program)
(36, 8), (37, 8), (38, 8), (39, 8), (40, 8),
-- Department 9 (Mechatronics Engineering Program)
(41, 9), (42, 9), (43, 9), (44, 9), (45, 9),
-- Department 10 (Sustainable Energy Engineering Program)
(46, 10), (47, 10), (48, 10), (49, 10), (50, 10),
-- Department 11 (Chemical Engineering Program)
(51, 11), (52, 11), (53, 11), (54, 11), (55, 11);

-- ==========================================
-- 12. COURSES_SEMESTERS (20 Rows)
-- ==========================================

INSERT INTO [Courses_Semesters] ([Course_Id], [Semester_Id]) VALUES
-- Department 1 Courses
(1, 2), (2, 2), (3, 2), (4, 2), (5, 2),
-- Department 2 Courses
(6, 2), (7, 2), (8, 2), (9, 2), (10, 2),
-- Department 3 Courses
(11, 2), (12, 2), (13, 2), (14, 2), (15, 2),
-- Department 4 Courses
(16, 2), (17, 2), (18, 2), (19, 2), (20, 2),
-- Department 5 Courses
(21, 2), (22, 2), (23, 2), (24, 2), (25, 2),
-- Department 6 Courses
(26, 2), (27, 2), (28, 2), (29, 2), (30, 2),
-- Department 7 Courses
(31, 2), (32, 2), (33, 2), (34, 2), (35, 2),
-- Department 8 Courses
(36, 2), (37, 2), (38, 2), (39, 2), (40, 2),
-- Department 9 Courses
(41, 2), (42, 2), (43, 2), (44, 2), (45, 2),
-- Department 10 Courses
(46, 2), (47, 2), (48, 2), (49, 2), (50, 2),
-- Department 11 Courses
(51, 2), (52, 2), (53, 2), (54, 2), (55, 2);

-- ==========================================
-- 13. COURSES_PREREQUISITES (Sample rows)
-- ==========================================

INSERT INTO [Courses_Prerequisites] ([Course_Id], [Prerequisite_Id]) VALUES
-- Communication Engineering Prerequisites
(2, 1), -- Digital Communications requires Analog Communications
(5, 2), -- Wireless Networks requires Digital Communications

-- Computer Engineering Prerequisites
(7, 6), -- Microprocessors requires Digital Logic Design
(8, 7), -- Computer Architecture requires Microprocessors
(9, 8), -- Embedded Systems requires Computer Architecture
(10, 9), -- VLSI Design requires Embedded Systems

-- Biomedical Engineering Prerequisites
(12, 11), -- Medical Image Processing requires Biomedical Instrumentation
(13, 11), -- Healthcare Info Systems requires Biomedical Instrumentation

-- Aeronautical Engineering Prerequisites
(22, 21), -- Aircraft Structures requires Aerodynamics
(23, 22), -- Flight Mechanics requires Aircraft Structures

-- Electrical Power Prerequisites
(32, 31), -- Electrical Machines requires Power Systems
(34, 33), -- Power Electronics requires High Voltage

-- Mechanical Design Prerequisites
(37, 36), -- Theory of Machines requires Machine Design
(38, 36), -- CAD/CAM requires Machine Design

-- Mechatronics Prerequisites
(42, 41), -- Robotics requires Mechatronics Design
(44, 43), -- Control Systems requires Sensors

-- Sustainable Energy Prerequisites
(47, 46), -- Solar Energy requires Renewable Energy
(48, 46), -- Wind Energy requires Renewable Energy

-- Chemical Engineering Prerequisites
(52, 51), -- Fluid Mechanics requires Chemical Engineering Thermodynamics
(53, 52), -- Heat and Mass Transfer requires Fluid Mechanics
(54, 53), -- Chemical Reaction Engineering requires Heat and Mass Transfer
(55, 53); -- Process Control and Design requires Heat and Mass Transfer



-- ==========================================
-- 14. ATTENDANCE (20 Rows linking students to lecture events)
-- ==========================================

INSERT INTO [Attendance] ([LectureEvents_Id], [Student_Id]) VALUES
(1, 1),
(1, 12),
(2, 1),
(3, 1),
(4, 12),
(6, 2),
(7, 2),
(8, 13),
(10, 13),
(12, 3),
(14, 3),
(15, 4),
(16, 4),
(17, 5),
(19, 6),
(20, 7),
(21, 7),
(22, 8),
(24, 9),
(25, 9),
(26, 10),
(27, 10),
(28, 11),
(30, 11),
(31, 11),
(33, 11),
(34, 11);





-- Example: Assign all current enrollments to Semester ID 1
UPDATE [Enrollments]
SET [Semester_Id] = 1
WHERE [Semester_Id] IS NULL;



















-- 1. Delete from child/junction tables first (tables with foreign keys pointing elsewhere)
DELETE FROM [Attendance];
DELETE FROM [Lecture_Events];
DELETE FROM [Enrollments];
DELETE FROM [Instructors_Courses];
DELETE FROM [Courses_Departments];
DELETE FROM [Courses_Semesters];
DELETE FROM [Courses_Prerequisites];

-- 2. Delete from core entity tables
DELETE FROM [Course_Schedules];
DELETE FROM [Students];
DELETE FROM [Instructors];
DELETE FROM [Admins];
DELETE FROM [Courses];
DELETE FROM [Semesters];


DISABLE TRIGGER trg_PreventDepartmentDelete ON Departments;

-- 2. Run your delete
DELETE FROM [Departments];

-- 3. Re-enable the trigger right after
ENABLE TRIGGER trg_PreventDepartmentDelete ON Departments;

-- 3. Delete from lookup/status tables last
DELETE FROM [StudentsStatus];
DELETE FROM [InstructorsStatus];
DELETE FROM [CoursesStatus];
DELETE FROM [EnrollmentsStatus];
DELETE FROM [AdminsStatus];





























-- ==========================================
-- 1. INSERT FRESHMAN COURSES (If not already added)
-- ==========================================
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Code] = 'M101')
BEGIN
    INSERT INTO [Courses] ([Name], [Name_AR], [Code], [Credit_Hours], [Difficulty], [Status_Id])
    VALUES 
        ('Calculus I', N'تفاضل وتكامل 1', 'M101', 3, 1, 1),
        ('Physics I', N'فيزياء هندسية 1', 'P101', 3, 1, 1),
        ('Intro to Programming', N'مقدمة في البرمجة', 'C101', 4, 1, 1),
        ('Academic Writing', N'كتابة أكاديمية', 'E101', 2, 1, 1),
        ('Calculus II', N'تفاضل وتكامل 2', 'M102', 3, 2, 1),
        ('Physics II', N'فيزياء هندسية 2', 'P102', 3, 2, 1),
        ('Data Structures', N'هياكل البيانات', 'C102', 4, 2, 1),
        ('Engineering Ethics', N'أخلاقيات الهندسة', 'G101', 2, 1, 1);
END

-- ==========================================
-- 2. ENROLL IBRAHIM KHALED IN FALL 2025 & SPRING 2026
-- ==========================================
DECLARE @StudentId INT;
SELECT @StudentId = [Id] FROM [Students] WHERE [Name] LIKE '%Ibrahim Khaled%';

-- Get Semester IDs for Fall 2025 and Spring 2026
DECLARE @Fall2025Id INT, @Spring2026Id INT;
SELECT @Fall2025Id = [Id] FROM [Semesters] WHERE [Name] = 'Fall 2025';
SELECT @Spring2026Id = [Id] FROM [Semesters] WHERE [Name] = 'Spring 2026';

-- Semester 1 (Fall 2025) Enrollments - First 4 Courses
INSERT INTO [Enrollments] ([Student_Id], [Course_Id], [Semester_Id], [Status_Id])
SELECT @StudentId, [Id], @Fall2025Id, 2 -- Status 2 = Passed/Enrolled
FROM [Courses] 
WHERE [Code] IN ('M101', 'P101', 'C101', 'E101')
  AND [Id] NOT IN (SELECT [Course_Id] FROM [Enrollments] WHERE [Student_Id] = @StudentId AND [Semester_Id] = @Fall2025Id);

-- Semester 2 (Spring 2026) Enrollments - Next 4 Courses
INSERT INTO [Enrollments] ([Student_Id], [Course_Id], [Semester_Id], [Status_Id])
SELECT @StudentId, [Id], @Spring2026Id, 2 
FROM [Courses] 
WHERE [Code] IN ('M102', 'P102', 'C102', 'G101')
  AND [Id] NOT IN (SELECT [Course_Id] FROM [Enrollments] WHERE [Student_Id] = @StudentId AND [Semester_Id] = @Spring2026Id);

-- ==========================================
-- 3. INSERT NUMERIC GRADES
-- ==========================================
-- ==========================================
-- 3. INSERT NUMERIC GRADES (Total_Grade is computed automatically)
-- ==========================================
INSERT INTO [Grades] ([Enrollment_Id], [Midterm_Grade], [CourseWork_Grade], [Final_Grade])
SELECT 
    e.[Id],
    24.00 AS Midterm_Grade,     -- Out of 30
    18.00 AS CourseWork_Grade,  -- Out of 20
    45.00 AS Final_Grade        -- Out of 50
FROM [Enrollments] e
WHERE e.[Student_Id] = @StudentId
  AND e.[Id] NOT IN (SELECT [Enrollment_Id] FROM [Grades]);