IF DB_ID(N'DormitoryPaymentsDb') IS NULL
BEGIN
    CREATE DATABASE [DormitoryPaymentsDb];
END;
GO

USE [DormitoryPaymentsDb];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Dormitories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Address] nvarchar(250) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Dormitories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] int NOT NULL IDENTITY,
        [StudentNumber] nvarchar(30) NOT NULL,
        [LastName] nvarchar(80) NOT NULL,
        [FirstName] nvarchar(80) NOT NULL,
        [Patronymic] nvarchar(80) NULL,
        [GroupName] nvarchar(30) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Tariffs] (
        [Id] int NOT NULL IDENTITY,
        [ServiceType] nvarchar(100) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [DueDay] tinyint NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Tariffs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Tariffs_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_Tariffs_Dates] CHECK ([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]),
        CONSTRAINT [CK_Tariffs_DueDay] CHECK ([DueDay] BETWEEN 1 AND 28)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [DormitoryId] int NOT NULL,
        [Number] nvarchar(20) NOT NULL,
        [Capacity] int NOT NULL,
        [Gender] tinyint NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Rooms_Capacity] CHECK ([Capacity] > 0),
        CONSTRAINT [FK_Rooms_Dormitories_DormitoryId] FOREIGN KEY ([DormitoryId]) REFERENCES [Dormitories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [Login] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [DisplayName] nvarchar(150) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2(0) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] bigint NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [PaymentDate] date NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] tinyint NOT NULL,
        [DocumentNumber] nvarchar(60) NOT NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2(0) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Payments_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_Payments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Accruals] (
        [Id] bigint NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [TariffId] int NOT NULL,
        [Period] date NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [DueDate] date NOT NULL,
        [Status] tinyint NOT NULL,
        [CreatedAtUtc] datetime2(0) NOT NULL,
        CONSTRAINT [PK_Accruals] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Accruals_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_Accruals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Accruals_Tariffs_TariffId] FOREIGN KEY ([TariffId]) REFERENCES [Tariffs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [Accommodations] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [RoomId] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        CONSTRAINT [PK_Accommodations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Accommodations_Dates] CHECK ([EndDate] IS NULL OR [EndDate] >= [StartDate]),
        CONSTRAINT [FK_Accommodations_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Accommodations_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] int NULL,
        [TimestampUtc] datetime2(0) NOT NULL,
        [Level] nvarchar(30) NOT NULL,
        [Action] nvarchar(120) NOT NULL,
        [Details] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE TABLE [PaymentAllocations] (
        [Id] bigint NOT NULL IDENTITY,
        [PaymentId] bigint NOT NULL,
        [AccrualId] bigint NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PaymentAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PaymentAllocations_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_PaymentAllocations_Accruals_AccrualId] FOREIGN KEY ([AccrualId]) REFERENCES [Accruals] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentAllocations_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Accommodations_RoomId_StartDate_EndDate] ON [Accommodations] ([RoomId], [StartDate], [EndDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Accommodations_StudentId_StartDate] ON [Accommodations] ([StudentId], [StartDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Accruals_Period_StudentId] ON [Accruals] ([Period], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Accruals_StudentId_TariffId_Period] ON [Accruals] ([StudentId], [TariffId], [Period]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Accruals_TariffId] ON [Accruals] ([TariffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_TimestampUtc] ON [AuditLogs] ([TimestampUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Dormitories_Name] ON [Dormitories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_AccrualId] ON [PaymentAllocations] ([AccrualId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentAllocations_PaymentId_AccrualId] ON [PaymentAllocations] ([PaymentId], [AccrualId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_DocumentNumber] ON [Payments] ([DocumentNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_PaymentDate_StudentId] ON [Payments] ([PaymentDate], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_StudentId] ON [Payments] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rooms_DormitoryId_Number] ON [Rooms] ([DormitoryId], [Number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_LastName_FirstName] ON [Students] ([LastName], [FirstName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_StudentNumber] ON [Students] ([StudentNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tariffs_ServiceType_EffectiveFrom] ON [Tariffs] ([ServiceType], [EffectiveFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Login] ON [Users] ([Login]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105133142_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105133142_InitialCreate', N'8.0.21');
END;
GO

COMMIT;
GO

