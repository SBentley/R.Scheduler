SET OUTDIR=C:\GitHub\R.Scheduler\src\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

::NuGet pack "%OUTDIR%R.Scheduler\R.Scheduler.nuspec"
NuGet pack "%OUTDIR%R.Scheduler.Contracts\R.Scheduler.Contracts.nuspec"


::nuget.exe push R.Scheduler.0.1.13.nupkg
nuget.exe push R.Scheduler.Contracts.0.1.7.nupkg

           
@ECHO === === === === === === === ===

PAUSE
