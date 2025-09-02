using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddedCharacterInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sl_character_info",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "integer", nullable: false),
                    physical_desc = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    personality_desc = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    personal_notes = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    character_secrets = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    exploitable_info = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    oocnotes = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sl_character_info", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_sl_character_info_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            string sql =
            """
                UPDATE sl_character_info 
                SET physical_desc = (
                    SELECT flavor_text
                    FROM profile
                    WHERE sl_character_info.profile_id = profile.profile_id
                );

                INSERT INTO sl_character_info (
                    "profile_id",
                    "physical_desc",
                    "personality_desc",
                    "personal_notes",
                    "character_secrets",
                    "exploitable_info",
                    "oocnotes")
                SELECT
                    profile_id,
                    flavor_text,
                    "" AS personality_desc,
                    "" AS personal_notes,
                    "" AS character_secrets,
                    "" AS exploitable_info,
                    "" AS oocnotes
                FROM profile
                WHERE profile_id NOT IN (SELECT profile_id FROM sl_character_info)
                AND flavor_text IS NOT "";
                """;
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sl_character_info");
        }
    }
}
