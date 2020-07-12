@Echo off
IF "%1" == "%2" dotnet pack --no-build --configuration %1 -o %3
Rem Alternatively, run this:
Rem dotnet pack --configuration Release -o .\pack