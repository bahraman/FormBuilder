using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendo.FormBuilder.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Earlier builds created SubscriberId/RestaurantId as uniqueidentifier.
    /// The model now uses int; this migration converts existing GUID columns to int.
    /// Guid values cannot be preserved — columns are rebuilt (SubscriberId defaults to 0).
    /// </remarks>
    public partial class ConvertFormTenantIdsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[Forms]', N'U') IS NULL
                    RETURN;

                DECLARE @subscriberType sysname =
                (
                    SELECT t.name
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[dbo].[Forms]')
                      AND c.name = N'SubscriberId'
                );

                DECLARE @restaurantType sysname =
                (
                    SELECT t.name
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[dbo].[Forms]')
                      AND c.name = N'RestaurantId'
                );

                -- Already correct (or columns missing and will be handled elsewhere).
                IF (@subscriberType IS NULL OR @subscriberType = N'int')
                   AND (@restaurantType IS NULL OR @restaurantType = N'int')
                    RETURN;

                -- Drop indexes that depend on the tenant columns.
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId_Slug_Version] ON [dbo].[Forms];

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId] ON [dbo].[Forms];

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId] ON [dbo].[Forms];

                -- Drop default constraints on these columns before dropping them.
                DECLARE @df sysname;
                DECLARE @sql nvarchar(max);

                SELECT @df = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Forms]')
                  AND c.name = N'SubscriberId';
                IF @df IS NOT NULL
                BEGIN
                    SET @sql = N'ALTER TABLE [dbo].[Forms] DROP CONSTRAINT [' + @df + N']';
                    EXEC sp_executesql @sql;
                END

                SET @df = NULL;
                SELECT @df = dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Forms]')
                  AND c.name = N'RestaurantId';
                IF @df IS NOT NULL
                BEGIN
                    SET @sql = N'ALTER TABLE [dbo].[Forms] DROP CONSTRAINT [' + @df + N']';
                    EXEC sp_executesql @sql;
                END

                IF COL_LENGTH(N'[dbo].[Forms]', N'SubscriberId') IS NOT NULL
                    ALTER TABLE [dbo].[Forms] DROP COLUMN [SubscriberId];

                IF COL_LENGTH(N'[dbo].[Forms]', N'RestaurantId') IS NOT NULL
                    ALTER TABLE [dbo].[Forms] DROP COLUMN [RestaurantId];

                ALTER TABLE [dbo].[Forms] ADD [RestaurantId] int NULL;
                ALTER TABLE [dbo].[Forms] ADD [SubscriberId] int NOT NULL
                    CONSTRAINT [DF_Forms_SubscriberId] DEFAULT (0);

                CREATE INDEX [IX_Forms_SubscriberId]
                    ON [dbo].[Forms] ([SubscriberId]);

                CREATE INDEX [IX_Forms_SubscriberId_RestaurantId]
                    ON [dbo].[Forms] ([SubscriberId], [RestaurantId]);

                CREATE UNIQUE INDEX [IX_Forms_SubscriberId_RestaurantId_Slug_Version]
                    ON [dbo].[Forms] ([SubscriberId], [RestaurantId], [Slug], [Version])
                    WHERE [IsDeleted] = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data conversion (int ←/→ uniqueidentifier). No-op down.
        }
    }
}
