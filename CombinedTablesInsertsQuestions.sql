-- Combined SQL Table Definitions, Sample Data Inserts, and Query Questions

-- TABLE DEFINITIONS
-- Student table
CREATE TABLE Student (
    StudentId INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE
);

-- Course table
CREATE TABLE Course (
    CourseId INT PRIMARY KEY IDENTITY,
    CourseName NVARCHAR(100) NOT NULL,
    Credits INT NOT NULL
);

-- Classroom table
CREATE TABLE Classroom (
    ClassroomId INT PRIMARY KEY IDENTITY,
    RoomNumber NVARCHAR(20) NOT NULL,
    Capacity INT NOT NULL
);

-- Enrollment table with foreign key relations
CREATE TABLE Enrollment (
    EnrollmentId INT PRIMARY KEY IDENTITY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    ClassroomId INT NULL,
    EnrollmentDate DATE NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Student(StudentId),
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId),
    FOREIGN KEY (ClassroomId) REFERENCES Classroom(ClassroomId)
);

-- SAMPLE DATA INSERTS
-- Insert sample students
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Alice', 'Smith', 'alice.smith@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Bob', 'Johnson', 'bob.johnson@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Carol', 'Williams', 'carol.williams@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('David', 'Brown', 'david.brown@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Eve', 'Jones', 'eve.jones@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Frank', 'Garcia', 'frank.garcia@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Grace', 'Martinez', 'grace.martinez@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Hank', 'Rodriguez', 'hank.rodriguez@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Ivy', 'Lee', 'ivy.lee@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Jack', 'Walker', 'jack.walker@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Kathy', 'Hall', 'kathy.hall@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Leo', 'Allen', 'leo.allen@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Mona', 'Young', 'mona.young@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Nina', 'Hernandez', 'nina.hernandez@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Oscar', 'King', 'oscar.king@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Paul', 'Wright', 'paul.wright@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Quinn', 'Lopez', 'quinn.lopez@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Rita', 'Hill', 'rita.hill@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Sam', 'Scott', 'sam.scott@email.com');
INSERT INTO Student (FirstName, LastName, Email) VALUES ('Tina', 'Green', 'tina.green@email.com');

-- Insert sample courses
INSERT INTO Course (CourseName, Credits) VALUES ('Mathematics', 3);
INSERT INTO Course (CourseName, Credits) VALUES ('History', 2);
INSERT INTO Course (CourseName, Credits) VALUES ('Physics', 4);
INSERT INTO Course (CourseName, Credits) VALUES ('Chemistry', 3);
INSERT INTO Course (CourseName, Credits) VALUES ('Literature', 2);

-- Insert sample classrooms
INSERT INTO Classroom (RoomNumber, Capacity) VALUES ('A101', 30);
INSERT INTO Classroom (RoomNumber, Capacity) VALUES ('B202', 25);
INSERT INTO Classroom (RoomNumber, Capacity) VALUES ('C303', 20);
INSERT INTO Classroom (RoomNumber, Capacity) VALUES ('D404', 35);
INSERT INTO Classroom (RoomNumber, Capacity) VALUES ('E505', 40);

-- Insert sample enrollments (some students enrolled in multiple courses)
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (1, 1, 1, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (1, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (2, 1, 1, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (2, 3, 3, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (3, 4, 4, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (3, 5, 5, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (4, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (5, 1, 1, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (6, 3, 3, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (7, 4, 4, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (8, 5, 5, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (9, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (10, 1, 1, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (11, 3, 3, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (12, 4, 4, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (13, 5, 5, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (14, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (15, 1, 1, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (16, 3, 3, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (17, 4, 4, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (18, 5, 5, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (19, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (20, 1, 1, '2024-09-01');
-- Additional enrollments for students in multiple courses
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (5, 2, 2, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (5, 3, 3, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (10, 4, 4, '2024-09-01');
INSERT INTO Enrollment (StudentId, CourseId, ClassroomId, EnrollmentDate) VALUES (10, 5, 5, '2024-09-01');

-- QUERY QUESTIONS
-- 1. List all students enrolled in the 'Mathematics' course.
-- 2. Find the total number of students enrolled in each course.
-- 3. Show all classrooms and their capacities.
-- 4. List students who are enrolled in more than one course.
-- 5. Find all courses that have no students enrolled.
-- 6. Show the enrollment details (student name, course name, classroom) for all enrollments on '2024-09-01'.
-- 7. List all students and the courses they are enrolled in.
-- 8. Find the classroom with the highest capacity.
-- 9. List all students who are not enrolled in any course.
-- 10. Show the number of enrollments per classroom.
