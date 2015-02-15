
LLParserGen.exe : FirstFollow.cs LexParserHelper.cs Main.cs regexp.cs U.dll
	#mcs -out:LLParserGen.exe FirstFollow.cs LexParserHelper.cs Main.cs regexp.cs -r:U.dll
	mcs -out:LLParserGen.exe FirstFollow.cs LexParserHelper.cs Main.cs regexp.cs ../U/*.cs
	cp LLParserGen.exe ~/bin/.

U.dll : 
	make -C ../U
	cp ../U/U.dll U.dll

clean :
	-rm -f LLParserGen.exe U.dll
	make -C ../U clean
