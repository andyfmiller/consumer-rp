using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Consumer.Migrations
{
    public partial class RemoveCreatorFromCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_AspNetUsers_CreatorId",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Course_CreatorId",
                table: "Course");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Course",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Course",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_CreatorId",
                table: "Course",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_AspNetUsers_CreatorId",
                table: "Course",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
