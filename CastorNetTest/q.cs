using System;
using System.Collections.Generic;
using System.Data.SQLite;

[Serializable]
public class RecNode
{
	public int f_NodeId { get; set; }
	public string f_NodeCode { get; set; }
	public string f_NodeShortNameEn { get; set; }
	public string f_NodeShortNameAr { get; set; }
	public string f_NodeLongNameEn { get; set; }
	public string f_NodeLongNameAr { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_NodeId = rd.GetInt32(0);
		f_NodeCode = rd.GetString(1);
		f_NodeShortNameEn = rd.GetString(2);
		f_NodeShortNameAr = rd.GetString(3);
		f_NodeLongNameEn = rd.GetString(4);
		f_NodeLongNameAr = rd.GetString(5);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT NodeId, NodeCode, NodeShortNameEn, NodeShortNameAr, NodeLongNameEn, NodeLongNameAr FROM Node";
		return cmd;
	}
	
	public static readonly string c_NodeId = "NodeId";
	public static readonly string c_NodeCode = "NodeCode";
	public static readonly string c_NodeShortNameEn = "NodeShortNameEn";
	public static readonly string c_NodeShortNameAr = "NodeShortNameAr";
	public static readonly string c_NodeLongNameEn = "NodeLongNameEn";
	public static readonly string c_NodeLongNameAr = "NodeLongNameAr";
}

public class RecBranch
{
	public int f_BranchId { get; set; }
	public string f_BranchName { get; set; }
	public int? f_DistanceKm { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_BranchId = rd.GetInt32(0);
		f_BranchName = rd.IsDBNull(1) ? (string)null : rd.GetString(1);
		f_DistanceKm = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT BranchId, BranchName, DistanceKm FROM Branch";
		return cmd;
	}
	
	public static readonly string c_BranchId = "BranchId";
	public static readonly string c_BranchName = "BranchName";
	public static readonly string c_DistanceKm = "DistanceKm";
}

public class RecBranchElement
{
	public int f_BranchId { get; set; }
	public int f_OrderNum { get; set; }
	public int? f_NodeId { get; set; }
	public int? f_DistanceKm { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_BranchId = rd.GetInt32(0);
		f_OrderNum = rd.GetInt32(1);
		f_NodeId = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2);
		f_DistanceKm = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT BranchId, OrderNum, NodeId, DistanceKm FROM BranchElement";
		return cmd;
	}
	
	public static readonly string c_BranchId = "BranchId";
	public static readonly string c_OrderNum = "OrderNum";
	public static readonly string c_NodeId = "NodeId";
	public static readonly string c_DistanceKm = "DistanceKm";
}

public class RecBranchGroup
{
	public int f_BranchGroupId { get; set; }
	public string f_BranchGroupName { get; set; }
	public int? f_ParentBranchGroupId { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_BranchGroupId = rd.GetInt32(0);
		f_BranchGroupName = rd.IsDBNull(1) ? (string)null : rd.GetString(1);
		f_ParentBranchGroupId = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT BranchGroupId, BranchGroupName, ParentBranchGroupId FROM BranchGroup";
		return cmd;
	}
	
	public static readonly string c_BranchGroupId = "BranchGroupId";
	public static readonly string c_BranchGroupName = "BranchGroupName";
	public static readonly string c_ParentBranchGroupId = "ParentBranchGroupId";
}

public class RecBranchGroupElement
{
	public int f_BranchGroupId { get; set; }
	public int f_OrderNum { get; set; }
	public int? f_BranchId { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_BranchGroupId = rd.GetInt32(0);
		f_OrderNum = rd.GetInt32(1);
		f_BranchId = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT BranchGroupId, OrderNum, BranchId FROM BranchGroupElement";
		return cmd;
	}
	
	public static readonly string c_BranchGroupId = "BranchGroupId";
	public static readonly string c_OrderNum = "OrderNum";
	public static readonly string c_BranchId = "BranchId";
}

public class RecRide
{
	public int f_OperatorId { get; set; }
	public int f_RideId { get; set; }
	public string f_RideCode { get; set; }
	public int? f_RideNumber { get; set; }
	public int? f_RideTypeId { get; set; }
	public string f_RideName { get; set; }
	public int? f_RideCategory { get; set; }
	public int? f_PeriodId { get; set; }
	public int? f_BranchGroupId { get; set; }
	public DateTime? f_RideStartDate { get; set; }
	public DateTime? f_RideEndDate { get; set; }
	public int? f_DepNodeId { get; set; }
	public TimeSpan? f_DepartureTime { get; set; }
	public int? f_ArrNodeId { get; set; }
	public TimeSpan? f_ArrivalTime { get; set; }
	public bool? f_IsSpecial { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_OperatorId = rd.GetInt32(0);
		f_RideId = rd.GetInt32(1);
		f_RideCode = rd.IsDBNull(2) ? (string)null : rd.GetString(2);
		f_RideNumber = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3);
		f_RideTypeId = rd.IsDBNull(4) ? (int?)null : rd.GetInt32(4);
		f_RideName = rd.IsDBNull(5) ? (string)null : rd.GetString(5);
		f_RideCategory = rd.IsDBNull(6) ? (int?)null : rd.GetInt32(6);
		f_PeriodId = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7);
		f_BranchGroupId = rd.IsDBNull(8) ? (int?)null : rd.GetInt32(8);
		f_RideStartDate = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9);
		f_RideEndDate = rd.IsDBNull(10) ? (DateTime?)null : rd.GetDateTime(10);
		f_DepNodeId = rd.IsDBNull(11) ? (int?)null : rd.GetInt32(11);
		f_DepartureTime = rd.IsDBNull(12) ? (TimeSpan?)null : rd.GetTimeSpan(12);
		f_ArrNodeId = rd.IsDBNull(13) ? (int?)null : rd.GetInt32(13);
		f_ArrivalTime = rd.IsDBNull(14) ? (TimeSpan?)null : rd.GetTimeSpan(14);
		f_IsSpecial = rd.IsDBNull(15) ? (bool?)null : rd.GetInt32(15) != 0;
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT OperatorId, RideId, RideCode, RideNumber, RideTypeId, RideName, RideCategory, PeriodId, BranchGroupId, RideStartDate, RideEndDate, DepNodeId, DepartureTime, ArrNodeId, ArrivalTime, IsSpecial FROM Ride";
		return cmd;
	}
	
	public static readonly string c_OperatorId = "OperatorId";
	public static readonly string c_RideId = "RideId";
	public static readonly string c_RideCode = "RideCode";
	public static readonly string c_RideNumber = "RideNumber";
	public static readonly string c_RideTypeId = "RideTypeId";
	public static readonly string c_RideName = "RideName";
	public static readonly string c_RideCategory = "RideCategory";
	public static readonly string c_PeriodId = "PeriodId";
	public static readonly string c_BranchGroupId = "BranchGroupId";
	public static readonly string c_RideStartDate = "RideStartDate";
	public static readonly string c_RideEndDate = "RideEndDate";
	public static readonly string c_DepNodeId = "DepNodeId";
	public static readonly string c_DepartureTime = "DepartureTime";
	public static readonly string c_ArrNodeId = "ArrNodeId";
	public static readonly string c_ArrivalTime = "ArrivalTime";
	public static readonly string c_IsSpecial = "IsSpecial";
}

public class RecRideStop
{
	public int f_OperatorId { get; set; }
	public int f_RideId { get; set; }
	public int f_OrderNum { get; set; }
	public int? f_NodeId { get; set; }
	public int? f_ArrivalDay { get; set; }
	public TimeSpan? f_ArrivalTime { get; set; }
	public TimeSpan? f_DepartureTime { get; set; }
	
	public void Read(SQLiteDataReader rd)
	{
		f_OperatorId = rd.GetInt32(0);
		f_RideId = rd.GetInt32(1);
		f_OrderNum = rd.GetInt32(2);
		f_NodeId = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3);
		f_ArrivalDay = rd.IsDBNull(4) ? (int?)null : rd.GetInt32(4);
		f_ArrivalTime = rd.IsDBNull(5) ? (TimeSpan?)null : rd.GetTimeSpan(5);
		f_DepartureTime = rd.IsDBNull(6) ? (TimeSpan?)null : rd.GetTimeSpan(6);
	}
	
	public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)
	{
		var cmd = cn.CreateCommand();
		cmd.CommandText = @"SELECT OperatorId, RideId, OrderNum, NodeId, ArrivalDay, ArrivalTime, DepartureTime FROM RideStop";
		return cmd;
	}
	
	public static readonly string c_OperatorId = "OperatorId";
	public static readonly string c_RideId = "RideId";
	public static readonly string c_OrderNum = "OrderNum";
	public static readonly string c_NodeId = "NodeId";
	public static readonly string c_ArrivalDay = "ArrivalDay";
	public static readonly string c_ArrivalTime = "ArrivalTime";
	public static readonly string c_DepartureTime = "DepartureTime";
}
