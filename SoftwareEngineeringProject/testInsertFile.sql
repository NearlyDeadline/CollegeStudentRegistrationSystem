DELETE FROM can_teach;
DELETE FROM prereq;
DELETE FROM takes;
DELETE FROM student;
DELETE FROM teaches;
DELETE FROM section;
DELETE FROM professor;
DELETE FROM course;
DELETE FROM time_slot;
DELETE FROM department;
DELETE FROM classroom;
DELETE FROM registrar;

INSERT INTO registrar VALUES ('admin', 'admin');
INSERT INTO classroom VALUES ('逸夫楼', '101', 70);
INSERT INTO classroom VALUES ('计算机楼', 'A209', 70);
INSERT INTO department VALUES ('计算机科学与技术学院', '计算机楼');
INSERT INTO time_slot VALUES ('1', 'Monday', 1, 16, 8, 0, 9, 40);
INSERT INTO time_slot VALUES ('1', 'Thursday', 1, 16, 13, 30, 15, 10);
INSERT INTO time_slot VALUES ('2', 'Wednesday', 9, 16, 13, 30, 16, 30);
INSERT INTO course VALUES ('CS-101', '程序设计基础', '计算机科学与技术学院', 5, 700);
INSERT INTO course VALUES ('CS-101E', 'C语言课程设计', '计算机科学与技术学院', 0.5, 200);
INSERT INTO professor VALUES (1000, '张三', '计算机科学与技术学院', 8000, 'zhangsan');
INSERT INTO professor (name, dept_name, salary) VALUES ('李四', '计算机科学与技术学院', 7000);
INSERT INTO section VALUES ('CS-101', '1', 'Fall', 2020, '逸夫楼', '101', '1');
INSERT INTO section VALUES ('CS-101E', '1', 'Fall', 2020, '计算机楼', 'A209', '2');
INSERT INTO teaches VALUES ('1000', 'CS-101', '1', 'Fall', 2020);
INSERT INTO teaches VALUES ('1001', 'CS-101E', '1', 'Fall', 2020);
INSERT INTO student VALUES ('201700001', '小明', '计算机科学与技术学院', 2, 'xiaoming');
INSERT INTO student (name, dept_name, tot_cred) VALUES ('杰哥', '计算机科学与技术学院', 0);
INSERT INTO takes VALUES (201700001, 'CS-101', '1', 'Fall', 2020, 'A', 0);
INSERT INTO takes (id, course_id, sec_id, semester, year)VALUES (201700002, 'CS-101', '1', 'Fall', 2020);
INSERT INTO takes (id, course_id, sec_id, semester, year) VALUES (201700001, 'CS-101E', '1', 'Fall', 2020);
INSERT INTO prereq VALUES ('CS-101E', 'CS-101');
INSERT INTO can_teach VALUES ('1000', 'CS-101');
INSERT INTO can_teach VALUES ('1000', 'CS-101E');
INSERT INTO can_teach VALUES ('1001', 'CS-101');