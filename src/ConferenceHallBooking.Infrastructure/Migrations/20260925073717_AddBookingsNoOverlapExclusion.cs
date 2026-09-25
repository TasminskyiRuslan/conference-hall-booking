using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceHallBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsNoOverlapExclusion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE "Bookings" ADD CONSTRAINT "Bookings_NoOverlap"
                EXCLUDE USING gist (
                    "HallId" WITH =,
                    tstzrange("StartTime", "EndTime", '[)') WITH &&
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Bookings" DROP CONSTRAINT IF EXISTS "Bookings_NoOverlap";""");
        }
    }
}
