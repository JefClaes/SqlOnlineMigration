using System;
using System.Threading.Tasks;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public class SchemaMigrationBuilder
    {
        private readonly string _connectionstring;
        private readonly string _database;

        private SmoDatabaseOperationsSettings _settings;
        private INamingConventions _namingConventions;
        private ILogger _logger;
        private SwapWrapper _swapWrapper;
        private bool _synchronizeData;

        public SchemaMigrationBuilder(string connectionstring, string database)
        {
            _connectionstring = connectionstring;
            _database = database;

            _namingConventions = new DefaultNamingConventions();
            _logger = new NopLogger();
            _swapWrapper = async swap => await swap();
            _settings = new SmoDatabaseOperationsSettings(10000, TimeSpan.Zero, 0);
            _synchronizeData = true;
        }
            
        public SchemaMigrationBuilder WithSwapWrappedIn(SwapWrapper swapWrapper)
        {
            _swapWrapper = swapWrapper;

            return this;
        }

        public SchemaMigrationBuilder WithNoSwap()
        {
            _swapWrapper = _ => Task.CompletedTask;

            return this;
        }

        public SchemaMigrationBuilder WithLogger(ILogger logger)
        {
            _logger = logger;

            return this;
        }

        public SchemaMigrationBuilder WithSettings(SmoDatabaseOperationsSettings settings)
        {
            _settings = settings;

            return this;
        }

        public SchemaMigrationBuilder WithoutDataSynchronization()
        {
            _synchronizeData = false;

            return this;
        }
     
        public SchemaMigration Build()
        {
            return new SchemaMigration(
                new SmoDatabaseOperations(_connectionstring, _database, _namingConventions, _logger, _settings),
                _namingConventions,
                _swapWrapper,
                _synchronizeData);
        }
    }
}
