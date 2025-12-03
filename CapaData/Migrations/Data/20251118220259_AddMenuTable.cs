using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CapaData.Migrations.Data
{
    /// <inheritdoc />
    public partial class AddMenuTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescripcionMenu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdPadre = table.Column<int>(type: "int", nullable: true),
                    Attach = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RolesUsers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Menus_IdPadre",
                        column: x => x.IdPadre,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Attach", "DescripcionMenu", "IdPadre", "RolesUsers" },
                values: new object[,]
                {
                    { 1, "", "Administracion", null, null },
                    { 2, "Roles/Index", "Roles", 1, null },
                    { 3, "UserRoles/Index", "Usuarios", 1, null },
                    { 4, "Menus/Index", "Menus", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IdPadre",
                table: "Menus",
                column: "IdPadre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
