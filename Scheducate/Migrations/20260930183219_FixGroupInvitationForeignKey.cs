using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheducate.Migrations
{
    /// <inheritdoc />
    public partial class FixGroupInvitationForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupInvitations_UserGroups_UserGroupId",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_UserGroupId",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "GroupInvitations");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_GroupId",
                table: "GroupInvitations",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupInvitations_UserGroups_GroupId",
                table: "GroupInvitations",
                column: "GroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupInvitations_UserGroups_GroupId",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_GroupId",
                table: "GroupInvitations");

            migrationBuilder.AddColumn<int>(
                name: "UserGroupId",
                table: "GroupInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_UserGroupId",
                table: "GroupInvitations",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupInvitations_UserGroups_UserGroupId",
                table: "GroupInvitations",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }
    }
}
