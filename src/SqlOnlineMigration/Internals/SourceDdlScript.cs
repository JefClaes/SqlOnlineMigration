﻿using System.Text.RegularExpressions;

namespace SqlOnlineMigration.Internals
{
    public class SourceDdlScript
    {
        public SourceDdlScript(TableName tableName, string value)
        {
            Value = value;
            TableName = tableName;
        }

        public string Value { get; }
        public TableName TableName { get; }

        public SourceDdlScript ToGhost(INamingConventions namingConventions)
        {
            var result = string.Empty;

            result = Regex.Replace(Value, "WITH NOCHECK ADD  CONSTRAINT \\[(.*?)\\]", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "WITH CHECK ADD  CONSTRAINT \\[(.*?)\\]", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "ADD  CONSTRAINT \\[(.*?)\\]  DEFAULT", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "CHECK CONSTRAINT \\[(.*?)\\]", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "REFERENCES \\[(.*?)\\].\\[(.*?)\\]", ToTableName);
            result = Regex.Replace(result, "CREATE UNIQUE NONCLUSTERED INDEX \\[(.*?)\\].\\[(.*?)\\]", m => NewTableName(m, namingConventions));
            result = Regex.Replace(result, "CREATE UNIQUE NONCLUSTERED INDEX \\[(.*?)\\]", m => NewTableName(m, namingConventions));
            result = Regex.Replace(result, "CREATE NONCLUSTERED INDEX \\[(.*?)\\].\\[(.*?)\\]", m => NewTableName(m, namingConventions));
            result = Regex.Replace(result, "CREATE NONCLUSTERED INDEX \\[(.*?)\\]", m => NewTableName(m, namingConventions));
            result = Regex.Replace(result, "CREATE TABLE \\[(.*?)\\].\\[(.*?)\\]", m => NewTableName(m, namingConventions));
            result = Regex.Replace(result, "CONSTRAINT \\[(.*?)\\] PRIMARY KEY CLUSTERED", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "CONSTRAINT \\[(.*?)\\] UNIQUE NONCLUSTERED ", m => MatchConstraint(m, namingConventions));
            result = Regex.Replace(result, "ALTER TABLE \\[(.*?)\\].\\[(.*?)\\]", m => NewTableName(m, namingConventions));

            return new SourceDdlScript(new TableName(TableName.Schema, namingConventions.GhostObject(TableName.Name)), result);
        }

        private string NewTableName(Match m, INamingConventions namingConventions)
        {
            var full = m.Groups[0].Value;
            var tableName = m.Groups.Count == 3 ? m.Groups[2].Value : m.Groups[1].Value; // with or without schema

            return full.Replace(new MultiPartIdentifier(tableName).ToString(), new MultiPartIdentifier(namingConventions.GhostObject(tableName)).ToString());
        }

        private string ToTableName(Match m)
        {
            var full = m.Groups[0].Value;

            return full;
        }

        private string MatchConstraint(Match m, INamingConventions namingConventions)
        {
            var full = m.Groups[0].Value;
            var constraint = m.Groups[1].Value;

            return full.Replace(constraint, namingConventions.GhostObject(constraint));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

