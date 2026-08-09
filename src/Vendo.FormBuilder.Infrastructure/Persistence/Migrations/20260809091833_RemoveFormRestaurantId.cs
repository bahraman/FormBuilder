using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendo.FormBuilder.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFormRestaurantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId_Slug_Version] ON [dbo].[Forms];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_RestaurantId'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    DROP INDEX [IX_Forms_SubscriberId_RestaurantId] ON [dbo].[Forms];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'RestaurantId') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Forms] DROP COLUMN [RestaurantId];
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Forms_SubscriberId_Slug_Version]
                        ON [dbo].[Forms] ([SubscriberId], [Slug], [Version])
                        WHERE [IsDeleted] = 0;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Forms_SubscriberId_Slug_Version'
                      AND object_id = OBJECT_ID(N'[dbo].[Forms]')
                )
                BEGIN
                    DROP INDEX [IX_Forms_SubscriberId_Slug_Version] ON [dbo].[Forms];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Forms]', N'RestaurantId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Forms] ADD [RestaurantId] int NULL;
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
    }
}
