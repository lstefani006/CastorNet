using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public static class Util
{
	public static string F(string fmt, params object [] args)
	{
		return string.Format(CultureInfo.InvariantCulture, fmt, args);
	}

	public static decimal ParseDecimal(string s)
	{
		decimal d = decimal.Parse(s, CultureInfo.InvariantCulture);
		return d;
	}
	public static int ParseInt(string s)
	{
		int d = int.Parse(s, CultureInfo.InvariantCulture);
		return d;
	}
	public static short ParseShort(string s)
	{
		short d = short.Parse(s, CultureInfo.InvariantCulture);
		return (short)d;
	}

	public class Set<T> : IEnumerable<T> where T : IEquatable<T>
	{
		public Set() { _r = new List<T>(); }
		public Set(Set<T> v) { _r = new List<T>(v._r); }

		public void Add(T v)
		{
			if (Contains(v) == false)
				_r.Add(v);
		}
		public bool Contains(T v)
		{
			foreach (var r in _r)
				if (v.Equals(r))
					return true;
			return false;
		}

		public void Remove(T v)
		{
			_r.Remove(v);
		}

		public override bool Equals(object obj)
		{
			Set<T> b = obj as Set<T>;
			if (b == null) return false;
			return this == b;
		}

		public static bool operator ==(Set<T> a, Set<T> b)
		{
			if ((object)a == null && (object)b == null)
				return true;
			if ((object)a == null || (object)b == null)
				return false;

			if (a._r.Count != b._r.Count) 
				return false;
			foreach (var va in a._r)
			{
				bool e = false;
				foreach (var vb in b._r)
					if (va.Equals(vb))
					{
						e = true;
						break;
					}
				if (e == false)
					return false;
			}
			return true;
		}

		public static bool operator !=(Set<T> a, Set<T> b)
		{
			return !(a == b);
		}

		public int Count { get { return _r.Count; } }

		public T this[int i]
		{
			get { return _r[i]; }
		}

		public override int GetHashCode()
		{
			return _r.GetHashCode();
		}

		List<T> _r;


		public IEnumerator<T> GetEnumerator()
		{
			return _r.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _r.GetEnumerator();
		}

		public static Set<T> operator +(Set<T> a, Set<T> b)
		{
			var r = new Set<T>(a);
			foreach (var v in b)
				r.Add(v);
			return r;
		}

		public override string ToString()
		{
			List<T> rr = new List<T>();
			foreach (T v in _r) rr.Add(v);
			rr.Sort();

			string r = "[";
			for (int i = 0; i < rr.Count; ++i)
			{
				if (i > 0) r += ", ";
				r += rr[i].ToString();
			}
			return r + "]";
		}
	}
}
