using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NehasBeed.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Colors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Badge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InStock = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    Reviews = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Badge", "Category", "Colors", "CreatedAt", "Description", "Image", "Images", "InStock", "Name", "OriginalPrice", "Price", "Rating", "Reviews", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Bestseller", "bags", "[\"Multicolor\",\"Earth Tones\",\"Blue Ocean\"]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A stunning handcrafted beaded tote bag featuring an intricate mosaic pattern. Each bead is meticulously placed by skilled artisans using premium Japanese Miyuki seed beads.", "images/bag-1.png", "[\"images/bag-1.png\",\"images/bag-2.png\"]", true, "Mosaic Tote", 220.00m, 185.00m, 5.0m, 124, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "New", "bags", "[\"Terracotta\",\"Rust\",\"Sand\"]", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "A sophisticated bucket bag in warm terracotta tones. Handcrafted with thousands of premium seed beads creating a rich, textured surface.", "images/bag-2.png", "[\"images/bag-2.png\",\"images/bag-1.png\"]", true, "Terra Bucket", null, 165.00m, 4.8m, 54, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Sale", "bags", "[\"Black\",\"Midnight\"]", new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "An elegant evening clutch in classic noir. The deep black beads create a sophisticated pattern perfect for formal occasions.", "images/bag-3.png", "[\"images/bag-3.png\"]", true, "Noir Clutch", 175.00m, 145.00m, 4.9m, 87, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "Limited", "bags", "[\"Pearl White\",\"Champagne\",\"Rose\"]", new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "A luxurious mini bag adorned with pearlescent beads. The delicate sheen and feminine silhouette make this a truly special piece.", "images/bag-4.png", "[\"images/bag-4.png\",\"images/bag-5.png\"]", true, "Pearl Mini", null, 210.00m, 5.0m, 67, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, null, "bags", "[\"Floral Mix\",\"Pink Garden\",\"Sage Green\"]", new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "A charming pouch featuring a delicate floral beaded design. Perfect for carrying your essentials with artistic flair.", "images/bag-5.png", "[\"images/bag-5.png\",\"images/bag-1.png\"]", true, "Garden Pouch", null, 125.00m, 4.7m, 43, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, "Bestseller", "chains", "[\"Gold\",\"Antique Gold\",\"Silver\"]", new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "A bold statement chain necklace featuring intricate gold-tone beadwork. Each link is handcrafted for a luxurious drape and finish.", "images/bag-3.png", "[\"images/bag-3.png\"]", true, "Sovereign Chain", 105.00m, 85.00m, 4.9m, 98, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, "New", "necklaces", "[\"Onyx Black\",\"Charcoal\"]", new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "A multi-strand layered necklace featuring deep onyx seed beads. The layered effect creates a rich, textured look perfect for any occasion.", "images/bag-1.png", "[\"images/bag-1.png\"]", true, "Onyx Layers", null, 95.00m, 5.0m, 76, new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, null, "accessories", "[\"Crystal Clear\",\"Rose Gold\",\"Sapphire\"]", new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delicate crystal bead earrings that catch the light beautifully. Lightweight yet statement-making, these earrings are perfect for day to evening wear.", "images/bag-4.png", "[\"images/bag-4.png\"]", true, "Crystal Drops", null, 55.00m, 4.8m, 112, new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
