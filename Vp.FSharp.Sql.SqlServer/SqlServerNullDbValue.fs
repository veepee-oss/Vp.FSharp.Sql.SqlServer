[<RequireQualifiedAccess>]
module Vp.FSharp.Sql.SqlServer.SqlServerNullDbValue

open Vp.FSharp.Sql


let ifNone toDbValue = NullDbValue.ifNone toDbValue SqlServerDbValue.Null
let ifError toDbValue = NullDbValue.ifError toDbValue (fun _ -> SqlServerDbValue.Null)
