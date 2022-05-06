using Microsoft.EntityFrameworkCore.Migrations;

namespace Travel.API.Migrations
{
    public partial class ModifyNameOfTouristRoutePictures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PictureRoutes_TouristRoutes_TouristRouteId",
                table: "PictureRoutes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PictureRoutes",
                table: "PictureRoutes");

            migrationBuilder.RenameTable(
                name: "PictureRoutes",
                newName: "TouristRoutePictures");

            migrationBuilder.RenameIndex(
                name: "IX_PictureRoutes_TouristRouteId",
                table: "TouristRoutePictures",
                newName: "IX_TouristRoutePictures_TouristRouteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TouristRoutePictures",
                table: "TouristRoutePictures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TouristRoutePictures_TouristRoutes_TouristRouteId",
                table: "TouristRoutePictures",
                column: "TouristRouteId",
                principalTable: "TouristRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TouristRoutePictures_TouristRoutes_TouristRouteId",
                table: "TouristRoutePictures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TouristRoutePictures",
                table: "TouristRoutePictures");

            migrationBuilder.RenameTable(
                name: "TouristRoutePictures",
                newName: "PictureRoutes");

            migrationBuilder.RenameIndex(
                name: "IX_TouristRoutePictures_TouristRouteId",
                table: "PictureRoutes",
                newName: "IX_PictureRoutes_TouristRouteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PictureRoutes",
                table: "PictureRoutes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PictureRoutes_TouristRoutes_TouristRouteId",
                table: "PictureRoutes",
                column: "TouristRouteId",
                principalTable: "TouristRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
