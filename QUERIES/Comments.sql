CREATE TABLE Comments
(
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    CommentText NVARCHAR(MAX) NOT NULL,
    BlogId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Blogs_Comments FOREIGN KEY (BlogId)
    REFERENCES Blogs(BlogId),

    CONSTRAINT FK_Users_Comments FOREIGN KEY (UserId)
    REFERENCES Users(UserId)
);


INSERT INTO Comments (CommentText, BlogId, UserId, CreatedAt, UpdatedAt)
VALUES 
('Test Comment 1', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Test Comment 2', 2, 1, GETUTCDATE(), GETUTCDATE()),
('Test Comment 3', 1, 3, GETUTCDATE(), GETUTCDATE());