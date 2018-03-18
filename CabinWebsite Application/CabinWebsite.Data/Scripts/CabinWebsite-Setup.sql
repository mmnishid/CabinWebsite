CREATE TABLE [user] (
userID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
vendorCode int NOT NULL,
username varchar(55) NOT NULL, 
email varchar(200), 
firstName varchar(55), 
lastName varchar(55), 
status int NOT NULL,	
lastLogin datetime,
prevLastLogin datetime
);

CREATE TABLE message (
messageID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
title varchar(55) NOT NULL, 
body varchar(1000), 
createdDate datetime NOT NULL,
createdBy int NOT NULL FOREIGN KEY REFERENCES [user](userID),
sentTo int NOT NULL FOREIGN KEY REFERENCES [user](userID)
);

CREATE TABLE newsboard (
newsboardID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
title varchar(55) NOT NULL, 
body varchar(1000), 
createdDate datetime NOT NULL,
createdBy int NOT NULL FOREIGN KEY REFERENCES [user](userID)
);

CREATE TABLE reservation (
reservationID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
reservationDate datetime NOT NULL,
createdDate datetime NOT NULL,
createdBy int NOT NULL FOREIGN KEY REFERENCES [user](userID)
);

CREATE TABLE [Version] (
	ID varchar(25) NOT NULL PRIMARY KEY,
	VersionNumber int
);

INSERT INTO Version VALUES('CabinWebsite', 1)
