﻿[<RequireQualifiedAccess>]
module Vp.FSharp.Sql.SqlServer.SqlServerCommand

open Vp.FSharp.Sql


/// Initialize a new command definition with the given text contained in the given string.
let text value : SqlServerCommandDefinition =
    SqlCommand.text value

/// Initialize a new command definition with the given text spanning over several strings (ie. list).
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

/// Update the command definition and sets whether the command should be prepared or not.
let prepare value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
    SqlCommand.prepare value commandDefinition

/// Update the command definition and sets whether the command should be wrapped in the given transaction.
let transaction value (commandDefinition: SqlServerCommandDefinition) : SqlServerCommandDefinition =
    SqlCommand.transaction value commandDefinition

/// Execute the command and return the sets of rows as an AsyncSeq accordingly to the command definition.
/// This function runs asynchronously.
let queryAsyncSeq connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.queryAsyncSeq
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the sets of rows as an AsyncSeq accordingly to the command definition.
/// This function runs synchronously.
let querySeqSync connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySeqSync
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the sets of rows as a list accordingly to the command definition.
/// This function runs asynchronously.
let queryList connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.queryList
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the sets of rows as a list accordingly to the command definition.
/// This function runs synchronously.
let queryListSync connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.queryListSync
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the first set of rows as a list accordingly to the command definition.
/// This function runs asynchronously.
let querySetList connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetList
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the first set of rows as a list accordingly to the command definition.
/// This function runs synchronously.
let querySetListSync connection read (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetListSync
        connection Constants.Deps SqlServerConfiguration.Snapshot read commandDefinition

/// Execute the command and return the 2 first sets of rows as a tuple of 2 lists accordingly to the command definition.
/// This function runs asynchronously.
let querySetList2 connection read1 read2 (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetList2
        connection Constants.Deps SqlServerConfiguration.Snapshot read1 read2 commandDefinition

/// Execute the command and return the 2 first sets of rows as a tuple of 2 lists accordingly to the command definition.
/// This function runs synchronously.
let querySetList2Sync connection read1 read2 (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetList2Sync
        connection Constants.Deps SqlServerConfiguration.Snapshot read1 read2 commandDefinition

/// Execute the command and return the 3 first sets of rows as a tuple of 3 lists accordingly to the command definition.
/// This function runs asynchronously.
let querySetList3 connection read1 read2 read3 (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetList3
        connection Constants.Deps SqlServerConfiguration.Snapshot read1 read2 read3 commandDefinition

/// Execute the command and return the 3 first sets of rows as a tuple of 3 lists accordingly to the command definition.
/// This function runs synchronously.
let querySetList3Sync  connection read1 read2 read3 (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.querySetList3Sync
        connection Constants.Deps SqlServerConfiguration.Snapshot read1 read2 read3 commandDefinition

/// Execute the command accordingly to its definition and,
/// - return the first cell value, if it is available and of the given type.
/// - throw an exception, otherwise.
/// This function runs asynchronously.
let executeScalar<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeScalar<'Scalar, _, _, _, _, _, _, _, _>
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition

/// Execute the command accordingly to its definition and,
/// - return the first cell value, if it is available and of the given type.
/// - throw an exception, otherwise.
/// This function runs synchronously.
let executeScalarSync<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeScalarSync<'Scalar, _, _, _, _, _, _, _, _>
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition

/// Execute the command accordingly to its definition and,
/// - return Some, if the first cell is available and of the given type.
/// - return None, if first cell is DBNull.
/// - throw an exception, otherwise.
/// This function runs asynchronously.
let executeScalarOrNone<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeScalarOrNone<'Scalar, _, _, _, _, _, _, _, _>
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition

/// Execute the command accordingly to its definition and,
/// - return Some, if the first cell is available and of the given type.
/// - return None, if first cell is DBNull.
/// - throw an exception, otherwise.
/// This function runs synchronously.
let executeScalarOrNoneSync<'Scalar> connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeScalarOrNoneSync<'Scalar, _, _, _, _, _, _, _, _>
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition

/// Execute the command accordingly to its definition and, return the number of rows affected.
/// This function runs asynchronously.
let executeNonQuery connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeNonQuery
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition

/// Execute the command accordingly to its definition and, return the number of rows affected.
/// This function runs synchronously.
let executeNonQuerySync connection (commandDefinition: SqlServerCommandDefinition) =
    SqlCommand.executeNonQuerySync
        connection Constants.Deps SqlServerConfiguration.Snapshot commandDefinition
