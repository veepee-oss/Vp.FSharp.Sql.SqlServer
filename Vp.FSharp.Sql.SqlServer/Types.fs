namespace Vp.FSharp.Sql.SqlServer

open System
open System.Data
open System.Threading.Tasks

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

[<AbstractClass; Sealed>]
type internal Constants private () =

    static member DbValueToParameter name value =
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

    static member Deps : SqlServerDependencies =
        let beginTransactionAsync (connection: SqlConnection) (isolationLevel: IsolationLevel) _ =
            ValueTask.FromResult(connection.BeginTransaction(isolationLevel))

        { CreateCommand = fun connection -> connection.CreateCommand()
          SetCommandTransaction = fun command transaction -> command.Transaction <- transaction
          BeginTransactionAsync = beginTransactionAsync
          ExecuteReaderAsync = fun command -> command.ExecuteReaderAsync
          DbValueToParameter = Constants.DbValueToParameter }
