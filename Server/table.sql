#注册商：id，登录密码
#注册商应在程序运行前就在数据库里存在，弄一个就行
CREATE TABLE registrar (
	id 			varchar(25),
	password 	varchar(25) DEFAULT '000000',
	PRIMARY KEY (id)
	);
	
#教室：所在建筑物名称，房间号，学生容量
#教室应在程序运行前就在数据库里存在，适当写几个即可
CREATE TABLE classroom (
	building 	varchar(15), 
	room_number varchar(7),
	capacity 	decimal(4) NOT NULL,
	PRIMARY KEY (building, room_number)
	);
	
#学院：学院名称，所在建筑物名称
#学院应当在程序运行前就在数据库里存在，适当写几个即可
CREATE TABLE department (
	dept_name 	varchar(20),
	building 	varchar(15),
	PRIMARY KEY (dept_name)
	);
	
#上课时间：id，星期几，第一次课开始于第几星期，最后一次课结束于第几星期，开始上课时间，下课时间
#上课时间应当在程序运行前就在数据库里存在，根据课程要求写几个即可
#下面解释一下各个列的意义：
#开始上课时间和下课时间为1-2位数字，表示第几节课（1-11节）
#time_slot_id的作用：
#例如软件工程课程，一周上两次课，周二为78节，周四为56节
#如果给该课程分配id为1的time_slot
#就需要加入两个表项，值为(1, '星期二', 1, 16, 7, 8)和(1, '星期四', 1, 16, 5, 6)
#然后在软件工程开设的section中，填写time_slot为1
#此外，软件工程还有另外一个班的上课时间，假设是周一56节和周五56节
#就需要再加入两个表项，值为(2, '星期一' ……)和(2, '星期五' ……)
CREATE TABLE time_slot (
	time_slot_id 	varchar(4),
	day 		varchar(3) ,
	start_wk 		decimal(2),
	end_wk 		decimal(2),
	start_tm 		decimal(2),
 	end_tm 		decimal(2),
 	CONSTRAINT `time_slot_check_day_in_enum` CHECK (day IN ('星期一','星期二','星期三','星期四','星期五','星期六','星期日')),
 	CONSTRAINT `time_slot_check_startweek_unsigned` CHECK (start_wk > 0),
 	CONSTRAINT `time_slot_check_endweek_limited` CHECK (end_wk <= 52),
	CONSTRAINT `time_slot_check_week_sequence` CHECK (start_wk <= end_wk),
	CONSTRAINT `time_slot_check_time_sequence` CHECK (start_tm <= end_tm),
	PRIMARY KEY (time_slot_id, day, start_wk, end_wk, start_tm, end_tm)
	);
	
#课程：课程id，课程名，课程所属学院名称，课程学分，课程费用
#课程应当在程序运行前就在数据库里存在，根据课程要求写几个即可
#学分最多有两位小数，支持课程设计等0.5学分的课
CREATE TABLE course (
	course_id 	varchar(8),
	title 		varchar(50) NOT NULL,
	dept_name 	varchar(20),
	credits 	decimal(4, 2) UNSIGNED DEFAULT 0,
	fee 		decimal(6) DEFAULT 0,
	PRIMARY KEY (course_id),
	FOREIGN KEY (dept_name) REFERENCES department(dept_name) ON DELETE SET NULL
	);
	
#教授：教授id，教授姓名，出生日期，身份证号，职称，教授所属学院名称，教授登录密码
#教授应当在程序运行前就在数据库里存在，根据课程要求写几个即可
#程序要求管理员能够对教授进行增删改查，请准备相关测试用例
#教授id原则上从1000开始分配
#出生日期写1970-01-01格式即可
#SSN为美国身份证号，为题目要求的列，自行构造9位不重复数字即可
#状态status为题目要求的列，我也不知道有什么用
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
	
#课程段，对应题目中的course_offering：课程id，课程段id，开课学期，开课年份，开课地点建筑物名称，开课地点房间号，上课时间段，讲课教师
#类似于“教学班”一样的存在，同一门课程会分为不同的教学班
#课程段应当在程序运行前就在数据库里存在，根据要求，每个课程尽量都写几个，覆盖时间（过去和现在），让教师选课和同学选课有较多选择
#course_id对应课程ID
#sec_id为每次开课的独有ID，比如软件工程，每学期开设4个教学班，这个sec_id就分配1、2、3、4
#semester为学期，限定为秋季学期或春季学期
#year为开课年份，一般的课程一年开一次
#building和room_number见之前的classroom表
#time_slot_id对应上课时间
#id是讲授这门课老师的id。以前开设的教学班都应有老师，本学期开设的教学班可以一部分没有老师，用来模拟选课环境
CREATE TABLE section (
	course_id 	 varchar(8),
	sec_id 		 varchar(8),
	semester 	 	varchar(2),
	year 		 year,
	building 	 	varchar(15),
	room_number  	varchar(7),
	time_slot_id 	varchar(4),
	id              int,
	CONSTRAINT `section_check_semester_in_enum` CHECK (semester IN ('秋季', '春季')),
	PRIMARY KEY (course_id, sec_id, semester, year),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (building,room_number) REFERENCES classroom(building,room_number) ON DELETE SET NULL,
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE SET NULL,
	FOREIGN KEY (time_slot_id) REFERENCES time_slot(time_slot_id)
	);
	
#学生：id，姓名，出生日期，SSN，状态，毕业年份，所属学院名称，获得的总学分，登录密码
#学生应当在程序运行前就在数据库里存在，根据课程要求写几个即可
#程序要求管理员能够对学生进行增删改查，请准备相关测试用例
#状态status为题目要求的列，我也不知道有什么用
#tot_cred为获得的总学分，随意填写吧。获得学分的程序应当属于SQL触发器的范畴，不在本程序范围内
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
#选课状态：已选和选中的解释详见需求分析文档，备选状态用于关闭注册时使用
CREATE TABLE takes (
	id	 		int, 
	course_id 	varchar(8),
	sec_id	 	varchar(8),
	semester 		varchar(6),
	year	 	year,
	grade	 	varchar(2),
	status 		varchar(5),
 	CONSTRAINT `takes_check_status_in_enum` CHECK (status IN ('已选', '选中', '备选1', '备选2')),
	PRIMARY KEY (id, course_id, sec_id, semester, year),
	FOREIGN KEY (course_id,sec_id, semester, year) REFERENCES section(course_id,sec_id, semester, year) ON DELETE CASCADE,
	FOREIGN KEY (id) REFERENCES student(id) ON DELETE CASCADE
	);
	
#先修课：课程id，要求的先修课程id
#先修课应当在程序运行前就在数据库里存在，根据课程course要求写几个即可，用来判断学生选课是否学了先修课
CREATE TABLE prereq (
	course_id varchar(8),
	prereq_id varchar(8),
	PRIMARY KEY (course_id, prereq_id),
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE,
	FOREIGN KEY (prereq_id) REFERENCES course(course_id)
	);
	
#教师能教授的课程：教授id，课程id
#能教授的课程应当在程序运行前就在数据库里存在，它与section开课的交集，组成了每位老师本学期“能教授的课程”，请确定好，以让老师有课可选
CREATE TABLE can_teach (
	id 			int,
	course_id 	varchar(8),
	PRIMARY KEY (id, course_id),
	FOREIGN KEY (id) REFERENCES professor(id) ON DELETE CASCADE,
	FOREIGN KEY (course_id) REFERENCES course(course_id) ON DELETE CASCADE
	);
	
	