using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Travel.Migrations
{
    /// <inheritdoc />
    public partial class FixForumPostCategoryRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumPostCategories",
                table: "ForumPostCategories");

            migrationBuilder.DropIndex(
                name: "IX_ForumPostCategories_PostId",
                table: "ForumPostCategories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ForumPostCategories");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "ForumPostCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "ForumPostCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumPostCategories",
                table: "ForumPostCategories",
                columns: new[] { "PostId", "CategoryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumPostCategories",
                table: "ForumPostCategories");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "ForumPostCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "ForumPostCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ForumPostCategories",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumPostCategories",
                table: "ForumPostCategories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPostCategories_PostId",
                table: "ForumPostCategories",
                column: "PostId");
        }
    }
}
