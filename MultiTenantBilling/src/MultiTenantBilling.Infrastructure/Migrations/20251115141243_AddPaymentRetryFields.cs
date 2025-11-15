using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantBilling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRetryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetry",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetryAttempt",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsRetry",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RetryAttempt",
                table: "Payments");
        }
    }
}
