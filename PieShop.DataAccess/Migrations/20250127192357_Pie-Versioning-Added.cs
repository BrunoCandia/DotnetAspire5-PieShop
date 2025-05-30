﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PieShop.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PieVersioningAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Pie",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Pie");
        }
    }
}
