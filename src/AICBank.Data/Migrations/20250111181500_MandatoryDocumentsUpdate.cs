using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AICBank.Data.Migrations
{
    /// <inheritdoc />
    public partial class MandatoryDocumentsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "MandatoryDocuments",
                newName: "AssociateType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssociateType",
                table: "MandatoryDocuments",
                newName: "Type");
        }
    }
}
