@Echo off
IF "%1" == "%2" dotnet pack --no-build --configuration %1 -o %3