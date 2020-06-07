#注册商：id，登录密码
CREATE TABLE registrar (
	id 			varchar(25),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id)
	);
	
#教室：所在建筑物名称，房间号，学生容量
CREATE TABLE classroom (
	building 	varchar(15), 
	room_number varchar(7),
	capacity 	decimal(4) NOT NULL,
	PRIMARY KEY (building, room_number)
	);
	
#学院：学院名称，所在建筑物名称
CREATE TABLE department (
	dept_name 	varchar(20),
	building 	varchar(15),
	PRIMARY KEY (dept_name)
	);
	
#上课时间：id，星期几，第一次课开始于第几星期，最后一次课结束于第几星期，开始上课时间，下课时间
CREATE TABLE time_slot (
	time_slot_id 	varchar(4),
	day 		varchar(9) ,
	start_wk 		decimal(2),
	end_wk 		decimal(2),
	start_tm 		time,
 	end_tm 		time,
 	CONSTRAINT `time_slot_check_day_in_enum` CHECK (day IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')),
 	CONSTRAINT `time_slot_check_startweek_unsigned` CHECK (start_wk > 0),
 	CONSTRAINT `time_slot_check_endweek_limited` CHECK (end_wk <= 52),
	CONSTRAINT `time_slot_check_week_sequence` CHECK (start_wk <= end_wk),
	CONSTRAINT `time_slot_check_time_sequence` CHECK (start_tm <= end_tm),
	PRIMARY KEY (time_slot_id, day, start_wk, end_wk, start_tm, end_tm)
	);
	
#课程：课程id，课程名，课程所属学院名称，课程学分，课程费用
CREATE TABLE course (
	course_id 	varchar(8),
	title 		varchar(50) NOT NULL,
	dept_name 	varchar(20),
	credits 	decimal(4, 2) UNSIGNED DEFAULT 0,
	fee 		decimal(6) DEFAULT 0,
	PRIMARY KEY (course_id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
#教授：教授id，教授姓名，出生日期，SSN，状态，教授所属学院名称，教授月工资，教授登录密码
CREATE TABLE professor (
	id 			int AUTO_INCREMENT,
	name 		varchar(20) NOT NULL,
 	date_of_birth 	DATE,
 	ssn 		decimal(9) NOT NULL UNIQUE,
 	status 		varchar(10),
	dept_name 	varchar(20),
	salary 		decimal(8),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
#课程段，对应题目中的course_offering：课程id，课程段id，开课学期，开课年份，开课地点建筑物名称，开课地点房间号，上课时间段
CREATE TABLE section (
	course_id 	 varchar(8),
	sec_id 		 varchar(8),
	semester 	 	varchar(6),
	year 		 year,
	building 	 	varchar(15),
	room_number  	varchar(7),
	time_slot_id 	varchar(4),
	CONSTRAINT `section_check_semester_in_enum` CHECK (semester IN ('Fall', 'Spring')),
	PRIMARY KEY (course_id, sec_id, semester, year),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (building,room_number) REFERENCES classroom(building,room_number) ON DELETE SET NULL,
	FOREIGN KEY (time_slot_id) REFERENCES time_slot(time_slot_id)
	);
	
#老师讲授课程：教授id，课程段主码
CREATE TABLE teaches (
	id 			int,
	course_id 	varchar(8),
	sec_id 		varchar(8),
	semester 		varchar(6),
	year 		year,
	PRIMARY KEY (id,course_id,sec_id,semester,year),
	FOREIGN KEY (course_id,sec_id,semester,year) REFERENCES section (course_id,sec_id,semester,year) ON DELETE CASCADE,
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE CASCADE
	);
	
#学生：id，姓名，出生日期，SSN，状态，毕业年份，所属学院名称，获得的总学分，登录密码
CREATE TABLE student (
	id 			int AUTO_INCREMENT,
	name 		varchar(20) NOT NULL,
 	date_of_birth 	DATE,
 	ssn 		decimal(9) NOT NULL UNIQUE,
 	status 		varchar(10),
 	graduate_date 	year,
	dept_name 	varchar(20),
	tot_cred 	decimal(5,2) UNSIGNED DEFAULT 0,
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
#学生选课：学生id，课程段主码，成绩，选课状态
#选课状态：registered代表已注册，choosed表示暂存但未注册，alternativeN表示第N个备选课
CREATE TABLE takes (
	id	 		int, 
	course_id 	varchar(8),
	sec_id	 	varchar(8),
	semester 		varchar(6),
	year	 	year,
	grade	 	varchar(2),
	status 		varchar(12),
 	CONSTRAINT `takes_check_status_in_enum` CHECK (status IN ('registered', 'choosed', 'alternative1', 'alternative2')),
	PRIMARY KEY (id, course_id, sec_id, semester, year),
	FOREIGN KEY (course_id,sec_id, semester, year) REFERENCES section(course_id,sec_id, semester, year) ON DELETE CASCADE,
	FOREIGN KEY (id) REFERENCES student(id) ON DELETE CASCADE
	);
	
#先修课：课程id，要求的先修课程id
CREATE TABLE prereq (
	course_id varchar(8),
	prereq_id varchar(8),
	PRIMARY KEY (course_id, prereq_id),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (prereq_id) REFERENCES course(course_id)
	);
	
#教师能教授的课程：教授id，课程id
CREATE TABLE can_teach (
	id 			int,
	course_id 	varchar(8),
	PRIMARY KEY (id, course_id),
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE CASCADE,
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE
	);
	
	