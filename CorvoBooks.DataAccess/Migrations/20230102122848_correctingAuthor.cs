using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorvoBooks.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class correctingAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Products");
        }
    }
}
