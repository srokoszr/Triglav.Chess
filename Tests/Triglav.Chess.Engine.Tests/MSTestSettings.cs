using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.ClassLevel, Workers = 1)]//no parallelism
