# Vp.FSharp.Sql.SqlServer


An opinionated F# library to interact with SQL Server databases following [`Vp.FSharp.Sql`](https://github.com/veepee-oss/Vp.FSharp.Sql) principles and relying on [`Microsoft.Data.SqlClient`](https://www.nuget.org/packages/Microsoft.Data.SqlClient).

## Slagging Hype

We aim at following highly controversial practices to the best of our ability!

Status | Package                
------ | ----------------------
OK     | [![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-green.svg)](https://conventionalcommits.org)
OK     | [![semver](https://img.shields.io/badge/semver-2.0.0-green)](https://semver.org/spec/v2.0.0.html)
TBD    | [![keep a changelog](https://img.shields.io/badge/keep%20a%20changelog-1.0.0-red)](https://keepachangelog.com/en/1.0.0)
TBD    | [![Semantic Release](https://img.shields.io/badge/Semantic%20Release-17.1.1-red)](https://semantic-release.gitbook.io/semantic-release)

[Conventional Commits]: https://conventionalcommits.org
[semver]: https://img.shields.io/badge/semver-2.0.0-blue
[Semantic Release]: https://semantic-release.gitbook.io/semantic-release
[keep a changelog]: https://keepachangelog.com/en/1.0.0

## NuGet Package

 Name                      | Version  | Command |
-------------------------- | -------- | ------- |
 `Vp.FSharp.Sql.SqlServer` | [![NuGet Status](http://img.shields.io/nuget/v/Vp.FSharp.Sql.SqlServer.svg)](https://www.nuget.org/packages/Vp.FSharp.Sql.SqlServer) | `Install-Package Vp.FSharp.Sql.SqlServer`

# 📚 How to Use

📝 Note: It's worth noting that the examples below illustrating functions running asynchronously do leverage `Async.RunSynchronously`. This is purely for the sake of reducing the nestedness that would happen if we were using `async` computation expressions instead.

## 💿 Supported Database Values

Just a little FYI:

```fsharp
/// Native SQL Server DB types.
/// See https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
/// and https://stackoverflow.com/a/968734/4636721
type SqlServerDbValue =
    | Null
    /// System.Boolean.
    /// An unsigned numeric value that can be 0, 1, or null.
    | Bit of bool

    | TinyInt  of uint8
    | SmallInt of int16
    | Int      of int32
    | BigInt   of int64

    | Real  of single
    | Float of double

    | SmallMoney of decimal
    | Money      of decimal
    | Decimal    of decimal
    | Numeric    of decimal
    
    | Binary     of uint8 array
    | VarBinary  of uint8 array
    | Image      of uint8 array
    | RowVersion of uint8 array
    | FileStream of uint8 array
    | Timestamp  of uint8 array

    | UniqueIdentifier of Guid

    | Time           of TimeSpan
    | Date           of DateTime
    | SmallDateTime  of DateTime
    | DateTime       of DateTime
    | DateTime2      of DateTime
    | DateTimeOffset of DateTimeOffset

    | Char     of string
    | NChar    of string
    | VarChar  of string
    | NVarChar of string
    | Text     of string
    | NText    of string
    
    | Xml of SqlXml

    | SqlVariant of obj
    
    | Custom of (SqlDbType * obj)
```

## 🧱`SqlServerCommand`

The main module is here to help you build and execute (Postgre)SQL commands (i.e. `SqlCommand` BTS).

### 🏗️ Command Construction

We are obviously going to talk about how to build `SqlCommand` definitions.

📝 Note: the meaning of the word "update" below has to be put in a F# perspective, i.e. **immutable** update, as in the update returns a new updated and immutable instance.

<details> 
<summary><code>text</code></summary>

> Initialize a new command definition with the given text contained in the given string.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
42
```

</details>

<details> 
<summary><code>textFromList</code></summary>

> Initialize a new command definition with the given text spanning over several strings (ie. list).

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55; ]
|> List.map (sprintf "SELECT %d;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryList connection (fun _ _ read -> read.Value<int32> 0)
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
[0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55]
```

</details>

<details> 
<summary><code>noLogger</code></summary>

> Update the command definition so that when executing the command, it doesn't use any logger.
> Be it the default one (Global, if any.) or a previously overriden one.

Example:
```fsharp
SqlServerConfiguration.Logger (printfn "Logging... %A")

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.noLogger
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
42
```

</details>

<details> 
<summary><code>overrideLogger</code></summary>

> Update the command definition so that when executing the command, it use the given overriding logger.
> instead of the default one, aka the Global logger, if any.

Example:
```fsharp
SqlServerConfiguration.NoLogger ()

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.overrideLogger (printfn "Logging... %A")
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```fsharp
Logging... ConnectionOpened Npgsql.NpgsqlConnection
Logging... CommandPrepared Npgsql.SqlCommand
Logging... CommandExecuted (Npgsql.SqlCommand, 00:00:00.0162810)
Logging... ConnectionClosed (Npgsql.NpgsqlConnection, 00:00:00.1007513)
42
```
</details>

<details> 
<summary><code>parameters</code></summary>

> Update the command definition with the given parameters.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT @a + @b;"
|> SqlServerCommand.parameters [ ("a", Int 42); ("b", Real 42.42f) ]
|> SqlServerCommand.executeScalar<double> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
84.0
```

Note: in case you want to pass some types that aren't yet supported by the library,  
you can use the `Custom` DU case which allows you to pass whatever underlying `SqlDbType` with the relevant `obj` value.

</details>

<details> 
<summary><code>cancellationToken</code></summary>

> Update the command definition with the given cancellation token.

This comes in handy when you need to interop with more traditional, C#-async, cancellation style.

Example:
```fsharp
try
    use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
    SqlServerCommand.text "SELECT 42;"
    |> SqlServerCommand.cancellationToken (CancellationToken(true))
    |> SqlServerCommand.executeScalar<int32> connection
    |> Async.RunSynchronously
    |> ignore
with
 | :? OperationCanceledException as e ->
     printfn "The Command execution has been cancelled, reason: %A" e.Message
```

Output:
```txt
The Command execution has been cancelled, reason: "A task was canceled."
```

</details>

<details> 
<summary><code>timeout</code></summary>

> Update the command definition with the given timeout.

</details>

<details> 
<summary><code>prepare</code></summary>

> Update the command definition and sets whether the command should be prepared or not.

As per [MS Docs](https://docs.microsoft.com/en-us/sql/ado/referento%20have%20the%20provider%20save%20a%20prepared%20(or%20compiled)%20version%20of%20the%20query%20specified%20in%20the%20CommandText%20property%20before%20a%20Command%20object's%20first%20execution.%20This%20may%20slow%20a%20command's%20first%20execution,%20but%20once%20the%20provider%20compiles%20a%20command,%20the%20provider%20will%20use%20the%20compiled%20version%20of%20the%20command%20for%20any%20subsequent%20executions,%20which%20will%20result%20in%20improved%20performance.e/ado-api/prepared-property-ado):

> Use the `Prepared` property to have the provider save a prepared (or compiled) version
> of the query specified in the CommandText property before a Command object's first
> execution.
>
> This may slow a command's first execution, but once the provider compiles
> a command, the provider will use the compiled version of the command for any subsequent
> executions, which will result in improved performance.

Example: TBD

</details>

<details> 
<summary><code>transaction</code></summary>

> Update the command definition and set whether the command should be wrapped in the given transaction.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

use transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted)

// Create a table
SqlServerCommand.text $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
|> SqlServerCommand.transaction transaction
|> SqlServerCommand.executeNonQuery connection
|> Async.RunSynchronously
|> printfn "%A"

// The table is created here
SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.transaction transaction
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"

transaction.Rollback()

// The table creation has been rollbacked
SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
-1
1
0
```

</details>

### ⚙ Command Execution

We are obviously going to talk about how to execute `SqlCommand` definitions.

<details> 
<summary><code>queryAsyncSeq</code></summary>

> Execute the command and return the sets of rows as an `AsyncSeq` accordingly to the command definition.
>
> This function runs asynchronously.

Example 1:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let getCounterQuery n =
    sprintf
        """
        WITH RECURSIVE counter(value) AS (VALUES(1) UNION ALL SELECT value + 1 FROM counter WHERE value < %d)
        SELECT value FROM counter;
        """ n

let readRow set record (read: SqlRecordReader<_>) =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5 ]
|> List.map getCounterQuery
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryAsyncSeq connection readRow
|> AsyncSeq.toListSynchronously
|> List.iter (fun x -> printfn "Set = %A; Row = %A; Data = %A" x.Set x.Record x.Data)
```

Output 1:
```txt
Set = 0; Row = 0; Data = [1]
Set = 1; Row = 0; Data = [1]
Set = 2; Row = 0; Data = [1]
Set = 3; Row = 0; Data = [1]
Set = 3; Row = 1; Data = [2]
Set = 4; Row = 0; Data = [1]
Set = 4; Row = 1; Data = [2]
Set = 4; Row = 2; Data = [3]
Set = 5; Row = 0; Data = [1]
Set = 5; Row = 1; Data = [2]
Set = 5; Row = 2; Data = [3]
Set = 5; Row = 3; Data = [4]
Set = 5; Row = 4; Data = [5]
```

Notes 📝:
- The output type must be consistent across all the result sets and records.
- If you need different types you may want to either:
    - Create DU with each type you want to output
    - Use `querySetList2` or `querySetList3` ⬇️
- The `read`er can also get the `Value` given a certain field name:

Example 2:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55; ]
|> List.map (sprintf "SELECT %d AS cola;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryList connection (fun _ _ read -> read.Value<int32> "cola")
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
[0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55]
```

</details>

<details> 
<summary><code>querySeqSync</code></summary>

> Execute the command and return the sets of rows as a `seq` accordingly to the command definition.
>
> This function runs synchronously.

Example 1:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let getCounterQuery n =
    sprintf
        """
        WITH RECURSIVE counter(value) AS (VALUES(1) UNION ALL SELECT value + 1 FROM counter WHERE value < %d)
        SELECT value FROM counter;
        """ n

let readRow set record (read: SqlRecordReader<_>) =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5 ]
|> List.map getCounterQuery
|> SqlServerCommand.textFromList
|> SqlServerCommand.querySeqSync connection readRow
|> Seq.iter (fun x -> printfn "Set = %A; Row = %A; Data = %A" x.Set x.Record x.Data)
```

Output 1:
```txt
Set = 0; Row = 0; Data = [1]
Set = 1; Row = 0; Data = [1]
Set = 2; Row = 0; Data = [1]
Set = 3; Row = 0; Data = [1]
Set = 3; Row = 1; Data = [2]
Set = 4; Row = 0; Data = [1]
Set = 4; Row = 1; Data = [2]
Set = 4; Row = 2; Data = [3]
Set = 5; Row = 0; Data = [1]
Set = 5; Row = 1; Data = [2]
Set = 5; Row = 2; Data = [3]
Set = 5; Row = 3; Data = [4]
Set = 5; Row = 4; Data = [5]
```

Notes 📝:
- The output type must be consistent across all the result sets and records.
- If you need different types you may want to either:
    - Create DU with each type you want to output
    - Use `querySetList2` or `querySetList3` ⬇️
- The `read`er can also get the `Value` given a certain field name:

Example 2:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55; ]
|> List.map (sprintf "SELECT %d AS cola;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryList connection (fun _ _ read -> read.Value<int32> "cola")
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
[0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55]
```

</details>

<details> 
<summary><code>queryList</code></summary>

> Execute the command and return the sets of rows as a list accordingly to the command definition.
>
> This function runs asynchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55; ]
|> List.map (sprintf "SELECT %d;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryList connection (fun _ _ read -> read.Value<int32> 0)
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
[0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55]
```

</details>

<details> 
<summary><code>queryListSync</code></summary>

> Execute the command and return the sets of rows as a list accordingly to the command definition.
>
> This function runs synchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55; ]
|> List.map (sprintf "SELECT %d;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.queryListSync connection (fun _ _ read -> read.Value<int32> 0)
|> printfn "%A"
```

Output:
```txt
[0; 1; 1; 2; 3; 5; 8; 13; 21; 34; 55]
```

</details>

<details> 
<summary><code>querySetList</code></summary>

> Execute the command and return the first set of rows as a list accordingly to the command definition.
>
> This function runs asynchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5 ]
|> List.map (sprintf "SELECT %d;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.querySetList connection (readRow 1)
|> Async.RunSynchronously
|> List.iter (fun x -> printfn "Set = %A; Row = %A; Data = %A" x.Set x.Record x.Data)
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
```

</details>

<details> 
<summary><code>querySetListSync</code></summary>

> Execute the command and return the first set of rows as a list accordingly to the command definition.
>
> This function runs synchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
[ 0; 1; 1; 2; 3; 5 ]
|> List.map (sprintf "SELECT %d;")
|> SqlServerCommand.textFromList
|> SqlServerCommand.querySetListSync connection (readRow 1)
|> List.iter (fun x -> printfn "Set = %A; Row = %A; Data = %A" x.Set x.Record x.Data)
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
```

</details>

<details> 
<summary><code>querySetList2</code></summary>

> Execute the command and return the 2 first sets of rows as a tuple of 2 lists accordingly to the command definition.
>
> This function runs asynchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

let printRow row = printfn "Set = %A; Row = %A; Data = %A" row.Set row.Record row.Data

let set1, set2 =
    use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
    [ 0; 1; 1; 2; 3; 5 ]
    |> List.map (sprintf "SELECT %d;")
    |> SqlServerCommand.textFromList
    |> SqlServerCommand.querySetList2 connection (readRow 1) (readRow 2)
    |> Async.RunSynchronously

List.iter printRow set1
List.iter printRow set2
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
Set = 2; Row = 0; Data = [1]
```

</details>

<details> 
<summary><code>querySetList2Sync</code></summary>

> Execute the command and return the 2 first sets of rows as a tuple of 2 lists accordingly to the command definition.
>
> This function runs synchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

let printRow row = printfn "Set = %A; Row = %A; Data = %A" row.Set row.Record row.Data

let set1, set2 =
    use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
    [ 0; 1; 1; 2; 3; 5 ]
    |> List.map (sprintf "SELECT %d;")
    |> SqlServerCommand.textFromList
    |> SqlServerCommand.querySetList2Sync connection (readRow 1) (readRow 2)

List.iter printRow set1
List.iter printRow set2
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
Set = 2; Row = 0; Data = [1]
```

</details>

<details> 
<summary><code>querySetList3</code></summary>

> Execute the command and return the 3 first sets of rows as a tuple of 3 lists accordingly to the command definition.
>
> This function runs asynchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

let printRow row = printfn "Set = %A; Row = %A; Data = %A" row.Set row.Record row.Data

let set1, set2, set3 =
    use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
    [ 0; 1; 1; 2; 3; 5 ]
    |> List.map (sprintf "SELECT %d;")
    |> SqlServerCommand.textFromList
    |> SqlServerCommand.querySetList3 connection (readRow 1) (readRow 2) (readRow 3)
    |> Async.RunSynchronously

List.iter printRow set1
List.iter printRow set2
List.iter printRow set3
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
Set = 2; Row = 0; Data = [1]
Set = 3; Row = 0; Data = [1]
```

</details>

<details> 
<summary><code>querySetList3Sync</code></summary>

> Execute the command and return the 3 first sets of rows as a tuple of 3 lists accordingly to the command definition.
>
> This function runs synchronously.

Example:
```fsharp
type Row<'T> = { Set: int32; Record: int32; Data: 'T list }

let readRow set record (read: SqlRecordReader<_>)  =
    { Set = set; Record = record; Data = List.init (read.Count) (read.Value<int32>) }

let printRow row = printfn "Set = %A; Row = %A; Data = %A" row.Set row.Record row.Data

let set1, set2, set3 =
    use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
    [ 0; 1; 1; 2; 3; 5 ]
    |> List.map (sprintf "SELECT %d;")
    |> SqlServerCommand.textFromList
    |> SqlServerCommand.querySetList3Sync connection (readRow 1) (readRow 2) (readRow 3)

List.iter printRow set1
List.iter printRow set2
List.iter printRow set3
```

Output:
```txt
Set = 1; Row = 0; Data = [0]
Set = 2; Row = 0; Data = [1]
Set = 3; Row = 0; Data = [1]
```

</details>

<details> 
<summary><code>executeScalar<'Scalar></code></summary>

> Execute the command accordingly to its definition and,
> - return the first cell value, if it is available and of the given type.
> - throw an exception, otherwise.
>
> This function runs asynchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
42
```

</details>


<details> 
<summary><code>executeScalarSync<'Scalar></code></summary>

> Execute the command accordingly to its definition and,
> - return the first cell value, if it is available and of the given type.
> - throw an exception, otherwise.
>
> This function runs synchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output:
```txt
42
```

</details>

<details> 
<summary><code>executeScalarOrNone<'Scalar></code></summary>

> Execute the command accordingly to its definition and,
> - return `Some`, if the first cell is available and of the given type.
> - return `None`, if first cell is `DBNull`.
> - throw an exception, otherwise.
>
> This function runs asynchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")

SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeScalarOrNone<int32> connection
|> Async.RunSynchronously
|> printfn "%A"

SqlServerCommand.text "SELECT NULL;"
|> SqlServerCommand.executeScalarOrNone<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
0
```

Output:
```txt
Some 42
None
```

</details>

<details> 
<summary><code>executeScalarOrNoneSync<'Scalar></code></summary>

> Execute the command accordingly to its definition and,
> - return `Some`, if the first cell is available and of the given type.
> - return `None`, if first cell is `DBNull`.
> - throw an exception, otherwise.
>
> This function runs synchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")

SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeScalarOrNoneSync<int32> connection
|> printfn "%A"

SqlServerCommand.text "SELECT NULL;"
|> SqlServerCommand.executeScalarOrNoneSync<int32> connection
|> printfn "%A"
0
```

Output:
```txt
Some 42
None
```

</details>

<details> 
<summary><code>executeNonQuery<'Scalar></code></summary>

> Execute the command accordingly to its definition and, return the number of rows affected.
>
> This function runs asynchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeNonQuery connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
-1
```

</details>

<details> 
<summary><code>executeNonQuerySync<'Scalar></code></summary>

> Execute the command accordingly to its definition and, return the number of rows affected.
>
> This function runs synchronously.

Example:
```fsharp
use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
SqlServerCommand.text "SELECT 42;"
|> SqlServerCommand.executeNonQuerySync connection
|> printfn "%A"
```

Output:
```txt
-1
```

</details>

## 🦮 `SqlServerNullDbValue`: Null Helpers

The module to handle options and results in parameters.

<details> 
<summary><code>ifNone</code></summary>

> Return SqlServer DB Null value if the given option is `None`, otherwise the underlying wrapped in `Some`.

Example:
```fsharp
[ "a", SqlServerNullDbValue.ifNone Integer (Some 42)
  "b", SqlServerNullDbValue.ifNone Integer (None) ]
|> printfn "%A"
```

Output:
```txt
[("a", Integer 42); ("b", Null)]
```

</details>

<details> 
<summary><code>ifError</code></summary>

> Return SqlServer DB Null value if the given option is `Error`, otherwise the underlying wrapped in `Ok`.

Example:
```fsharp
[ "a", SqlServerNullDbValue.ifError Integer (Ok 42)
  "b", SqlServerNullDbValue.ifError Integer (Error "meh") ]
|> printfn "%A"
```

Output:
```txt
[("a", Integer 42); ("b", Null)]
```

</details>

## 🚄 `SqlServerTransaction`: Transaction Helpers

This is the main module to interact with `SqlTransaction`.

📝 Note: The default isolation level is [`ReadCommitted`](https://docs.microsoft.com/en-us/dotnet/api/system.data.isolationlevel).

<details> 
<summary><code>commit</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation, cancellation token and transaction body.
>
> This function runs asynchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commit (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    return!
        SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.executeScalar<int32> connection
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
1
1
```

</details>

<details> 
<summary><code>commitSync</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitSync (IsolationLevel.ReadCommitted) connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.executeScalarSync<int32> connection
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output:
```txt
1
1
```

</details>

<details> 
<summary><code>notCommit</code></summary>

> Create and do not commit an automatically generated transaction with the given connection, isolation, cancellation token and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.notCommit (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);" 
        |> SqlServerCommand.text
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    return!
        $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
1
0
```

</details>

<details> 
<summary><code>notCommitSync</code></summary>

> Create and do not commit an automatically generated transaction with the given connection, isolation and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.notCommitSync (IsolationLevel.ReadCommitted) connection (fun connection _ -> 
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);" 
    |> SqlServerCommand.text
    |> SqlServerCommand.executeNonQuery connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output:
```txt
1
0
```

</details>

<details> 
<summary><code>commitOnSome</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation, cancellation token and transaction body.
>
> The commit phase only occurs if the transaction body returns Some.
>
> This function runs asynchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnSome (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Some 42
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 1:
```txt
Some 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnSome (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';" 
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return None
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
None
0
```

</details>

<details> 
<summary><code>commitOnSomeSync</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation and transaction body.
>
> The commit phase only occurs if the transaction body returns Some.
>
> This function runs synchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnSomeSync (IsolationLevel.ReadCommitted) connection (fun connection _ -> 
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Some 42
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 1:
```txt
Some 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnSomeSync (IsolationLevel.ReadCommitted) connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';" 
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return None
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 2:
```txt
None
0
```

</details>

<details> 
<summary><code>commitOnOk</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation, cancellation token and transaction body.
>
> The commit phase only occurs if the transaction body returns Ok.
>
> This function runs asynchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnOk (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Ok 42
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 1:
```txt
Ok 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnOk (CancellationToken.None) (IsolationLevel.ReadCommitted) connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Error "fail"
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
Error "fail"
0
```

</details>

<details> 
<summary><code>commitOnOkSync</code></summary>

> Create and commit an automatically generated transaction with the given connection, isolation and transaction body.
>
> The commit phase only occurs if the transaction body returns Ok.
>
> This function runs synchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnOkSync (IsolationLevel.ReadCommitted) connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Ok 42
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 1:
```txt
Ok 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.commitOnOkSync (IsolationLevel.ReadCommitted) connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Error "fail"
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 2:
```txt
Error "fail"
0
```

</details>

<details> 
<summary><code>defaultCommit</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> This function runs asynchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommit connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    return!
        SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.executeScalar<int32> connection
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
1
1
```

</details>

<details> 
<summary><code>defaultCommitSync</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitSync connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    SqlServerCommand.text $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.executeScalarSync<int32> connection
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output:
```txt
1
1
```

</details>

<details> 
<summary><code>defaultNotCommit</code></summary>

> Create and do not commit an automatically generated transaction with the given connection and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultNotCommit connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);" 
        |> SqlServerCommand.text
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    return!
        $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output:
```txt
1
0
```

</details>

<details> 
<summary><code>defaultNotCommitSync</code></summary>

> Create and do not commit an automatically generated transaction with the given connection and transaction body.
>
> This function runs synchronously.

Example:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultNotCommitSync connection (fun connection _ -> 
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);" 
    |> SqlServerCommand.text
    |> SqlServerCommand.executeNonQuery connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output:
```txt
1
0
```

</details>

<details> 
<summary><code>defaultCommitOnSome</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> The commit phase only occurs if the transaction body returns Some.
>
> This function runs asynchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnSome connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Some 42
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 1:
```txt
Some 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnSome connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';" 
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return None
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
None
0
```

</details>

<details> 
<summary><code>defaultCommitOnSomeSync</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> The commit phase only occurs if the transaction body returns Some.
>
> This function runs synchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnSomeSync connection (fun connection _ -> 
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Some 42
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 1:
```txt
Some 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnSomeSync connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';" 
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return None
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 2:
```txt
None
0
```

</details>

<details> 
<summary><code>defaultCommitOnOk</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> The commit phase only occurs if the transaction body returns Ok.
>
> This function runs asynchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnOk connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Ok 42
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 1:
```txt
Ok 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnOk connection (fun connection _ -> async {
    do! $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
        |> SqlServerCommand.text 
        |> SqlServerCommand.executeNonQuery connection
        |> Async.Ignore

    do! $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
        |> SqlServerCommand.text
        |> SqlServerCommand.executeScalar<int32> connection
        |> Async.Ignore
    return Error "fail"
})
|> Async.RunSynchronously
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalar<int32> connection
|> Async.RunSynchronously
|> printfn "%A"
```

Output 2:
```txt
Error "fail"
0
```

</details>

<details> 
<summary><code>defaultCommitOnOkSync</code></summary>

> Create and commit an automatically generated transaction with the given connection and transaction body.
>
> The commit phase only occurs if the transaction body returns Ok.
>
> This function runs synchronously.

Example 1:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnOkSync connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Ok 42
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 1:
```txt
Ok 42
1
```

Example 2:
```fsharp
let tableName = "people"

use connection = new SqlConnection("Server=192.168.0.1,4242;Database=MyDatabase;User Id=myuser;Password=mypassword")
connection.Open()

SqlServerTransaction.defaultCommitOnOkSync connection (fun connection _ ->
    $"CREATE TABLE {tableName} (id int IDENTITY(1,1) PRIMARY KEY, name TEXT NOT NULL);"
    |> SqlServerCommand.text 
    |> SqlServerCommand.executeNonQuerySync connection
    |> ignore

    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
    |> SqlServerCommand.text
    |> SqlServerCommand.executeScalarSync<int32> connection
    |> ignore
    return Error "fail"
)
|> printfn "%A"

$"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}';"
|> SqlServerCommand.text 
|> SqlServerCommand.executeScalarSync<int32> connection
|> printfn "%A"
```

Output 2:
```txt
Error "fail"
0
```

</details>

# ❤ How to Contribute
Bug reports, feature requests, and pull requests are very welcome! Please read the [Contribution Guidelines](./CONTRIBUTION.md) to get started.

# 📜 Licensing
The project is licensed under MIT. For more information on the license see the [license file](./LICENSE).
