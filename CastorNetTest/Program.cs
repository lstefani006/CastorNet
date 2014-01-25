using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CastorNet;

public static class Ciccio
{
	public static TimeSpan GetTimeSpan(this SQLiteDataReader rd, int n)
	{
		var tt = rd.GetDateTime(n);
		return new TimeSpan(tt.Hour, tt.Minute, tt.Second);
	}
}

namespace CastorNetTest
{
	class Program
	{
		public class RelNode : R
		{
			readonly Ref<int> nodeId;
			readonly Ref<string> nodeCode;
			readonly Ref<string> shortName;

			public RelNode(Ref<int> nodeId, Ref<string> nodeCode, Ref<string> shortName)
			{
				this.nodeId = nodeId;
				this.nodeCode = nodeCode;
				this.shortName = shortName;
			}

			public override string ToString()
			{
				return ToString("RelNode", nodeId, nodeCode, shortName);
			}

			protected override IEnumerable<bool> Exec(bool fromOr)
			{
				using (SQLiteConnection cn = new SQLiteConnection())
				{
					cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
					cn.Open();
					using (var cmd = cn.CreateCommand())
					{
						bool defined_nodeId = nodeId.Defined();
						bool defined_nodeCode = nodeCode.Defined();
						bool defined_shortName = shortName.Defined();

						cmd.CommandText = @"SELECT NodeId, NodeCode, NodeShortNameEn, NodeShortNameAr, NodeLongNameEn, NodeLongNameAr FROM Node where 1=1";
						if (defined_nodeId)
						{
							cmd.CommandText += " and NodeId=@1";
							cmd.Parameters.AddWithValue("@1", nodeId.Value);
						}
						if (defined_nodeCode)
						{
							cmd.CommandText += " and NodeCode=@2";
							cmd.Parameters.AddWithValue("@2", nodeCode.Value);
						}
						if (defined_shortName)
						{
							cmd.CommandText += " and NodeShortNameEn=@3";
							cmd.Parameters.AddWithValue("@3", shortName.Value);
						}

						using (var rd = cmd.ExecuteReader())
						{
							while (rd.Read())
							{
								if (!defined_nodeId) nodeId.Value = rd.GetInt32(0);
								if (!defined_nodeCode) nodeCode.Value = rd.GetString(1);
								if (!defined_shortName) shortName.Value = rd.GetString(2);

								yield return true;

								if (!defined_nodeId) nodeId.Reset();
								if (!defined_nodeCode) nodeCode.Reset();
								if (!defined_shortName) shortName.Reset();
							}
						}
					}
				}
			}
		}

		public static R Node(Ref<int> nodeId, Ref<string> nodeCode, Ref<string> shortName)
		{
			return new RelNode(nodeId, nodeCode, shortName);
		}


		public class relationNodeRide : R
		{
			Ref<TimeSpan> tReqPar;
			Ref<int> nodeId;
			Ref<int> rideId;
			Ref<int> orderNum;
			Ref<TimeSpan> tPar;

			Dictionary<int, List<Record>> _Node2Ride = new Dictionary<int, List<Record>>();
			Dictionary<int, List<Record>> _Ride2Node = new Dictionary<int, List<Record>>();

			public relationNodeRide(Ref<TimeSpan> tReqPar, Ref<int> nodeId, Ref<int> orderNum, Ref<int> rideId, Ref<TimeSpan> tPar)
			{
				this.tReqPar = tReqPar;
				this.nodeId = nodeId;
				this.orderNum = orderNum;
				this.rideId = rideId;
				this.tPar = tPar;

				using (SQLiteConnection cn = new SQLiteConnection())
				{
					cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
					cn.Open();
					using (var cmd = cn.CreateCommand())
					{
						cmd.CommandText = @"SELECT RideId, OrderNum, NodeId, ArrivalDay, ArrivalTime, DepartureTime FROM RideStop where 1=1";

						using (var rd = cmd.ExecuteReader())
						{
							while (rd.Read())
							{
								Record r = new Record();

								r.rideId = rd.GetInt32(0);
								r.orderNum = rd.GetInt32(1);
								r.nodeId = rd.GetInt32(2);
								r.tPar = rd.GetTimeSpan(5);

								if (_Node2Ride.ContainsKey(r.nodeId) == false) _Node2Ride.Add(r.nodeId, new List<Record>());
								if (_Ride2Node.ContainsKey(r.rideId) == false) _Ride2Node.Add(r.rideId, new List<Record>());

								_Node2Ride[r.nodeId].Add(r);
								_Ride2Node[r.rideId].Add(r);
							}
						}
					}
				}
			}

			public override string ToString()
			{
				return ToString("NodeRide", nodeId, orderNum, rideId, tPar);
			}

			class Record
			{
				public int nodeId;
				public int rideId;
				public int orderNum;
				public TimeSpan tPar;

			}

			protected override IEnumerable<bool> Exec(bool fromOr)
			{
				bool defined_nodeId = nodeId.Defined();
				bool defined_rideId = rideId.Defined();
				bool defined_orderNum = orderNum.Defined();
				bool defined_tPar = tPar.Defined();

				if (defined_rideId)
				{
					foreach (var r in _Ride2Node[rideId.Value])
					{
						if (defined_nodeId) if (r.nodeId != nodeId.Value) continue;
						if (defined_rideId) if (r.rideId != rideId.Value) continue;
						if (defined_orderNum) if (r.orderNum != orderNum.Value) continue;
						if (defined_tPar) if (r.tPar != tPar.Value) continue;
						if (tReqPar.Defined())
						{
							if (!(tReqPar.Value <= r.tPar && r.tPar <= tReqPar.Value.Add(new TimeSpan(1, 0, 0))))
								continue;
						}

						if (!defined_nodeId) nodeId.Value = r.nodeId;
						if (!defined_rideId) rideId.Value = r.rideId;
						if (!defined_orderNum) orderNum.Value = r.orderNum;
						if (!defined_tPar) tPar.Value = r.tPar;

						yield return true;

						if (!defined_nodeId) nodeId.Reset();
						if (!defined_rideId) rideId.Reset();
						if (!defined_orderNum) orderNum.Reset();
						if (!defined_tPar) tPar.Reset();
					}
				}
				else if (defined_nodeId)
				{
					foreach (var r in _Node2Ride[nodeId.Value])
					{
						if (defined_nodeId) if (r.nodeId != nodeId.Value) continue;
						if (defined_rideId) if (r.rideId != rideId.Value) continue;
						if (defined_orderNum) if (r.orderNum != orderNum.Value) continue;
						if (defined_tPar) if (r.tPar != tPar.Value) continue;
						if (tReqPar.Defined())
						{
							if (!(tReqPar.Value <= r.tPar && r.tPar <= tReqPar.Value.Add(new TimeSpan(1, 0, 0))))
								continue;
						}


						if (!defined_nodeId) nodeId.Value = r.nodeId;
						if (!defined_rideId) rideId.Value = r.rideId;
						if (!defined_orderNum) orderNum.Value = r.orderNum;
						if (!defined_tPar) tPar.Value = r.tPar;



						yield return true;

						if (!defined_nodeId) nodeId.Reset();
						if (!defined_rideId) rideId.Reset();
						if (!defined_orderNum) orderNum.Reset();
						if (!defined_tPar) tPar.Reset();
					}
				}
				else
				{
					using (SQLiteConnection cn = new SQLiteConnection())
					{
						cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
						cn.Open();
						using (var cmd = cn.CreateCommand())
						{
							cmd.CommandText = @"SELECT RideId, OrderNum, NodeId, ArrivalDay, ArrivalTime, DepartureTime FROM RideStop where 1=1";
							if (defined_nodeId)
							{
								cmd.CommandText += " and NodeId=@1";
								cmd.Parameters.AddWithValue("@1", nodeId.Value);
							}
							if (defined_rideId)
							{
								cmd.CommandText += " and RideId=@2";
								cmd.Parameters.AddWithValue("@2", rideId.Value);
							}
							if (defined_orderNum)
							{
								cmd.CommandText += " and OrderNum=@3";
								cmd.Parameters.AddWithValue("@3", orderNum.Value);
							}

							using (var rd = cmd.ExecuteReader())
							{
								while (rd.Read())
								{
									if (!defined_rideId) rideId.Value = rd.GetInt32(0);
									if (!defined_orderNum) orderNum.Value = rd.GetInt32(1);
									if (!defined_nodeId) nodeId.Value = rd.GetInt32(2);
									if (!defined_tPar) tPar.Value = rd.GetTimeSpan(5);

									if (tReqPar.Defined())
									{
										if (!(tReqPar.Value <= rd.GetTimeSpan(5) && rd.GetTimeSpan(5) <= tReqPar.Value.Add(new TimeSpan(1, 0, 0))))
											continue;
									}


									yield return true;

									if (!defined_rideId) rideId.Reset();
									if (!defined_orderNum) orderNum.Reset();
									if (!defined_nodeId) nodeId.Reset();
									if (!defined_tPar) tPar.Reset();
								}
							}
						}
					}
				}
			}
		}
		public static R NodeRide(Ref<TimeSpan> tReqPar, Ref<int> nodeId, Ref<int> orderNum, Ref<int> rideId, Ref<TimeSpan> tPar) { return new relationNodeRide(tReqPar, nodeId, orderNum, rideId, tPar); }

		public class relationRide : R
		{
			Ref<int> rideId;
			Ref<string> rideCode;

			public relationRide(Ref<int> rideId, Ref<string> rideCode)
			{
				this.rideId = rideId;
				this.rideCode = rideCode;
			}

			protected override IEnumerable<bool> Exec(bool fromOr)
			{
				bool defined_rideId = rideId.Defined();
				bool defined_rideCode = rideCode.Defined();

				using (SQLiteConnection cn = new SQLiteConnection())
				{
					cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
					cn.Open();
					using (var cmd = cn.CreateCommand())
					{
						cmd.CommandText = @"SELECT RideId, RideCode FROM Ride where 1=1";
						if (defined_rideId)
						{
							cmd.CommandText += " and RideId=@1";
							cmd.Parameters.AddWithValue("@1", rideId.Value);
						}
						if (defined_rideCode)
						{
							cmd.CommandText += " and RideCode=@2";
							cmd.Parameters.AddWithValue("@2", rideCode.Value);
						}

						using (var rd = cmd.ExecuteReader())
						{
							while (rd.Read())
							{
								if (!defined_rideId) rideId.Value = rd.GetInt32(0);
								if (!defined_rideCode) rideCode.Value = rd.GetString(1);

								yield return true;

								if (!defined_rideId) rideId.Reset();
								if (!defined_rideCode) rideCode.Reset();
							}
						}
					}
				}
			}

			public override string ToString()
			{
				return ToString("Ride", rideId, rideCode);
			}
		}
		public static R Ride(Ref<int> rideId, Ref<string> rideCode) { return new relationRide(rideId, rideCode); }

		public static R Coincidenza(Ref<TimeSpan> a, Ref<TimeSpan> b)
		{
			return R.pred(() => a.Value < b.Value && b.Value < a.Value.Add(new TimeSpan(1, 0, 0)));
		}


		public static R Viaggio1(Ref<TimeSpan> tReqPar, Ref<int> ndPar, Ref<int> ndArr, Ref<TimeSpan> tPar, Ref<TimeSpan> tArr, Ref<int> rideId)
		{
			Ref<int> ordeNumPar = new Ref<int>();
			Ref<int> ordeNumArr = new Ref<int>();
			Ref<TimeSpan> tReqArr = new Ref<TimeSpan>();

			return NodeRide(tReqPar, ndPar, ordeNumPar, rideId, tPar)
				& NodeRide(tReqArr, ndArr, ordeNumArr, rideId, tArr)
				& (ordeNumPar < ordeNumArr)
				;
		}

		public static R Viaggio2(Ref<TimeSpan> tReqPar, Ref<int> ndPar, Ref<int> ndArr, Ref<int> rideId1, Ref<int> rideId2, Ref<int> ndTra,
			Ref<TimeSpan> tp1,
			Ref<TimeSpan> ta1,
			Ref<TimeSpan> tp2,
			Ref<TimeSpan> ta2
		)
		{
			Ref<TimeSpan> tReqPar2 = new Ref<TimeSpan>();

			return Viaggio1(tReqPar, ndPar, ndTra, tp1, ta1, rideId1)
				& Viaggio1(tReqPar2, ndTra, ndArr, tp2, ta2, rideId2)
				& rideId1 != rideId2
				& Coincidenza(ta1, tp2)
				;
		}

		static R inc(Ref<string> a, Ref<string> n)
		{
			return R.@is(a, () => n.Value + "a") & R.defined(a);
		}

		static void Main(string[] args)
		{
			if (true)
			{
				using (var db = new enrEntities())
				{
					var q = from node in db.Node
							join branch in db.BranchElement on node.NodeId equals branch.NodeId into ps
							where node.NodeId == 99
							select new { NodeId = node.NodeId, Branches = ps };

					foreach (var node in q)
					{
						var ww = node.Branches.ToList();
						Console.WriteLine("{0}", node.NodeId, node.Branches.ToList());
					}
				}

			}


			BranchGroupManager brMan = new BranchGroupManager();
			brMan.BuildMap();

			if (true)
			{
				Ref<TimeSpan> tReqPar = new Ref<TimeSpan>();
				Ref<int> ndPar = new Ref<int>();
				Ref<int> ndArr = new Ref<int>();
				Ref<int> rideId1 = new Ref<int>();
				Ref<string> rideCode1 = new Ref<string>();
				Ref<int> rideId2 = new Ref<int>();
				Ref<string> rideCode2 = new Ref<string>();

				Ref<TimeSpan> tp1 = new Ref<TimeSpan>();
				Ref<TimeSpan> ta1 = new Ref<TimeSpan>();
				Ref<TimeSpan> tp2 = new Ref<TimeSpan>();
				Ref<TimeSpan> ta2 = new Ref<TimeSpan>();

				Ref<int> ndTra = new Ref<int>();

				List<Tuple<int, int>> sol = new List<Tuple<int, int>>();

				tReqPar.Value = new TimeSpan(7, 0, 0);
				var r = Viaggio2(tReqPar, ndPar, ndArr, rideId1, rideId2, ndTra, tp1, ta1, tp2, ta2)
					& R.pred(() =>
					{
						Tuple<int, int> rr = new Tuple<int, int>(rideId1.Value, rideId2.Value);
						if (sol.Contains(rr))
							return false;
						sol.Add(rr);
						return true;
					})
					& Ride(rideId1, rideCode1)
					& Ride(rideId2, rideCode2)
					;

				ndPar.Value = 101;
				ndArr.Value = 819;
				foreach (var ssss in r.Exec())
				{
					Console.WriteLine("r1={0} r2={1} tra={2} p1={3} a1={4} p2={5} a2={6}", rideId1, rideId2, ndTra, tp1, ta1, tp2, ta2);

					int km1 = KmOfRide(brMan, rideId1.Value, ndPar.Value, ndTra.Value);
					int km2 = KmOfRide(brMan, rideId2.Value, ndTra.Value, ndArr.Value);

					Console.WriteLine("km1={0} km2={1}", km1, km2);
				}
			}
		}

		public static int KmOfRide(BranchGroupManager brm, int rideId, int node1, int node2)
		{
			RecRide ride = null;
			using (SQLiteConnection cn = new SQLiteConnection())
			{
				cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
				cn.Open();

				using (var cmd = RecRide.CreateCommandSelect(cn))
				{
					cmd.CommandText += " where RideId=@1";
					cmd.Parameters.AddWithValue("@1", rideId);

					using (var rd = cmd.ExecuteReader())
						if (rd.Read())
						{
							ride = new RecRide();
							ride.Read(rd);
						}
				}
			}
			if (ride == null)
				return -1;
			if (ride.f_BranchGroupId.HasValue == false)
				return -1;
			return brm.GetDistance(ride.f_BranchGroupId.Value, node1, node2);
		}
	}

	class BranchGroupManager
	{
		public BranchGroupManager()
		{
			_map = null;
		}

		public int GetDistance(int branchGroupId, int node1, int node2)
		{
			if (_map == null)
				BuildMap();

			var nd = _map[branchGroupId];

			RecBranchElement b1 = null;
			RecBranchElement b2 = null;
			int m = 0;
			foreach (var t in nd)
			{
				if (b1 == null && (t.f_NodeId == node1 || t.f_NodeId == node2))
				{
					b1 = t;
					continue;
				}
				else if (b1 != null && (t.f_NodeId == node1 || t.f_NodeId == node2))
				{
					b2 = t;
					m += t.f_DistanceKm.Value;
					break;
				}

				if (b1 != null)
					m += t.f_DistanceKm.Value;
			}

			if (b1 != null && b2 != null)
				return m;

			return -1;

		}

		public void BuildMap()
		{
			_map = new Dictionary<int, List<RecBranchElement>>();

			using (SQLiteConnection cn = new SQLiteConnection())
			{
				cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
				cn.Open();


				var branchGroupList = new List<RecBranchGroup>();
				using (var cmd = RecBranch.CreateCommandSelect(cn))
				{
					using (var rd = cmd.ExecuteReader())
					{
						while (rd.Read())
						{
							var r = new RecBranchGroup();
							r.Read(rd);
							branchGroupList.Add(r);
						}
					}
				}

				foreach (var branchGroupId in branchGroupList)
				{
					var branchGroupElementList = new List<RecBranchGroupElement>();
					using (var cmd = RecBranchGroupElement.CreateCommandSelect(cn))
					{
						cmd.CommandText += @" where BranchGroupId=@1 order by OrderNum";
						cmd.Parameters.AddWithValue("@1", branchGroupId.f_BranchGroupId);

						using (var rd = cmd.ExecuteReader())
						{
							while (rd.Read())
							{
								var bg = new RecBranchGroupElement();
								bg.Read(rd);
								branchGroupElementList.Add(bg);
							}
						}
					}


					var v = new List<Tuple<RecBranch, List<RecBranchElement>>>();
					foreach (var branchElement in branchGroupElementList)
					{
						var branch = new RecBranch();
						using (var cmd = RecBranch.CreateCommandSelect(cn))
						{
							cmd.CommandText += @" where BranchId=@1";
							cmd.Parameters.AddWithValue("@1", branchElement.f_BranchId.Value);

							using (var rd = cmd.ExecuteReader())
								while (rd.Read())
								{
									branch.Read(rd);
									break;
								}
						}


						var branchElementList = new List<RecBranchElement>();
						using (var cmd = RecBranchElement.CreateCommandSelect(cn))
						{
							cmd.CommandText += @" where BranchId=@1 order by OrderNum";
							cmd.Parameters.AddWithValue("@1", branchElement.f_BranchId.Value);

							int Dist = 0;
							using (var rd = cmd.ExecuteReader())
							{
								while (rd.Read())
								{
									var r = new RecBranchElement();
									r.Read(rd);
									branchElementList.Add(r);
									Dist += r.f_DistanceKm.Value;
								}
							}

							if (Dist != branch.f_DistanceKm)
							{
								Console.WriteLine("Branch={0} {1} total distance differ {2} {3}", branch.f_BranchId, branch.f_BranchName, Dist, branch.f_DistanceKm);
							}
						}
						v.Add(new Tuple<RecBranch, List<RecBranchElement>>(branch, branchElementList));
					}


					// metto in ordine i branch.
					var nd = new List<RecBranchElement>();
					if (true)
					{
						int LastNodeId = -1;

						for (int j = 0; j < v.Count; ++j)
						{
							var current = v[j];
							if (LastNodeId == -1)
							{
								if (v.Count >= 2)
								{
									var next = v[j + 1];

									var current_first = current.Item2[0].f_NodeId.Value;
									var current_last = current.Item2[current.Item2.Count - 1].f_NodeId.Value;

									var next_first = next.Item2[0].f_NodeId.Value;
									var next_last = next.Item2[next.Item2.Count - 1].f_NodeId.Value;

									if (current_last == next_first || current_last == next_last)
									{
										// e` gia dritta o e' la prima
										foreach (var tt in current.Item2)
										{
											nd.Add(tt);
											LastNodeId = tt.f_NodeId.Value;
										}
									}
									else
									{
										// devo girare.
										int LastDistance = 0;
										for (int i = current.Item2.Count - 1; i >= 0; --i)
										{
											int NodeId = current.Item2[i].f_NodeId.Value;
											var b = new RecBranchElement();
											b.f_BranchId = current.Item1.f_BranchId;
											b.f_NodeId = NodeId;
											b.f_OrderNum = nd.Count;
											b.f_DistanceKm = LastDistance;
											nd.Add(b);
											LastDistance = current.Item2[i].f_DistanceKm.Value;
											LastNodeId = NodeId;
										}
									}
								}
								else
								{
									// e` gia dritta o e' la prima
									foreach (var tt in current.Item2)
									{
										nd.Add(tt);
										LastNodeId = tt.f_NodeId.Value;
									}
								}
							}
							else
							{

								if (current.Item2[0].f_NodeId == LastNodeId)
								{
									// e` gia dritta o e' la prima
									foreach (var tt in current.Item2)
									{
										nd.Add(tt);
										LastNodeId = tt.f_NodeId.Value;
									}
								}
								else if (current.Item2[current.Item2.Count - 1].f_NodeId.Value == LastNodeId)
								{
									// devo girare.
									int LastDistance = 0;
									for (int i = current.Item2.Count - 1; i >= 0; --i)
									{
										int NodeId = current.Item2[i].f_NodeId.Value;
										var b = new RecBranchElement();
										b.f_BranchId = current.Item1.f_BranchId;
										b.f_NodeId = NodeId;
										b.f_OrderNum = nd.Count;
										b.f_DistanceKm = LastDistance;
										nd.Add(b);
										LastDistance = current.Item2[i].f_DistanceKm.Value;
										LastNodeId = NodeId;
									}
								}
								else
									Debug.Assert(false);
							}
						}
					}


					if (true)
					{
						List<RecBranchElement> no = new List<RecBranchElement>();
						for (int j = 0; j < nd.Count; ++j)
						{
							var c = nd[j + 0];
							var n = j + 1 < nd.Count ? nd[j + 1] : null;

							if (n != null && c.f_NodeId == n.f_NodeId)
							{
								if (c.f_DistanceKm > 0)
									no.Add(c);
								else if (n.f_DistanceKm > 0)
									no.Add(n);
								else
									Debug.Assert(true);
								j++;
							}
							else
								no.Add(c);
						}
						nd = no;
					}

					_map.Add(branchGroupId.f_BranchGroupId, nd);
				}
			}
		}

		private Dictionary<int, List<RecBranchElement>> _map;
	};
}
