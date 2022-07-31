using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    countryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.countryID);
                });

            migrationBuilder.CreateTable(
                name: "FuelType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fuel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Makes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Makes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleColor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleColor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    makeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Makes_makeID",
                        column: x => x.makeID,
                        principalTable: "Makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phonenumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    clientURI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    availableListings = table.Column<int>(type: "int", nullable: false),
                    branchID = table.Column<long>(type: "bigint", nullable: true),
                    companyID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_branchID",
                        column: x => x.branchID,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Companies_companyID",
                        column: x => x.companyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfilePictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imgLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfilePictures_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    price = table.Column<float>(type: "real", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    km = table.Column<int>(type: "int", nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    postDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    manufactureYear = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    user = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    countryID = table.Column<long>(type: "bigint", nullable: false),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modelID = table.Column<long>(type: "bigint", nullable: false),
                    model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    makeID = table.Column<long>(type: "bigint", nullable: false),
                    make = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vehicleColorId = table.Column<long>(type: "bigint", nullable: false),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vehicleTypeId = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fuelTypeId = table.Column<long>(type: "bigint", nullable: false),
                    fuel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    firstImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cc = table.Column<long>(type: "bigint", nullable: false),
                    power = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Makes_makeID",
                        column: x => x.makeID,
                        principalTable: "Makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    VehicleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favorites_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Promoted",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicleId = table.Column<long>(type: "bigint", nullable: false),
                    isPromoted = table.Column<bool>(type: "bit", nullable: false),
                    promotedUntil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promoted", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promoted_Vehicles_vehicleId",
                        column: x => x.vehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buyerID = table.Column<long>(type: "bigint", nullable: false),
                    finalprice = table.Column<float>(type: "real", nullable: false),
                    proofOfPay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vehicleID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Users_buyerID",
                        column: x => x.buyerID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sales_Vehicles_vehicleID",
                        column: x => x.vehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vehicleID = table.Column<long>(type: "bigint", nullable: false),
                    order = table.Column<int>(type: "int", nullable: false),
                    filename = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleImages_Vehicles_vehicleID",
                        column: x => x.vehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "countryID", "name" },
                values: new object[,]
                {
                    { 1L, "England" },
                    { 2L, "Austria" },
                    { 3L, "Belgium" },
                    { 4L, "Bulgaria" },
                    { 5L, "Canada" },
                    { 6L, "Czech Republic" },
                    { 7L, "Croatia" },
                    { 8L, "Denmark" },
                    { 9L, "Estonia" },
                    { 10L, "Finland" },
                    { 11L, "France" },
                    { 12L, "Germany" },
                    { 13L, "Greece" },
                    { 14L, "Ireland" },
                    { 15L, "Italy" },
                    { 16L, "Japan" },
                    { 17L, "Luxembourg" },
                    { 18L, "Norvegia" },
                    { 19L, "Holland" },
                    { 20L, "Poland" },
                    { 21L, "Romania" },
                    { 22L, "Russia" },
                    { 23L, "Slovacia" },
                    { 24L, "Slovenia" },
                    { 25L, "Spain" },
                    { 26L, "USA" },
                    { 27L, "Sweden" },
                    { 28L, "Turkey" },
                    { 29L, "Ukraine" },
                    { 30L, "Hungary" },
                    { 31L, "Portugal" },
                    { 32L, "China" },
                    { 33L, "South Koreea" },
                    { 34L, "India" },
                    { 35L, "Australia" },
                    { 36L, "Mexico" },
                    { 37L, "Brasil" },
                    { 38L, "Argentina" },
                    { 39L, "Iceland" },
                    { 40L, "Lithuania" },
                    { 41L, "Lativa" },
                    { 42L, "Other" }
                });

            migrationBuilder.InsertData(
                table: "FuelType",
                columns: new[] { "Id", "fuel" },
                values: new object[,]
                {
                    { 1L, "Diesel" },
                    { 2L, "Petrol" },
                    { 3L, "Petrol + LPG" },
                    { 4L, "Hybrid" },
                    { 5L, "Electric" },
                    { 6L, "Hydrogen" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Firstname", "Lastname", "PasswordHash", "Phonenumber", "Role", "availableListings", "branchID", "clientURI", "companyID", "isConfirmed" },
                values: new object[] { 1L, "andrei_2008118@yahoo.com", "Andrei", "Lupu", "$2a$11$ZimoVKD9wZ9B65V3FUn65uS2L1b8VrR.17H.3VLW35RgR4E0oipIW", "+40745575094", "Admin", 0, null, null, null, true });

            migrationBuilder.InsertData(
                table: "VehicleColor",
                columns: new[] { "Id", "color" },
                values: new object[,]
                {
                    { 1L, "Black" },
                    { 2L, "White" },
                    { 3L, "Blue" },
                    { 4L, "Green" },
                    { 5L, "Grey" },
                    { 6L, "Red" },
                    { 7L, "Brown" },
                    { 8L, "Silver" },
                    { 9L, "Orange" },
                    { 10L, "Yellow" },
                    { 11L, "Burgundy" },
                    { 12L, "Other" }
                });

            migrationBuilder.InsertData(
                table: "VehicleType",
                columns: new[] { "Id", "type" },
                values: new object[,]
                {
                    { 1L, "Saloon" },
                    { 2L, "Estate" },
                    { 3L, "Hatchback" },
                    { 4L, "SUV" },
                    { 5L, "Van" },
                    { 6L, "Pick-up Truck" },
                    { 7L, "Cabrio" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyID",
                table: "Branches",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId",
                table: "Favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_VehicleId",
                table: "Favorites",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_makeID",
                table: "Models",
                column: "makeID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilePictures_UserID",
                table: "ProfilePictures",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promoted_vehicleId",
                table: "Promoted",
                column: "vehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_buyerID",
                table: "Sales",
                column: "buyerID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_vehicleID",
                table: "Sales",
                column: "vehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_branchID",
                table: "Users",
                column: "branchID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_companyID",
                table: "Users",
                column: "companyID",
                unique: true,
                filter: "[companyID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImages_vehicleID",
                table: "VehicleImages",
                column: "vehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_makeID",
                table: "Vehicles",
                column: "makeID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserID",
                table: "Vehicles",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "FuelType");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "ProfilePictures");

            migrationBuilder.DropTable(
                name: "Promoted");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "VehicleColor");

            migrationBuilder.DropTable(
                name: "VehicleImages");

            migrationBuilder.DropTable(
                name: "VehicleType");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Makes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
