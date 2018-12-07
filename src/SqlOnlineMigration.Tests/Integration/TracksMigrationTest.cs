using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlOnlineMigration.Tests.Integration
{
    public class TracksMigrationTest
    {
        [Test]
        public async Task SchemaMigrated()
        {
            var schema = $"{nameof(TracksMigrationTest)}_{nameof(SchemaMigrated)}";

            await Sut(schema)
                .ThenDDLUnchanged(new TableName(schema, "Artist"))
                .ThenDDLUnchanged(new TableName(schema, "Album"))
                .ThenDDLUnchanged(new TableName(schema, "Track"))
                .Run().ConfigureAwait(false);
        }

        [Test]
        public async Task SourceArchived()
        {
            var schema = $"{nameof(TracksMigrationTest)}_{nameof(SourceArchived)}";

            await Sut(schema)
                .ThenSourceArchived()
                .Run().ConfigureAwait(false);
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
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Artist] ([Name]) VALUES (@Name)", new { Name = "Oscar and the Wolf" }))
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Album] ([Title], [ArtistId]) VALUES (@Title, @ArtistId)", new { Title = "Infinity", ArtistId = 1 }))
                .SeededWith(conn => conn.ExecuteAsync($"INSERT INTO [{schema}].[Track] ([Name], [AlbumId]) VALUES (@Name, @AlbumId)", new { Name = "So Real", AlbumId = 1 }))
                .WhenMigrating(new Source(new TableName(schema, "Track"), "Id"));
        }
    }
}
