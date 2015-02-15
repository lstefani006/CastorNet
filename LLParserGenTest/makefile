
Test.exe : M.cs main.cs
	mcs -out:Text.exe M.cs main.cs ../U/U_L*.cs


M.cs : M.gram
	~/bin/LLParserGen.exe M.gram
