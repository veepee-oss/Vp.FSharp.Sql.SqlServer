[<RequireQualifiedAccess>]
module Vp.FSharp.Sql.SqlServer.SqlServerNullDbValue

open Vp.FSharp.Sql


/// Return SQL Server DB Null value if the given option is None, otherwise the underlying wrapped in Some.
let ifNone toDbValue = NullDbValue.ifNone toDbValue SqlServerDbValue.Null

/// Return SQL Server DB Null value if the given option is Error, otherwise the underlying wrapped in Ok.
let ifError toDbValue = NullDbValue.ifError toDbValue (fun _ -> SqlServerDbValue.Null)
