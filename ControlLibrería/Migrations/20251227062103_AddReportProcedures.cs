using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class AddReportProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Daily Sales Procedure
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetDailySales
                AS
                BEGIN
                    SELECT 
                        ISNULL(SUM(Total), 0) as TotalSales,
                        COUNT(*) as TransactionCount
                    FROM Sales
                    WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE);
                END
            ");

            // 2. Low Stock Procedure
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetLowStockProducts
                AS
                BEGIN
                    SELECT * 
                    FROM Products
                    WHERE StockQuantity <= MinStock;
                END
            ");

            // 3. Top Selling Products
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetTopSellingProducts
                AS
                BEGIN
                    SELECT TOP 5
                        p.Name,
                        SUM(sd.Quantity) as TotalSold,
                        SUM(sd.Subtotal) as Revenue
                    FROM SaleDetails sd
                    JOIN Products p ON sd.ProductId = p.Id
                    GROUP BY p.Name
                    ORDER BY TotalSold DESC;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetDailySales");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetLowStockProducts");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetTopSellingProducts");
        }
    }
}
