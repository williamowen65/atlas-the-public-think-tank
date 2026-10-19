// Independent classes may overlap; methods within each class remain sequential.
// Process-wide Console fixtures opt out explicitly; SQL fixtures use unique databases.
[assembly: Parallelize(Workers = 2, Scope = ExecutionScope.ClassLevel)]
