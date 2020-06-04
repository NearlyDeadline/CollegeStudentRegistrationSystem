CREATE TABLE registrar (
	id 			varchar(25),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id)
	);
	
CREATE TABLE classroom (
	building 	varchar(15), 
	room_number varchar(7),
	capacity 	decimal(4) NOT NULL CHECK (capacity > 0),
	PRIMARY KEY (building, room_number)
	);
	
CREATE TABLE department (
	dept_name 	varchar(20),
	building 	varchar(15),
	PRIMARY KEY (dept_name)
	);
	
CREATE TABLE time_slot (
	time_slot_id 	varchar(4),
	day 			varchar(9) CHECK (day IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')),
	start_wk 		decimal(2) NOT NULL CHECK (start_wk > 0),
	end_wk 			decimal(2) NOT NULL CHECK (end_wk < 52),
	start_hr 		decimal(2) NOT NULL CHECK (start_hr > 0),
	start_min 		decimal(2) NOT NULL CHECK (start_min >= 0 and start_min < 60),
	end_hr 			decimal(2) NOT NULL CHECK (end_hr < 24),
	end_min 		decimal(2) NOT NULL CHECK (end_min >= 0 and end_min < 60),
	CONSTRAINT `time_slot_ck_wk_seq` CHECK (start_wk <= end_wk),
	CONSTRAINT `time_slot_ck_hr_seq` CHECK (start_hr <= end_hr),
	PRIMARY KEY (time_slot_id, day, start_wk, end_wk, start_hr, start_min)
	);
	
CREATE TABLE course (
	course_id 	varchar(8),
	title 		varchar(50) NOT NULL,
	dept_name 	varchar(20),
	credits 	decimal(4, 2) CHECK (credits >= 0) DEFAULT 0,
	fee 		decimal(6) CHECK (fee >= 0) DEFAULT 0,
	PRIMARY KEY (course_id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
CREATE TABLE professor (
	id 			int AUTO_INCREMENT,
	name 		varchar(20) NOT NULL,
	dept_name 	varchar(20),
	salary 		decimal(8) CHECK (salary > 0),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
CREATE TABLE section (
	course_id 	 varchar(8),
	sec_id 		 varchar(8),
	semester 	 varchar(6) CHECK (semester IN ('Fall', 'Spring')),
	year 		 decimal(4) CHECK (year > 2000 and year < 2100),
	building 	 varchar(15),
	room_number  varchar(7),
	time_slot_id varchar(4),
	PRIMARY KEY (course_id, sec_id, semester, year),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (building,room_number) REFERENCES classroom(building,room_number) ON DELETE SET NULL,
	FOREIGN KEY (time_slot_id) REFERENCES time_slot(time_slot_id)
	);
	
CREATE TABLE teaches (
	id 			int,
	course_id 	varchar(8),
	sec_id 		varchar(8),
	semester 	varchar(6),
	year 		decimal(4),
	PRIMARY KEY (id,course_id,sec_id,semester,year),
	FOREIGN KEY (course_id,sec_id,semester,year) REFERENCES section (course_id,sec_id,semester,year) ON DELETE CASCADE,
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE CASCADE
	);
	
CREATE TABLE student (
	id 			int AUTO_INCREMENT,
	name 		varchar(20) NOT NULL,
	dept_name 	varchar(20),
	tot_cred 	decimal(5,2) CHECK (tot_cred >= 0 ),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
CREATE TABLE takes (
	id	 		int, 
	course_id 	varchar(8),
	sec_id	 	varchar(8),
	semester 	varchar(6),
	year	 	decimal(4),
	grade	 	varchar(2),
	status 		decimal(1),
	PRIMARY KEY (id, course_id, sec_id, semester, year),
	FOREIGN KEY (course_id,sec_id, semester, year) REFERENCES section(course_id,sec_id, semester, year) ON DELETE CASCADE,
	FOREIGN KEY (id) REFERENCES student(id) ON DELETE CASCADE
	);
	
CREATE TABLE prereq (
	course_id varchar(8),
	prereq_id varchar(8),
	PRIMARY KEY (course_id, prereq_id),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (prereq_id) REFERENCES course(course_id)
	);
	
CREATE TABLE can_teach (
	id 			int,
	course_id 	varchar(8),
	PRIMARY KEY (id, course_id),
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE CASCADE,
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE
	);
	
	