clean: 
	rm -rf bin/
	dotnet clean

build: 
	dotnet build -c Release