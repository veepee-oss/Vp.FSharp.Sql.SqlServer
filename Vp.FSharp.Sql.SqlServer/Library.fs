namespace Vp.FSharp.Sql.SqlServer

open System
open System.Data

open Microsoft.Data.SqlClient

open Vp.FSharp.Sql


/// Native SQL Server DB types.
/// See https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
/// and https://stackoverflow.com/a/968734/4636721
type SqlServerDbValue =
    | Null
    | Bit of bool

    | TinyInt of uint8
    | SmallInt of int16
    | Int of int32
    | BigInt of int64

    | Real of single
    | Float of double

    | Numeric of decimal
    | SmallMoney of decimal
    | Money of decimal
    | Decimal of decimal

    | Binary of uint8 array
    | VarBinary of uint8 array
    | Image of uint8 array
    | RowVersion of uint8 array
    | FileStream of uint8 array
    | Timestamp of uint8 array

    | UniqueIdentifier of Guid

    | Time of TimeSpan
    | Date of DateTime
    | SmallDateTime of DateTime
    | DateTime of DateTime
    | DateTime2 of DateTime
    | DateTimeOffset of DateTimeOffset

    | Char of string
    | NChar of string
    | VarChar of string
    | NVarChar of string
    | Text of string
    | NText of string
    | Xml of string

    | SqlVariant of obj

type SqlServerCommandDefinition =
    CommandDefinition<
        SqlConnection,
        SqlCommand,
        SqlParameter,
        SqlDataReader,
        SqlTransaction,
        SqlServerDbValue>

type SqlServerConfiguration =
    SqlConfigurationCache<
        SqlConnection,
        SqlCommand>

type SqlServerDependencies =
    SqlDependencies<
        SqlConnection,
        SqlCommand,
        SqlParameter,
        SqlDataReader,
        SqlTransaction,
        SqlServerDbValue>

[<RequireQualifiedAccess>]
module SqlServerNullDbValue =
    let ifNone toDbValue = NullDbValue.ifNone toDbValue SqlServerDbValue.Null
    let ifError toDbValue = NullDbValue.ifError toDbValue (fun _ -> SqlServerDbValue.Null)

[<RequireQualifiedAccess>]
module SqlServerCommand =

    let private dbValueToParameter name value =
        let parameter = SqlParameter()
        parameter.ParameterName <- name
        match value with
        | Null ->
            parameter.Value <- DBNull.Value
        | Bit value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Bit
        | TinyInt value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.SmallInt
        | SmallInt value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.SmallInt
        | Int value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Int
        | BigInt value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.BigInt
        | Real value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Real
        | Float value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Float
        | Numeric value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Decimal
        | Decimal value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Decimal
        | SmallMoney value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.SmallMoney
        | Money value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Money

        | Binary value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.VarBinary
        | FileStream value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.VarBinary
        | VarBinary value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.VarBinary
        | Image value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Binary
        | RowVersion value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Timestamp
        | Timestamp value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Timestamp

        | UniqueIdentifier value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.UniqueIdentifier

        | Time value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Time
        | Date value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Date
        | SmallDateTime value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.DateTime
        | DateTime value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.DateTime
        | DateTime2 value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.DateTime
        | DateTimeOffset value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.DateTimeOffset

        | Char value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Char
        | NChar value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.NChar
        | VarChar value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.VarChar
        | NVarChar value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.NVarChar
        | Text value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Text
        | NText value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.NText
        | Xml value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Xml

        | SqlVariant value ->
            parameter.Value <- value
            parameter.SqlDbType <- SqlDbType.Variant
        parameter

    let private deps: SqlServerDependencies =
        { CreateCommand = fun connection -> connection.CreateCommand()
          ExecuteReaderAsync = fun command -> command.ExecuteReaderAsync
          DbValueToParameter = dbValueToParameter }

    /// Initialize a command definition with the given text contained in the given string.
    let text value : SqlServerCommandDefinition =
        SqlCommand.text value

    /// Initialize a command definition with the given text spanning over several strings (ie. list).
    let textFromList value : SqlServerCommandDefinition =
        SqlCommand.textFromList value

    /// Update the command definition so that when executing the command, it doesn't use any logger.
    /// Be it the default one (Global, if any.) or a previously overriden one.
    let noLogger commandDefinition = { commandDefinition with Logger = LoggerKind.Nothing }

    /// Update the command definition so that when executing the command, it use the given overriding logger.
    /// instead of the default one, aka the Global logger, if any.
    let overrideLogger value commandDefinition = { commandDefinition with Logger = LoggerKind.Override value }

    /// Update the command definition with the given parameters.
    let parameters value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.parameters value commandDefinition

    /// Update the command definition with the given cancellation token.
    let cancellationToken value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.cancellationToken value commandDefinition

    /// Update the command definition with the given timeout.
    let timeout value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.timeout value commandDefinition

    /// Update the command definition and sets the command type (ie. how it should be interpreted).
    let commandType value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.commandType value commandDefinition

    /// Update the command definition and sets whether the command should be prepared or not.
    let prepare value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.prepare value commandDefinition

    /// Update the command definition and sets whether the command should be wrapped in the given transaction.
    let transaction value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
        SqlCommand.transaction value commandDefinition

    /// Return the sets of rows as an AsyncSeq accordingly to the command definition.
    let queryAsyncSeq connection read (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.queryAsyncSeq
            connection deps (SqlServerConfiguration.Snapshot) read commandDefinition

    /// Return the sets of rows as a list accordingly to the command definition.
    let queryList connection read (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.queryList
            connection deps (SqlServerConfiguration.Snapshot) read commandDefinition

    /// Return the first set of rows as a list accordingly to the command definition.
    let querySetList connection read (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.querySetList
            connection deps (SqlServerConfiguration.Snapshot) read commandDefinition

    /// Return the 2 first sets of rows as a tuple of 2 lists accordingly to the command definition.
    let querySetList2 connection read1 read2 (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.querySetList2
            connection deps (SqlServerConfiguration.Snapshot) read1 read2 commandDefinition

    /// Return the 3 first sets of rows as a tuple of 3 lists accordingly to the command definition.
    let querySetList3 connection read1 read2 read3 (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.querySetList3
            connection deps (SqlServerConfiguration.Snapshot) read1 read2 read3 commandDefinition

    /// Execute the command accordingly to its definition and,
    /// - return the first cell value, if it is available and of the given type.
    /// - throw an exception, otherwise.
    let executeScalar<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.executeScalar<'Scalar, _, _, _, _, _, _, _, _, _>
            connection deps (SqlServerConfiguration.Snapshot) commandDefinition

    /// Execute the command accordingly to its definition and,
    /// - return Some, if the first cell is available and of the given type.
    /// - return None, if first cell is DbNull.
    /// - throw an exception, otherwise.
    let executeScalarOrNone<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.executeScalarOrNone<'Scalar, _, _, _, _, _, _, _, _, _>
            connection deps (SqlServerConfiguration.Snapshot) commandDefinition

    /// Execute the command accordingly to its definition and, return the number of rows affected.
    let executeNonQuery connection (commandDefinition: SqlServerCommandDefinition) =
        SqlCommand.executeNonQuery
            connection deps (SqlServerConfiguration.Snapshot) commandDefinition
