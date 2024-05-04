CREATE TABLE CommentRevisions (
    CommentRevisionId INT PRIMARY KEY IDENTITY,
    CommentText NVARCHAR(MAX),
    BlogId INT,
    UserId INT,
    CreatedAt DATETIME,
    FOREIGN KEY (BlogId) REFERENCES Blogs(BlogId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);