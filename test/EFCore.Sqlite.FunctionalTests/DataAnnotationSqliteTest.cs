// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationSqliteTest : DataAnnotationTestBase<DataAnnotationSqliteTest.DataAnnotationSqliteFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public DataAnnotationSqliteTest(DataAnnotationSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public override ModelBuilder Non_public_annotations_are_enabled()
        {
            var modelBuilder = base.Non_public_annotations_are_enabled();

            var relational = GetProperty<PrivateMemberAnnotationClass>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Field_annotations_are_enabled()
        {
            var modelBuilder = base.Field_annotations_are_enabled();

            var relational = GetProperty<FieldAnnotationClass>(modelBuilder, "_personFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_column_work_together()
        {
            var modelBuilder = base.Key_and_column_work_together();

            var relational = GetProperty<ColumnKeyAnnotationClass1>(modelBuilder, "PersonFirstName").Relational();
            Assert.Equal("dsdsd", relational.ColumnName);
            Assert.Equal("nvarchar(128)", relational.ColumnType);

            return modelBuilder;
        }

        public override ModelBuilder Key_and_MaxLength_64_produce_nvarchar_64()
        {
            var modelBuilder = base.Key_and_MaxLength_64_produce_nvarchar_64();

            var property = GetProperty<ColumnKeyAnnotationClass2>(modelBuilder, "PersonFirstName");
            Assert.Equal("TEXT", TestServiceFactory.Instance.Create<SqliteTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "MaxTimestamp");
            Assert.Equal("BLOB", TestServiceFactory.Instance.Create<SqliteTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder Timestamp_takes_precedence_over_MaxLength_with_value()
        {
            var modelBuilder = base.Timestamp_takes_precedence_over_MaxLength_with_value();

            var property = GetProperty<TimestampAndMaxlen>(modelBuilder, "NonMaxTimestamp");
            Assert.Equal("BLOB", TestServiceFactory.Instance.Create<SqliteTypeMapper>().FindMapping(property).StoreType);

            return modelBuilder;
        }

        public override ModelBuilder TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = base.TableNameAttribute_affects_table_name_in_TPH();

            var relational = modelBuilder.Model.FindEntityType(typeof(TNAttrBase)).Relational();
            Assert.Equal("A", relational.TableName);

            return modelBuilder;
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Contains(
                @"SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion"", ""r"".""UniqueNo"", ""r"".""Details_Name"", ""r"".""UniqueNo"", ""r"".""AdditionalDetails_Name""" + EOL +
                @"FROM ""Sample"" AS ""r""" + EOL +
                @"WHERE ""r"".""UniqueNo"" = 1" + EOL +
                @"LIMIT 1",
                Sql);

            Assert.Contains(
                @"SELECT ""r"".""UniqueNo"", ""r"".""MaxLengthProperty"", ""r"".""Name"", ""r"".""RowVersion"", ""r"".""UniqueNo"", ""r"".""Details_Name"", ""r"".""UniqueNo"", ""r"".""AdditionalDetails_Name""" + EOL +
                @"FROM ""Sample"" AS ""r""" + EOL +
                @"WHERE ""r"".""UniqueNo"" = 1" + EOL +
                @"LIMIT 1" + EOL +
                EOL +
                @"@p2='1' (DbType = String)" + EOL +
                @"@p0='ModifiedData' (Nullable = false) (Size = 12)" + EOL +
                @"@p1='00000000-0000-0000-0003-000000000001' (DbType = String)" + EOL +
                @"@p3='00000001-0000-0000-0000-000000000001' (DbType = String)" + EOL +
                EOL +
                @"UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1" + EOL +
                @"WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;" + EOL +
                @"SELECT changes();" + EOL +
                EOL +
                @"@p2='1' (DbType = String)" + EOL +
                @"@p0='ChangedData' (Nullable = false) (Size = 11)" + EOL +
                @"@p1='00000000-0000-0000-0002-000000000001' (DbType = String)" + EOL +
                @"@p3='00000001-0000-0000-0000-000000000001' (DbType = String)" + EOL +
                EOL +
                @"UPDATE ""Sample"" SET ""Name"" = @p0, ""RowVersion"" = @p1" + EOL +
                @"WHERE ""UniqueNo"" = @p2 AND ""RowVersion"" = @p3;" + EOL +
                @"SELECT changes();",
                Sql);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Contains(
                @"@p0='' (DbType = String)" + EOL +
                @"@p1='Third' (Nullable = false) (Size = 5)" + EOL +
                @"@p2='00000000-0000-0000-0000-000000000003' (DbType = String)" + EOL +
                @"@p3='Third Additional Name' (Size = 21)" + EOL +
                @"@p4='Third Name' (Size = 10)" + EOL +
                EOL +
                @"INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")" + EOL +
                @"VALUES (@p0, @p1, @p2, @p3, @p4);" + EOL +
                @"SELECT ""UniqueNo""" + EOL +
                @"FROM ""Sample""" + EOL +
                @"WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);
        }

        // Sqlite does not support length
        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(10, context.Model.FindEntityType(typeof(One)).FindProperty("MaxLengthProperty").GetMaxLength());
            }
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Contains(
                @"@p1='1' (DbType = String)" + EOL,
                Sql);

            Assert.Contains(
                @"@p1='' (Nullable = false) (DbType = String)" + EOL,
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Contains(
                @"@p0='' (DbType = String)" + EOL +
                @"@p1='ValidString' (Nullable = false) (Size = 11)" + EOL +
                @"@p2='00000000-0000-0000-0000-000000000001' (DbType = String)" + EOL +
                @"@p3='Two' (Size = 3)" + EOL +
                @"@p4='One' (Size = 3)" + EOL +
                EOL +
                @"INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")" + EOL +
                @"VALUES (@p0, @p1, @p2, @p3, @p4);" + EOL +
                @"SELECT ""UniqueNo""" + EOL +
                @"FROM ""Sample""" + EOL +
                @"WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);

            Assert.Contains(
                @"@p0='' (DbType = String)" + EOL +
                @"@p1='' (Nullable = false) (DbType = String)" + EOL +
                @"@p2='00000000-0000-0000-0000-000000000002' (DbType = String)" + EOL +
                @"@p3='Two' (Size = 3)" + EOL +
                @"@p4='One' (Size = 3)" + EOL +
                EOL +
                @"INSERT INTO ""Sample"" (""MaxLengthProperty"", ""Name"", ""RowVersion"", ""AdditionalDetails_Name"", ""Details_Name"")" + EOL +
                @"VALUES (@p0, @p1, @p2, @p3, @p4);" + EOL +
                @"SELECT ""UniqueNo""" + EOL +
                @"FROM ""Sample""" + EOL +
                @"WHERE changes() = 1 AND ""UniqueNo"" = last_insert_rowid();",
                Sql);
        }

        // Sqlite does not support length
        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(16, context.Model.FindEntityType(typeof(Two)).FindProperty("Data").GetMaxLength());
            }
        }

        // Sqlite does not support rowversion. See issue #2195
        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Model.FindEntityType(typeof(Two)).FindProperty("Timestamp").IsConcurrencyToken);
            }
        }

        private static readonly string EOL = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class DataAnnotationSqliteFixture : DataAnnotationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
