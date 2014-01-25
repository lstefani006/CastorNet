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
	}
	class Program
	{
		static void Main(string[] args)
		{
			using (SQLiteConnection cn = new SQLiteConnection())
			{
				cn.ConnectionString = @"data source=C:\Sviluppo\enr_trunk\Monetica\Debug\enr.db";
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

						if (c.colName == "A")
							Debugger.Break();

						cols.Add(c);
					}

					foreach (Col col in cols)
					{
						string csType = null;
						switch (col.dataType)
						{
						case "int": csType = "int"; break;
						case "char": csType = "string"; break;
						case "nvarchar": csType = "string"; break;
						case "bool": csType = "bool"; break;
						case "date": csType = "DateTime"; break;
						case "time": csType = "TimeSpan"; break;
						}
						if (col.isNullable)
						{
							if (csType != "string")
								csType += "?";
						}
						
						cs.WriteLine("public {0} f_{1} {{ get; set; }}", csType, col.colName);
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

					cs.WriteLine();
					if (true)
					{
						int colNum = 0;
						foreach (var col in cols)
						{
							cs.WriteLine("public static readonly string c_{0} = \"{0}\";", col.colName);
							colNum += 1;
						}
					}


					cs.WriteLine("}");
				}
			}
		}
	}
}
