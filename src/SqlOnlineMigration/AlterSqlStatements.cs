using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public delegate string[] AlterSqlStatements(GhostTable target, INamingConventions namingConventions);
}
