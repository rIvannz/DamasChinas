using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Damas_Chinas_Server;
using Damas_Chinas_Server.Dtos;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Damas_Chinas_Server.Tests.Logica
{
	[TestClass]
	public class FriendRepositoryTests
	{
		[TestMethod]
		public void GetFriends_ReturnsEmptyList_WhenNoFriends()
		{
			var mockDb = new Mock<damas_chinasEntities>();
			mockDb.Setup(db => db.amistades).Returns(new TestDbSet<amistades>());
			var repo = new FriendRepositoryTestable(mockDb.Object);

			var result = repo.GetFriends(1);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void AgregarAmistad_ThrowsException_WhenSameUser()
		{
			var repo = new FriendRepository();

			Assert.ThrowsException<Exception>(() => repo.AgregarAmistad(1, 1));
		}

		[TestMethod]
		public void AgregarAmistad_ThrowsException_WhenUserDoesNotExist()
		{
			var mockDb = new Mock<damas_chinasEntities>();
			mockDb.Setup(db => db.usuarios).Returns(new TestDbSet<usuarios>());
			var repo = new FriendRepositoryTestable(mockDb.Object);

			Assert.ThrowsException<Exception>(() => repo.AgregarAmistad(1, 2));
		}

		[TestMethod]
		public void EliminarAmistad_ThrowsException_WhenFriendshipDoesNotExist()
		{
			var mockDb = new Mock<damas_chinasEntities>();
			mockDb.Setup(db => db.amistades).Returns(new TestDbSet<amistades>());
			var repo = new FriendRepositoryTestable(mockDb.Object);

			Assert.ThrowsException<Exception>(() => repo.EliminarAmistad(1, 2));
		}
	}

	// Clase de prueba para inyectar el contexto simulado
	public class FriendRepositoryTestable : FriendRepository
	{
		private readonly damas_chinasEntities _db;
		public FriendRepositoryTestable(damas_chinasEntities db)
		{
			_db = db;
		}
		protected override damas_chinasEntities CreateDbContext()
		{
			return _db;
		}
	}

	// Implementación mínima de DbSet para pruebas
	public class TestDbSet<T> : List<T>, IQueryable<T>
	{
		public Type ElementType => typeof(T);
		public System.Linq.Expressions.Expression Expression => this.AsQueryable().Expression;
		public IQueryProvider Provider => this.AsQueryable().Provider;
	}
}
