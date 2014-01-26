using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Utility
{
	public static class U2
	{
		public static string F(string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}

		public static T? ReadNullable<T>(DataRow dr, string field) where T : struct
		{
			if (dr.IsNull(field))
				return null;

			return (T?)dr[field];
		}
		public static string ReadString(DataRow dr, string field)
		{
			if (dr.IsNull(field))
				return null;

			return (string)dr[field];
		}
		public static T Read<T>(DataRow dr, string columnName) where T : struct
		{
			return (T)dr[columnName];
		}
		public static int? ConvertToNullableInt32(decimal? d)
		{
			if (d.HasValue == false) return null;
			return (int)d.Value;
		}


		public static DataRow[] SelectDistinct(DataTable dt, string column, string expression)
		{
			List<DataRow> ret = new List<DataRow>();

			foreach (DataRow dr in dt.Select(expression))
			{
				if (dr.IsNull(column))
					continue;

				IComparable comparableValue = dr[column] as IComparable;
				foreach (DataRow row in ret)
				{
					if (comparableValue.CompareTo(row[column]) == 0)
						goto found;
				}

				ret.Add(dr);

			found: ;
			}

			return ret.ToArray();
		}




		public delegate bool SameGroup<T>(T a, T b);
		public static IEnumerable<List<T>> Group<T>(IEnumerable<T> l, SameGroup<T> sameGroup)
		{
			List<T> ret = new List<T>();

			T last = default(T);

			foreach (T t in l)
			{
				if (ret.Count > 0 && sameGroup(last, t) == false)
				{
					yield return ret;
					ret = new List<T>();
				}

				ret.Add(t);

				last = t;
			}

			if (ret.Count > 0)
				yield return ret;
		}
		public static IEnumerable<List<T>> Group<T>(IEnumerable l, SameGroup<T> sameGroup)
		{
			List<T> ret = new List<T>();

			T last = default(T);

			foreach (T t in l)
			{
				if (ret.Count > 0 && sameGroup(last, t) == false)
				{
					yield return ret;
					ret = new List<T>();
				}

				ret.Add(t);

				last = t;
			}

			if (ret.Count > 0)
				yield return ret;
		}

		public interface INamedObject
		{
			string ObjectName { get; }
		}


		[Serializable]
		public class NamedList<T> : IEnumerable<T>
			where T : INamedObject
		{
			public NamedList() { _a = new List<T>(); }
			public NamedList(List<T> c) { _a = new List<T>(); foreach (T e in c) this.Add(e); }

			public T this[int i] { get { return _a[i]; } set { _a[i] = value; } }

			public T this[string v]
			{
				get
				{
					int i = IndexOf(v);
					if (i >= 0) return _a[i];
					throw new ArgumentException("Cannot find value in collection", "v");
				}
			}

			public int Count { get { return _a.Count; } }
			public void Add(T value) { _a.Add(value); }
			public void Clear() { _a.Clear(); }
			public void Remove(T value) { _a.Remove(value); }
			public void RemoveAt(int index) { _a.RemoveAt(index); }
			public void Insert(int index, T value) { _a.Insert(index, value); }
			public int IndexOf(T value) { return _a.IndexOf(value); }
			public int IndexOf(T value, int startIndex) { return _a.IndexOf(value, startIndex); }
			public int IndexOf(T value, int startIndex, int count) { return _a.IndexOf(value, startIndex, count); }
			public bool Contains(T item) { return _a.Contains(item); }

			public int IndexOf(string item) { return IndexOf(item, 0, this.Count); }
			public int IndexOf(string item, int index) { return IndexOf(item, index, this.Count); }
			public int IndexOf(string item, int index, int count)
			{
				int n = (_a.Count < index + count) ? _a.Count : index + count;
				for (int i = index; i < n; ++i)
					if (item.Equals(this[i].ObjectName))
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
			public T[] ToArray() { return _a.ToArray(); }
			
			public bool IsLast(T v)
			{
				if (_a.Count == 0)
					return false;
				
				return _a[_a.Count - 1].ObjectName == v.ObjectName;
			}

			#region IEnumerable

			public IEnumerator<T> GetEnumerator() { return new snNamedListEnumerator(this); }
			IEnumerator IEnumerable.GetEnumerator() { return new snNamedListEnumerator(this); }


			public class snNamedListEnumerator : IEnumerator<T>, IDisposable
			{
				public snNamedListEnumerator(NamedList<T> t) { _t = t; _i = -1; }
				public bool MoveNext() { return ++_i < _t.Count; }
				public T Current { get { return _t[_i]; } }
				object IEnumerator.Current { get { return _t[_i]; } }
				T IEnumerator<T>.Current { get { return _t[_i]; } }
				public void Reset() { _i = -1; }
				public void Dispose() { _t = null; }

				private int _i;
				private NamedList<T> _t;
			}
			#endregion

			private List<T> _a;

		}

	}
}
