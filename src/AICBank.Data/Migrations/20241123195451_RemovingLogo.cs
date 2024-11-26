using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AICBank.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovingLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "BankAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "BankAccounts",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
