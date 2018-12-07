# Sql Online Migration

Library that executes SQL Server schema changes as an online operation, reducing table and schema locks.

## Schema migration

1. Create `Ghost table`
2. Execute `DDL statements` on `Ghost table`
3. Attach `DML Triggers` on `Source table` to keep `Ghost table` in sync
4. Merge data in the `Source table` with data in the `Ghost table`
5. Swap the `Ghost table` with `Source table`, and archive the `Source table`

## Example

```C#

var migration = new SchemaMigrationBuilder(ConnectionString, Database)
    .WithLogger(new TestContextLogger())
    .WithSwapWrappedIn(async swap =>
        await Retry.OnDeadlock(swap, 3, TimeSpan.FromSeconds(1)).ConfigureAwait(false))
    .Build()

await migration.Run(
        new Source(new TableName("dbo", "Users"), "Id"), 
        (target, namingconv) => 
            new[] { $"ALTER TABLE {target}..." } ).ConfigureAwait(false);

```

## Disclaimer

This has only been tested on low-scale local environments/simulations so far, but will be tested in staging/production the coming months.
In case you're feeling adventurous, always tests migrations before executing on production. Use a tool like [OpenDBDiff](https://github.com/OpenDBDiff/OpenDBDiff) to verify schema changes before and after.