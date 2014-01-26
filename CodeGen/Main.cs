using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Microsoft.CSharp;
using System.Text.RegularExpressions;

namespace CodeGen
{
	internal class _
	{
		private static void Error(bool exit, string fmt, params object[] a)
		{
			if (fmt != null)
			{
				Console.Error.WriteLine(fmt, a);
				Console.Error.WriteLine();
			}
			
			Console.Error.WriteLine();
			Console.Error.WriteLine("CodeGen [-FileOut:<file.cs>] [-Line] [-SaveScript:<script.cs>] [-NoRun] [-ShowConfig] [-Debug] [-ReferencedAssembly:<ass.dll>] [configFile.xml | genFile.gen]");
			Console.Error.WriteLine("-Help                       print help");
			Console.Error.WriteLine("-ReferencedAssembly:<file>  reference assembly for compiling .gen");
			Console.Error.WriteLine("-FileOut:<file>             generate output to <file>");
			Console.Error.WriteLine("-Line[+/-]                  generate script with #line lines; default +");
			Console.Error.WriteLine("-SaveScript:<file>          save script to <file> enabling debug info");
			Console.Error.WriteLine("-RunScript[+/-]             run script; default +");
			Console.Error.WriteLine("-ShowConfig[+/-]            show xml configuration on Console; default -");
			Console.Error.WriteLine("-Debug[+/-]                 invoke debugger before executing script");
			
			if (exit)
				Environment.Exit(-1);
		}

		private static void Error(string fmt, params object[] a)
		{
			Error(true, fmt, a);
		}
		private static void Error(string pgm, string fmt, params object[] a)
		{
			Error(true, fmt, a);
		}


		public class OptionPair
		{
			public string Option;
			public string Argument;
		}

		[STAThread]
		private static void Main(string[] args)
		{
			bool genLine = true;
			string saveScript = null;
			bool runScript = true;
			string configFile = null;
			string fileGen = null;
			bool showConfig = false;
			bool help = false;
			bool debug = false;
			
			Environment.CurrentDirectory = "/home/leo/Scrivania/CodeGen";
			
			string opArgs = "Help|FileOut:|Line+|SaveScript:|RunScript+|ShowConfig+|Debug+|RefencedAssembly:|*";
			XmlDocument Config = new XmlDocument();
			if (true)
			{
				U.CommandProcessor cmdProcessor = new U.CommandProcessor(opArgs, args);
				cmdProcessor.Error = /*new U.CommandProcessor.ErrorDelegate*/(Error);
				
				List<OptionPair> s = new List<OptionPair>();
				
				while (cmdProcessor.Read())
				{
					switch (cmdProcessor.Option)
					{
					case "Line":
						genLine = (cmdProcessor.Switch != "-");
						break;
					
					case "SaveScript":
						saveScript = cmdProcessor.Argument;
						if (Path.GetExtension(saveScript).ToLower() != ".cs")
							Error("saveScript requires .cs file");
						break;
					
					case "RunScript":
						runScript = (cmdProcessor.Switch != "-");
						break;
					
					case "ShowConfig":
						showConfig = (cmdProcessor.Switch != "-");
						break;
					
					case "Debug":
						debug = (cmdProcessor.Switch != "-");
						break;
					
					case "Help":
						help = true;
						break;
					
					case "":
						configFile = cmdProcessor.Argument;
						if (Path.GetExtension(configFile).ToLower() == ".gen")
						{
							fileGen = configFile;
							configFile = null;
						}
						break;
					
					case "ReferencedAssembly":
					case "FileOut":
					default:
						{
							// FileOut lo memorizziamo nel file di configurazione xml
							OptionPair op = new OptionPair();
							op.Option = cmdProcessor.Option;
							op.Argument = cmdProcessor.Argument != string.Empty ? cmdProcessor.Argument : cmdProcessor.Switch;
							s.Add(op);
						}
						break;
					}
				}
				
				if (configFile == null && fileGen == null)
					Error("Config file or Gen file required");
				
				if (debug && saveScript == null)
					Error("Debug option requires SaveScript");
				
				
				// creo o apro il file di configurazione
				if (configFile == null)
				{
					XmlElement root = Config.CreateElement("CodeGen");
					Config.AppendChild(root);
					
					if (fileGen != null)
					{
						XmlElement e = Config.CreateElement("FileGen");
						e.AppendChild(Config.CreateTextNode(fileGen));
						root.AppendChild(e);
					}
				}
				else
				{
					try
					{
						Config.Load(configFile);
						fileGen = Config.DocumentElement.SelectSingleNode("FileGen").InnerText;
					}
					catch (Exception ex)
					{
						Error("{0}: {1}", configFile, ex.Message);
					}
				}
				
				// copio gli argomenti della linea di comando 
				// nel file di configurazione xml
				foreach (OptionPair p in s)
				{
					XmlElement e = Config.CreateElement(p.Option);
					e.AppendChild(Config.CreateTextNode(p.Argument));
					Config.DocumentElement.AppendChild(e);
				}
			}
			
			if (showConfig)
			{
				XmlTextWriter xw = new XmlTextWriter(Console.Out);
				xw.Indentation = 1;
				xw.IndentChar = '\t';
				xw.Formatting = Formatting.Indented;
				Config.Save(xw);
				xw.Close();
				Console.WriteLine();
				
				Environment.Exit(0);
			}
			
			// cerco il file .gen
			if (!File.Exists(fileGen))
			{
				Error("Could not find file \"{0}\"", fileGen);
			}
			
			
			// Qui si genera lo script nella stringa scriptCode
			// In scriptReferencedAssemblies c'è la lista degli assembly referenziati nello script
			StringWriter w = new StringWriter();
			string nameSpace = "CodeGen";
			string className = Path.GetFileNameWithoutExtension(fileGen);
			using (StreamReader r = File.OpenText(fileGen))
			{
				CodeBuilder.WriteCode(fileGen, r, genLine, nameSpace, className, w);
			}
			string scriptCode = w.GetStringBuilder().ToString();
			
			if (true)
			{
				// qui formattiamo il codice generato (quello che poi compilato generera' il programma in uscita
				string tempFile = Path.GetTempFileName();
				using (CsStreamWriter csw = new CsStreamWriter(File.CreateText(tempFile)))
				{
					string lineCode;
					StringReader sr = new StringReader(scriptCode);
					while ((lineCode = sr.ReadLine()) != null)
						csw.WriteLine(lineCode);
				}
				using (TextReader rd = File.OpenText(tempFile))
				{
					scriptCode = rd.ReadToEnd();
				}
				File.Delete(tempFile);
			}
			
			// Qui si salva il codice
			// In questo modo si puo' fare debug
			if (saveScript != null)
			{
				using (TextWriter csw = File.CreateText(saveScript))
				{
					csw.Write(scriptCode);
				}
				if (debug == false)
					Environment.Exit(0);
			}
			
			Assembly scriptAssembly;
			if (true)
			{
				CSharpCodeProvider cp = new CSharpCodeProvider();
				
				string codeGenExe = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, AppDomain.CurrentDomain.FriendlyName);
				
				CompilerParameters cpa = new CompilerParameters();
				cpa.ReferencedAssemblies.Add("System.dll");
				cpa.ReferencedAssemblies.Add("System.Data.dll");
				cpa.ReferencedAssemblies.Add("System.Xml.dll");
				cpa.ReferencedAssemblies.Add(codeGenExe);
				foreach (XmlNode f in Config.DocumentElement.SelectNodes("ReferencedAssembly"))
					cpa.ReferencedAssemblies.Add(f.InnerText);
				cpa.WarningLevel = 4;
				
				CompilerResults scriptCompilerResult;
				
				if (saveScript != null && debug && runScript)
				{
					cpa.IncludeDebugInformation = true;
					cpa.GenerateInMemory = false;
					cpa.OutputAssembly = Path.GetFullPath(Path.ChangeExtension(saveScript, ".dll"));
					scriptCompilerResult = cp.CompileAssemblyFromFile(cpa, saveScript);
				}
				else
				{
					cpa.IncludeDebugInformation = false;
					cpa.GenerateInMemory = true;
					scriptCompilerResult = cp.CompileAssemblyFromSource(cpa, scriptCode);
				}
				
				if (scriptCompilerResult.Errors.Count > 0)
				{
					for (int i = 0; i < scriptCompilerResult.Errors.Count; i++)
						Console.Error.WriteLine(scriptCompilerResult.Errors[i].ToString());
					
					if (scriptCompilerResult.Errors.HasErrors)
						Environment.Exit(scriptCompilerResult.NativeCompilerReturnValue);
				}
				
				if (cpa.GenerateInMemory)
					scriptAssembly = scriptCompilerResult.CompiledAssembly;
				else
					scriptAssembly = Assembly.LoadFile(scriptCompilerResult.PathToAssembly);
			}
			
			if (help)
			{
				CodeGenBase cg = null;
				try
				{
					cg = (CodeGenBase)scriptAssembly.CreateInstance(nameSpace + "." + className);
					Error(false, null, null);
					Console.Error.WriteLine();
					cg.Help();
					Environment.Exit(0);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e.Message);
					Console.Error.WriteLine(e.StackTrace);
					
					Error(false, null, null);
					Environment.Exit(-1);
				}
				finally
				{
					if (cg != null)
						cg.Dispose();
				}
				Environment.Exit(0);
			}
			
			if (runScript)
			{
				CodeGenBase cg = null;
				try
				{
					cg = (CodeGenBase)scriptAssembly.CreateInstance(nameSpace + "." + className);
					if (debug)
						cg.DebugBreak();
					
					U.CommandProcessor cmdProcessor = new U.CommandProcessor(opArgs, args);
					List<string> r = new List<string>();
					cg.GetOptions(r);
						while (cmdProcessor.Read())
					{
						switch (cmdProcessor.Option)
						{
						case "Line":
						case "SaveScript":
						case "RunScript":
						case "ShowConfig":
						case "Debug":
						case "Help":
						case "":	
						case "ReferencedAssembly":
						case "FileOut":
							break;
						
						default:
							{
								bool found = false;
								foreach (var op in r)
								{
									if (op == cmdProcessor.Option)
									{
										found = true;
										break;
									}
								}
								if (found == false)
									Error("unkown option'{0}'", cmdProcessor.Option);
							}
							break;
						}
					}
						
					cg.Init(Config);
					cg.Run();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e.Message);
					Console.Error.WriteLine(e.StackTrace);
					
					Environment.Exit(-1);
				}
				finally
				{
					if (cg != null)
						cg.Dispose();
					
				}
			}
			
			Environment.Exit(0);
		}
	}

	public class CodeBuilder
	{
		private enum CodeType
		{
			Text,
			Assign,
			Code
		}

		private static string makeCSharpString(string c)
		{
			StringBuilder s = new StringBuilder();
			
			if (c.IndexOf('"') < 0)
			{
				s.Append('"');
				s.Append(c);
				s.Append('"');
				return s.ToString();
			}
			
			s.Append("@\"");
			for (int i = 0; i < c.Length; i++)
			{
				char ch = c[i];
				s.Append(ch);
				if (ch == '"')
					s.Append(ch);
			}
			s.Append('"');
			
			return s.ToString();
		}

		public static void WriteCode(string fn, TextReader tr, bool genLine, string nameSpace, string className, TextWriter tw)
		{
			int lineNo = 0;
			string currentLine;

			StringCollection csUsing = new StringCollection();
			
			for (;;)
			{
				currentLine = tr.ReadLine();
				lineNo++;
				if (currentLine == null)
					break;
				
				if (currentLine.StartsWith("%%%%%"))
					break;
				
				csUsing.Add(currentLine);
			}
			
			tw.WriteLine("using System;");
			tw.WriteLine("using System.IO;");
			tw.WriteLine("using System.Text;");
			foreach (string u in csUsing)
			{
				string s = u.Trim();
				if (s == "using System;")
						continue;
				if (s == "using System.IO;")
					continue;
				if (s == "using System.Text;")
					continue;
		
				tw.WriteLine(u);
			}
			tw.WriteLine();
			
			tw.WriteLine("namespace {0}", nameSpace);
			tw.WriteLine("{");
			tw.WriteLine("	class {0} : CodeGen.CodeGenBase", className);
			tw.WriteLine("	{");
			tw.WriteLine("		public override void DoWork()");
			tw.WriteLine("		{");
			
			CodeType ct = CodeType.Text;
			
			
			int debuggerBreakLine = lineNo + 1;
			
			for (;;)
			{
				currentLine = tr.ReadLine();
				lineNo++;
				if (currentLine == null)
					break;
				
				if (currentLine.StartsWith("%%%%%"))
					break;
				
				if (genLine)
					tw.WriteLine("#line {0} \"{1}\"", lineNo, fn);
				
				// se la linea inizia per ~ è un codice.
				if (currentLine.Length > 0 && currentLine[0] == '~')
				{
					tw.WriteLine(currentLine.Substring(1));
					ct = CodeType.Text;
					continue;
				}
				
				// linea con ^[ ]*$ (soli spazi): si va a capo
				if (currentLine.Trim().Length == 0 && ct == CodeType.Text)
				{
					tw.WriteLine("Out.WriteLine();");
					continue;
				}
				
				int ci = 0;
				while (ci < currentLine.Length)
				{
					int ni;
					
					switch (ct)
					{
					case CodeType.Text:
						ni = currentLine.IndexOf("<%=", ci);
						if (ni >= 0)
						{
							if (ni > ci)
								tw.Write("Out.Write({0}); ", makeCSharpString(currentLine.Substring(ci, ni - ci)));
							tw.Write("Out.Write(");
							ci = ni + 3;
							ct = CodeType.Assign;
						}

						else
						{
							ni = currentLine.IndexOf("<%", ci);
							if (ni >= 0)
							{
								if (ni > ci)
									tw.Write("Out.Write({0});", makeCSharpString(currentLine.Substring(ci, ni - ci)));
								ci = ni + 2;
								ct = CodeType.Code;
							}

							else
							{
								tw.WriteLine("Out.WriteLine({0});", makeCSharpString(currentLine.Substring(ci)));
								ci = currentLine.Length;
							}
						}

						break;
					
					case CodeType.Assign:
						ni = currentLine.IndexOf("%>", ci);
						if (ni >= 0)
						{
							if (ni + 2 == currentLine.Length)
							{
								tw.WriteLine("{0}); Out.WriteLine();", currentLine.Substring(ci, ni - ci));
							}

							else
								tw.Write("{0});", currentLine.Substring(ci, ni - ci));
							ci = ni + 2;
							ct = CodeType.Text;
						}

						else
						{
							tw.WriteLine(currentLine.Substring(ci));
							ci = currentLine.Length;
						}

						break;
					
					case CodeType.Code:
						ni = currentLine.IndexOf("%>", ci);
						if (ni >= 0)
						{
							tw.WriteLine(currentLine.Substring(ci, ni - ci));
							ci = ni + 2;
							ct = CodeType.Text;
						}

						else
						{
							tw.WriteLine(currentLine.Substring(ci));
							ci = currentLine.Length;
						}

						break;
					}
				}
			}
			tw.WriteLine("		}");
			
			if (currentLine != null)
			{
				for (;;)
				{
					currentLine = tr.ReadLine();
					lineNo++;
					if (currentLine == null)
						break;
					if (genLine)
						tw.WriteLine("#line {0} \"{1}\"", lineNo, fn);
					tw.WriteLine(currentLine);
				}
			}
			
			tw.WriteLine("#line {0} \"{1}\"", debuggerBreakLine, fn);
			tw.WriteLine("public override void DebugBreak() { System.Diagnostics.Debugger.Break(); }");
			tw.WriteLine("	}");
			tw.WriteLine("}");
		}
	}

	public class CodeGenBase : IDisposable
	{
		protected TextWriter Out;
		private string FileOut;
		private string TempFile;
		protected XmlDocument Config;

		public CodeGenBase()
		{
			Out = null;
			FileOut = null;
			Config = null;
			TempFile = null;
		}

		public virtual void Init(XmlDocument config)
		{
			Config = config;
			FileOut = Config.DocumentElement.SelectSingleNode("FileOut").InnerText;
			TempFile = Path.GetTempFileName();
			Out = File.CreateText(TempFile);
		}
		
		public virtual void GetOptions(List<string> options)
		{
		}

		public void Dispose()
		{
			Out.Close();
			Out = null;
			
			string text;
			using (var rd = File.OpenText(TempFile))
				text = rd.ReadToEnd();
			File.Delete(TempFile);
			
			Out = Console.Out;
			if (FileOut != null && FileOut != string.Empty)
				Out = File.CreateText(FileOut);
			
			using (CsStreamWriter sw = new CsStreamWriter(Out))
			{
				StringReader rd = new StringReader(text);
				string line;
				while ((line = rd.ReadLine()) != null)
					sw.WriteLine(line);
			}
			
			if (Out != Console.Out)
				Out.Close();
		}

		public void Run()
		{
			OnStart();
			DoWork();
			OnEnd();
		}

		public virtual void DebugBreak()
		{
			Debugger.Break();
			// si ridefinisce per consentire al debugger di aprirsi nello script
		}

		public virtual void OnStart()
		{
		}

		public virtual void DoWork()
		{
		}

		public virtual void OnEnd()
		{
		}


		public virtual void Help()
		{
		}

		protected string GetConfig(string key)
		{
			XmlNode el = Config.DocumentElement.SelectSingleNode(key);
			if (el == null)
				return null;
			return el.InnerText;
		}
		protected string GetConfig(string key, string defaultValue)
		{
			string ret = GetConfig(key);
			if (ret == null || ret == string.Empty)
				return defaultValue;
			return ret;
		}

		protected string GetConfigOrDie(string key)
		{
			string v = GetConfig(key);
			if (v == null || v == string.Empty)
				Error("Missing or empty config tag '{0}'\nPlease specify -{0} or -{0}:value in command prompt\nor in .xml file", key);
			return v;
		}

		protected static void Error(string fmt, params object[] a)
		{
			Console.Error.WriteLine(fmt, a);
			Environment.Exit(1);
		}

		protected static void Error(string fmt)
		{
			Console.Error.WriteLine(fmt);
			Environment.Exit(1);
		}
	}

	public class CsStreamWriter : IDisposable
	{
		public CsStreamWriter(TextWriter s)
		{
			_s = s;
			
			_r = new List<Regex>();
			_r.Add(new Regex(@"^if \(.*\)$"));
			_r.Add(new Regex(@"^else$"));
			_r.Add(new Regex(@"^while \(.*\)$"));
			_r.Add(new Regex(@"^do$"));
			_r.Add(new Regex(@"^switch \(.*\)$"));
			_r.Add(new Regex(@"^for \(.*\)$"));
			_r.Add(new Regex(@"^foreach \(.*\)$"));
		}

		bool Match()
		{
			if (_last == null || _last == string.Empty)
				return false;
			foreach (var r in _r)
				if (r.Match(_last).Success == true)
					return true;
			return false;
		}

		public void WriteLine()
		{
			WriteLine("");
		}
		public void WriteLine(string fmt, params object[] args)
		{
			WriteLine(string.Format(fmt, args));
		}
		public void WriteLine(string s)
		{
			s = s.Trim();
			
			if (s == "{")
			{
				Tb();
				_s.WriteLine(s);
				_nTabs += 1;
			}

			else if (s == "}")
			{
				_nTabs -= 1;
				Tb();
				_s.WriteLine(s);
			}
			else if (Match())
			{
				_nTabs += 1;
				Tb();
				_s.WriteLine(s);
				_nTabs -= 1;
			}
			else
			{
				Tb();
				_s.WriteLine(s);
			}
			
			_last = s;
		}

		private int _nTabs;
		private TextWriter _s;
		private string _last = string.Empty;

		private List<Regex> _r;

		private void Tb()
		{
			for (int i = 0; i < _nTabs; ++i)
				_s.Write("\t");
			
		}


		#region IDisposable implementation
		public void Dispose()
		{
			if (_s != null)
				_s.Dispose();
		}
		#endregion
	}
	
}