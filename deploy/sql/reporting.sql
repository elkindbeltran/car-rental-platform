IF SCHEMA_ID(N'reporting') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [reporting]');
END;
GO

IF OBJECT_ID(N'[reporting].[ReportSubscriptions]', N'U') IS NULL
BEGIN
    CREATE TABLE [reporting].[ReportSubscriptions]
    (
        [Id] uniqueidentifier NOT NULL,
        [OwnerUserId] nvarchar(256) NOT NULL,
        [RecipientEmail] nvarchar(320) NOT NULL,
        [ReportType] nvarchar(100) NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ReportSubscriptions] PRIMARY KEY ([Id])
    );
END;
GO

IF OBJECT_ID(N'[reporting].[Reports]', N'U') IS NULL
BEGIN
    CREATE TABLE [reporting].[Reports]
    (
        [Id] uniqueidentifier NOT NULL,
        [SubscriptionId] uniqueidentifier NOT NULL,
        [OwnerUserId] nvarchar(256) NOT NULL,
        [RecipientEmail] nvarchar(320) NOT NULL,
        [ReportType] nvarchar(100) NOT NULL,
        [PeriodStartUtc] datetimeoffset NOT NULL,
        [PeriodEndUtc] datetimeoffset NOT NULL,
        [BlobContainer] nvarchar(63) NOT NULL,
        [BlobName] nvarchar(1024) NOT NULL,
        [ContentLength] bigint NOT NULL,
        [GeneratedAtUtc] datetimeoffset NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [EventPublishedAtUtc] datetimeoffset NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reports_ReportSubscriptions] FOREIGN KEY ([SubscriptionId])
            REFERENCES [reporting].[ReportSubscriptions] ([Id])
    );

    CREATE UNIQUE INDEX [IX_Reports_Subscription_Period]
        ON [reporting].[Reports] ([SubscriptionId], [PeriodStartUtc], [PeriodEndUtc]);
END;
GO
