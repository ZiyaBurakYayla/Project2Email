using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project2Email.Migrations
{
    public partial class mig3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStatus",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "UserMessageStates",
                columns: table => new
                {
                    UserMessageStateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsStarred = table.Column<bool>(type: "bit", nullable: false),
                    IsTrash = table.Column<bool>(type: "bit", nullable: false),
                    Folder = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessageStates", x => x.UserMessageStateId);
                    table.ForeignKey(
                        name: "FK_UserMessageStates_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMessageStates_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CategoryId",
                table: "Messages",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageStates_AppUserId",
                table: "UserMessageStates",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageStates_MessageId",
                table: "UserMessageStates",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Categories_CategoryId",
                table: "Messages",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Categories_CategoryId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "UserMessageStates");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CategoryId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsStatus",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
