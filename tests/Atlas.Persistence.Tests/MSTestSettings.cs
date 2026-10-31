// Each test method owns its fixture state and exclusively leases its databases.
// AtlasTime clocks are AsyncLocal; shared parity IDs are immutable and databases
// isolate their rows. Method scope lets workers pick up individual parity tests.
// Workers=0 selects available logical processors; runsettings may cap SQL pressure.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
