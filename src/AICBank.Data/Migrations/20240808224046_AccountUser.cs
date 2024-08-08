using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AICBank.Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountUserId",
                table: "BankAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccountUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankAccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_AccountUserId",
                table: "BankAccounts",
                column: "AccountUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_AccountUsers_AccountUserId",
                table: "BankAccounts",
                column: "AccountUserId",
                principalTable: "AccountUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_AccountUsers_AccountUserId",
                table: "BankAccounts");

            migrationBuilder.DropTable(
                name: "AccountUsers");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_AccountUserId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "AccountUserId",
                table: "BankAccounts");
        }
    }
}
