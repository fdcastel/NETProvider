﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Tests.EndToEnd
{
	public class InsertTests : EntityFrameworkCoreTestsBase
	{
		class InsertContext : FbTestDbContext
		{
			public InsertContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnTestModelCreating(ModelBuilder modelBuilder)
			{
				base.OnTestModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<InsertEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID");
				insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
				insertEntityConf.ToTable("TEST_INSERT");
			}
		}
		class InsertEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
		[Test]
		public void Insert()
		{
			using (var db = GetDbContext<InsertContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_insert (id int primary key, name varchar(20))");
				var entity = new InsertEntity() { Id = -6, Name = "foobar" };
				db.Add(entity);
				db.SaveChanges();
				Assert.AreEqual(-6, entity.Id);
			}
		}

		class IdentityInsertContext : FbTestDbContext
		{
			public IdentityInsertContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnTestModelCreating(ModelBuilder modelBuilder)
			{
				base.OnTestModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<IdentityInsertEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID")
					.UseFirebirdIdentityColumn();
				insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
				insertEntityConf.ToTable("TEST_INSERT_IDENTITY");
			}
		}
		class IdentityInsertEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
		[Test]
		public void IdentityInsert()
		{
			if (!EnsureVersion(new Version("3.0.0.0")))
				return;

			using (var db = GetDbContext<IdentityInsertContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_insert_identity (id int generated by default as identity (start with 26) primary key, name varchar(20))");
				var entity = new IdentityInsertEntity() { Name = "foobar" };
				db.Add(entity);
				db.SaveChanges();
				Assert.AreEqual(27, entity.Id);
			}
		}

		class SequenceInsertContext : FbTestDbContext
		{
			public SequenceInsertContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnTestModelCreating(ModelBuilder modelBuilder)
			{
				base.OnTestModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<SequenceInsertEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID")
					.UseFirebirdSequenceTrigger();
				insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
				insertEntityConf.ToTable("TEST_INSERT_SEQUENCE");
			}
		}
		class SequenceInsertEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
		[Test]
		public void SequenceInsert()
		{
			using (var db = GetDbContext<SequenceInsertContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_insert_sequence (id int primary key, name varchar(20))");
				db.Database.ExecuteSqlCommand("create sequence seq_test_insert_sequence");
				db.Database.ExecuteSqlCommand("alter sequence seq_test_insert_sequence restart with 30");
				db.Database.ExecuteSqlCommand("create trigger test_insert_sequence_id before insert on test_insert_sequence as begin if (new.id is null) then begin new.id = next value for seq_test_insert_sequence; end end");
				var entity = new SequenceInsertEntity() { Name = "foobar" };
				db.Add(entity);
				db.SaveChanges();
				Assert.AreEqual(31, entity.Id);
			}
		}

		class DefaultValuesInsertContext : FbTestDbContext
		{
			public DefaultValuesInsertContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnTestModelCreating(ModelBuilder modelBuilder)
			{
				base.OnTestModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<DefaultValuesInsertEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID")
					.ValueGeneratedOnAdd();
				insertEntityConf.Property(x => x.Name).HasColumnName("NAME")
					.ValueGeneratedOnAdd();
				insertEntityConf.ToTable("TEST_INSERT_DEVAULTVALUES");
			}
		}
		class DefaultValuesInsertEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
		[Test]
		public void DefaultValuesInsert()
		{
			if (!EnsureVersion(new Version("3.0.0.0")))
				return;

			using (var db = GetDbContext<DefaultValuesInsertContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_insert_devaultvalues (id int generated by default as identity (start with 26) primary key, name generated always as (id || 'foobar'))");
				var entity = new DefaultValuesInsertEntity() { };
				db.Add(entity);
				db.SaveChanges();
				Assert.AreEqual(27, entity.Id);
				Assert.AreEqual("27foobar", entity.Name);
			}
		}

		class TwoComputedInsertContext : FbTestDbContext
		{
			public TwoComputedInsertContext(string connectionString)
				: base(connectionString)
			{ }

			protected override void OnTestModelCreating(ModelBuilder modelBuilder)
			{
				base.OnTestModelCreating(modelBuilder);

				var insertEntityConf = modelBuilder.Entity<TwoComputedInsertEntity>();
				insertEntityConf.Property(x => x.Id).HasColumnName("ID")
					.UseFirebirdIdentityColumn();
				insertEntityConf.Property(x => x.Name).HasColumnName("NAME");
				insertEntityConf.Property(x => x.Computed1).HasColumnName("COMPUTED1")
					.ValueGeneratedOnAddOrUpdate();
				insertEntityConf.Property(x => x.Computed2).HasColumnName("COMPUTED2")
					.ValueGeneratedOnAddOrUpdate();
				insertEntityConf.ToTable("TEST_INSERT_2COMPUTED");
			}
		}
		class TwoComputedInsertEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Computed1 { get; set; }
			public string Computed2 { get; set; }
		}
		[Test]
		public void TwoComputedInsert()
		{
			if (!EnsureVersion(new Version("3.0.0.0")))
				return;

			using (var db = GetDbContext<TwoComputedInsertContext>())
			{
				db.Database.ExecuteSqlCommand("create table test_insert_2computed (id int generated by default as identity (start with 26) primary key, name varchar(20), computed1 generated always as ('1' || name), computed2 generated always as ('2' || name))");
				var entity = new TwoComputedInsertEntity() { Name = "foobar" };
				db.Add(entity);
				db.SaveChanges();
				Assert.AreEqual("1foobar", entity.Computed1);
				Assert.AreEqual("2foobar", entity.Computed2);
			}
		}
	}
}
