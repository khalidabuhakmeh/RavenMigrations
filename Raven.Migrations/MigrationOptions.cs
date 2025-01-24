using System;
using System.Collections.Generic;
using System.Reflection;

namespace Raven.Migrations
{
    public class MigrationOptions
    {
        public MigrationOptions()
            : this(new SimpleMigrationResolver())
        {
        }

        public MigrationOptions(IMigrationResolver migrationResolver)
        {
            Direction = Directions.Up;
            Assemblies = new List<Assembly>();
            Profiles = new List<string>();
            MigrationResolver = migrationResolver;
            Assemblies = new List<Assembly>();
            ToVersion = 0;
            Conventions = new MigrationConventions();
            MigrationResolver = migrationResolver;
            PreventSimultaneousMigrations = true;
            SimultaneousMigrationTimeout = TimeSpan.FromHours(1);
        }

        public string? Database { get; set; }
        public Directions Direction { get; set; }
        public IList<Assembly> Assemblies { get; set; }
        public IList<string> Profiles { get; set; }
        public IMigrationResolver MigrationResolver { get; set; }
        public long ToVersion { get; set; }
        public MigrationConventions Conventions { get; set; }
        public IMigrationRecordStore? MigrationRecordStore { get; set; }
        
        /// <summary>
        /// Whether to prevent simultaenous migrations (e.g. from other web app instances) from running.
        /// Defaults to true. 
        /// If enabled, a Raven compare-exchange value will be set during migrations. The compare-exchange value will be deleted when migrations complete. 
        /// Additional attempts to run migration will fail while this compare-exchange value exists.
        /// </summary>
        public bool PreventSimultaneousMigrations { get; set; }

        /// <summary>
        /// If <see cref="PreventSimultaneousMigrations"/> is enabled, any attempt to run migrations during this time will fail.
        /// </summary>
        public TimeSpan SimultaneousMigrationTimeout { get; set; }

        /// <summary>
        /// What to do when migration lock cannot be obtained. Default behavior is to skip running migrations.
        /// </summary>
        public SimultaneousMigrationBehavior SimultaneousMigrationBehavior { get; set; } = SimultaneousMigrationBehavior.Skip;
    }

    /// <summary>
    /// Behavior options when attempt to run simultaneous migrations.
    /// </summary>
    public enum SimultaneousMigrationBehavior
    {
        /// <summary>
        /// Skip migration and log warning-
        /// </summary>
        Skip,

        /// <summary>
        /// Throw an exception and effectively cancel the migration attempt.
        /// </summary>
        Error
    }
}