using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApplication.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Auctions_AuctionId",
                table: "ProductImage");

            migrationBuilder.AlterColumn<int>(
                name: "AuctionId",
                table: "ProductImage",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Auctions_AuctionId",
                table: "ProductImage",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Auctions_AuctionId",
                table: "ProductImage");

            migrationBuilder.AlterColumn<int>(
                name: "AuctionId",
                table: "ProductImage",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Auctions_AuctionId",
                table: "ProductImage",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id");
        }
    }
}
