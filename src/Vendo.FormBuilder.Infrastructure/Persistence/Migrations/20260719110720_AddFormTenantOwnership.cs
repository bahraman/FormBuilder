using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendo.FormBuilder.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFormTenantOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop only if present — some databases never created this index
            // (e.g. schema created from a later model, or index already removed).
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    DROP INDEX [IX_Forms_Slug_Version] ON [dbo].[Forms];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'RestaurantId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Forms] ADD [RestaurantId] int NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'SubscriberId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Forms] ADD [SubscriberId] int NOT NULL CONSTRAINT [DF_Forms_SubscriberId] DEFAULT (0);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    CREATE INDEX [IX_Forms_SubscriberId] ON [dbo].[Forms] ([SubscriberId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    CREATE INDEX [IX_Forms_SubscriberId_RestaurantId]
                        ON [dbo].[Forms] ([SubscriberId], [RestaurantId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Forms_SubscriberId_RestaurantId_Slug_Version]
                        ON [dbo].[Forms] ([SubscriberId], [RestaurantId], [Slug], [Version])
                        WHERE [IsDeleted] = 0;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId_Slug_Version] ON [dbo].[Forms];
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId] ON [dbo].[Forms];
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                    DROP INDEX [IX_Forms_SubscriberId] ON [dbo].[Forms];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'RestaurantId') IS NOT NULL
                    ALTER TABLE [dbo].[Forms] DROP COLUMN [RestaurantId];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'SubscriberId') IS NOT NULL
                BEGIN
                    DECLARE @df sysname;
                    SELECT @df = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Forms]')
                      AND c.name = N'SubscriberId';
                    IF @df IS NOT NULL
                        EXEC(N'ALTER TABLE [dbo].[Forms] DROP CONSTRAINT [' + @df + N']');
                    ALTER TABLE [dbo].[Forms] DROP COLUMN [SubscriberId];
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Forms_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Forms_Slug_Version]
                        ON [dbo].[Forms] ([Slug], [Version])
                        WHERE [IsDeleted] = 0;
                END
                """);
        }
    }
}
