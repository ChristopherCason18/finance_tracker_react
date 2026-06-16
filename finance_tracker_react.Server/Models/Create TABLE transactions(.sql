Drop table if EXISTS transactions
Create TABLE transactions(
id int IDENTITY(1,1) PRIMARY KEY NOT NULL,
type varchar(200) NOT NULL,
details varchar(200) NOT NULL,
particulars varchar(20),
code varchar(20),
reference varchar(20),
amount decimal NOT NULL,
date varchar(MAX) NOT NULL,
);