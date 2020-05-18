// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "DbContext don't need to be disposed", Scope = "namespaceanddescendants", Target = "AutoPocoIO", MessageId = "AppDbContext")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "AutoPocoIO.Exceptions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "AutoPocoIO.DynamicSchema.Db")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "AutoPocoIO.Resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "AutoPocoIO.Extensions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "PartialView Name", Scope = "namespaceanddescendants", Target = "AutoPocoIO.Dashboard.Pages")]
