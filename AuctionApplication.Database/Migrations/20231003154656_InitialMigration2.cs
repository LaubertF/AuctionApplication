using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Auctions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Auctions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PictureBase64",
                table: "Auctions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_OwnerId",
                table: "Auctions",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_User_OwnerId",
                table: "Auctions",
                column: "OwnerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_User_OwnerId",
                table: "Auctions");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_OwnerId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "PictureBase64",
                table: "Auctions");
        }
    }
}
