using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenants.IdentityServer.Persistence.Migrations.TenantAdmin
{
    public partial class FirstMultiTenantMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenIdAuthority",
                table: "TenantInfo");

            migrationBuilder.DropColumn(
                name: "OpenIdClientId",
                table: "TenantInfo");

            migrationBuilder.DropColumn(
                name: "OpenIdClientSecret",
                table: "TenantInfo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenIdAuthority",
                table: "TenantInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenIdClientId",
                table: "TenantInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenIdClientSecret",
                table: "TenantInfo",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
