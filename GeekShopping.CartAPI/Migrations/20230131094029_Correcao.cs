using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeekShopping.CartAPI.Migrations
{
    /// <inheritdoc />
    public partial class Correcao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_detail_product_ProducId",
                table: "cart_detail");

            migrationBuilder.DropIndex(
                name: "IX_cart_detail_ProducId",
                table: "cart_detail");

            migrationBuilder.DropColumn(
                name: "ProducId",
                table: "cart_detail");

            migrationBuilder.CreateIndex(
                name: "IX_cart_detail_ProductId",
                table: "cart_detail",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_detail_product_ProductId",
                table: "cart_detail",
                column: "ProductId",
                principalTable: "product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_detail_product_ProductId",
                table: "cart_detail");

            migrationBuilder.DropIndex(
                name: "IX_cart_detail_ProductId",
                table: "cart_detail");

            migrationBuilder.AddColumn<long>(
                name: "ProducId",
                table: "cart_detail",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_detail_ProducId",
                table: "cart_detail",
                column: "ProducId");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_detail_product_ProducId",
                table: "cart_detail",
                column: "ProducId",
                principalTable: "product",
                principalColumn: "Id");
        }
    }
}
