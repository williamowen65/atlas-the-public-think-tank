// Classes run concurrently; methods within a class remain sequential.
// Workers=0 selects the machine's logical processor count. Override via runsettings
// when SQL Server becomes the bottleneck. Exclusive database leases retain isolation.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
