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
            // btree_gist lets the EXCLUDE constraint mix an integer equality key with a range.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                // Equal HallId and disjoint ranges must hold simultaneously. '[)' allows
                // back-to-back bookings (one ends exactly when the next starts), no overlap.
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
