using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidsCartoonPipeline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TopicSeedCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_TopicSeeds_TopicSeedId",
                table: "Episodes");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_TopicSeeds_TopicSeedId",
                table: "Episodes",
                column: "TopicSeedId",
                principalTable: "TopicSeeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_TopicSeeds_TopicSeedId",
                table: "Episodes");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_TopicSeeds_TopicSeedId",
                table: "Episodes",
                column: "TopicSeedId",
                principalTable: "TopicSeeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
