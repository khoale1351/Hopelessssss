USE TourismDB
GO
UPDATE AspNetUserRoles
SET RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'Admin@gmail.com');
