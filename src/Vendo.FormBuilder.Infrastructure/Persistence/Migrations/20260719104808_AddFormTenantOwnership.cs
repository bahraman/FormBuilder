using System;
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

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "Forms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberId",
                table: "Forms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
