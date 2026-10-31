// Independent classes may overlap; methods within each class remain sequential.
// Process-wide Console fixtures opt out explicitly; SQL fixtures exclusively lease unique databases and reset data between leases.
[assembly: Parallelize(Workers = 2, Scope = ExecutionScope.ClassLevel)]
