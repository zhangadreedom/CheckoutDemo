using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckoutDemo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BillingCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    MethodType = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckoutPaymentId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentSessionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentSessionSecret = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
