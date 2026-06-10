using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToaToa.Data.MigrationsCatalogo
{
    /// <inheritdoc />
    public partial class CaixaUnicoAberto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Caixas_Status",
                table: "Caixas",
                column: "Status",
                unique: true,
                filter: "Status = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Caixas_Status",
                table: "Caixas");
        }
    }
}
