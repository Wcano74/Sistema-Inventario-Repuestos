using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklySalesProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetWeeklySales
                AS
                BEGIN
                    -- Get last 7 days including today
                    SELECT 
                        FORMAT(d.DateValue, 'dddd', 'es-ES') as DayName,
                        ISNULL(SUM(s.Total), 0) as TotalSales
                    FROM (
                        SELECT CAST(GETDATE() - 6 AS DATE) AS DateValue UNION ALL
                        SELECT CAST(GETDATE() - 5 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 4 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 3 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 2 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 1 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() AS DATE)
                    ) d
                    LEFT JOIN Sales s ON CAST(s.Date AS DATE) = d.DateValue
                    GROUP BY d.DateValue
                    ORDER BY d.DateValue;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetWeeklySales");
        }
    }
}
