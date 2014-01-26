using System; 
using System.Collections;
using System.Data.SqlClient;

namespace CodeGen 
{ 
	[Serializable]
	public class DbInstList : IEnumerable
	{
		public DbInstList() { _a = new ArrayList(); }
		public DbInstList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbInst e in c) this.Add(e);
		}

		public DbInst this [int i]
		{
			get { return (DbInst)_a[i]; }
			set { _a[i] = value; }
		}

		public DbInst this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbInst) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbInst value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbInst value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbInst value) { _a.Insert(index, value); }
		public int IndexOf(DbInst value) { return _a.IndexOf(value); }
		public int IndexOf(DbInst value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbInst value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbInst item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].Name))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbInst [] ToTypedArray() { return (DbInst []) _a.ToArray(typeof(DbInst)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbInstListEnumerator(this); }
		public DbInstListEnumerator GetEnumerator() { return new DbInstListEnumerator(this); }

		public class DbInstListEnumerator : IEnumerator, IDisposable
		{
			public DbInstListEnumerator(DbInstList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbInst Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbInstList _t;
		}
		#endregion

		private ArrayList _a;
	}


	[Serializable]
	public class DbTableList : IEnumerable
	{
		public DbTableList() { _a = new ArrayList(); }
		public DbTableList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbTable e in c) this.Add(e);
		}

		public DbTable this [int i]
		{
			get { return (DbTable)_a[i]; }
			set { _a[i] = value; }
		}

		public DbTable this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbTable) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbTable value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbTable value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbTable value) { _a.Insert(index, value); }
		public int IndexOf(DbTable value) { return _a.IndexOf(value); }
		public int IndexOf(DbTable value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbTable value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbTable item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].Name))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbTable [] ToTypedArray() { return (DbTable []) _a.ToArray(typeof(DbTable)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbTableListEnumerator(this); }
		public DbTableListEnumerator GetEnumerator() { return new DbTableListEnumerator(this); }

		public class DbTableListEnumerator : IEnumerator, IDisposable
		{
			public DbTableListEnumerator(DbTableList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbTable Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbTableList _t;
		}
		#endregion

		private ArrayList _a;
	}


	[Serializable]
	public class DbViewList : IEnumerable
	{
		public DbViewList() { _a = new ArrayList(); }
		public DbViewList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbView e in c) this.Add(e);
		}

		public DbView this [int i]
		{
			get { return (DbView)_a[i]; }
			set { _a[i] = value; }
		}

		public DbView this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbView) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbView value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbView value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbView value) { _a.Insert(index, value); }
		public int IndexOf(DbView value) { return _a.IndexOf(value); }
		public int IndexOf(DbView value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbView value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbView item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].Name))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbView [] ToTypedArray() { return (DbView []) _a.ToArray(typeof(DbView)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbViewListEnumerator(this); }
		public DbViewListEnumerator GetEnumerator() { return new DbViewListEnumerator(this); }

		public class DbViewListEnumerator : IEnumerator, IDisposable
		{
			public DbViewListEnumerator(DbViewList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbView Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbViewList _t;
		}
		#endregion

		private ArrayList _a;
	}


	[Serializable]
	public class DbStoredProcedureList : IEnumerable
	{
		public DbStoredProcedureList() { _a = new ArrayList(); }
		public DbStoredProcedureList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbStoredProcedure e in c) this.Add(e);
		}

		public DbStoredProcedure this [int i]
		{
			get { return (DbStoredProcedure)_a[i]; }
			set { _a[i] = value; }
		}

		public DbStoredProcedure this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbStoredProcedure) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbStoredProcedure value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbStoredProcedure value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbStoredProcedure value) { _a.Insert(index, value); }
		public int IndexOf(DbStoredProcedure value) { return _a.IndexOf(value); }
		public int IndexOf(DbStoredProcedure value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbStoredProcedure value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbStoredProcedure item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].Name))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbStoredProcedure [] ToTypedArray() { return (DbStoredProcedure []) _a.ToArray(typeof(DbStoredProcedure)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbStoredProcedureListEnumerator(this); }
		public DbStoredProcedureListEnumerator GetEnumerator() { return new DbStoredProcedureListEnumerator(this); }

		public class DbStoredProcedureListEnumerator : IEnumerator, IDisposable
		{
			public DbStoredProcedureListEnumerator(DbStoredProcedureList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbStoredProcedure Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbStoredProcedureList _t;
		}
		#endregion

		private ArrayList _a;
	}


	[Serializable]
	public class SqlParameterList : IEnumerable
	{
		public SqlParameterList() { _a = new ArrayList(); }
		public SqlParameterList(ICollection c)
		{
			_a = new ArrayList();
			foreach (SqlParameter e in c) this.Add(e);
		}

		public SqlParameter this [int i]
		{
			get { return (SqlParameter)_a[i]; }
			set { _a[i] = value; }
		}

		public SqlParameter this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (SqlParameter) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(SqlParameter value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(SqlParameter value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, SqlParameter value) { _a.Insert(index, value); }
		public int IndexOf(SqlParameter value) { return _a.IndexOf(value); }
		public int IndexOf(SqlParameter value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(SqlParameter value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(SqlParameter item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].ParameterName))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public SqlParameter [] ToTypedArray() { return (SqlParameter []) _a.ToArray(typeof(SqlParameter)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new SqlParameterListEnumerator(this); }
		public SqlParameterListEnumerator GetEnumerator() { return new SqlParameterListEnumerator(this); }

		public class SqlParameterListEnumerator : IEnumerator, IDisposable
		{
			public SqlParameterListEnumerator(SqlParameterList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public SqlParameter Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private SqlParameterList _t;
		}
		#endregion

		private ArrayList _a;
	}


	[Serializable]
	public class DbCommandResultList : IEnumerable
	{
		public DbCommandResultList() { _a = new ArrayList(); }
		public DbCommandResultList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbCommandResult e in c) this.Add(e);
		}

		public DbCommandResult this [int i]
		{
			get { return (DbCommandResult)_a[i]; }
			set { _a[i] = value; }
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbCommandResult value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbCommandResult value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbCommandResult value) { _a.Insert(index, value); }
		public int IndexOf(DbCommandResult value) { return _a.IndexOf(value); }
		public int IndexOf(DbCommandResult value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbCommandResult value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbCommandResult item) { return _a.Contains(item); }

		public DbCommandResult [] ToTypedArray() { return (DbCommandResult []) _a.ToArray(typeof(DbCommandResult)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbCommandResultListEnumerator(this); }
		public DbCommandResultListEnumerator GetEnumerator() { return new DbCommandResultListEnumerator(this); }

		public class DbCommandResultListEnumerator : IEnumerator, IDisposable
		{
			public DbCommandResultListEnumerator(DbCommandResultList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbCommandResult Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbCommandResultList _t;
		}
		#endregion

		private ArrayList _a;
	}

	[Serializable]
	public class DbColumnList : IEnumerable
	{
		public DbColumnList() { _a = new ArrayList(); }
		public DbColumnList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbColumn e in c) this.Add(e);
		}

		public DbColumn this [int i]
		{
			get { return (DbColumn)_a[i]; }
			set { _a[i] = value; }
		}

		public DbColumn this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbColumn) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbColumn value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbColumn value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbColumn value) { _a.Insert(index, value); }
		public int IndexOf(DbColumn value) { return _a.IndexOf(value); }
		public int IndexOf(DbColumn value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbColumn value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbColumn item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].ColumnName))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbColumn [] ToTypedArray() { return (DbColumn []) _a.ToArray(typeof(DbColumn)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbColumnListEnumerator(this); }
		public DbColumnListEnumerator GetEnumerator() { return new DbColumnListEnumerator(this); }

		public class DbColumnListEnumerator : IEnumerator, IDisposable
		{
			public DbColumnListEnumerator(DbColumnList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbColumn Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbColumnList _t;
		}
		#endregion

		private ArrayList _a;
	}



	[Serializable]
	public class DbColumnDataList : IEnumerable
	{
		public DbColumnDataList() { _a = new ArrayList(); }
		public DbColumnDataList(ICollection c)
		{
			_a = new ArrayList();
			foreach (DbColumnData e in c) this.Add(e);
		}

		public DbColumnData this [int i]
		{
			get { return (DbColumnData)_a[i]; }
			set { _a[i] = value; }
		}

		public DbColumnData this [string v]
		{
			get 
			{ 
				int i = IndexOf(v);
				if (i >= 0) return (DbColumnData) _a[i];
				throw new ArgumentException("Cannot find value in collection", "v");				
			}
		}

		public int Count { get { return _a.Count; } }
		public void Add(DbColumnData value) { _a.Add(value); }
		public void Clear() { _a.Clear(); }
		public void Remove(DbColumnData value) { _a.Remove(value); }
		public void RemoveAt(int index) { _a.RemoveAt(index); }
		public void Insert(int index, DbColumnData value) { _a.Insert(index, value); }
		public int IndexOf(DbColumnData value) { return _a.IndexOf(value); }
		public int IndexOf(DbColumnData value, int startIndex) { return _a.IndexOf(value, startIndex); }
		public int IndexOf(DbColumnData value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
		public bool Contains(DbColumnData item) { return _a.Contains(item); }

		public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
		public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
		public int IndexOf(string item, int index, int count)
		{
			int n = (_a.Count < index + count) ? _a.Count : index + count;
			for (int i = index; i < n; ++i)
				if (item.Equals(this[i].Name))
					return i;
			return -1;
		}
		public bool Contains(string item) { return IndexOf(item) >= 0; }
		public void Remove(string value)
		{
			int i = IndexOf(value);
			if (i < 0) throw new ArgumentException("value not found on collection", "value");
			_a.RemoveAt(i);
		}
		public DbColumnData [] ToTypedArray() { return (DbColumnData []) _a.ToArray(typeof(DbColumnData)); }

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator() { return new DbColumnDataListEnumerator(this); }
		public DbColumnDataListEnumerator GetEnumerator() { return new DbColumnDataListEnumerator(this); }

		public class DbColumnDataListEnumerator : IEnumerator, IDisposable
		{
			public DbColumnDataListEnumerator(DbColumnDataList t) { _t = t; _i = -1; }
			public bool MoveNext() { return ++_i < _t.Count; }
			public DbColumnData Current { get { return _t[_i]; } }
			object IEnumerator.Current { get { return _t[_i]; } }
			public void Reset() { _i = -1; }
			public void Dispose() { _t = null; }

			private int _i;
			private DbColumnDataList _t;
		}
		#endregion

		private ArrayList _a;
	}

}

