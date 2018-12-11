using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlOnlineMigration.Tests.Integration
{
    public class TracksMigrationTest
    {
        [Test]
        public async Task WhenMigratingSourceSchemaMigrated()
        {
            var schema = $"{nameof(TracksMigrationTest)}_{nameof(WhenMigratingSourceSchemaMigrated)}";

            (await Sut(schema)
                .Run()
                .ConfigureAwait(false))
            .AllTableDdlUnchanged()
            .SourceTableObjectIdsAreNotEqual();
        }

        [Test]
        public async Task WhenMigratingSourceTableIsArchived()
        {
            var schema = $"{nameof(TracksMigrationTest)}_{nameof(WhenMigratingSourceTableIsArchived)}";

            (await Sut(schema)
                .Run()
                .ConfigureAwait(false))
            .ArchivedTableObjectNotNull();
        }

        [Test]
        public async Task WhenMigratingMoreThanOnceWithoutSwappingResultIsOk()
        {
            var schema = $"{nameof(TracksMigrationTest)}_{nameof(WhenMigratingMoreThanOnceWithoutSwappingResultIsOk)}";

            (await Sut(schema)
                .GivenNoSwap()
                .Run()
                .ConfigureAwait(false))
            .ArchivedTableNull();

            (await Sut(schema)
                    .Run()
                    .ConfigureAwait(false))
            .ArchivedTableObjectNotNull();
        }

        private MigrationScenario Sut(string schema)
        {
            return new MigrationScenario(schema)
                .GivenSchema($@"                                          
                    CREATE TABLE [{schema}].[Artist] (
                        [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [Name] [nvarchar](50) NOT NULL,
                     CONSTRAINT [PK_Artist] PRIMARY KEY CLUSTERED ([Id] ASC))
                    
                    ALTER TABLE [{schema}].[Artist]
                    ADD CONSTRAINT UX_Artist_Name UNIQUE (Name)                    

                    CREATE TABLE [{schema}].[Album] (
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [Title] [varchar](50) NOT NULL,
	                    [ArtistId] [int] NOT NULL,
                     CONSTRAINT [PK_Album] PRIMARY KEY CLUSTERED ([Id] ASC))
                    
                    ALTER TABLE [{schema}].[Album]  WITH CHECK ADD  CONSTRAINT [FK_Album_Artist] FOREIGN KEY([ArtistId])
                    REFERENCES [{schema}].[Artist] ([Id])

                    ALTER TABLE [{schema}].[Album] CHECK CONSTRAINT [FK_Album_Artist]

                    CREATE TABLE [{schema}].[Track] (
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [Name] [varchar](50) NOT NULL,
	                    [AlbumId] [int] NOT NULL,
                     CONSTRAINT [PK_Track] PRIMARY KEY CLUSTERED ([Id] ASC))                    

                    ALTER TABLE [{schema}].[Track]  WITH CHECK ADD  CONSTRAINT [FK_Track_Album] FOREIGN KEY([AlbumId])
                    REFERENCES [{schema}].[Album] ([Id])

                    ALTER TABLE [{schema}].[Track] CHECK CONSTRAINT [FK_Track_Album]")
                .GivenTable(new TableName(schema, "Artist"))
                .GivenTable(new TableName(schema, "Album"))
                .GivenTable(new TableName(schema, "Track"))
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Artist] ([Name]) VALUES (@Name)", new { Name = "Oscar and the Wolf" }))
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Album] ([Title], [ArtistId]) VALUES (@Title, @ArtistId)", new { Title = "Infinity", ArtistId = 1 }))
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Track] ([Name], [AlbumId]) VALUES (@Name, @AlbumId)", new { Name = "So Real", AlbumId = 1 }))
                .WhenMigrating(new Source(new TableName(schema, "Track"), "Id"));
        }
    }
}
