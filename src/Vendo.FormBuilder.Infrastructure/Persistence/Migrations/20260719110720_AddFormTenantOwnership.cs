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
            migrationBuilder.DropIndex(
                name: "IX_Forms_Slug_Version",
                table: "Forms");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Forms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriberId",
                table: "Forms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_SubscriberId",
                table: "Forms",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_SubscriberId_RestaurantId",
                table: "Forms",
                columns: new[] { "SubscriberId", "RestaurantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_SubscriberId_RestaurantId_Slug_Version",
                table: "Forms",
                columns: new[] { "SubscriberId", "RestaurantId", "Slug", "Version" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Forms_SubscriberId",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "IX_Forms_SubscriberId_RestaurantId",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "IX_Forms_SubscriberId_RestaurantId_Slug_Version",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                table: "Forms");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_Slug_Version",
                table: "Forms",
                columns: new[] { "Slug", "Version" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
