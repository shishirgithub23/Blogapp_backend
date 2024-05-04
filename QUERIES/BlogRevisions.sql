CREATE TABLE BlogRevisions (
    BlogRevisionId INT PRIMARY KEY IDENTITY,
    BlogTitle NVARCHAR(MAX),
    BlogContent NVARCHAR(MAX),
    BlogId INT,
    UserId INT,
    CreatedAt DATETIME,
    FOREIGN KEY (BlogId) REFERENCES Blogs(BlogId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
