-- Development-only seed data for testing

-- Sample admin user (if using ASP.NET Identity)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@example.com')
BEGIN
    DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
    DECLARE @AdminRoleId UNIQUEIDENTIFIER;
    
    SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin';
    
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                           PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
                           TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AdminId, 'admin@example.com', 'ADMIN@EXAMPLE.COM', 'admin@example.com', 'ADMIN@EXAMPLE.COM', 1,
           'AQAAAAEAACcQAAAAEL5faDbEtZjY0Eyct9DepnMMp95JFoU77CQp2UQrnHVS5uILr/wEakJpXu7X3EF+Sw==', -- Password: Admin123!
           NEWID(), NEWID(), 0, 0, 1, 0);
           
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@AdminId, @AdminRoleId);
END

-- Sample discussion topics
IF NOT EXISTS (SELECT 1 FROM Topics WHERE Title = 'Climate Change Policy')
BEGIN
    INSERT INTO Topics (Id, Title, Description, CreatedByUserId, CreatedDate, LastActivityDate)
    VALUES 
        (1, 'Climate Change Policy', 'Discussion on current climate change initiatives', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), GETDATE(), GETDATE()),
        (2, 'Education Reform', 'Ideas for improving the education system', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), GETDATE(), GETDATE()),
        (3, 'Healthcare Accessibility', 'Discussing ways to improve healthcare access', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), GETDATE(), GETDATE())
END

-- Sample posts/comments
IF NOT EXISTS (SELECT 1 FROM Posts WHERE TopicId = 1)
BEGIN
    INSERT INTO Posts (Id, TopicId, Content, CreatedByUserId, CreatedDate)
    VALUES 
        (1, 1, 'What are your thoughts on carbon tax initiatives?', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), GETDATE()),
        (2, 1, 'I think we need to focus more on renewable energy investments.', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), DATEADD(HOUR, 1, GETDATE())),
        (3, 2, 'Should we focus on standardized testing or more holistic approaches?', 
           (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'admin@example.com'), GETDATE())
END