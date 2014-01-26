using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace CodeGen
{
	internal class SqlConnectionCloser : IDisposable
	{
		private SqlConnection _cn;
		private bool _cnToBeClosed;

		public SqlConnectionCloser(SqlConnection cn)
		{
			_cn = cn;
			if (_cn.State == ConnectionState.Closed)
			{
				_cn.Open();
				_cnToBeClosed = true;
			}
			else
				_cnToBeClosed = false;
		}

		public void Dispose()
		{
			if (_cnToBeClosed)
				_cn.Close();
		}

		public static implicit operator SqlConnection(SqlConnectionCloser cc)
		{
			return cc._cn;
		}

		public SqlConnection Connection { get { return _cn; } }
	}

	internal class OleDbConnectionCloser : IDisposable
	{
		private OleDbConnection _cn;
		private bool _cnToBeClosed;

		public OleDbConnectionCloser(OleDbConnection cn)
		{
			_cn = cn;
			if (_cn.State == ConnectionState.Closed)
			{
				_cn.Open();
				_cnToBeClosed = true;
			}
			else
				_cnToBeClosed = false;
		}

		public void Dispose()
		{
			if (_cnToBeClosed)
				_cn.Close();
		}

		public static implicit operator OleDbConnection(OleDbConnectionCloser cc)
		{
			return cc._cn;
		}

		public OleDbConnection Connection { get { return _cn; } }
	}

	/// <summary>
	/// Summary description for DbUtility.
	/// </summary>
	public class DbUtility
	{
		private static StringCollection s_colTypeWithPrecision = null;
		private static StringCollection s_colStandardType = null;

		static DbUtility()
		{
			s_colTypeWithPrecision = new StringCollection();
			s_colTypeWithPrecision.Add("char");
			s_colTypeWithPrecision.Add("nchar");
			s_colTypeWithPrecision.Add("varchar");
			s_colTypeWithPrecision.Add("nvarchar");
			s_colTypeWithPrecision.Add("binary");
			s_colTypeWithPrecision.Add("varbinary");

			s_colStandardType = new StringCollection();
			s_colStandardType.Add("sql_variant");
			s_colStandardType.Add("uniqueidentifier");
			s_colStandardType.Add("ntext");
			s_colStandardType.Add("nvarchar");
			s_colStandardType.Add("sysname");
			s_colStandardType.Add("nchar");
			s_colStandardType.Add("bit");
			s_colStandardType.Add("tinyint");
			s_colStandardType.Add("bigint");
			s_colStandardType.Add("image");
			s_colStandardType.Add("varbinary");
			s_colStandardType.Add("binary");
			s_colStandardType.Add("timestamp");
			s_colStandardType.Add("text");
			s_colStandardType.Add("char");
			s_colStandardType.Add("numeric");
			s_colStandardType.Add("decimal");
			s_colStandardType.Add("money");
			s_colStandardType.Add("smallmoney");
			s_colStandardType.Add("int");
			s_colStandardType.Add("smallint");
			s_colStandardType.Add("float");
			s_colStandardType.Add("real");
			s_colStandardType.Add("datetime");
			s_colStandardType.Add("smalldatetime");
			s_colStandardType.Add("varchar");
		}

		public static SqlParameter[] Derive(SqlConnection cn, string spName)
		{
			SqlCommand cmd = new SqlCommand("sp_procedure_params_rowset", cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@procedure_name", spName);

			ArrayList ret = new ArrayList();
			try
			{
				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						SqlParameter prm = new SqlParameter();
						prm.ParameterName = (string) rd["PARAMETER_NAME"];
						prm.SqlDbType = GetSqlDbTypeFromOleDbType((short) rd["DATA_TYPE"], (string) rd["TYPE_NAME"]);
						object objCML = rd["CHARACTER_MAXIMUM_LENGTH"];
						if (objCML is int)
							prm.Size = (int) objCML;

						prm.Direction = ParameterDirectionFromOleDbDirection((short) rd["PARAMETER_TYPE"]);
						if (prm.SqlDbType == SqlDbType.Decimal)
						{
							prm.Scale = (byte) (((short) rd["NUMERIC_SCALE"]) & 0xff);
							prm.Precision = (byte) (((short) rd["NUMERIC_PRECISION"]) & 0xff);
						}
						ret.Add(prm);
					}
				}
			}
			finally
			{
				cmd.Connection = null;
			}
			if (ret.Count == 0)
				throw new Exception("ADP.NoStoredProcedureExists(this.CommandText)");

			return (SqlParameter[]) ret.ToArray(typeof (SqlParameter));
		}

		public static SqlDbType GetSqlDbTypeFromOleDbType(short dbType, string typeName)
		{
			OleDbType oleDbType = (OleDbType) dbType;

			switch (oleDbType)
			{
			case OleDbType.SmallInt:
			case OleDbType.UnsignedSmallInt:
				return SqlDbType.SmallInt;

			case OleDbType.Integer:
				return SqlDbType.Int;

			case OleDbType.Single:
				return SqlDbType.Real;

			case OleDbType.Double:
				return SqlDbType.Float;

			case OleDbType.Currency:
				return (typeName == "money") ? SqlDbType.Money : SqlDbType.SmallMoney;

			case OleDbType.Date:
			case OleDbType.Filetime:
			case OleDbType.DBDate:
			case OleDbType.DBTime:
			case OleDbType.DBTimeStamp:
				return (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;

			case OleDbType.IDispatch:
			case OleDbType.Error:
			case OleDbType.IUnknown:
			case ((OleDbType) 15):
			case OleDbType.UnsignedInt:
			case OleDbType.Variant:
			case (OleDbType.Binary | OleDbType.Single):
				return SqlDbType.Variant;

			case OleDbType.Boolean:
				return SqlDbType.Bit;

			case OleDbType.Decimal:
			case OleDbType.Numeric:
				return SqlDbType.Decimal;

			case OleDbType.TinyInt:
			case OleDbType.UnsignedTinyInt:
				return SqlDbType.TinyInt;

			case OleDbType.BigInt:
				return SqlDbType.BigInt;

			case OleDbType.Guid:
				return SqlDbType.UniqueIdentifier;

			case OleDbType.Binary:
			case OleDbType.VarBinary:
				return (typeName == "binary") ? SqlDbType.Binary : SqlDbType.VarBinary;

			case OleDbType.Char:
			case OleDbType.VarChar:
				return (typeName == "char") ? SqlDbType.Char : SqlDbType.VarChar;

			case OleDbType.WChar:
			case OleDbType.VarWChar:
			case OleDbType.BSTR:
				return (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;

			case OleDbType.LongVarChar:
				return SqlDbType.Text;

			case OleDbType.LongVarWChar:
				return SqlDbType.NText;

			case OleDbType.LongVarBinary:
				return SqlDbType.Image;

			default:
				return SqlDbType.Variant;
			}
		}

		public static ParameterDirection ParameterDirectionFromOleDbDirection(short oledbDirection)
		{
			switch (oledbDirection)
			{
			case 2:
				return ParameterDirection.InputOutput;
			case 3:
				return ParameterDirection.Output;
			case 4:
				return ParameterDirection.ReturnValue;
			default:
				return ParameterDirection.Input;
			}
		}

		public static StringCollection GetDbList(SqlConnection cn)
		{
			StringCollection r = new StringCollection();

			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText = "select * from master..sysdatabases";
				cmd.CommandType = CommandType.Text;

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						string dbName = rd.GetString(0);
						r.Add(dbName);
					}
				}

				cmd.Connection = null;
			}

			return r;
		}

		public static void UseDb(SqlConnection cn, string dbName)
		{
			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText = "use [" + dbName + "]";
				cmd.CommandType = CommandType.Text;
				cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		public static StringCollection GetPKCols(SqlConnection cn, string tbName)
		{
			StringCollection r = new StringCollection();

			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText = "sp_pkeys";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@table_name", tbName);

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						string colName = (string) rd["COLUMN_NAME"];
						short keypos = (short) rd["KEY_SEQ"];
						r.Insert(keypos - 1, colName);
					}
				}
				cmd.Connection = null;
			}

			return r;
		}

		public static void GetTableViewList(SqlConnection cn, string dbName, out StringCollection tables,
		                                    out StringCollection views)
		{
			tables = new StringCollection();
			views = new StringCollection();

			UseDb(cn, dbName);

			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText = "sp_tables";
				cmd.CommandType = CommandType.StoredProcedure;

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						string tbName = (string) rd["TABLE_NAME"];
						string tbType = (string) rd["TABLE_TYPE"];
						if (tbType == "TABLE")
						{
							tables.Add(tbName);
						}
						else if (tbType == "VIEW")
						{
							views.Add(tbName);
						}
					}
				}

				cmd.Connection = null;
			}
		}

		public static DbColumnData[] GetColumnList(SqlConnection cn, string dbName, string tbName)
		{
			ArrayList r = new ArrayList();

			Hashtable userType2dbType = UserType2DbType(cn, dbName);

			UseDb(cn, dbName);

			StringCollection pk = GetPKCols(cn, tbName);


			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText = "sp_columns";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@table_name", tbName);

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					int ordScale = rd.GetOrdinal("SCALE");

					while (rd.Read())
					{
						DbColumnData col = new DbColumnData();
						col.Name = (string) rd["COLUMN_NAME"];

						// questo nome e` il tipo colonna eventualmente tipo "user defined data type"
						col.DbType = (string) rd["TYPE_NAME"];
						if (col.DbType.EndsWith(" identity"))
						{
							int idx = col.DbType.IndexOf(" identity");
							col.Identity = true;
							col.DbType = col.DbType.Remove(idx, " identity".Length);
						}


						col.Precision = (int) rd["PRECISION"];
						if (!s_colTypeWithPrecision.Contains(col.DbType))
							col.Precision = 0;

						col.Scale = 0;
						if (!rd.IsDBNull(ordScale))
							col.Scale = rd.GetInt16(ordScale);

						if (pk.Contains(col.Name))
							col.Nullable = ColNullableType.PK;
						else
							col.Nullable = ((short) rd["NULLABLE"] == 1) ? ColNullableType.Null : ColNullableType.NotNull;


						if (s_colStandardType.Contains(col.DbType))
							col.UserType = false;
						else
						{
							col.UserType = true;

							if (!col.UserType) continue;
							col.UserType_DbType = (string) userType2dbType[col.DbType];
							col.UserType_Precision = (int) rd["PRECISION"];
							if (!s_colTypeWithPrecision.Contains(col.UserType_DbType))
								col.UserType_Precision = 0;

							col.UserType_Scale = 0;
							if (!rd.IsDBNull(ordScale))
								col.UserType_Scale = rd.GetInt16(ordScale);
						}

						r.Add(col);
					}
				}

				cmd.Connection = null;

				return (DbColumnData[]) r.ToArray(typeof (DbColumnData));
			}
		}

		public static StringCollection GetSPList(SqlConnection cn, string dbName)
		{
			StringCollection r = new StringCollection();

			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText =
					string.Format(
						@"
				select 
				o.name 
				from 
				[{0}].dbo.sysobjects o 
				where 
				xtype='P'
				and o.name not like N'#%%'  
				and o.name not like N'dt_%'  
				order by o.name
				",
						dbName);
				cmd.CommandType = CommandType.Text;

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						string sp = (string) rd["name"];
						r.Add(sp);
					}
				}
				cmd.Connection = null;
			}
			return r;
		}

		public static SqlParameter[] GetSPArgumentList(SqlConnection cn, string dbName, string spName)
		{
			UseDb(cn, dbName);

			SqlCommand cmd = new SqlCommand("sp_procedure_params_rowset", cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@procedure_name", spName);


			ArrayList ret = new ArrayList();
			using (SqlDataReader rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					SqlParameter prm = new SqlParameter();
					prm.ParameterName = (string) rd["PARAMETER_NAME"];
					prm.SqlDbType = GetSqlDbTypeFromOleDbType((short) rd["DATA_TYPE"], (string) rd["TYPE_NAME"]);
					object objCML = rd["CHARACTER_MAXIMUM_LENGTH"];
					if (objCML is int)
						prm.Size = (int) objCML;

					prm.Direction = ParameterDirectionFromOleDbDirection((short) rd["PARAMETER_TYPE"]);
					if (prm.SqlDbType == SqlDbType.Decimal)
					{
						prm.Scale = (byte) (((short) rd["NUMERIC_SCALE"]) & 0xff);
						prm.Precision = (byte) (((short) rd["NUMERIC_PRECISION"]) & 0xff);
					}
					else
					{
						prm.Scale = 0;
						prm.Precision = 0;
					}

					ret.Add(prm);
				}
			}

			return (SqlParameter[]) ret.ToArray(typeof (SqlParameter));
		}

		public static Hashtable UserType2DbType(SqlConnection cn, string dbName)
		{
			UseDb(cn, dbName);

			Hashtable ret = new Hashtable();

			SqlCommand cmd = new SqlCommand("select xtype, name from systypes", cn);
			cmd.CommandType = CommandType.Text;

			using (SqlDataReader rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					int xtype = System.Convert.ToInt32(rd[0]);
					string userType = rd.GetString(1);

					if (s_colStandardType.Contains(userType)) continue;

					// tipo user defined.

					string dbType;
					switch (xtype)
					{
					case 127:
						dbType = "bigint";
						break;
					case 173:
						dbType = "binary";
						break;
					case 104:
						dbType = "bit";
						break;
					case 175:
						dbType = "char";
						break;
					case 61:
						dbType = "datetime";
						break;
					case 106:
						dbType = "decimal";
						break;
					case 62:
						dbType = "float";
						break;
					case 34:
						dbType = "image";
						break;
					case 56:
						dbType = "int";
						break;
					case 60:
						dbType = "money";
						break;
					case 239:
						dbType = "nchar";
						break;
					case 99:
						dbType = "ntext";
						break;
					case 108:
						dbType = "numeric";
						break;
					case 231:
						dbType = "nvarchar";
						break;
					case 59:
						dbType = "real";
						break;
					case 58:
						dbType = "smalldatetime";
						break;
					case 52:
						dbType = "smallint";
						break;
					case 122:
						dbType = "smallmoney";
						break;
					case 98:
						dbType = "sql_variant";
						break;
						//case 231 :tt =  "sysname";break;
					case 35:
						dbType = "text";
						break;
					case 189:
						dbType = "timestamp";
						break;
					case 48:
						dbType = "tinyint";
						break;
					case 36:
						dbType = "uniqueidentifier";
						break;
					case 165:
						dbType = "varbinary";
						break;
					case 167:
						dbType = "varchar";
						break;
					default:
						dbType = "__error__";
						break;
					}

					ret.Add(userType, dbType);
				}
			}
			return ret;
		}

		//		public static DataTable[] GetSPTableResult(SqlConnection cn, string dbName, string spName)
		//		{
		//			UseDb(cn, dbName);
		//			SqlParameter[] args = Derive(cn, spName);
		//			return GetSPTableResult(cn, dbName, spName, args);
		//		}
		//
		//		public static DataTable[] GetSPTableResult(SqlConnection cn, string dbName, string spName, SqlParameter[] args)
		//		{
		//			if (cn.State == ConnectionState.Open)
		//				cn.Close();
		//
		//
		//			DataTable tb = null;
		//
		//			SqlTransaction tr = null;
		//
		//			ArrayList ret = new ArrayList();
		//
		//			try
		//			{
		//				cn.Open();
		//
		//				SqlCommand cmd = cn.CreateCommand();
		//				cmd.Transaction = tr;
		//
		//				cmd.CommandText = "use [" + dbName + "]";
		//				cmd.CommandType = CommandType.Text;
		//				cmd.Parameters.Clear();
		//				cmd.ExecuteNonQuery();
		//
		//
		//				tr = cn.BeginTransaction();
		//
		//				cmd.Transaction = tr;
		//				cmd.CommandText = spName;
		//				cmd.CommandType = CommandType.StoredProcedure;
		//				cmd.Parameters.Clear();
		//				foreach (SqlParameter a in args)
		//				{
		//					if (a.Direction == ParameterDirection.Input || a.Direction == ParameterDirection.InputOutput)
		//					{
		//						cmd.Parameters.Add(a);
		//						a.Value = CreateObject(a.SqlDbType);
		//					}
		//				}
		//
		//
		//				using (SqlDataReader rd = cmd.ExecuteReader(CommandBehavior.KeyInfo /*| CommandBehavior.SchemaOnly*/))
		//				{
		//					do
		//					{
		//						tb = rd.GetSchemaTable();
		//						if (tb != null)
		//						{
		//							ret.Add(tb.Copy());
		//							tb.Clear();
		//						}
		//					} while (rd.NextResult());
		//				}
		//
		//				tr.Rollback();
		//				tr = null;
		//
		//
		//				cmd.Connection = null;
		//			}
		//			finally
		//			{
		//				if (tr != null) tr.Rollback();
		//			}
		//
		//
		//			return (DataTable[]) ret.ToArray(typeof (DataTable));
		//		}
		//
		//		public static DataTable[] GetSPTableResult2(SqlConnection cn, string dbName, string spName)
		//		{
		//			UseDb(cn, dbName);
		//			SqlParameter[] args = Derive(cn, spName);
		//
		//
		//			if (cn.State == ConnectionState.Open)
		//				cn.Close();
		//
		//
		//			DataTable tb = null;
		//
		//			SqlTransaction tr = null;
		//
		//			ArrayList ret = new ArrayList();
		//
		//			try
		//			{
		//				cn.Open();
		//
		//				SqlCommand cmd = cn.CreateCommand();
		//
		//				cmd.Transaction = tr;
		//				cmd.CommandText = "use [" + dbName + "]";
		//				cmd.CommandType = CommandType.Text;
		//				cmd.Parameters.Clear();
		//				cmd.ExecuteNonQuery();
		//
		//				cmd.Transaction = tr;
		//				cmd.CommandText = "SET FMTONLY OFF; SET NO_BROWSETABLE ON; SET FMTONLY ON";
		//				cmd.CommandType = CommandType.Text;
		//				cmd.Parameters.Clear();
		//				cmd.ExecuteNonQuery();
		//
		//				tr = cn.BeginTransaction();
		//
		//				cmd.Transaction = tr;
		//				cmd.CommandText = spName;
		//				cmd.CommandType = CommandType.StoredProcedure;
		//				cmd.Parameters.Clear();
		//				foreach (SqlParameter a in args)
		//				{
		//					if (a.Direction == ParameterDirection.Input || a.Direction == ParameterDirection.InputOutput)
		//					{
		//						cmd.Parameters.Add(a);
		//						a.Value = DBNull.Value;
		//					}
		//				}
		//
		//
		//				using (SqlDataReader rd = cmd.ExecuteReader())
		//				{
		//					do
		//					{
		//						int fc = rd.FieldCount;
		//						tb = rd.GetSchemaTable();
		//						if (tb != null)
		//						{
		//							ret.Add(tb.Copy());
		//							tb.Clear();
		//						}
		//					} while (rd.NextResult());
		//				}
		//
		//				tr.Rollback();
		//				tr = null;
		//
		//				cmd.Transaction = tr;
		//				cmd.CommandText = "SET FMTONLY OFF; SET NO_BROWSETABLE ON";
		//				cmd.CommandType = CommandType.Text;
		//				cmd.Parameters.Clear();
		//				cmd.ExecuteNonQuery();
		//
		//
		//				cmd.Connection = null;
		//			}
		//			finally
		//			{
		//				if (tr != null) tr.Rollback();
		//			}
		//
		//
		//			return (DataTable[]) ret.ToArray(typeof (DataTable));
		//		}
		//

		public static StringCollection GetFNList(SqlConnection cn, string dbName)
		{
			StringCollection r = new StringCollection();

			using (SqlCommand cmd = cn.CreateCommand())
			{
				cmd.CommandText =
					string.Format(
						@"
				select 
				id, 
				owner = user_name(uid), 
				name,
				type 
				from [{0}].dbo.sysobjects 
				where 
				type = N'TF' or 
				type = N'IF' or 
				type = N'FN' 
				order by name",
						dbName);

				cmd.CommandType = CommandType.Text;

				using (SqlDataReader rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						string fn = (string) rd["name"];
						r.Add(fn);
					}
				}
				cmd.Connection = null;
			}

			return r;
		}

		public static object CreateObject(SqlDbType e)
		{
			switch (e)
			{
			case SqlDbType.BigInt:
				return (Int64) 0;

			case SqlDbType.Binary:
				return new Byte[0];

			case SqlDbType.Bit:
				return false;

			case SqlDbType.Char:
				return ' ';

			case SqlDbType.DateTime:
				return DateTime.Now;

			case SqlDbType.Decimal:
				return (Decimal) 0;

			case SqlDbType.Float:
				return (Double) 0;

			case SqlDbType.Image:
				return new Byte[0];

			case SqlDbType.Int:
				return /*(Int32)*/ 0;

			case SqlDbType.Money:
				return (Decimal) 0;

			case SqlDbType.NChar:
				return string.Empty;

			case SqlDbType.NText:
				return string.Empty;

			case SqlDbType.NVarChar:
				return string.Empty;

			case SqlDbType.Real:
				return (Single) 0;

			case SqlDbType.SmallDateTime:
				return DateTime.Now;

			case SqlDbType.SmallInt:
				return (Int16) 0;

			case SqlDbType.SmallMoney:
				return (Decimal) 0;

			case SqlDbType.Text:
				return string.Empty;

			case SqlDbType.Timestamp:
				return new Byte[0];

			case SqlDbType.TinyInt:
				return (Byte) 0;

			case SqlDbType.UniqueIdentifier:
				return Guid.NewGuid();

			case SqlDbType.VarBinary:
				return new Byte[0];

			case SqlDbType.VarChar:
				return string.Empty;

			case SqlDbType.Variant:
				return new Object();

			default:
				return new object();
			}
		}

		public static Type Convert(SqlDbType e)
		{
			switch (e)
			{
			case SqlDbType.BigInt:
				return typeof (Int64);

			case SqlDbType.Binary:
				return typeof (Byte[]);

			case SqlDbType.Bit:
				return typeof (Boolean);

			case SqlDbType.Char:
				return typeof (String);

			case SqlDbType.DateTime:
				return typeof (DateTime);

			case SqlDbType.Decimal:
				return typeof (Decimal);

			case SqlDbType.Float:
				return typeof (Double);

			case SqlDbType.Image:
				return typeof (Byte[]);

			case SqlDbType.Int:
				return typeof (Int32);

			case SqlDbType.Money:
				return typeof (Decimal);

			case SqlDbType.NChar:
				return typeof (String);

			case SqlDbType.NText:
				return typeof (String);

			case SqlDbType.NVarChar:
				return typeof (String);

			case SqlDbType.Real:
				return typeof (Single);

			case SqlDbType.SmallDateTime:
				return typeof (DateTime);

			case SqlDbType.SmallInt:
				return typeof (Int16);

			case SqlDbType.SmallMoney:
				return typeof (Decimal);

			case SqlDbType.Text:
				return typeof (String);

			case SqlDbType.Timestamp:
				return typeof (Byte[]);

			case SqlDbType.TinyInt:
				return typeof (Byte);

			case SqlDbType.UniqueIdentifier:
				return typeof (Guid);

			case SqlDbType.VarBinary:
				return typeof (Byte[]);

			case SqlDbType.VarChar:
				return typeof (string);

			case SqlDbType.Variant:
				return typeof (Object);

			default:
				return typeof (object);
			}
		}

		public static DataTable[] GetSPTableResult(DbServer srv, string dbName, string spName)
		{
			SqlParameter[] args;
			using (SqlConnectionCloser sqlConnectionCloser = new SqlConnectionCloser(srv.SqlConnection))
			{
				UseDb(sqlConnectionCloser.Connection, dbName);
				args = Derive(sqlConnectionCloser.Connection, spName);
			}


			DataTable[] r = null;
			try
			{
				r = GetSPTableResultNoExecute(srv, dbName, spName, args);
			}
			catch
			{
			}

			if (r == null)
				r = GetSPTableResultExecute(srv, dbName, spName, args);

			return r;
		}

		/// <summary>
		/// Questa versione funziona se la SP non usa tabelle temporanee.
		/// LA SP non viene realmente eseguita (ed e` per questo che non trovando la tabella temporanea
		/// non funziona).
		/// </summary>
		/// <param name="srv"></param>
		/// <param name="dbName"></param>
		/// <param name="spName"></param>
		/// <returns></returns>
		public static DataTable[] GetSPTableResultNoExecute(DbServer srv, string dbName, string spName, SqlParameter[] args)
		{
			using (OleDbConnectionCloser cc = new OleDbConnectionCloser(srv.OleDbConnection))
			{
				OleDbConnection cn = cc.Connection;

				if (cn.State == ConnectionState.Open)
					cn.Close();

				DataTable tb;

				OleDbTransaction tr = null;

				ArrayList ret = new ArrayList();

				try
				{
					cn.Open();

					OleDbCommand cmd = cn.CreateCommand();
					cmd.Transaction = tr;

					cmd.CommandText = "use [" + dbName + "]";
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Clear();
					cmd.ExecuteNonQuery();

					cmd.Transaction = tr;
					cmd.CommandText = "SET FMTONLY OFF; SET NO_BROWSETABLE ON; SET FMTONLY ON";
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Clear();
					cmd.ExecuteNonQuery();


					tr = cn.BeginTransaction();

					cmd.Transaction = tr;
					cmd.CommandText = spName;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Clear();
					// TODO parametri

					foreach (SqlParameter a in args)
					{
						if (a.Direction == ParameterDirection.Input || a.Direction == ParameterDirection.InputOutput)
						{
							cmd.Parameters.AddWithValue(a.ParameterName, CreateObject(a.SqlDbType));
						}
					}


					using (OleDbDataReader rd = cmd.ExecuteReader())
					{
						do
						{
							tb = rd.GetSchemaTable();
							if (tb != null) ret.Add(tb);
						}
						while (rd.NextResult());
					}

					tr.Rollback();
					tr = null;

					cmd.Transaction = tr;
					cmd.CommandText = "SET FMTONLY OFF; SET NO_BROWSETABLE ON";
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Clear();
					cmd.ExecuteNonQuery();

					cmd.Connection = null;
				}
				finally
				{
					if (tr != null) tr.Rollback();
					if (cn.State == ConnectionState.Open) cn.Close();
				}
				return (DataTable[]) ret.ToArray(typeof (DataTable));
			}
		}

		/// <summary>
		/// Questa versione funziona anche per le SP che hanno tabelle temporanee.
		/// La SP viene ESEGUITA !!!!!!
		/// Alla fine dell'esecuzione si fa rollback per non sporcare il DB 
		/// </summary>
		/// <param name="srv"></param>
		/// <param name="dbName"></param>
		/// <param name="spName"></param>
		/// <returns></returns>
		public static DataTable[] GetSPTableResultExecute(DbServer srv, string dbName, string spName, SqlParameter[] args)
		{
			using (OleDbConnectionCloser cc = new OleDbConnectionCloser(srv.OleDbConnection))
			{
				OleDbConnection cn = cc.Connection;

				if (cn.State == ConnectionState.Open)
					cn.Close();


				DataTable tb;

				OleDbTransaction tr = null;

				ArrayList ret = new ArrayList();

				try
				{
					cn.Open();

					OleDbCommand cmd = cn.CreateCommand();
					cmd.Transaction = tr;

					cmd.CommandText = "use [" + dbName + "]";
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Clear();
					cmd.ExecuteNonQuery();


					tr = cn.BeginTransaction();

					cmd.Transaction = tr;
					cmd.CommandText = spName;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Clear();
					foreach (SqlParameter a in args)
					{
						if (a.Direction == ParameterDirection.Input || a.Direction == ParameterDirection.InputOutput)
						{
							if (a.SqlDbType == SqlDbType.Char)
								cmd.Parameters.AddWithValue(a.ParameterName, " ");
							else
								cmd.Parameters.AddWithValue(a.ParameterName, CreateObject(a.SqlDbType));
						}
					}

					using (OleDbDataReader rd = cmd.ExecuteReader(CommandBehavior.KeyInfo))
					{
						do
						{
							tb = rd.GetSchemaTable();
							if (tb != null)
								ret.Add(tb);
						}
						while (rd.NextResult());
					}

					tr.Rollback();
					tr = null;


					cmd.Connection = null;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
				finally
				{
					if (tr != null) tr.Rollback();
				}

				return (DataTable[]) ret.ToArray(typeof (DataTable));
			}
		}
	}

	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////
	public class DbServer
	{
		public DbServer(string connectionString)
		{
			_SqlConnection = new SqlConnection(connectionString);
			_OleDbConnection = new OleDbConnection("Provider=\"SQLOLEDB.1\";" + connectionString);
		}

		public DbServer()
			: this(ConfigurationManager.AppSettings["ConnectionString"])
		{
		}

		public SqlConnection SqlConnection { get { return _SqlConnection; } }

		public OleDbConnection OleDbConnection { get { return _OleDbConnection; } }

		public DbInstList GetDB()
		{
			using (SqlConnectionCloser cc = new SqlConnectionCloser(_SqlConnection))
			{
				StringCollection dbl = DbUtility.GetDbList(cc);
				DbInstList r = new DbInstList();
				for (int i = 0; i < dbl.Count; ++i)
					r.Add(new DbInst(this, dbl[i]));
				return r;
			}
		}

		public DbInst GetDB(string dbName)
		{
			DbInstList r = GetDB();
			int idx = r.IndexOf(dbName);
			if (idx >= 0) return r[idx];
			return null;
		}

		private readonly SqlConnection _SqlConnection;
		private readonly OleDbConnection _OleDbConnection;

		public static string GetCSharpVariableType(SqlDbType t)
		{
			switch (t)
			{
			case SqlDbType.BigInt:
				return "long";
			case SqlDbType.Binary:
				return "byte []";
			case SqlDbType.Bit:
				return "bool";
			case SqlDbType.Char:
				return "string";
			case SqlDbType.DateTime:
				return "DateTime";
			case SqlDbType.Decimal:
				return "decimal";
			case SqlDbType.Float:
				return "double";
			case SqlDbType.Image:
				return "byte []";
			case SqlDbType.Int:
				return "int";
			case SqlDbType.Money:
				return "decimal";
			case SqlDbType.NChar:
				return "string";
			case SqlDbType.NText:
				return "string";
			case SqlDbType.NVarChar:
				return "string";
			case SqlDbType.Real:
				return "float";
			case SqlDbType.SmallDateTime:
				return "DateTime";
			case SqlDbType.SmallInt:
				return "short";
			case SqlDbType.SmallMoney:
				return "decimal";
			case SqlDbType.Text:
				return "string";
			case SqlDbType.Timestamp:
				return "byte []";
			case SqlDbType.TinyInt:
				return "byte";
			case SqlDbType.UniqueIdentifier:
				return "Guid";
			case SqlDbType.VarBinary:
				return "byte []";
			case SqlDbType.VarChar:
				return "string";
			case SqlDbType.Variant:
				return "object";
			default:
				return "object";
			}
		}

		public static string GetSqlType(SqlDbType t)
		{
			switch (t)
			{
			case SqlDbType.BigInt:
				return typeof (SqlInt64).Name;
			case SqlDbType.Binary:
				return typeof (SqlBinary).Name;
			case SqlDbType.Bit:
				return typeof (SqlBoolean).Name;
			case SqlDbType.Char:
				return typeof (SqlString).Name;
			case SqlDbType.DateTime:
				return typeof (SqlDateTime).Name;
			case SqlDbType.Decimal:
				return typeof (SqlDecimal).Name;
			case SqlDbType.Float:
				return typeof (SqlDouble).Name;
			case SqlDbType.Image:
				return typeof (SqlBinary).Name;
			case SqlDbType.Int:
				return typeof (SqlInt32).Name;
			case SqlDbType.Money:
				return typeof (SqlDecimal).Name;
			case SqlDbType.NChar:
				return typeof (SqlString).Name;
			case SqlDbType.NText:
				return typeof (SqlString).Name;
			case SqlDbType.NVarChar:
				return typeof (SqlString).Name;
			case SqlDbType.Real:
				return typeof (SqlSingle).Name;
			case SqlDbType.SmallDateTime:
				return typeof (SqlDateTime).Name;
			case SqlDbType.SmallInt:
				return typeof (SqlInt16).Name;
			case SqlDbType.SmallMoney:
				return typeof (SqlDecimal).Name;
			case SqlDbType.Text:
				return typeof (SqlString).Name;
			case SqlDbType.Timestamp:
				return typeof (SqlBinary).Name;
			case SqlDbType.TinyInt:
				return typeof (SqlByte).Name;
			case SqlDbType.UniqueIdentifier:
				return typeof (SqlGuid).Name;
			case SqlDbType.VarBinary:
				return typeof (SqlBinary).Name;
			case SqlDbType.VarChar:
				return typeof (SqlString).Name;
			case SqlDbType.Variant:
				return "__UNSUPPORTED__" + t.ToString();
			default:
				return "__UNSUPPORTED__" + t.ToString();
			}
		}
	}

	public class DbInst
	{
		private readonly DbServer _svr;
		private readonly string _name;

		public string Name { get { return _name; } }

		internal DbInst(DbServer s, string n)
		{
			_svr = s;
			_name = n;
		}

		public DbServer Server { get { return _svr; } }

		public DbTableList Tables
		{
			get
			{
				if (_tables == null)
				{
					using (SqlConnectionCloser cc = new SqlConnectionCloser(Server.SqlConnection))
					{
						StringCollection tables;
						StringCollection views;
						DbUtility.GetTableViewList(cc, _name, out tables, out views);

						_tables = new DbTableList();
						for (int i = 0; i < tables.Count; ++i)
							_tables.Add(new DbTable(this, tables[i]));
					}
				}
				return _tables;
			}
		}

		private DbTableList _tables = null;

		public DbViewList Views
		{
			get
			{
				if (_views == null)
				{
					using (SqlConnectionCloser cc = new SqlConnectionCloser(Server.SqlConnection))
					{
						StringCollection tables;
						StringCollection views;
						DbUtility.GetTableViewList(cc, _name, out tables, out views);

						_views = new DbViewList();
						for (int i = 0; i < views.Count; ++i)
							_views.Add(new DbView(this, views[i]));
					}
				}
				return _views;
			}
		}

		private DbViewList _views = null;

		public DbStoredProcedureList StoredProcedures
		{
			get
			{
				if (_storedProcedures == null)
				{
					using (SqlConnectionCloser cc = new SqlConnectionCloser(Server.SqlConnection))
					{
						StringCollection sp = DbUtility.GetSPList(cc, _name);

						_storedProcedures = new DbStoredProcedureList();
						for (int i = 0; i < sp.Count; ++i)
							_storedProcedures.Add(new DbStoredProcedure(this, sp[i]));
					}
				}
				return _storedProcedures;
			}
		}

		private DbStoredProcedureList _storedProcedures = null;
	}

	public class DbTable
	{
		private DbInst _db;
		private string _name;

		public DbTable(DbInst db, string n)
		{
			_db = db;
			_name = n;
		}

		public string Name { get { return _name; } }
		public DbServer Server { get { return _db.Server; } }
		public DbInst DbInst { get { return _db; } }

		private DbColumnDataList _columns = null;

		public DbColumnDataList Columns
		{
			get
			{
				if (_columns == null)
				{
					using (SqlConnectionCloser cc = new SqlConnectionCloser(Server.SqlConnection))
					{
						DbColumnData[] r = DbUtility.GetColumnList(cc, _db.Name, Name);
						_columns = new DbColumnDataList(r);
					}
				}
				return _columns;
			}
		}
	}

	public class DbView : DbTable
	{
		public DbView(DbInst db, string n)
			: base(db, n)
		{
		}
	}

	public enum ColNullableType
	{
		PK,
		NotNull,
		Null,
	}

	public class DbColumnData
	{
		public string Name;
		public string DbType;
		public int Precision;
		public int Scale;
		public ColNullableType Nullable;
		public bool Identity;

		public bool UserType;
		public string UserType_DbType;
		public int UserType_Precision;
		public int UserType_Scale;

		public override String ToString()
		{
			if (Precision == 0)
				return string.Format("{0} ({1}, {2})", Name, DbType, Nullable);
			if (Scale == 0)
				return string.Format("{0} ({1}({2}), {3})", Name, DbType, Precision, Nullable);

			return string.Format("{0} ({1}({2},{3}), {4})", Name, DbType, Precision, Scale, Nullable);
		}
	}

	public class DbStoredProcedure
	{
		private DbInst _db;
		private string _name;

		public string Name { get { return _name; } }

		internal DbServer Server { get { return _db.Server; } }

		public DbStoredProcedure(DbInst db, string n)
		{
			_db = db;
			_name = n;
		}

		public SqlParameterList Parameters
		{
			get
			{
				if (_parameters == null)
				{
					using (SqlConnectionCloser cc = new SqlConnectionCloser(Server.SqlConnection))
					{
						SqlParameter[] parameters = DbUtility.GetSPArgumentList(cc, _db.Name, this.Name);
						_parameters = new SqlParameterList(parameters);
					}
				}
				return _parameters;
			}
		}

		private SqlParameterList _parameters;

		public DbCommandResultList CommandResult
		{
			get
			{
				if (_CommandResult == null)
				{
					DataTable[] r = DbUtility.GetSPTableResult(Server, _db.Name, this.Name);

					_CommandResult = new DbCommandResultList();

					for (int i = 0; i < r.Length; ++i)
					{
						DbColumnList rr = new DbColumnList();

						foreach (DataRow dr in r[i].Rows)
						{
							DbColumn c = new DbColumn();

							bool isOleDB = true;

							c.ColumnName = ToString(dr, "ColumnName", "");
							c.ColumnOrdinal = ToInt32(dr, "ColumnOrdinal", -1);
							c.ColumnSize = ToInt32(dr, "ColumnSize", -1);
							c.NumericPrecision = ToInt32(dr, "NumericPrecision", -1); // -1 su null
							c.NumericScale = ToInt32(dr, "NumericScale", -1); // -1 su null

							c.IsUnique = ToBoolean(dr, "IsUnique", false);
							c.IsKey = ToBoolean(dr, "IsKey", false);

							if (!isOleDB) c.BaseServerName = ToString(dr, "BaseServerName", "");
							c.BaseCatalogName = ToString(dr, "BaseCatalogName", "");
							c.BaseColumnName = ToString(dr, "BaseColumnName", "");
							c.BaseSchemaName = ToString(dr, "BaseSchemaName", "");
							c.BaseTableName = ToString(dr, "BaseTableName", "");

							c.DataType = (Type) dr["DataType"];
							c.AllowDBNull = ToBoolean(dr, "AllowDBNull", false);
							c.ProviderType = ToString(dr, "ProviderType", "");
							if (!isOleDB) c.IsIdentity = ToBoolean(dr, "IsIdentity", false);
							c.IsAutoIncrement = ToBoolean(dr, "IsAutoIncrement", false);
							c.IsRowVersion = ToBoolean(dr, "IsRowVersion", false);
							if (!isOleDB) c.IsHidden = ToBoolean(dr, "IsHidden", false);
							c.IsLong = ToBoolean(dr, "IsLong", false);
							c.IsReadOnly = ToBoolean(dr, "IsReadOnly", false);

							rr.Add(c);
						}

						_CommandResult.Add(new DbCommandResult(rr));
					}
				}
				return _CommandResult;
			}
		}

		private DbCommandResultList _CommandResult;

		private static int ToInt32(DataRow dr, string columnName, int def)
		{
			if (dr.IsNull(columnName))
				return def;
			object v = dr[columnName];
			return Convert.ToInt32(v);
		}

		private static string ToString(DataRow dr, string columnName, string def)
		{
			if (dr.IsNull(columnName))
				return def;
			object v = dr[columnName];
			return Convert.ToString(v);
		}

		private static bool ToBoolean(DataRow dr, string columnName, bool def)
		{
			if (dr.IsNull(columnName))
				return def;
			object v = dr[columnName];
			return Convert.ToBoolean(v);
		}
	}

	public class DbCommandResult
	{
		internal DbCommandResult(DbColumnList c)
		{
			_columns = c;
		}

		public DbColumnList Columns { get { return _columns; } }

		private DbColumnList _columns;
	}

	public class DbColumn
	{
		public string Name { get { return ColumnName; } }

		public string ColumnName;
		public int ColumnOrdinal;
		public int ColumnSize;
		public int NumericPrecision; // -1 su null
		public int NumericScale; // -1 su null
		public bool IsUnique;
		public bool IsKey;

		public string BaseServerName;
		public string BaseCatalogName;
		public string BaseColumnName;
		public string BaseSchemaName;
		public string BaseTableName;

		public Type DataType;
		public bool AllowDBNull;
		public object ProviderType;

		public bool IsAliased;
		public bool IsExpression;
		public bool IsIdentity;
		public bool IsAutoIncrement;
		public bool IsRowVersion;
		public bool IsHidden;
		public bool IsLong;
		public bool IsReadOnly;
	}
}