using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStockCodeToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add StockCode column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "StockCode",
                table: "Products",
                type: "TEXT",
                nullable: true);

            // 2. Generate stock codes for existing products
            migrationBuilder.Sql(@"
                UPDATE Products 
                SET StockCode = (
                    SELECT substr('ABCDEFGHIJKLMNOPQRSTUVWXYZ', abs(random()) % 26 + 1, 1) ||
                           substr('ABCDEFGHIJKLMNOPQRSTUVWXYZ', abs(random()) % 26 + 1, 1) ||
                           substr('ABCDEFGHIJKLMNOPQRSTUVWXYZ', abs(random()) % 26 + 1, 1) ||
                           substr('0123456789', abs(random()) % 10 + 1, 1) ||
                           substr('0123456789', abs(random()) % 10 + 1, 1) ||
                           substr('0123456789', abs(random()) % 10 + 1, 1)
                ) || '-' || Id
                WHERE StockCode IS NULL;
            ");

            // 3. Make StockCode non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "StockCode",
                table: "Products",
                type: "TEXT",
                nullable: false);

            // 4. Create unique index
            migrationBuilder.CreateIndex(
                name: "IX_Products_StockCode",
                table: "Products",
                column: "StockCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_StockCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockCode",
                table: "Products");
        }
    }
}
