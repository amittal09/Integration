using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Database
{
	internal class DbSession : IDbSession, IDisposable
	{
		private readonly IDbConnection _connection;

		private readonly Stack<IDbTransaction> _transactions;

		public IDbConnection Connection
		{
			get
			{
				return this._connection;
			}
		}

		public IDbTransaction CurrentTransaction
		{
			get
			{
				if (this._transactions.Count <= 0)
				{
					return null;
				}
				return this._transactions.Peek();
			}
		}

		public DbSession(IDbConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this._connection = connection;
			this._connection.Open();
			this._transactions = new Stack<IDbTransaction>(3);
		}

		public IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = null)
		{
			IDbTransaction dbTransaction = (isolationLevel.HasValue ? this._connection.BeginTransaction(isolationLevel.Value) : this._connection.BeginTransaction());
			this._transactions.Push(dbTransaction);
			return new DbSession.TransactionScope(dbTransaction, () => this._transactions.Pop());
		}

		public virtual void Dispose()
		{
			this._connection.Dispose();
		}

		public int Execute(string sql, dynamic param = null)
		{
			return (int)SqlMapper.Execute(this._connection, sql, param, this.CurrentTransaction);
		}

		public T ExecuteScalar<T>(string sql, dynamic param = null)
		{
			return (T)SqlMapper.ExecuteScalar<T>(this._connection, sql, param, this.CurrentTransaction);
		}

		public IEnumerable<dynamic> Query(string sql, dynamic param = null)
		{
			return (IEnumerable<object>)SqlMapper.Query(this._connection, sql, param, this.CurrentTransaction);
		}

		public IEnumerable<T> Query<T>(string sql, dynamic param = null)
		{
			return (IEnumerable<T>)SqlMapper.Query<T>(this._connection, sql, param, this.CurrentTransaction);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id")
		{
			int? nullable = null;
			CommandType? nullable1 = null;
			return this._connection.Query<TFirst, TSecond, TReturn>(sql, map, param, this.CurrentTransaction, true, splitOn, nullable, nullable1);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id")
		{
			int? nullable = null;
			CommandType? nullable1 = null;
			return this._connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, this.CurrentTransaction, true, splitOn, nullable, nullable1);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = "Id")
		{
			int? nullable = null;
			CommandType? nullable1 = null;
			return this._connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, this.CurrentTransaction, true, splitOn, nullable, nullable1);
		}

		public SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null)
		{
			return (SqlMapper.GridReader)SqlMapper.QueryMultiple(this._connection, sql, param, this.CurrentTransaction);
		}

		private class TransactionScope : IDbTransaction, IDisposable
		{
			private readonly IDbTransaction _transaction;

			private readonly Action _beforeDispose;

			public IDbConnection Connection
			{
				get
				{
					return this._transaction.Connection;
				}
			}

			public IsolationLevel IsolationLevel
			{
				get
				{
					return this._transaction.IsolationLevel;
				}
			}

			public TransactionScope(IDbTransaction transaction, Action beforeDispose)
			{
				this._transaction = transaction;
				this._beforeDispose = beforeDispose;
			}

			public void Commit()
			{
				this._transaction.Commit();
			}

			public void Dispose()
			{
				this._beforeDispose();
				this._transaction.Dispose();
			}

			public void Rollback()
			{
				this._transaction.Rollback();
			}
		}
	}
}