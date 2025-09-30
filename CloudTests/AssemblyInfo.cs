using Microsoft.VisualStudio.TestTools.UnitTesting;

// Cannot run the test project in parallel because the db connection pool 
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
