CREATE TABLE CommentVotes
(
    CommentVoteId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CommentId INT NOT NULL,
    Upvotes INT DEFAULT 0,
    Downvotes INT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (CommentId) REFERENCES Comments(CommentId)
);

ALTER TABLE CommentVotes
ADD CreatedAt DATETIME DEFAULT GETDATE();

UPDATE CommentVotes
SET CreatedAt = GETDATE()
WHERE CreatedAt IS NULL;