using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalProjectNotes.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivitiesCarts_Carts_CartId",
                table: "ActivitiesCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivitiesListBoards_Boards_BoardId",
                table: "ActivitiesListBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityListCarts_ListCarts_ListCartId",
                table: "ActivityListCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_ListCarts_ListCartId",
                table: "Carts");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitiesCarts_Carts_CartId",
                table: "ActivitiesCarts",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitiesListBoards_Boards_BoardId",
                table: "ActivitiesListBoards",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityListCarts_ListCarts_ListCartId",
                table: "ActivityListCarts",
                column: "ListCartId",
                principalTable: "ListCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_ListCarts_ListCartId",
                table: "Carts",
                column: "ListCartId",
                principalTable: "ListCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivitiesCarts_Carts_CartId",
                table: "ActivitiesCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivitiesListBoards_Boards_BoardId",
                table: "ActivitiesListBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityListCarts_ListCarts_ListCartId",
                table: "ActivityListCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_ListCarts_ListCartId",
                table: "Carts");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitiesCarts_Carts_CartId",
                table: "ActivitiesCarts",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitiesListBoards_Boards_BoardId",
                table: "ActivitiesListBoards",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityListCarts_ListCarts_ListCartId",
                table: "ActivityListCarts",
                column: "ListCartId",
                principalTable: "ListCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_ListCarts_ListCartId",
                table: "Carts",
                column: "ListCartId",
                principalTable: "ListCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
