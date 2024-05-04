CREATE TABLE Users
(
    UserId INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(50) NOT NULL,
    Password NVARCHAR(50) NOT NULL,
    Email NVARCHAR(50) NOT NULL,
    Role INT NOT NULL CHECK (Role IN (0, 1, 2))
);

INSERT INTO Users (UserName, Password, Email, Role)
VALUES 
('JohnDoe', 'password123', 'johndoe@example.com', 0),
('AdminUser', 'admin123', 'admin@example.com', 1),
('BloggerUser', 'blogger123', 'blogger@example.com', 2);

ALTER TABLE Users
ADD PasswordHash NVARCHAR(255);

ALTER TABLE Users
DROP COLUMN Password;

ALTER TABLE Users
ADD ResetToken NVARCHAR(255) NULL,
    ResetTokenExpiry DATETIME NULL;