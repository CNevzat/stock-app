using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut ürünlerin stokları için initial StockMovement kayıtları oluştur
            migrationBuilder.Sql(@"
                INSERT INTO StockMovements (ProductId, CategoryId, Type, Quantity, Description, CreatedAt)
                SELECT 
                    Id as ProductId,
                    CategoryId,
                    1 as Type,
                    StockQuantity as Quantity,
                    'İlk stok girişi' as Description,
                    CreatedAt
                FROM Products
                WHERE StockQuantity > 0
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Initial StockMovement kayıtlarını sil
            migrationBuilder.Sql(@"
                DELETE FROM StockMovements 
                WHERE Description = 'İlk stok girişi'
            ");
        }
    }
}
