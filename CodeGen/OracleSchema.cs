using System;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Utility;

/*


CREATE OR REPLACE PACKAGE "HR"."PACK_HELLO_WORLD" AS
	FUNCTION "FX_HELLO_WORLD" (MYARG IN OUT NOCOPY NUMBER)
		RETURN NUMBER;
	PROCEDURE "PROC_HELLO_WORLD" (MYARG IN OUT NOCOPY NUMBER);

    -- cursore tipato che ritorna una tabella
    type rc_a is ref cursor return countries%rowtype;

    -- cursore tipato che ritorna un record definito
    type dRecord is record
    (
        a number,
        b varchar(100)
    );
    type rc_d is ref cursor return dRecord;

    procedure "Leo"(a out sys_refcursor, b out sys_refcursor, c out rc_a, d out rc_d);
END;

CREATE OR REPLACE PACKAGE BODY "HR"."PACK_HELLO_WORLD" AS
	FUNCTION "FX_HELLO_WORLD" (MYARG IN OUT NOCOPY NUMBER)
		RETURN NUMBER
	IS
	BEGIN
		DBMS_OUTPUT.PUT_LINE('Hello world!');
		RETURN 1;
	END;
	PROCEDURE "PROC_HELLO_WORLD" (MYARG IN OUT NOCOPY NUMBER)
	IS
	BEGIN
		DBMS_OUTPUT.PUT_LINE('Hello world!');
	END;


    Procedure "Leo"(a out sys_refcursor, b out sys_refcursor, c out rc_a, d out rc_d)
    is
    begin
		DBMS_OUTPUT.PUT_LINE('Hello world!');
        open a for select * from countries;
        open b for select * from departments;
        open c for select * from countries;
        open d for select 1, 'Leo' from dual;
    end;
END;

*/

namespace OracleSchema
{
	public class SchemaBuilder : IDisposable
	{
		private DbConnection _cn;

	
		public SchemaBuilder(string connectionString)
		{
				_cn = new OracleConnection(connectionString);
				_cn.Open();
		}
		public void Dispose()
		{
			if (_cn != null && _cn.State == ConnectionState.Open)
				_cn.Close();
			_cn = null;
		}


		public U2.NamedList<Table> GetTables(string Owner, string Table)
		{
			U2.NamedList<Table> tables = new U2.NamedList<Table>();

			DataTable dt = this.Get_Tables(Owner, Table);

			foreach (DataRow dr in dt.Rows)
			{
				Table table;

				if (true)
				{
					string tableName = U2.ReadString(dr, "TABLE_NAME");
					string owner = U2.ReadString(dr, "OWNER");
					string type = U2.ReadString(dr, "TYPE");

					table = new Table(tableName, owner, type);
				}

				U2.NamedList<Column> cols = GetColumns(table.Owner, table.Name);
				foreach (Column col in cols)
					col._table = table;
				table._columns = cols;

				string[] pkColumns = GetPrimaryKeyColumnNames(table.Owner, table.Name);

				table._pk = new U2.NamedList<Column>();

				foreach (string pkColumn in pkColumns)
					table.PK.Add(table.Columns[pkColumn]);

				tables.Add(table);
			}

			return tables;
		}

		private U2.NamedList<Column> GetColumns(string restrictionOwner, string restrictionTableName)
		{
			if (restrictionOwner == null) throw new ArgumentNullException("restrictionOwner");
			if (restrictionTableName == null) throw new ArgumentException("restrictionTableName");

			U2.NamedList<Column> ret = new U2.NamedList<Column>();

			DataTable dt = Get_Columns(restrictionOwner, restrictionTableName, null);

			foreach (DataRow dr in dt.Rows)
			{
				string owner = U2.ReadString(dr, "OWNER");
				string columnName = U2.ReadString(dr, "COLUMN_NAME");
				decimal id = U2.Read<decimal>(dr, "ID");
				string dataType = U2.ReadString(dr, "DATATYPE");
				decimal length = U2.Read<decimal>(dr, "LENGTH");
				decimal? precision = U2.ReadNullable<decimal>(dr, "PRECISION");
				decimal? scale = U2.ReadNullable<decimal>(dr, "SCALE");
				string nullable = U2.ReadString(dr, "NULLABLE");

				Column col = new Column();
				col._owner = owner;
				col._name = columnName;
				col._id = id;
				col._dataType = dataType;
				col._length = length;
				col._precision = precision;
				col._scale = scale;
				col._nullable = nullable == "Y";

				ret.Add(col);
			}

			return ret;
		}

		private string[] GetPrimaryKeyColumnNames(string restrictionTableOwner, string restrictionTableName)
		{
			DataTable dt = Get_PrimaryKeys(restrictionTableOwner, restrictionTableName, null);
			if (dt.Rows.Count == 0)
				return new string[0];

			//string constraintName = dt.Rows[0]["CONSTRAINT_NAME"] as string;
			string indexName = dt.Rows[0]["INDEX_NAME"] as string;

			dt = Get_IndexColumns(null, indexName, restrictionTableOwner, restrictionTableName, null);

			string[] ret = new string[dt.Rows.Count];

			for (int i = 0; i < dt.Rows.Count; ++i)
				ret[i] = dt.Rows[0]["COLUMN_NAME"] as string;

			return ret;
		}

		public U2.NamedList<Package> GetPackages(string Owner, string Name)
		{
			U2.NamedList<Package> ret = new U2.NamedList<Package>();

			DataTable Packages = Get_Packages(Owner, Name);

			foreach (DataRow dr in Packages.Rows)
			{
				string name = (string)dr["OBJECT_NAME"];
				string owner = (string)dr["OWNER"];

				Package package = new Package(name, owner);
				package._procedure = GetProcedure(owner, package);

				ret.Add(package);
			}
			return ret;
		}

		public U2.NamedList<Procedure> GetProcedure(string Owner)
		{
			U2.NamedList<Procedure> ret = new U2.NamedList<Procedure>();

			DataTable ProcedureParameters = Get_ProcedureParameters(Owner, null);

			DataRow[] drProcedure = U2.SelectDistinct(ProcedureParameters, "OBJECT_NAME", "PACKAGE_NAME is NULL");

			foreach (DataRow param in drProcedure)
			{
				string procedureName = (string)param["OBJECT_NAME"];

				U2.NamedList<ProcedureArgument> args = GetProcedureParameters(Owner, null, procedureName);

				if (args[0].Direction == ParameterDirection.Return)
				{
					Function function = new Function(null, procedureName);
					function._return = args[0];
					args.RemoveAt(0);
					function._argument = args;
					ret.Add(function);
				}
				else
				{
					Procedure procedure = new Procedure(null, procedureName);
					procedure._argument = args;
					ret.Add(procedure);
				}


			}

			return ret;
		}

		private U2.NamedList<Procedure> GetProcedure(string Owner, Package package)
		{
			U2.NamedList<Procedure> ret = new U2.NamedList<Procedure>();

			DataTable ProcedureParameters = Get_ProcedureParameters(Owner, null);

			DataRow[] drProcedure = U2.SelectDistinct(ProcedureParameters, "OBJECT_NAME", U.F("PACKAGE_NAME ='{0}'", package.Name));

			foreach (DataRow param in drProcedure)
			{
				string procedureName = (string)param["OBJECT_NAME"];

				U2.NamedList<ProcedureArgument> args = GetProcedureParameters(Owner, package.Name, procedureName);

				if (args[0].Direction == ParameterDirection.Return)
				{
					Function function = new Function(package, procedureName);
					function._return = args[0];
					args.RemoveAt(0);
					function._argument = args;
					ret.Add(function);
				}
				else
				{
					Procedure procedure = new Procedure(package, procedureName);
					procedure._argument = args;
					ret.Add(procedure);
				}

			}

			return ret;
		}

		private U2.NamedList<ProcedureArgument> GetProcedureParameters(string Owner, string packageName, string procedureName)
		{
			U2.NamedList<ProcedureArgument> ret = new U2.NamedList<ProcedureArgument>();

			DataTable ProcedureParameters = Get_ProcedureParameters(Owner, procedureName);

			// prendo solo i parametri della package/procedure
			// e li sorto per posizione.
			DataRow[] drParams = ProcedureParameters.Select(
				U.F("PACKAGE_NAME ='{0}' and OBJECT_NAME='{1}'", packageName, procedureName),
				"POSITION");

			bool refCursor = false;

			foreach (var param in drParams)
			{
				if (param.IsNull("ARGUMENT_NAME"))
				{
					if (param.IsNull("DATA_TYPE"))
					{
						// e' probabilmente una procedura senza parametri

						Debug.Assert(drParams.Length == 1);  // controllo che non ci siano altri parametri
						break;
					}
					else
					{
						// è probabilmente una funzione e questo è il valore di ritorno
						Debug.Assert(U2.ReadString(param, "IN_OUT") == "OUT");
					}

				}

				string argName = U2.ReadString(param, "ARGUMENT_NAME");
				string dataType = U2.ReadString(param, "DATA_TYPE");
				string inOut = U2.ReadString(param, "IN_OUT");
				decimal? dataLenght = U2.ReadNullable<decimal>(param, "DATA_LENGTH");
				decimal? dataPrecision = U2.ReadNullable<decimal>(param, "DATA_PRECISION");
				decimal? dataScale = U2.ReadNullable<decimal>(param, "DATA_SCALE");
				decimal sequence = U2.Read<decimal>(param, "SEQUENCE");

				ProcedureArgument arg = new ProcedureArgument();

				arg._name = argName;
				arg._type = dataType;

				if (inOut == "IN") arg._direction = ParameterDirection.In;
				else if (inOut == "OUT") arg._direction = ParameterDirection.Out;
				else if (inOut == "IN/OUT") arg._direction = ParameterDirection.InOut;
				else throw new ArgumentOutOfRangeException("IN_OUT");
				
				if (argName == null)
					arg._direction = ParameterDirection.Return;

				arg._dataLenght = U2.ConvertToNullableInt32(dataLenght);
				arg._dataPrecision = U2.ConvertToNullableInt32(dataPrecision);
				arg._dataScale = U2.ConvertToNullableInt32(dataScale);
				arg._sequence = (int)sequence;


				if (dataType == "REF CURSOR")
					refCursor = true;


				ret.Add(arg);
			}

			if (refCursor)
			{
				DataTable ProcedureParameters2 = Get_Arguments(Owner, packageName, procedureName, null);

				for (int i = 0; i < ret.Count; ++i)
				{
					ProcedureArgument arg = ret[i];

					if (arg.Type == "REF CURSOR")
					{
						int startSequence = arg._sequence + 1;

						int nextSequence;
						if (i + 1 < ret.Count)
							nextSequence = ret[i + 1]._sequence;
						else
							nextSequence = 1000 * 1000;// arg._sequence + 1;


						DataRow[] drParams2 = ProcedureParameters2.Select(
							U.F("PACKAGE_NAME ='{0}' and OBJECT_NAME='{1}' and SEQUENCE = {2} and DATA_TYPE='PL/SQL RECORD'", packageName, procedureName, startSequence),
							"SEQUENCE");
						if (drParams2.Length == 0)
						{
							// è in ref cursor NON tipato
							continue;
						}

						// qui prendo tutti i parametri.
						drParams2 = ProcedureParameters2.Select(
							U.F("PACKAGE_NAME ='{0}' and OBJECT_NAME='{1}' and SEQUENCE > {2} and SEQUENCE < {3}", packageName, procedureName, startSequence, nextSequence),
							"SEQUENCE");


						arg._record = new U2.NamedList<RecordField>();

						foreach (DataRow row in drParams2)
						{
							RecordField rf = new RecordField();

							string argName = (string)row["ARGUMENT_NAME"];
							string dataType = (string)row["DATA_TYPE"];
							string inOut = (string)row["IN_OUT"];
							decimal? dataLenght = U2.ReadNullable<decimal>(row, "DATA_LENGTH");
							decimal? dataPrecision = U2.ReadNullable<decimal>(row, "DATA_PRECISION");
							decimal? dataScale = U2.ReadNullable<decimal>(row, "DATA_SCALE");

							rf._name = argName;
							rf._type = dataType;

							if (inOut == "IN") rf._direction = ParameterDirection.In;
							else if (inOut == "OUT") rf._direction = ParameterDirection.Out;
							else if (inOut == "IN/OUT") rf._direction = ParameterDirection.InOut;
							else throw new ArgumentOutOfRangeException("IN_OUT");

							rf._dataLenght = U2.ConvertToNullableInt32(dataLenght);
							rf._dataPrecision = U2.ConvertToNullableInt32(dataPrecision);
							rf._dataScale = U2.ConvertToNullableInt32(dataScale);

							arg._record.Add(rf);
						}
					}
				}
			}

			return ret;
		}


		#region Get_ methods
		public DataTable Get_Users(string UserName)
		{
			string[] r = new string[1];
			r[0] = UserName;
			return _cn.GetSchema("Users", r);
		}
		public DataTable Get_Tables(string Owner, string Table)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Table;
			return _cn.GetSchema("Tables", r);
		}
		public DataTable Get_Columns(string Owner, string Table, string Column)
		{
			string[] r = new string[3];
			r[0] = Owner;
			r[1] = Table;
			r[2] = Column;
			return _cn.GetSchema("Columns", r);
		}
		public DataTable Get_Views(string Owner, string View)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = View;
			return _cn.GetSchema("Views", r);
		}
		public DataTable Get_Synonyms(string Owner, string Synonym)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Synonym;
			return _cn.GetSchema("Synonyms", r);
		}
		public DataTable Get_Sequences(string Owner, string Sequence)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Sequence;
			return _cn.GetSchema("Sequences", r);
		}
		public DataTable Get_ProcedureParameters(string Owner, string ObjectName)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = ObjectName;
			return _cn.GetSchema("ProcedureParameters", r);
		}
		public DataTable Get_Functions(string Owner, string Name)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Name;
			return _cn.GetSchema("Functions", r);
		}
		public DataTable Get_IndexColumns(string Owner, string Name, string TableOwner, string TableName, string Column)
		{
			string[] r = new string[5];
			r[0] = Owner;
			r[1] = Name;
			r[2] = TableOwner;
			r[3] = TableName;
			r[4] = Column;
			return _cn.GetSchema("IndexColumns", r);
		}
		public DataTable Get_Indexes(string Owner, string Name, string TableOwner, string TableName)
		{
			string[] r = new string[4];
			r[0] = Owner;
			r[1] = Name;
			r[2] = TableOwner;
			r[3] = TableName;
			return _cn.GetSchema("Indexes", r);
		}
		public DataTable Get_Packages(string Owner, string Name)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Name;
			return _cn.GetSchema("Packages", r);
		}
		public DataTable Get_PackageBodies(string Owner, string Name)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Name;
			return _cn.GetSchema("PackageBodies", r);
		}
		public DataTable Get_Arguments(string Owner, string PackageName, string ObjectName, string ArgumentName)
		{
			string[] r = new string[4];
			r[0] = Owner;
			r[1] = PackageName;
			r[2] = ObjectName;
			r[3] = ArgumentName;
			return _cn.GetSchema("Arguments", r);
		}
		public DataTable Get_Procedures(string Owner, string Name)
		{
			string[] r = new string[2];
			r[0] = Owner;
			r[1] = Name;
			return _cn.GetSchema("Procedures", r);
		}
		public DataTable Get_UniqueKeys(string Owner, string Table_Name, string Constraint_Name)
		{
			string[] r = new string[3];
			r[0] = Owner;
			r[1] = Table_Name;
			r[2] = Constraint_Name;
			return _cn.GetSchema("UniqueKeys", r);
		}
		public DataTable Get_PrimaryKeys(string Owner, string Table_Name, string Constraint_Name)
		{
			string[] r = new string[3];
			r[0] = Owner;
			r[1] = Table_Name;
			r[2] = Constraint_Name;
			return _cn.GetSchema("PrimaryKeys", r);
		}
		public DataTable Get_ForeignKeys(string Foreign_Key_Owner, string Foreign_Key_Table_Name, string Foreign_Key_Constraint_Name)
		{
			string[] r = new string[3];
			r[0] = Foreign_Key_Owner;
			r[1] = Foreign_Key_Table_Name;
			r[2] = Foreign_Key_Constraint_Name;
			return _cn.GetSchema("ForeignKeys", r);
		}
		public DataTable Get_ForeignKeyColumns(string Owner, string Table_Name, string Constraint_Name)
		{
			string[] r = new string[3];
			r[0] = Owner;
			r[1] = Table_Name;
			r[2] = Constraint_Name;
			return _cn.GetSchema("ForeignKeyColumns", r);
		}

		#region Get_ builder
		public string CreateGetFunctions()
		{
			DataTable dt = _cn.GetSchema("Restrictions");

			StringWriter sw = new StringWriter();


			U.SameGroup<DataRow> sameGroup = delegate(DataRow a, DataRow b)
			{
				string ca = a["CollectionName"] as string;
				string cb = b["CollectionName"] as string;
				return ca == cb;
			};

			foreach (List<DataRow> restrictions in U.Group<DataRow>(dt.Rows, sameGroup))
			{
				string collectionName = restrictions[0]["CollectionName"] as string;

				sw.Write("public DataTable Get_{0}(", collectionName);

				for (int i = 0; i < restrictions.Count; ++i)
				{
					string restrictionName = restrictions[i]["RestrictionName"] as string;

					if (i < restrictions.Count - 1)
						sw.Write("string {0},", restrictionName);
					else
						sw.Write("string {0}", restrictionName);

				}
				sw.WriteLine(")");
				sw.WriteLine("{");
				sw.WriteLine("\tstring [] r = new string[{0}];", restrictions.Count);
				for (int i = 0; i < restrictions.Count; ++i)
				{
					string restrictionName = restrictions[i]["RestrictionName"] as string;
					sw.WriteLine("\tr[{0}] = {1};", i, restrictionName);
				}

				sw.WriteLine("\treturn _cn.GetSchema(\"{0}\", r);", collectionName);
				sw.WriteLine("}");
			}

			string ret = sw.GetStringBuilder().ToString();
			return ret;
		}
		#endregion

		#endregion

	}


	public enum ParameterDirection { In, Out, InOut, Return }

	[Serializable]
	public class Package : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return _name; } }
		internal Package(string name, string owner)
		{
			this._name = name;
			this._owner = owner;
		}

		public string Name { get { return this._name; } }
		public string Owner { get { return this._owner; } }
		public U2.NamedList<Procedure> Procedure { get { return this._procedure; } }

		readonly private string _name;
		readonly private string _owner;
		internal U2.NamedList<Procedure> _procedure;

	}

	[Serializable]
	public class Procedure : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return _name; } }

		private Package _package;
		private string _name;
		internal U2.NamedList<ProcedureArgument> _argument;

		internal Procedure(Package package, string name)
		{
			this._package = package;
			this._name = name;
		}

		public Package Package { get { return this._package; } }
		public string Name { get { return this._name; } }
		public U2.NamedList<ProcedureArgument> Argument { get { return this._argument; } }
	}

	[Serializable]
	public class Function : Procedure
	{
		internal Function(Package package, string name) : base(package, name) { }

		internal ProcedureArgument _return;

		public ProcedureArgument Return { get { return this._return; } }
	}

	[Serializable]
	public class ProcedureArgument : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return _name; } }

		internal string _name;
		internal string _type;
		internal ParameterDirection _direction;
		internal int? _dataLenght;
		internal int? _dataPrecision;
		internal int? _dataScale;
		internal int _sequence;
		internal U2.NamedList<RecordField> _record; // solo per REF CURSOR


		public string Name { get { return this._name; } }
		public string Type { get { return this._type; } }
		public ParameterDirection Direction { get { return this._direction; } }
		public int? DataLenght { get { return this._dataLenght; } }
		public int? DataPrecision { get { return this._dataPrecision; } }
		public int? DataScale { get { return this._dataScale; } }
		public U2.NamedList<RecordField> Record { get { return this._record; } }


		internal ProcedureArgument() { }
	}

	public class RecordField : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return _name; } }
		internal RecordField() { }

		public string Name { get { return this._name; } }
		public string Type { get { return this._type; } }
		public ParameterDirection Direction { get { return this._direction; } }
		public int? DataLenght { get { return this._dataLenght; } }
		public int? DataPrecision { get { return this._dataPrecision; } }
		public int? DataScale { get { return this._dataScale; } }

		internal string _name;
		internal string _type;
		internal ParameterDirection _direction;
		internal int? _dataLenght;
		internal int? _dataPrecision;
		internal int? _dataScale;
	}

	public class Table : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return this.Name; } }

		public Table(string tableName, string owner, string type)
		{
			this._name = tableName;
			this._owner = owner;
			this._tableType = type;
		}

		private readonly string _name;
		private readonly string _owner;
		private readonly string _tableType;
		internal U2.NamedList<Column> _columns;
		internal U2.NamedList<Column> _pk;

		public string Name { get { return this._name; } }
		public string Owner { get { return this._owner; } }
		public string TableType { get { return this._tableType; } }

		public U2.NamedList<Column> Columns { get { return this._columns; } }
		public U2.NamedList<Column> PK { get { return this._pk; } }
	}

	public class Column : U2.INamedObject
	{
		string U2.INamedObject.ObjectName { get { return this._name; } }
		internal Column() { }

		internal string _owner;
		internal string _name;
		internal decimal _id;
		internal string _dataType;
		internal decimal _length;
		internal decimal? _precision;
		internal decimal? _scale;
		internal bool _nullable;

		internal Table _table;

		public string Owner { get { return this._owner; } }
		public string Name { get { return this._name; } }
		public decimal Id { get { return this._id; } }
		public string DataType { get { return this._dataType; } }
		public decimal Length { get { return this._length; } }
		public decimal? Precision { get { return this._precision; } }
		public decimal? Scale { get { return this._scale; } }
		public bool Nullable { get { return this._nullable; } }

		public Table Table { get { return this._table; } }
	}

}