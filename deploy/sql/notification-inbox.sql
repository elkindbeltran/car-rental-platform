IF SCHEMA_ID(N'worker') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [worker]');
END;
GO

IF OBJECT_ID(N'[worker].[NotificationInbox]', N'U') IS NULL
BEGIN
    CREATE TABLE [worker].[NotificationInbox]
    (
        [MessageId] nvarchar(128) NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Attempts] int NOT NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NOT NULL,
        [LockedUntilUtc] datetimeoffset NULL,
        [LastFailure] nvarchar(2000) NULL,
        CONSTRAINT [PK_NotificationInbox] PRIMARY KEY ([MessageId])
    );

    CREATE INDEX [IX_NotificationInbox_Status_UpdatedAtUtc]
        ON [worker].[NotificationInbox] ([Status], [UpdatedAtUtc]);
END;
GO
