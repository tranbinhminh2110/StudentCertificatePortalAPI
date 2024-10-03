CREATE TABLE [Users] (
  [user_id] integer IDENTITY(1,1) PRIMARY KEY,
  [username] nvarchar(255),
  [password] nvarchar(255),
  [email] nvarchar(255),
  [fullname] nvarchar(255),
  [dob] datetime,
  [address] nvarchar(255),
  [phone_number] varchar(255),
  [role] nvarchar(255),
  [status] bit,
  [user_createdAt] datetime,
  [user_image] text
)
GO

CREATE TABLE [Majors] (
  [major_id] integer IDENTITY(1,1) PRIMARY KEY,
  [major_code] nvarchar(255),
  [major_name] nvarchar(255),
  [major_description] nvarchar
)
GO

CREATE TABLE [Major_Position] (
  [major_id] integer ,
  [job_position_id] integer,
  PRIMARY KEY ([major_id], [job_position_id])
)
GO

CREATE TABLE [Job_Positions] (
  [job_position_id] integer PRIMARY KEY,
  [job_position_code] nvarchar(255),
  [job_position_name] nvarchar(255),
  [job_position_description] nvarchar(255)
)
GO

CREATE TABLE [Certifications] (
  [cert_id] integer IDENTITY(1,1) PRIMARY KEY,
  [cert_name] nvarchar(255),
  [cert_code] nvarchar(255),
  [cert_description] nvarchar(255),
  [cert_cost] integer,
  [cert_point_system] nvarchar(255),
  [cert_image] text,
  [cert_prerequisite] nvarchar(255),
  [expiry_date] datetime,
  [type_id] integer,
  [organize_id] integer
)
GO

CREATE TABLE [Cert_Cert] (
  [cert_id] integer,
  [cert_id_two] integer,
  PRIMARY KEY ([cert_id], [cert_id_two])
)
GO

CREATE TABLE [Cert_Types] (
  [type_id] integer IDENTITY(1,1) PRIMARY KEY,
  [type_code] nvarchar(255),
  [type_name] nvarchar(255)
)
GO

CREATE TABLE [Exam_Sessions] (
  [session_id] integer IDENTITY(1,1) PRIMARY KEY,
  [session_name] nvarchar(255),
  [session_code] nvarchar(255),
  [session_date] datetime,
  [session_address] nvarchar(255),
  [cert_id] integer,
  [session_createdAt] datetime
)
GO

CREATE TABLE [Simulation_Exams] (
  [exam_id] integer IDENTITY(1,1) PRIMARY KEY,
  [exam_name] nvarchar(255),
  [exam_code] nvarchar(255),
  [cert_id] integer,
  [exam_description] nvarchar(255),
  [exam_fee] integer,
  [exam_discount_fee] integer,
  [exam_image] text
)
GO

CREATE TABLE [Questions] (
  [question_id] integer IDENTITY(1,1) PRIMARY KEY,
  [question_name] nvarchar(255),
  [exam_id] integer,
  [question_answer] nvarchar(255),
  [correct_answer] bit
)
GO

CREATE TABLE [Cart] (
  [cart_id] integer IDENTITY(1,1) PRIMARY KEY,
  [total_price] integer,
  [user_id] integer UNIQUE
)
GO

CREATE TABLE [Cart_Detail] (
  [cart_id] integer,
  [exam_id] integer,
  PRIMARY KEY ([cart_id], [exam_id])
)
GO

CREATE TABLE [Job_Cert] (
  [cert_id] integer,
  [job_position_id] integer,
  PRIMARY KEY ([cert_id], [job_position_id])
)
GO

CREATE TABLE [Feedbacks] (
  [feedback_id] integer IDENTITY(1,1) PRIMARY KEY,
  [feedback_description] nvarchar(255),
  [user_id] integer,
  [exam_id] integer,
  [feedback_createdAt] datetime,
  [feedback_image] text
)
GO

CREATE TABLE [Payments] (
  [payment_id] integer IDENTITY(1,1) PRIMARY KEY,
  [payment_date] datetime,
  [payment_amount] integer,
  [payment_method] nvarchar(255),
  [payment_status] nvarchar(255),
  [wallet_id] integer,
  [exam_enrollment_id] integer,
  [course_enrollment_id] integer
)
GO

CREATE TABLE [Exams_Enrollment] (
  [exam_enrollment_id] integer IDENTITY(1,1) PRIMARY KEY,
  [exam_enrollment_date] datetime,
  [exam_enrollment_status] nvarchar(255),
  [total_price] integer,
  [user_id] integer
)
GO

CREATE TABLE [Student_Of_Exam] (
  [enrollment_id] integer,
  [exam_id] integer,
  [creation_date] datetime,
  [expiry_date] datetime,
  [price] integer,
  [status] bit,
  PRIMARY KEY ([enrollment_id], [exam_id])
)
GO

CREATE TABLE [Vouchers] (
  [voucher_id] integer IDENTITY(1,1) PRIMARY KEY,
  [voucher_name] nvarchar(255),
  [voucher_description] nvarchar(255),
  [percentage] integer,
  [creation_date] datetime,
  [expiry_date] datetime,
  [voucher_status] bit
)
GO

CREATE TABLE [Voucher_Of_Exam] (
  [exam_id] integer,
  [voucher_id] integer,
  PRIMARY KEY ([exam_id], [voucher_id])
)
GO

CREATE TABLE [Wallet] (
  [wallet_id] integer IDENTITY(1,1) PRIMARY KEY,
  [point] integer,
  [user_id] integer UNIQUE,
  [deposit_date] datetime,
  [history] nvarchar(255),
  [wallet_status] nvarchar(255)
)
GO

CREATE TABLE [Organize] (
  [organize_id] integer IDENTITY(1,1) PRIMARY KEY,
  [organize_name] nvarchar(255),
  [organize_address] nvarchar(255),
  [organize_contact] nvarchar(255)
)
GO

CREATE TABLE [Courses] (
  [course_id] integer IDENTITY(1,1) PRIMARY KEY,
  [course_name] nvarchar(255),
  [course_code] nvarchar(255),
  [course_time] nvarchar(255),
  [course_description] nvarchar(255),
  [cert_id] integer
)
GO

CREATE TABLE [Courses_Enrollment] (
  [course_enrollment_id] integer IDENTITY(1,1) PRIMARY KEY,
  [course_enrollment_date] datetime,
  [course_enrollment_status] nvarchar(255),
  [total_price] integer,
  [user_id] integer
)
GO

CREATE TABLE [Student_Of_Course] (
  [couse_enrollment_id] integer,
  [course_id] integer,
  [creation_date] datetime,
  [expiry_date] datetime,
  [price] integer,
  [status] bit,
  PRIMARY KEY ([couse_enrollment_id], [course_id])
)
GO

ALTER TABLE [Cart] ADD FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id])
GO

ALTER TABLE [Wallet] ADD FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id])
GO

ALTER TABLE [Cart_Detail] ADD FOREIGN KEY ([cart_id]) REFERENCES [Cart] ([cart_id])
GO

ALTER TABLE [Cart_Detail] ADD FOREIGN KEY ([exam_id]) REFERENCES [Simulation_Exams] ([exam_id])
GO

ALTER TABLE [Feedbacks] ADD FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id])
GO

ALTER TABLE [Feedbacks] ADD FOREIGN KEY ([exam_id]) REFERENCES [Simulation_Exams] ([exam_id])
GO

ALTER TABLE [Exams_Enrollment] ADD FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id])
GO

ALTER TABLE [Student_Of_Exam] ADD FOREIGN KEY ([enrollment_id]) REFERENCES [Exams_Enrollment] ([exam_enrollment_id])
GO

ALTER TABLE [Student_Of_Exam] ADD FOREIGN KEY ([exam_id]) REFERENCES [Simulation_Exams] ([exam_id])
GO

ALTER TABLE [Payments] ADD FOREIGN KEY ([wallet_id]) REFERENCES [Wallet] ([wallet_id])
GO

ALTER TABLE [Payments] ADD FOREIGN KEY ([exam_enrollment_id]) REFERENCES [Exams_Enrollment] ([exam_enrollment_id])
GO

ALTER TABLE [Voucher_Of_Exam] ADD FOREIGN KEY ([exam_id]) REFERENCES [Simulation_Exams] ([exam_id])
GO

ALTER TABLE [Voucher_Of_Exam] ADD FOREIGN KEY ([voucher_id]) REFERENCES [Vouchers] ([voucher_id])
GO

ALTER TABLE [Simulation_Exams] ADD FOREIGN KEY ([cert_id]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Certifications] ADD FOREIGN KEY ([type_id]) REFERENCES [Cert_Types] ([type_id])
GO

ALTER TABLE [Exam_Sessions] ADD FOREIGN KEY ([cert_id]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Certifications] ADD FOREIGN KEY ([organize_id]) REFERENCES [Organize] ([organize_id])
GO

ALTER TABLE [Job_Cert] ADD FOREIGN KEY ([cert_id]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Job_Cert] ADD FOREIGN KEY ([cert_id]) REFERENCES [Job_Positions] ([job_position_id])
GO

ALTER TABLE [Major_Position] ADD FOREIGN KEY ([job_position_id]) REFERENCES [Job_Positions] ([job_position_id])
GO

ALTER TABLE [Major_Position] ADD FOREIGN KEY ([major_id]) REFERENCES [Majors] ([major_id])
GO

ALTER TABLE [Cert_Cert] ADD FOREIGN KEY ([cert_id]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Cert_Cert] ADD FOREIGN KEY ([cert_id_two]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Courses] ADD FOREIGN KEY ([cert_id]) REFERENCES [Certifications] ([cert_id])
GO

ALTER TABLE [Student_Of_Course] ADD FOREIGN KEY ([course_id]) REFERENCES [Courses] ([course_id])
GO

ALTER TABLE [Student_Of_Course] ADD FOREIGN KEY ([couse_enrollment_id]) REFERENCES [Courses_Enrollment] ([course_enrollment_id])
GO

ALTER TABLE [Courses_Enrollment] ADD FOREIGN KEY ([user_id]) REFERENCES [Users] ([user_id])
GO

ALTER TABLE [Payments] ADD FOREIGN KEY ([course_enrollment_id]) REFERENCES [Courses_Enrollment] ([course_enrollment_id])
GO

ALTER TABLE [Questions] ADD FOREIGN KEY ([exam_id]) REFERENCES [Simulation_Exams] ([exam_id])
GO
