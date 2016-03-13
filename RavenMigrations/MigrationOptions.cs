﻿using System.Collections.Generic;
using System.Reflection;

namespace RavenMigrations
{
    public class MigrationOptions
    {
        public MigrationOptions()
        {
            Direction = Directions.Up;
            Assemblies = new List<Assembly>();
            Profiles = new List<string>();
            MigrationResolver = new DefaultMigrationResolver();
            Assemblies = new List<Assembly>();
            ToVersion = 0;
            ConvertToDocumentId = RavenMigrationHelpers.GetMigrationIdFromName;
        }

        public Directions Direction { get; set; }
        public IList<Assembly> Assemblies { get; set; }
        public IList<string> Profiles { get; set; }
        public IMigrationResolver MigrationResolver { get; set; }
        public long ToVersion { get; set; }
        public MigrationToDocumentIdConversion ConvertToDocumentId { get; set; }
    }
}