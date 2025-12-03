using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualEventTicketing.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseUserAndRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodePath",
                table: "PurchaseItems",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "PurchaseItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_AspNetUsers_UserId",
                table: "Purchases",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_AspNetUsers_UserId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "QrCodePath",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PurchaseItems");
        }
    }
}
