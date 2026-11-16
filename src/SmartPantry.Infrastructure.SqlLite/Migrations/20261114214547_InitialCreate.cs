using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartPantry.Infrastructure.SqlLite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diets", x => x.Id);
                    table.CheckConstraint("CK_Diets_Name", "length(trim(Name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Calories = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.CheckConstraint("CK_Ingredients_Name", "length(trim(Name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "LegacyImportState",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    LegacyImportCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyImportState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Servings = table.Column<int>(type: "INTEGER", nullable: false),
                    DietId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdProfiles", x => x.Id);
                    table.CheckConstraint("CK_HouseholdProfiles_Name", "length(trim(Name)) BETWEEN 1 AND 50");
                    table.CheckConstraint("CK_HouseholdProfiles_Servings", "Servings BETWEEN 1 AND 20");
                    table.ForeignKey(
                        name: "FK_HouseholdProfiles_Diets_DietId",
                        column: x => x.DietId,
                        principalTable: "Diets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdProfileAllergies",
                columns: table => new
                {
                    HouseholdProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdProfileAllergies", x => new { x.HouseholdProfileId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_HouseholdProfileAllergies_HouseholdProfiles_HouseholdProfileId",
                        column: x => x.HouseholdProfileId,
                        principalTable: "HouseholdProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdProfileAllergies_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdProfileExclusions",
                columns: table => new
                {
                    HouseholdProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdProfileExclusions", x => new { x.HouseholdProfileId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_HouseholdProfileExclusions_HouseholdProfiles_HouseholdProfileId",
                        column: x => x.HouseholdProfileId,
                        principalTable: "HouseholdProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdProfileExclusions_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Diets",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1d08a48a-ce28-4822-b99e-8f2267cc8ac6"), new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Pescatarian", null, null },
                    { new Guid("267fa4c3-ce74-4ee9-ad64-c26fbca85b0c"), new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Vegan", null, null },
                    { new Guid("26e71575-0e24-4bfc-8383-a4e1d3de3240"), new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Other", null, null },
                    { new Guid("ba940e9a-f5dc-436b-9aec-427d87279eab"), new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "No specific restriction", null, null },
                    { new Guid("cc91e38d-96b3-4b78-9324-835093f80ca8"), new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Vegetarian", null, null }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "Calories", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("44608ed1-cb3e-4bfe-b2bb-81f992b6d71e"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Peanuts", null, null },
                    { new Guid("686266ca-56ab-4545-b6f5-0df8be99a85f"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Fish", null, null },
                    { new Guid("75ea73c3-add9-497c-894c-96b4df59bb59"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Milk / Dairy", null, null },
                    { new Guid("89e5fffd-9810-4631-8c77-23c393bacc6c"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Shellfish", null, null },
                    { new Guid("b0dd237b-2b46-4e00-b767-0a84ae758496"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Eggs", null, null },
                    { new Guid("d9f41a73-36a7-4178-a329-cf6eb4f0309b"), 0, new DateTime(2026, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "", null, null, "Tree nuts", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdProfileAllergies_IngredientId",
                table: "HouseholdProfileAllergies",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdProfileExclusions_IngredientId",
                table: "HouseholdProfileExclusions",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdProfiles_DietId",
                table: "HouseholdProfiles",
                column: "DietId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdProfiles_IsDefault",
                table: "HouseholdProfiles",
                column: "IsDefault",
                unique: true,
                filter: "IsDefault = 1 AND DeletedAt IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseholdProfileAllergies");

            migrationBuilder.DropTable(
                name: "HouseholdProfileExclusions");

            migrationBuilder.DropTable(
                name: "LegacyImportState");

            migrationBuilder.DropTable(
                name: "HouseholdProfiles");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Diets");
        }
    }
}
