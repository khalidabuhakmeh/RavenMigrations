﻿using System.Linq;
using System.Text;
using FluentAssertions;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;
using RavenMigrations.Migrations;
using Xunit;

namespace RavenMigrations.Tests
{
    public class PatchMigrationTests : RavenTestBase
    {
        [Fact]
        public void Patch_runs_only_for_the_given_entity_type()
        {
            var collector = new AttributeBasedMigrationCollector(new DefaultMigrationResolver(),
                () => new[] {typeof (CreateDocument), typeof (PatchDocument)});

            using (var store = NewDocumentStore())
            {
                Runner.Run(store, migrationCollector: collector);
                using (var session = store.OpenSession())
                {
                    var sampleDocument = session.Load<SampleDoc>("sample-document");
                    sampleDocument.Name.Should().Be("woot patched");
                    var otherSampleDocument = session.Load<OtherSampleDoc>("other-sample-document");
                    otherSampleDocument.Name.Should().Be("woot");
                }
            }
        }

        [Fact]
        public void Bad_patches_should_cause_errors()
        {
            var collector = new AttributeBasedMigrationCollector(new DefaultMigrationResolver(),
                () => new[] {typeof (CreateDocument), typeof (BlowUp)});

            using (var store = NewDocumentStore())
            {
                Runner.Run(store, migrationCollector: collector);
                using (var session = store.OpenSession())
                {
                    var migration = collector.GetOrderedMigrations(new string[] {}).Last();

                    var sampleDocument = session.Load<MigrationDocument>(migration.GetMigrationId());
                    Assert.True(sampleDocument.HasError);
                }
            }
        }

        [Fact]
        public void Can_run_patch_down()
        {
            var collector = new AttributeBasedMigrationCollector(new DefaultMigrationResolver(),
                   () => new[] { typeof(CreateDocument), typeof(PatchDocument) });

            using (var store = NewDocumentStore())
            {
                Runner.Run(store, migrationCollector: collector);
                Runner.Run(store, new MigrationOptions() {Direction = Directions.Down}, collector);
                using (var session = store.OpenSession())
                {
                    var sampleDocument = session.Load<SampleDoc>("sample-document");
                    sampleDocument.Name.Should().Be("woot");
                    var otherSampleDocument = session.Load<OtherSampleDoc>("other-sample-document");
                    otherSampleDocument.Name.Should().Be("woot");
                }
            }
        }        
        
        [Fact]
        public void Can_run_patch_on_index()
        {
            var collector = new AttributeBasedMigrationCollector(new DefaultMigrationResolver(),
                   () => new[] { typeof(PatchByIndex) });

            using (var store = NewDocumentStore())
            {
                new SampleDocIndex().Execute(store);
                using (var session = store.OpenSession())
                {
                    session.Store(new SampleDoc{Id = "first-doc", Name ="Ali baba"});
                    session.Store(new SampleDoc{Id = "second-doc", Name ="Aqui baba"});
                    session.Store(new SampleDoc{Id = "third-doc", Name ="Ali bebe"});
                    session.SaveChanges();
                }

                Runner.Run(store, migrationCollector: collector);
                using (var session = store.OpenSession())
                {
                    session.Load<SampleDoc>("first-doc")
                        .Name.Should().Be("Ali baba patched");
                    session.Load<SampleDoc>("second-doc")
                        .Name.Should().Be("Aqui baba");
                    session.Load<SampleDoc>("third-doc")
                        .Name.Should().Be("Ali bebe patched");
                }
            }
        }
    }



    internal class SampleDoc
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    internal class OtherSampleDoc
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    class SampleDocIndex : AbstractIndexCreationTask<SampleDoc>
    {
        public SampleDocIndex()
        {
            Map = docs => from doc in docs
                select new
                {
                    doc.Name
                };
            Index(doc => doc.Name, FieldIndexing.Analyzed);
        }
    }

    [Migration(1)]
    internal class CreateDocument : Migration
    {
        public override void Up()
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(new SampleDoc {Id = "sample-document", Name = "woot"});
                session.Store(new OtherSampleDoc {Id = "other-sample-document", Name = "woot"});
                session.SaveChanges();
            }
        }
    }

    [Migration(2)]
    internal class PatchDocument : CollectionPatchMigration<SampleDoc>
    {
        public override string UpPatch
        {
            get { return @"
this.Name = this.Name + ' patched';
"; }
        }

        public override string DownPatch
        {
            get { return @"
this.Name = this.Name.replace(' patched','');
"; }
        }
    }
    
    [Migration(4)]
    internal class BlowUp : CollectionPatchMigration<SampleDoc>
    {
        public override string UpPatch
        {
            get { return @"
WillBlowUp().Something();
"; }
        }
    }

    [Migration(3)]
    internal class PatchByIndex : IndexPatchMigration<SampleDocIndex>
    {
        public override string UpPatch
        {
            get { return @"
this.Name = this.Name + ' patched'"; }
        }

        protected override string Query
        {
            get { return "Name:Ali*"; }
        }
    }
}