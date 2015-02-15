using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace SqlLiteGen
{
	class Col
	{
		public string colName;
		public string dataType;
		public bool primary;
		public bool isNullable;
		public string csType;
	}
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: SqlLiteGen <file.db>");
				return;
			}

			using (SQLiteConnection cn = new SQLiteConnection())
			{
				//cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
				cn.ConnectionString = "data source=" + args[0];
				cn.Open();

				U.CsStreamWriter cs = new U.CsStreamWriter(Console.Out);

				cs.WriteLine("using System;");
				cs.WriteLine("using System.Collections.Generic;");
				cs.WriteLine("using System.Data.SQLite;");
				cs.WriteLine();

				var tbs = cn.GetSchema("tables");
				foreach (DataRow tb in tbs.Rows)
				{
					string tableName = tb["TABLE_NAME"].ToString();
					string tableType = tb["TABLE_TYPE"].ToString();
					if (tableType != "table")
						continue;

					cs.WriteLine();
					cs.WriteLine("[Serializable]");
					cs.WriteLine("public partial class Rec{0}", tableName);
					cs.WriteLine("{");

					var cols = new List<Col>();
					foreach (DataRow col in cn.GetSchema("columns").Rows)
					{
						string coltb = col["TABLE_NAME"].ToString();
						if (coltb != tableName)
							continue;

						Col c = new Col();
						c.colName = col["COLUMN_NAME"].ToString();
						c.dataType = col["DATA_TYPE"].ToString();
						c.primary = (bool)col["PRIMARY_KEY"];
						c.isNullable = (bool)col["IS_NULLABLE"];

						switch (c.dataType)
						{
						case "int": c.csType = "int"; break;
						case "char": c.csType = "string"; break;
						case "nvarchar": c.csType = "string"; break;
						case "bool": c.csType = "bool"; break;
						case "date": c.csType = "DateTime"; break;
						case "time": c.csType = "TimeSpan"; break;
						}
						if (c.isNullable)
						{
							if (c.csType != "string")
								c.csType += "?";
						}

						cols.Add(c);
					}

					////////////

					foreach (Col col in cols)
					{
						cs.WriteLine("public {0} f_{1} {{ get; set; }}", col.csType, col.colName);
					}

					cs.WriteLine();
					if (true)
					{
						cs.WriteLine("public void Read(SQLiteDataReader rd)");
						cs.WriteLine("{");
						int colNum = 0;
						foreach (Col col in cols)
						{
							switch (col.dataType)
							{
							case "int":
								if (col.isNullable)
									cs.WriteLine("f_{0} = rd.IsDBNull({1}) ? (int?)null : rd.GetInt32({1});", col.colName, colNum);
								else
									cs.WriteLine("f_{0} = rd.GetInt32({1});", col.colName, colNum);
								break;

							case "char":
							case "nvarchar":
							case "string":
							case "text":
								if (col.isNullable)
									cs.WriteLine("f_{0} = rd.IsDBNull({1}) ? (string)null : rd.GetString({1});", col.colName, colNum);
								else
									cs.WriteLine("f_{0} = rd.GetString({1});", col.colName, colNum);
								break;

							case "bool":
								if (col.isNullable)
									cs.WriteLine("f_{0} = rd.IsDBNull({1}) ? (bool?)null : rd.GetInt32({1}) != 0;", col.colName, colNum);
								else
									cs.WriteLine("f_{0} = rd.GetInt32({1}) != 0;", col.colName, colNum);
								break;

							case "date":
								if (col.isNullable)
									cs.WriteLine("f_{0} = rd.IsDBNull({1}) ? (DateTime?)null : rd.GetDateTime({1});", col.colName, colNum);
								else
									cs.WriteLine("f_{0} = rd.GetDateTime({1});", col.colName, colNum);
								break;

							case "time":
								if (col.isNullable)
									cs.WriteLine("f_{0} = rd.IsDBNull({1}) ? (TimeSpan?)null : rd.GetTimeSpan({1});", col.colName, colNum);
								else
									cs.WriteLine("f_{0} = rd.GetTimeSpan({1});", col.colName, colNum);
								break;

							default:
								cs.WriteLine("f_{0} = rd.GetXYZ({1});", col.colName, colNum);
								break;
							}
							colNum += 1;
						}
						cs.WriteLine("}");
					}

					cs.WriteLine();
					if (true)
					{
						StringBuilder sb = new StringBuilder();
						sb.Append("SELECT ");
						bool first = true;
						foreach (var col in cols)
						{
							if (!first)
								sb.Append(", ");
							else
								first = false;

							sb.Append(col.colName);
						}
						sb.AppendFormat(" FROM {0}", tableName);
						cs.WriteLine("public static SQLiteCommand CreateCommandSelect(SQLiteConnection cn)");
						cs.WriteLine("{");
						cs.WriteLine("var cmd = cn.CreateCommand();");
						cs.WriteLine("cmd.CommandText = @\"{0}\";", sb.ToString());
						cs.WriteLine("return cmd;");
						cs.WriteLine("}");
					}

					// LoadPK
					cs.WriteLine();
					if (true)
					{
						StringBuilder sb = new StringBuilder();
						foreach (var col in cols)
						{
							if (!col.primary) continue;
							sb.AppendFormat(", {0} {1}", col.csType, col.colName);
						}

						cs.WriteLine("public bool LoadPK(SQLiteConnection cn{0})", sb.ToString());
						cs.WriteLine("{");
						cs.WriteLine("using(var cmd = CreateCommandSelect(cn))");
						cs.WriteLine("{");

						sb.Clear();
						int n = 0;
						foreach (var col in cols)
						{
							if (!col.primary) continue;
							if (n > 0) sb.Append(" AND");
							sb.AppendFormat(" {0} = @{1}", col.colName, n++);
						}

						cs.WriteLine("cmd.CommandText += \" WHERE{0}\";", sb.ToString());
						n = 0;
						foreach (var col in cols)
							if (col.primary)
								cs.WriteLine("cmd.Parameters.AddWithValue(\"@{0}\", {1});", n++, col.colName);

						cs.WriteLine("using (var rd = cmd.ExecuteReader())");
						cs.WriteLine("{");
						cs.WriteLine("if (rd.Read())");
						cs.WriteLine("{");
						cs.WriteLine("this.Read(rd);");
						cs.WriteLine("if (rd.Read()) throw new Exception(\"Invalid db\");");
						cs.WriteLine("return true;");
						cs.WriteLine("}");
						cs.WriteLine("}");
						cs.WriteLine("return false;");

						cs.WriteLine("}");
						cs.WriteLine("}");
					}

					// Insert
					if (true)
					{
						cs.WriteLine("public void Insert(SQLiteConnection cn{0})");
						cs.WriteLine("{");
						cs.WriteLine("using(var cmd = cn.CreateCommand(cn))");
						cs.WriteLine("{");

						StringBuilder sb = new StringBuilder();
						sb.AppendFormat("INSERT INTO {0}(", tableName);
						bool first = true;
						foreach (var col in cols)
						{
							if (first) { first = false; } else { sb.Append(", "); }
							sb.Append(col.colName);
						}
						sb.Append(") VALUES (");
						first = true;
						int n = 0;
						foreach (var col in cols)
						{
							if (first) { first = false; } else { sb.Append(", "); }
							sb.AppendFormat("@{0}", n++);
						}
						sb.Append(")");
						cs.WriteLine("cmd.CommandText = \"{0}\";", sb.ToString());

						n = 0;
						foreach (var col in cols)
							if (col.isNullable)
								cs.WriteLine("cmd.Parameters.AddWithValue(\"@{0}\", f_{1}.HasValue ? f_{1}.Value : DBNull.Value);", n++, col.colName);
							else
							cs.WriteLine("cmd.Parameters.AddWithValue(\"@{0}\", f_{1});", n++, col.colName);

						cs.WriteLine("cmd.ExecuteScalar();");
						cs.WriteLine("}");
						cs.WriteLine("}");
					}


					cs.WriteLine();
					foreach (var col in cols)
						cs.WriteLine("public static readonly string c_{0} = \"{0}\";", col.colName);

					cs.WriteLine("}");
				}
			}
		}
	}
}
