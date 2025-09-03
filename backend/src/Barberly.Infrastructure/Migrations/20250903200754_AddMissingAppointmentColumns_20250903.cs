using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barberly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingAppointmentColumns_20250903 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Appointments",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Appointments");
        }
    }
}
