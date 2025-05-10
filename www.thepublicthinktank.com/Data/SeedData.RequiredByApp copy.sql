-- Required seed data that the application needs to function properly

-- Roles (if using ASP.NET Identity)
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'User')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'User', 'USER', NEWID())
END

-- Default categories/tags
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Id, Name, Description)
    VALUES 
        (1, 'Politics', 'Political discussions and topics'),
        (2, 'Economics', 'Economic policies and discussions'),
        (3, 'Environment', 'Environmental issues and policies'),
        (4, 'Education', 'Educational reform and policy discussions'),
        (5, 'Healthcare', 'Healthcare policies and reform discussions')
END

-- Default settings
IF NOT EXISTS (SELECT 1 FROM SiteSettings WHERE [Key] = 'SiteName')
BEGIN
    INSERT INTO SiteSettings ([Key], Value)
    VALUES 
        ('SiteName', 'The Public Think Tank'),
        ('WelcomeMessage', 'Welcome to our public forum for policy discussion')
END