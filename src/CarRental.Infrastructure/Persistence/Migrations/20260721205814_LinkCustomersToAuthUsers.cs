using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkCustomersToAuthUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalUserId",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ExternalUserId",
                schema: "customer",
                table: "Customers",
                column: "ExternalUserId",
                unique: true,
                filter: "[ExternalUserId] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_ExternalUserId",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                schema: "customer",
                table: "Customers");
        }
    }
}
