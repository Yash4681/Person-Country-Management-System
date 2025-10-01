using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InsertPerson2_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"
                CREATE PROCEDURE [dbo].[InsertPerson]
                (@PersonID uniqueidentifier, @PersonName nvarchar(50), @Email nvarchar(100), @DateOfBirth datetime2(7), @CountryID uniqueidentifier, @Gender nvarchar(10), @Address nvarchar(200), @ReceiveNewsLetter bit)
                AS BEGIN
                INSERT INTO [dbo].[Persons] (PersonID, PersonName, Email, DateOfBirth, CountryID, Gender, Address, ReceiveNewsLetter) VALUES ( @PersonID, @PersonName, @Email, @DateOfBirth, @CountryID, @Gender, @Address, @ReceiveNewsLetter
                )
                END
            ";

            migrationBuilder.Sql(sp_InsertPerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"
                DROP PROCEDURE [dbo].[InsertPerson]
            ";

            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
