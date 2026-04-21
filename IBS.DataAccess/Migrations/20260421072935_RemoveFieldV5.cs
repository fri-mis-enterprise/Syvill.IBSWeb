using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_provisional_receipts_series_number_company",
                table: "provisional_receipts");

            migrationBuilder.DropColumn(
                name: "company",
                table: "provisional_receipts");

            migrationBuilder.CreateIndex(
                name: "ix_provisional_receipts_series_number",
                table: "provisional_receipts",
                column: "series_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_provisional_receipts_series_number",
                table: "provisional_receipts");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "provisional_receipts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_provisional_receipts_series_number_company",
                table: "provisional_receipts",
                columns: new[] { "series_number", "company" },
                unique: true);
        }
    }
}
