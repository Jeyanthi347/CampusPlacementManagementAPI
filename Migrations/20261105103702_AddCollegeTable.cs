using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusPlacementManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colleges",
                columns: table => new
                {
                    CollegeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CollegeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleges", x => x.CollegeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_CollegeCode",
                table: "Colleges",
                column: "CollegeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_Email",
                table: "Colleges",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Colleges_CollegeId",
                table: "Students",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "CollegeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Colleges_CollegeId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Colleges");
        }
    }
}
