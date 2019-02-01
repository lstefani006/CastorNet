using CastorNet.Details;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CastorNet
{
	public abstract partial class R
	{
		public class InvalidDeref : Exception { }

		public static R operator |(R a, R b) { return new relationOr(a, b); }
		public static R operator &(R a, R b) { return new relationAnd(a, b); }
		public static R operator ^(R a, R b) { return new relationXor(a, b); }

		public static R eq<T>(Ref<T> a, Ref<T> b) where T : IComparable<T> { return new relationEq<T>(a, b); }
		public static R eq<T>(Ref<T> a, T b) where T : IComparable<T> { return new relationEq<T>(a, new Ref<T>(b)); }
		public static R eq<T>(T a, Ref<T> b) where T : IComparable<T> { return new relationEq<T>(new Ref<T>(a), b); }
		public static R eq<T>(Ref<ComparableList<T>> a, Ref<ComparableList<T>> b) where T : IComparable<T> { return new relationEq<ComparableList<T>>(a, b); }
		public static R eq<T>(ComparableList<T> a, Ref<ComparableList<T>> b) where T : IComparable<T> { return new relationEq<ComparableList<T>>(new Ref<ComparableList<T>>(a), b); }
		public static R eq<T>(Ref<ComparableList<T>> a, ComparableList<T> b) where T : IComparable<T> { return new relationEq<ComparableList<T>>(a, new Ref<ComparableList<T>>(b)); }
		public static R @is<T>(Ref<T> a, Func<T> f) where T : IComparable<T> { return new relationEqIs<T>(a, f); }

		public static R pred(Func<bool> f) { return new relationPredicate(f); }
		public static R defined<T>(Ref<T> a) where T : IComparable<T> { return pred(() => a.Defined()); }
		public static R not_defined<T>(Ref<T> a) where T : IComparable<T> { return pred(() => !a.Defined()); }

		public static R fail() { return new relationFail(); }
		public static R cut() { return new relationCut(); }

		public static R empty<T>(Ref<ComparableList<T>> list) where T : IComparable<T> { return new relationEmpty<T>(list); }
		public static R head<T>(Ref<ComparableList<T>> list, Ref<T> _head) where T : IComparable<T> { return new relationHead<T>(list, _head); }
		public static R head<T>(Ref<ComparableList<T>> list, T _head) where T : IComparable<T> { return new relationHead<T>(list, new Ref<T>(_head)); }
		public static R tail<T>(Ref<ComparableList<T>> list, Ref<ComparableList<T>> _tail) where T : IComparable<T> { return new relationTail<T>(list, _tail); }
		public static R tail<T>(Ref<ComparableList<T>> list, ComparableList<T> _tail) where T : IComparable<T> { return new relationTail<T>(list, new Ref<ComparableList<T>>(_tail)); }
		public static R head_tail<T>(Ref<ComparableList<T>> list, Ref<T> head, Ref<ComparableList<T>> tail) where T : IComparable<T> { return new relationHeadTail<T>(list, head, tail); }
		public static R member<T>(Ref<ComparableList<T>> list, Ref<T> v) where T : IComparable<T>
		{
			var t = new Ref<ComparableList<T>>();
			return head(list, v)
				| tail(list, t) & call(member, t, v);
		}
		public static R at<T>(Ref<ComparableList<T>> list, Ref<int> index, Ref<T> v) where T : IComparable<T> { return new relationAt<T>(list, index, v); }
		public static R at<T>(Ref<ComparableList<T>> list, int index, Ref<T> v) where T : IComparable<T> { return new relationAt<T>(list, new Ref<int>(index), v); }
		public static R count<T>(Ref<ComparableList<T>> list, Ref<int> index) where T : IComparable<T> { return new relationCount<T>(list, index); }

		public static R range(Ref<int> begin, Ref<int> v, Ref<int> end) { return new relationRange(begin, v, end); }
		public static R range(int begin, Ref<int> v, Ref<int> end) { return new relationRange(new Ref<int>(begin), v, end); }
		public static R range(Ref<int> begin, Ref<int> v, int end) { return new relationRange(new Ref<int>(begin), v, new Ref<int>(end)); }
		public static R range(int begin, Ref<int> v, int end) { return new relationRange(new Ref<int>(begin), v, new Ref<int>(end)); }

		public static R write<T>(T a) { return pred(() => { Console.Write(a); return true; }); }
		public static R write<T>(Ref<T> a) where T : IComparable<T> { return pred(() => { Console.Write(a); return true; }); }
		public static R write<T>(string lbl, Ref<T> a) where T : IComparable<T> { return pred(() => { Console.Write(lbl); Console.Write(a); return true; }); }
		public static R write_line() { return pred(() => { Console.WriteLine(); return true; }); }
		public static R write_line<T>(T a) { return pred(() => { Console.WriteLine(a); return true; }); }
		public static R write_line<T>(Ref<T> a) where T : IComparable<T> { return pred(() => { Console.WriteLine(a); return true; }); }
		public static R write_line<T>(string lbl, Ref<T> a) where T : IComparable<T> { return pred(() => { Console.Write(lbl); Console.WriteLine(a); return true; }); }

		#region call
		public delegate R f1<A>(Ref<A> a)
			where A : IComparable<A>;

		public delegate R f2<A, B>(Ref<A> a, Ref<B> b)
			where A : IComparable<A>
			where B : IComparable<B>;

		public delegate R f3<A, B, C>(Ref<A> a, Ref<B> b, Ref<C> c)
			where A : IComparable<A>
			where B : IComparable<B>
			where C : IComparable<C>;

		public static R call<A>(f1<A> f, Ref<A> a) 
			where A : IComparable<A>
		{
			return new relationRel(() => f(a));
		}

		public static R call<A, B>(f2<A, B> f, Ref<A> a, Ref<B> b)
			where A : IComparable<A>
			where B : IComparable<B>
		{
			return new relationRel(() => f(a, b));
		}

		public static R call<A, B, C>(f3<A, B, C> f, Ref<A> a, Ref<B> b, Ref<C> c)
			where A : IComparable<A>
			where B : IComparable<B>
			where C : IComparable<C>
		{
			return new relationRel(() => f(a, b, c));
		}
		#endregion

		//////////////////////////////////////////////////////////////////
		#region ToString
		protected string ToString(string f, object a) { return f + "(" + a.ToString() + ")"; }
		protected string ToString(string f, object a, object b)
		{
			return f + "(" + a.ToString() + ", " + b.ToString() + ")";
		}
		protected string ToString(string f, object a, object b, object c)
		{
			return f + "(" + a.ToString() + ", " + b.ToString() + ", " + c.ToString() + ")";
		}
		protected string ToString(string f, params object [] p)
		{
			 f += "(";
			 for (int i = 0; i < p.Length; ++i)
			 {
				 if (i > 0) f += ", ";
				 f += p[i].ToString();
			 }
			return f + ")";
		}
		#endregion

		public IEnumerable<bool> Exec() { return Exec(false); }
		protected internal abstract IEnumerable<bool> Exec(bool fromOr);
	}

	public class Ref<T> where T : IComparable<T>
	{
		#region Compare
		public static R operator >(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.Gt); }
		public static R operator <(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.Lt); }
		public static R operator >=(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.Ge); }
		public static R operator <=(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.Le); }
		public static R operator !=(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.Ne); }
		public static R operator ==(Ref<T> a, Ref<T> b) { return new relationCmp<T>(a, b, OpType.eq); }

		public static R operator >(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.Gt); }
		public static R operator <(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.Lt); }
		public static R operator >=(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.Ge); }
		public static R operator <=(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.Le); }
		public static R operator !=(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.Ne); }
		public static R operator ==(Ref<T> a, T b) { return new relationCmp<T>(a, new Ref<T>(b), OpType.eq); }

		public static R operator >(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.Gt); }
		public static R operator <(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.Lt); }
		public static R operator >=(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.Ge); }
		public static R operator <=(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.Le); }
		public static R operator !=(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.Ne); }
		public static R operator ==(T a, Ref<T> b) { return new relationCmp<T>(new Ref<T>(a), b, OpType.eq); }
		#endregion

		public Ref() { _defined = false; }
		public Ref(T a) { this._defined = true; this._v = a; }
		public Ref(Ref<T> a) { this._defined = a._defined; this._v = a._v; }
		public override string ToString()
		{
			if (!_defined) return "<undef>";
			return _v.ToString();
		}
		public override int GetHashCode() { return _v.GetHashCode(); }
		public override bool Equals(object obj)
		{
			if (obj != null && obj is Ref<T>)
			{
				Ref<T> r = (Ref<T>)obj;
				if (this._defined == r._defined) return this._v.Equals(r._v);
				return false;
			}
			return false;
		}

		public bool Defined() { return this._defined; }
		public T Value
		{
			get
			{
				if (!_defined) throw new R.InvalidDeref();
				return _v;
			}
			set
			{
				if (this._defined) throw new R.InvalidDeref();
				this._defined = true;
				this._v = value;
			}
		}

		private T _v;
		private bool _defined;

		public void Reset()
		{
			if (!this._defined) throw new R.InvalidDeref();
			this._defined = false;
			this._v = default(T);
		}
	}

	public class ComparableList<T> : IComparable<ComparableList<T>> where T : IComparable<T>
	{
		public ComparableList() { _v = new List<T>(); }
		public ComparableList(ComparableList<T> c) : this() { _v.AddRange(c._v); }
		public ComparableList(IList<T> c) : this() { _v.AddRange(c); }

		public int CompareTo(ComparableList<T> c)
		{
			int r = c._v.Count.CompareTo(this._v.Count);
			if (r != 0) return r;
			for (int i = 0; i < _v.Count; ++i)
			{
				r = _v[i].CompareTo(c._v[i]);
				if (r != 0) return r;
			}
			return 0;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i < _v.Count; ++i)
			{
				if (i > 0) sb.Append(", ");
				sb.Append(_v[i].ToString());
			}
			sb.Append("[");
			return sb.ToString();
		}

		public T this[int index]
		{
			get { return _v[index]; }
			set { _v[index] = value; }
		}

		public int Count { get { return _v.Count; } }

		public void Add(T v) { _v.Add(v); }

		public ComparableList<T> Tail { get { return new ComparableList<T>(_v.GetRange(1, _v.Count - 1)); } }

		List<T> _v;
	}

	///////////////////////////////////////////////////////////////

	namespace Details
	{
		internal enum OpType { Gt, Ge, Lt, Le, eq, Ne };

		internal class relationAnd : R
		{
			public relationAnd(R a, R b) { this.a = a; this.b = b; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				bool go = true;
				foreach (var va in a.Exec(false))
				{
					if (!va) go = false;                 // caso ! & B  faccio backtracking di B e alla fine ritorno false
					                                     // se ho va == false e` garantito che non c'e` niente da iterare in <a>

					foreach (var vb in b.Exec(false))
					{
						if (!vb) { go = false; break; }  // A & (! & B) non devo piu` trovare soluzioni di (! & B) (false e` l'ultimo per definizione ) e non devo fare backtracking di A
						yield return true;
					}

					if (!go) break;                    // esco dall'iteratore <va>
				}
				if (!go)
					yield return false;
			}
			public override string ToString() { return "(" + a.ToString() + " & " + b.ToString() + ")"; }

			readonly R a;
			readonly R b;
		}
		internal class relationOr : R
		{
			public relationOr(R a, R b) { this.a = a; this.b = b; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				bool go = true;

				if (go)
				{
					foreach (var va in a.Exec(true))
					{
						if (!va) { go = false; break; }	
						yield return true;
					}
				}

				if (go)
				{
					foreach (var vb in b.Exec(true))
					{
						if (!vb) { go = false; break; }
						yield return true;
					}
				}
				
				if (!go && fromOr)   // se chi mi chiama e` un or
					yield return false;
			}

			public override string ToString() { return "(" + a.ToString() + " | " + b.ToString() + ")"; }

			readonly R a;
			readonly R b;
		}
		internal class relationXor : R
		{
			public relationXor(R a, R b) { this.a = a; this.b = b; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				bool go = true;

				bool oka = false;

				if (go)
				{
					// basta che lato <a> ci sia un successo che non si fa piu` <b>
					// se c'e` un cut il funzionamento e` come or
					foreach (var va in a.Exec(true))
					{
						if (!va) { go = false; break; }
						oka = true;
						yield return true;
					}
				}

				if (!oka && go)
				{
					foreach (var vb in b.Exec(true))
					{
						if (!vb) { go = false; break; }
						yield return true;
					}
				}

				if (!go && fromOr)   // se chi mi chiama e` un or
					yield return false;
			}

			public override string ToString() { return "(" + a.ToString() + " ^ " + b.ToString() + ")"; }

			readonly R a;
			readonly R b;
		}
		internal class relationHead<T> : R where T : IComparable<T>
		{
			readonly Ref<ComparableList<T>> list;
			readonly Ref<T> head;

			public relationHead(Ref<ComparableList<T>> list, Ref<T> head) { this.list = list; this.head = head; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (list.Defined() && head.Defined())
				{
					if (list.Value.Count > 0)
						if (list.Value[0].CompareTo(head.Value) == 0)
							yield return true;
				}
				else if (list.Defined() && !head.Defined())
				{
					if (list.Value.Count > 0)
					{
						head.Value = list.Value[0];
						yield return true;
						head.Reset();
					}
				}
				else
					throw new InvalidDeref();
			}
			public override string ToString() { return ToString("head", list, head); }
		}
		internal class relationTail<T> : R where T : IComparable<T>
		{
			readonly Ref<ComparableList<T>> list;
			readonly Ref<ComparableList<T>> tail;

			public relationTail(Ref<ComparableList<T>> list, Ref<ComparableList<T>> tail) { this.list = list; this.tail = tail; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (list.Defined() && tail.Defined())
				{
					if (list.Value.Count == tail.Value.Count + 1)
					{
						int i;
						for (i = 0; i < tail.Value.Count; ++i)
							if (list.Value[i + 1].CompareTo(tail.Value[i]) != 0)
								break;
						if (i == tail.Value.Count)
							yield return true;
					}
				}
				else if (list.Defined() && !tail.Defined())
				{
					if (list.Value.Count > 0)
					{
						tail.Value = list.Value.Tail;
						yield return true;
						tail.Reset();
					}
				}
				else
					throw new InvalidDeref();
			}

			public override string ToString() { return ToString("tail", list, tail); }
		}
		internal class relationHeadTail<T> : R where T : IComparable<T>
		{
			readonly Ref<ComparableList<T>> _list;
			readonly Ref<T> _head;
			readonly Ref<ComparableList<T>> _tail;

			public relationHeadTail(Ref<ComparableList<T>> list, Ref<T> head, Ref<ComparableList<T>> tail) { this._list = list; this._head = head; this._tail = tail; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (this._list.Defined() && this._head.Defined() && this._tail.Defined())
				{
					if (this._list.Value.Count == this._tail.Value.Count + 1)
					{
						int c = this._list.Value[0].CompareTo(this._head.Value);
						if (c == 0)
						{
							for (int i = 0; i < this._tail.Value.Count; ++i)
							{
								c = this._list.Value[i + 1].CompareTo(this._tail.Value[i]);
								if (c != 0)
									break;
							}
							if (c == 0)
								yield return true;
						}
					}
				}
				else if (this._list.Defined())
				{
					if (this._list.Value.Count > 0)
					{
						this._head.Value = this._list.Value[0];
						this._tail.Value = this._list.Value.Tail;
						yield return true;
						this._head.Reset();
						this._tail.Reset();
					}
				}
				else
					throw new R.InvalidDeref();
			}

			public override string ToString() { return ToString("head_tail", _list, _head, _tail); }
		}
		internal class relationEmpty<T> : R where T : IComparable<T>
		{
			readonly Ref<ComparableList<T>> list;

			public relationEmpty(Ref<ComparableList<T>> list) { this.list = list; }

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (list.Defined())
				{
					if (list.Value.Count > 0)
						yield return true;
				}
				else
				{
					list.Value = new ComparableList<T>();
					yield return true;
					list.Reset();
				}
			}
			public override string ToString() { return ToString("empty", list); }
		}
		internal class relationAt<T> : R where T : IComparable<T>
		{
			Ref<ComparableList<T>> list;
			Ref<int> index;
			Ref<T> v;
			public relationAt(Ref<ComparableList<T>> list, Ref<int> index, Ref<T> v) { this.list = list; this.index = index; this.v = v; }
			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (this.list.Defined() && this.index.Defined() && this.v.Defined())
				{
					if (index.Value < 0 || index.Value >= this.list.Value.Count)
						throw new IndexOutOfRangeException("index");

					int c = this.list.Value[index.Value].CompareTo(this.v.Value);
					if (c == 0)
						yield return true;
				}
				else if (this.list.Defined() && this.index.Defined())
				{
					if (index.Value < 0 || index.Value >= this.list.Value.Count)
						throw new IndexOutOfRangeException("index");

					this.v.Value = this.list.Value[index.Value];
					yield return true;
					this.v.Reset();
				}
				else if (this.list.Defined() && this.v.Defined())
				{
					for (int i = 0; i < list.Value.Count; ++i)
					{
						int c = v.Value.CompareTo(this.list.Value[i]);
						if (c == 0)
						{
							index.Value = i;
							yield return true;
							index.Reset();
						}
					}
				}
				else if (this.list.Defined())
				{
					for (int i = 0; i < list.Value.Count; ++i)
					{
						v.Value = this.list.Value[i];
						index.Value = i;
						yield return true;
						v.Reset();
						index.Reset();
					}
				}
				else
					throw new R.InvalidDeref();
			}
			public override string ToString() { return ToString("at", this.list, this.index, this.v); }
		}
		internal class relationCount<T> : R where T : IComparable<T>
		{
			Ref<ComparableList<T>> list;
			Ref<int> count;
			public relationCount(Ref<ComparableList<T>> list, Ref<int> count) { this.list = list; this.count = count; }
			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (this.list.Defined() && this.count.Defined())
				{
					if (this.count.Value == this.list.Value.Count)
						yield return true;
				}
				else if (this.list.Defined())
				{
					this.count.Value = this.list.Value.Count;
					yield return true;
					this.count.Reset();
				}
				else
					throw new R.InvalidDeref();
			}
			public override string ToString() { return ToString("count", this.list, this.count); }
		}
		/////////////////////////////////////////////////////////
		internal class relationEq<T> : R where T : IComparable<T>
		{
			public relationEq(Ref<T> a, Ref<T> b)
			{
				this.a = a;
				this.b = b;
			}

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (a.Defined() && b.Defined())
				{
					int c = a.Value.CompareTo(b.Value);
					if (c == 0) yield return true;
				}
				else if (a.Defined())
				{
					b.Value = a.Value;
					yield return true;
					b.Reset();
				}
				else if (b.Defined())
				{
					a.Value = b.Value;
					yield return true;
					a.Reset();
				}
				else
					throw new InvalidDeref();
			}

			public override string ToString()
			{
				return "eq(" + a.ToString() + ", " + b.ToString() + ")";
			}
			readonly Ref<T> a;
			readonly Ref<T> b;
		}
		/////////////////////////////////////////////////////////
		internal class relationCmp<T> : R where T : IComparable<T>
		{
			public relationCmp(Ref<T> a, Ref<T> b, OpType op)
			{
				this.a = a;
				this.b = b;
				this.op = op;
			}

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (a.Defined() && b.Defined())
				{
					int c = a.Value.CompareTo(b.Value);
					switch (op)
					{
					case OpType.eq:
						if (c == 0) yield return true;
						break;
					case OpType.Ne:
						if (c != 0) yield return true;
						break;
					case OpType.Lt:
						if (c < 0) yield return true;
						break;
					case OpType.Gt:
						if (c > 0) yield return true;
						break;
					case OpType.Le:
						if (c <= 0) yield return true;
						break;
					case OpType.Ge:
						if (c >= 0) yield return true;
						break;
					}
				}
				else
					throw new InvalidDeref();
			}

			public override string ToString()
			{
				string s = "<op>";
				switch (op)
				{
				case OpType.eq:
					s = "==";
					break;
				case OpType.Ne:
					s = "!=";
					break;
				case OpType.Lt:
					s = "<";
					break;
				case OpType.Gt:
					s = ">";
					break;
				case OpType.Le:
					s = "<=";
					break;
				case OpType.Ge:
					s = ">=";
					break;
				}

				return "(" + a.ToString() + " " + s + " " + b.ToString() + ")";
			}
			readonly Ref<T> a;
			readonly Ref<T> b;
			readonly OpType op;
		}
		/////////////////////////////////////////////////////////
		internal class relationEqIs<T> : R where T : IComparable<T>
		{
			public relationEqIs(Ref<T> a, Func<T> b)
			{
				this.a = a;
				this.b = b;
			}

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (a.Defined())
				{
					T r = b();
					if (a.Value.CompareTo(r) == 0)
						yield return true;
				}
				else if (!a.Defined())
				{
					a.Value = b();
					yield return true;
					a.Reset();
				}
				else
					throw new InvalidDeref();
			}

			public override string ToString()
			{
				return "eq_is(" + a.ToString() + ", " + b.ToString() + ")";
			}
			readonly Ref<T> a;
			readonly Func<T> b;
		}
		/////////////////////////////////////////////////////////
		internal class relationPredicate : R
		{
			public relationPredicate(Func<bool> b)
			{
				this.b = b;
			}

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (b())
					yield return true;
			}

			public override string ToString()
			{
				return "predicate(" + b.ToString() + ")";
			}
			readonly Func<bool> b;
		}
		/////////////////////////////////////////////////////////
		internal class relationRel : R
		{
			public relationRel(Func<IEnumerable<bool>> a)
			{
				this.a = a;
			}
			public relationRel(Func<R> b)
			{
				this.b = b;
			}

			protected internal override IEnumerable<bool> Exec(bool fromOr)
			{
				if (a != null)
					return a();
				else
					return b().Exec(fromOr);
			}

			public override string ToString()
			{
				return "rel(" + b.ToString() + ")";
			}
			readonly Func<IEnumerable<bool>> a;
			readonly Func<R> b;
		}
		/////////////////////////////////////////////////////////
		internal class relationFail : R
		{
			public relationFail() { }
			protected internal override IEnumerable<bool> Exec(bool fromOr) { yield break; }
			public override string ToString() { return "fail"; }
		}
		/////////////////////////////////////////////////////////
		internal class relationCut : R
		{
			// se C :- P,Q,R,!,S,T,U
			//    C :- V
			// quando si arriva al ! non si potra` fare piu` backtracking su P,Q,R e neanche su V
			// mentre sara` possibile fra backtraking su S,T,U
			public relationCut() { }
			protected internal override IEnumerable<bool> Exec(bool fromOr) { yield return false; }
			public override string ToString() { return "cut"; }
		}
		/////////////////////////////////////////////////////////
		internal class relationRange : R
		{
			Ref<int> begin; Ref<int> v; Ref<int> end;
			public relationRange(Ref<int> begin, Ref<int> v, Ref<int> end) { this.begin = begin; this.v = v; this.end = end; }
			protected internal override IEnumerable<bool> Exec(bool fromOr) 
			{
				if (this.begin.Defined() && this.v.Defined() && this.end.Defined())
				{
					if (begin.Value <= v.Value && v.Value < end.Value)
						yield return true;
				}
				else if (this.begin.Defined() && this.end.Defined())
				{
					for (int i = begin.Value; i < end.Value; ++i)
					{
						v.Value = i;
						yield return true;
						v.Reset();
					}
				}
				else
					throw new R.InvalidDeref();
			}
			public override string ToString() { return ToString("range", this.begin, this.v, this.end); }
		}
	}
}

namespace CastorNetTest
{
	using CastorNet;

	class _
	{
		static void Test()
		{
			Ref<ComparableList<int>> list = new Ref<ComparableList<int>>();
			Ref<int> v = new Ref<int>();

			R r = R.member(list, v)
				& v > 10
				& v < 20;

			list.Value = new ComparableList<int>();
			list.Value.Add(1);
			list.Value.Add(10);
			list.Value.Add(11);
			list.Value.Add(30);

			foreach (var n in r.Exec())
			{
				Console.WriteLine("{0}", v.Value);
			}
		}
	}
}

