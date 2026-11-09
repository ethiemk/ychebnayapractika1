USE [DormitoryPaymentsDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = N'Администратор')
    INSERT INTO [Roles] ([Name]) VALUES (N'Администратор');
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = N'Бухгалтер')
    INSERT INTO [Roles] ([Name]) VALUES (N'Бухгалтер');
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = N'Комендант')
    INSERT INTO [Roles] ([Name]) VALUES (N'Комендант');
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = N'Просмотр')
    INSERT INTO [Roles] ([Name]) VALUES (N'Просмотр');

DECLARE @AdministratorRoleId int = (SELECT [Id] FROM [Roles] WHERE [Name] = N'Администратор');
DECLARE @AccountantRoleId int = (SELECT [Id] FROM [Roles] WHERE [Name] = N'Бухгалтер');
DECLARE @CommandantRoleId int = (SELECT [Id] FROM [Roles] WHERE [Name] = N'Комендант');
DECLARE @ViewerRoleId int = (SELECT [Id] FROM [Roles] WHERE [Name] = N'Просмотр');

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Login] = N'admin')
    INSERT INTO [Users] ([RoleId], [Login], [PasswordHash], [DisplayName], [IsActive], [CreatedAtUtc])
    VALUES (@AdministratorRoleId, N'admin', N'PBKDF2-SHA256$210000$UHyRaQHalBcyxk3eKxpZKw==$K6qw/fbW9wXRURn1sAXnVaiawP2/TyF3IofAb4fJGqE=', N'Администратор системы', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Login] = N'accountant')
    INSERT INTO [Users] ([RoleId], [Login], [PasswordHash], [DisplayName], [IsActive], [CreatedAtUtc])
    VALUES (@AccountantRoleId, N'accountant', N'PBKDF2-SHA256$210000$HvRfFqOWfV6ijxnyCXGhOA==$Vl6tRaBPvQSLzAFG4KY6pH8LjHTIyW/46hHACdWkEhc=', N'Бухгалтер', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Login] = N'commandant')
    INSERT INTO [Users] ([RoleId], [Login], [PasswordHash], [DisplayName], [IsActive], [CreatedAtUtc])
    VALUES (@CommandantRoleId, N'commandant', N'PBKDF2-SHA256$210000$jfcSY7JQVSp7I/fatlyUxQ==$JgyPhuhSXHYhlRcgH0porRsR8ETvQpN1GbiAqcWvqwA=', N'Комендант', 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Login] = N'viewer')
    INSERT INTO [Users] ([RoleId], [Login], [PasswordHash], [DisplayName], [IsActive], [CreatedAtUtc])
    VALUES (@ViewerRoleId, N'viewer', N'PBKDF2-SHA256$210000$ctPZNkPWJHvzbQAKMFHKMw==$4WRfqKltTqRzIHxRJ/Lj4u0XSD97RjgAu0mjJ5XDEyU=', N'Руководитель (просмотр)', 1, SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM [Dormitories] WHERE [Name] = N'Общежитие № 1')
    INSERT INTO [Dormitories] ([Name], [Address], [IsActive])
    VALUES (N'Общежитие № 1', N'г. Москва, Студенческая ул., д. 12', 1);
IF NOT EXISTS (SELECT 1 FROM [Dormitories] WHERE [Name] = N'Общежитие № 2')
    INSERT INTO [Dormitories] ([Name], [Address], [IsActive])
    VALUES (N'Общежитие № 2', N'г. Москва, Университетский пр-т, д. 8', 1);

DECLARE @Dormitory1 int = (SELECT [Id] FROM [Dormitories] WHERE [Name] = N'Общежитие № 1');
DECLARE @Dormitory2 int = (SELECT [Id] FROM [Dormitories] WHERE [Name] = N'Общежитие № 2');

IF NOT EXISTS (SELECT 1 FROM [Rooms] WHERE [DormitoryId] = @Dormitory1 AND [Number] = N'101')
    INSERT INTO [Rooms] ([DormitoryId], [Number], [Capacity], [Gender], [IsActive]) VALUES (@Dormitory1, N'101', 3, 2, 1);
IF NOT EXISTS (SELECT 1 FROM [Rooms] WHERE [DormitoryId] = @Dormitory1 AND [Number] = N'102')
    INSERT INTO [Rooms] ([DormitoryId], [Number], [Capacity], [Gender], [IsActive]) VALUES (@Dormitory1, N'102', 3, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Rooms] WHERE [DormitoryId] = @Dormitory1 AND [Number] = N'205')
    INSERT INTO [Rooms] ([DormitoryId], [Number], [Capacity], [Gender], [IsActive]) VALUES (@Dormitory1, N'205', 2, 0, 1);
IF NOT EXISTS (SELECT 1 FROM [Rooms] WHERE [DormitoryId] = @Dormitory2 AND [Number] = N'114')
    INSERT INTO [Rooms] ([DormitoryId], [Number], [Capacity], [Gender], [IsActive]) VALUES (@Dormitory2, N'114', 4, 2, 1);
IF NOT EXISTS (SELECT 1 FROM [Rooms] WHERE [DormitoryId] = @Dormitory2 AND [Number] = N'215')
    INSERT INTO [Rooms] ([DormitoryId], [Number], [Capacity], [Gender], [IsActive]) VALUES (@Dormitory2, N'215', 4, 1, 1);

IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24001')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24001', N'Волкова', N'Анна', N'Игоревна', N'ИС-21', N'+7 900 111-22-33', N'volkova@example.ru', 1);
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24002')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24002', N'Смирнов', N'Максим', N'Олегович', N'ИС-21', N'+7 900 222-33-44', N'smirnov@example.ru', 1);
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24003')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24003', N'Кузнецова', N'Мария', N'Андреевна', N'ЭК-22', N'+7 900 333-44-55', N'kuznetsova@example.ru', 1);
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24004')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24004', N'Попов', N'Артём', N'Сергеевич', N'ПИ-23', N'+7 900 444-55-66', N'popov@example.ru', 1);
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24005')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24005', N'Орлова', N'Елена', N'Дмитриевна', N'ПИ-23', N'+7 900 555-66-77', N'orlova@example.ru', 1);
IF NOT EXISTS (SELECT 1 FROM [Students] WHERE [StudentNumber] = N'СТ-24006')
    INSERT INTO [Students] ([StudentNumber], [LastName], [FirstName], [Patronymic], [GroupName], [Phone], [Email], [IsActive]) VALUES (N'СТ-24006', N'Соколов', N'Илья', N'Павлович', N'ЮР-24', N'+7 900 666-77-88', N'sokolov@example.ru', 1);

DECLARE @CurrentPeriod date = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @StartDate date = DATEADD(MONTH, -4, @CurrentPeriod);
DECLARE @Room101 int = (SELECT [Id] FROM [Rooms] WHERE [DormitoryId] = @Dormitory1 AND [Number] = N'101');
DECLARE @Room102 int = (SELECT [Id] FROM [Rooms] WHERE [DormitoryId] = @Dormitory1 AND [Number] = N'102');
DECLARE @Room114 int = (SELECT [Id] FROM [Rooms] WHERE [DormitoryId] = @Dormitory2 AND [Number] = N'114');
DECLARE @Room215 int = (SELECT [Id] FROM [Rooms] WHERE [DormitoryId] = @Dormitory2 AND [Number] = N'215');

IF NOT EXISTS (SELECT 1 FROM [Accommodations])
BEGIN
    INSERT INTO [Accommodations] ([StudentId], [RoomId], [StartDate], [EndDate])
    SELECT [Id], @Room101, @StartDate, NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24001'
    UNION ALL SELECT [Id], @Room102, @StartDate, NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24002'
    UNION ALL SELECT [Id], @Room101, DATEADD(DAY, 4, @StartDate), NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24003'
    UNION ALL SELECT [Id], @Room215, DATEADD(MONTH, 1, @StartDate), NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24004'
    UNION ALL SELECT [Id], @Room114, DATEADD(DAY, 2, @StartDate), NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24005'
    UNION ALL SELECT [Id], @Room102, DATEADD(MONTH, 2, @StartDate), NULL FROM [Students] WHERE [StudentNumber] = N'СТ-24006';
END;

IF NOT EXISTS (SELECT 1 FROM [Tariffs] WHERE [ServiceType] = N'Проживание')
    INSERT INTO [Tariffs] ([ServiceType], [Amount], [EffectiveFrom], [EffectiveTo], [DueDay], [IsActive])
    VALUES (N'Проживание', 2850.00, @StartDate, NULL, 10, 1);

DECLARE @TariffId int = (SELECT TOP (1) [Id] FROM [Tariffs] WHERE [ServiceType] = N'Проживание' ORDER BY [EffectiveFrom] DESC);

;WITH [Periods] AS
(
    SELECT DATEADD(MONTH, -2, @CurrentPeriod) AS [Period]
    UNION ALL SELECT DATEADD(MONTH, -1, @CurrentPeriod)
    UNION ALL SELECT @CurrentPeriod
)
INSERT INTO [Accruals] ([StudentId], [TariffId], [Period], [Amount], [DueDate], [Status], [CreatedAtUtc])
SELECT s.[Id], @TariffId, p.[Period], 2850.00,
       DATEFROMPARTS(YEAR(p.[Period]), MONTH(p.[Period]), 10), 1, SYSUTCDATETIME()
FROM [Students] s
CROSS JOIN [Periods] p
WHERE s.[IsActive] = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM [Accruals] a
      WHERE a.[StudentId] = s.[Id] AND a.[TariffId] = @TariffId AND a.[Period] = p.[Period]
  );

DECLARE @Student1 int = (SELECT [Id] FROM [Students] WHERE [StudentNumber] = N'СТ-24001');
DECLARE @Student2 int = (SELECT [Id] FROM [Students] WHERE [StudentNumber] = N'СТ-24002');
DECLARE @Student3 int = (SELECT [Id] FROM [Students] WHERE [StudentNumber] = N'СТ-24003');
DECLARE @Student5 int = (SELECT [Id] FROM [Students] WHERE [StudentNumber] = N'СТ-24005');

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0001')
    INSERT INTO [Payments] ([StudentId], [PaymentDate], [Amount], [Method], [DocumentNumber], [Note], [CreatedAtUtc]) VALUES (@Student1, DATEADD(DAY, 4, @CurrentPeriod), 5700.00, 2, N'ДЕМО-0001', N'Демонстрационный платёж', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0002')
    INSERT INTO [Payments] ([StudentId], [PaymentDate], [Amount], [Method], [DocumentNumber], [Note], [CreatedAtUtc]) VALUES (@Student2, DATEADD(DAY, 8, DATEADD(MONTH, -1, @CurrentPeriod)), 2850.00, 3, N'ДЕМО-0002', N'Демонстрационный платёж', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0003')
    INSERT INTO [Payments] ([StudentId], [PaymentDate], [Amount], [Method], [DocumentNumber], [Note], [CreatedAtUtc]) VALUES (@Student3, DATEADD(DAY, 2, @CurrentPeriod), 4000.00, 1, N'ДЕМО-0003', N'Демонстрационный платёж', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0004')
    INSERT INTO [Payments] ([StudentId], [PaymentDate], [Amount], [Method], [DocumentNumber], [Note], [CreatedAtUtc]) VALUES (@Student5, DATEADD(DAY, 6, DATEADD(MONTH, -1, @CurrentPeriod)), 8550.00, 2, N'ДЕМО-0004', N'Демонстрационный платёж', SYSUTCDATETIME());

DECLARE @Payment1 bigint = (SELECT [Id] FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0001');
DECLARE @Payment2 bigint = (SELECT [Id] FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0002');
DECLARE @Payment3 bigint = (SELECT [Id] FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0003');
DECLARE @Payment4 bigint = (SELECT [Id] FROM [Payments] WHERE [DocumentNumber] = N'ДЕМО-0004');

;WITH [Candidates] AS
(
    SELECT @Payment1 AS [PaymentId], a.[Id] AS [AccrualId], a.[Amount], ROW_NUMBER() OVER (ORDER BY a.[Period]) AS [RowNumber]
    FROM [Accruals] a WHERE a.[StudentId] = @Student1
)
INSERT INTO [PaymentAllocations] ([PaymentId], [AccrualId], [Amount])
SELECT [PaymentId], [AccrualId], [Amount] FROM [Candidates]
WHERE [RowNumber] <= 2 AND NOT EXISTS (SELECT 1 FROM [PaymentAllocations] x WHERE x.[PaymentId] = [Candidates].[PaymentId] AND x.[AccrualId] = [Candidates].[AccrualId]);

;WITH [Candidates] AS
(
    SELECT @Payment2 AS [PaymentId], a.[Id] AS [AccrualId], a.[Amount], ROW_NUMBER() OVER (ORDER BY a.[Period]) AS [RowNumber]
    FROM [Accruals] a WHERE a.[StudentId] = @Student2
)
INSERT INTO [PaymentAllocations] ([PaymentId], [AccrualId], [Amount])
SELECT [PaymentId], [AccrualId], [Amount] FROM [Candidates]
WHERE [RowNumber] = 1 AND NOT EXISTS (SELECT 1 FROM [PaymentAllocations] x WHERE x.[PaymentId] = [Candidates].[PaymentId] AND x.[AccrualId] = [Candidates].[AccrualId]);

;WITH [Candidates] AS
(
    SELECT @Payment3 AS [PaymentId], a.[Id] AS [AccrualId],
           CASE WHEN ROW_NUMBER() OVER (ORDER BY a.[Period]) = 1 THEN 2850.00 ELSE 1150.00 END AS [Amount],
           ROW_NUMBER() OVER (ORDER BY a.[Period]) AS [RowNumber]
    FROM [Accruals] a WHERE a.[StudentId] = @Student3
)
INSERT INTO [PaymentAllocations] ([PaymentId], [AccrualId], [Amount])
SELECT [PaymentId], [AccrualId], [Amount] FROM [Candidates]
WHERE [RowNumber] <= 2 AND NOT EXISTS (SELECT 1 FROM [PaymentAllocations] x WHERE x.[PaymentId] = [Candidates].[PaymentId] AND x.[AccrualId] = [Candidates].[AccrualId]);

;WITH [Candidates] AS
(
    SELECT @Payment4 AS [PaymentId], a.[Id] AS [AccrualId], a.[Amount], ROW_NUMBER() OVER (ORDER BY a.[Period]) AS [RowNumber]
    FROM [Accruals] a WHERE a.[StudentId] = @Student5
)
INSERT INTO [PaymentAllocations] ([PaymentId], [AccrualId], [Amount])
SELECT [PaymentId], [AccrualId], [Amount] FROM [Candidates]
WHERE [RowNumber] <= 3 AND NOT EXISTS (SELECT 1 FROM [PaymentAllocations] x WHERE x.[PaymentId] = [Candidates].[PaymentId] AND x.[AccrualId] = [Candidates].[AccrualId]);

UPDATE a
SET [Status] = CASE
    WHEN paid.[Amount] IS NULL OR paid.[Amount] = 0 THEN 1
    WHEN paid.[Amount] >= a.[Amount] THEN 3
    ELSE 2
END
FROM [Accruals] a
OUTER APPLY (SELECT SUM(x.[Amount]) AS [Amount] FROM [PaymentAllocations] x WHERE x.[AccrualId] = a.[Id]) paid;

IF NOT EXISTS (SELECT 1 FROM [AuditLogs] WHERE [Action] = N'DatabaseSeeded')
    INSERT INTO [AuditLogs] ([UserId], [TimestampUtc], [Level], [Action], [Details])
    VALUES (NULL, SYSUTCDATETIME(), N'Information', N'DatabaseSeeded', N'Созданы демонстрационные справочники, начисления и платежи из SQL-скрипта.');

COMMIT TRANSACTION;
GO
