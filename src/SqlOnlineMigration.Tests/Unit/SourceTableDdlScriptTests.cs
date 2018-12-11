using NUnit.Framework;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration.Tests.Unit
{
    public class SourceTableDdlScriptTests
    {
        [Test]
        public void Constraint()
        {
            var script = @"                  
                    CREATE TABLE [dbo].[Tx](
	                    [Created] [datetime] NOT NULL,	                
                     CONSTRAINT [Tx_PK] PRIMARY KEY CLUSTERED ([PKey] ASC)                                        

                    ALTER TABLE [dbo].[Tx] ADD  CONSTRAINT [Tx_Created_DF]  DEFAULT (GETDATE()) FOR [Created]";

            var sut = new SourceTableDdlScript(new TableName("dbo", "Tx"), new DdlScript(new [] {script}));

            Assert.AreEqual(@"                  
                    CREATE TABLE [dbo].[Tx_Ghost](
	                    [Created] [datetime] NOT NULL,	                
                     CONSTRAINT [Tx_PK_Ghost] PRIMARY KEY CLUSTERED ([PKey] ASC)                                        

                    ALTER TABLE [dbo].[Tx_Ghost] ADD  CONSTRAINT [Tx_Created_DF_Ghost]  DEFAULT (GETDATE()) FOR [Created]",
                sut.ToGhost(new DefaultNamingConventions()).Script.Value);
        }
    }
}
