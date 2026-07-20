using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NehasBeed.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SelectedColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "City", "Country", "CreatedAt", "CustomerEmail", "CustomerName", "CustomerPhone", "InvoiceNumber", "PaymentMethod", "Postcode", "ShippingAddress", "ShippingCost", "Status", "Subtotal", "TotalAmount", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, "London", "United Kingdom", new DateTime(2026, 7, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "sophia@example.com", "Sophia Loren", "+44 7700 900001", "NB-10001", "Card", "SW1A 1AA", "123 Luxury Lane", 0m, "Confirmed", 185.00m, 185.00m, new DateTime(2026, 7, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, "Monaco", "Monaco", new DateTime(2026, 7, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), "grace@example.com", "Grace Kelly", "+377 99 99 00 00", "NB-10002", "PayPal", "98000", "Palace Avenue", 12.00m, "Dispatched", 210.00m, 222.00m, new DateTime(2026, 7, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 3, "Dubai", "UAE", new DateTime(2026, 7, 17, 8, 0, 0, 0, DateTimeKind.Unspecified), "zara@example.com", "Zara Ahmed", "+971 50 000 0000", "NB-10003", "Card", "00000", "Sheikh Zayed Road", 0m, "Pending", 120.00m, 120.00m, new DateTime(2026, 7, 17, 8, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 4, "Milan", "Italy", new DateTime(2026, 7, 13, 14, 0, 0, 0, DateTimeKind.Unspecified), "maria@example.com", "Maria Rossi", "+39 02 0000000", "NB-10004", "Card", "20121", "Via della Moda 5", 0m, "Rejected", 145.00m, 145.00m, new DateTime(2026, 7, 13, 14, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 5, "Berlin", "Germany", new DateTime(2026, 7, 12, 11, 0, 0, 0, DateTimeKind.Unspecified), "lena@example.com", "Lena Böhm", "+49 30 00000000", "NB-10005", "Card", "10719", "Kurfürstendamm 10", 12.00m, "Confirmed", 165.00m, 177.00m, new DateTime(2026, 7, 12, 11, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "ProductId", "ProductImage", "ProductName", "Quantity", "SelectedColor", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, 1, "images/bag-1.png", "Mosaic Tote", 1, "Multicolor", 185.00m },
                    { 2, 2, 4, "images/bag-4.png", "Pearl Mini", 1, "Pearl White", 210.00m },
                    { 3, 3, 6, "images/bag-3.png", "Sovereign Chain", 1, "Gold", 85.00m },
                    { 4, 3, 7, "images/bag-1.png", "Onyx Layers", 1, "Onyx Black", 35.00m },
                    { 5, 4, 3, "images/bag-3.png", "Noir Clutch", 1, "Black", 145.00m },
                    { 6, 5, 2, "images/bag-2.png", "Terra Bucket", 1, "Terracotta", 165.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
