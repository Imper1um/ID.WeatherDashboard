﻿using Microsoft.Data.SqlClient;
using System.Diagnostics;

public static class ExceptionExtensions
{
    public static string GetFullMessage(this Exception ex)
    {
        var str = $"{ex.GetType()}: {ex.Message}";
        str += $"{Environment.NewLine}:StackTrace =";
        var stackTrace = new StackTrace(ex, true);
        foreach (var frame in stackTrace.GetFrames())
        {
            str += $"{Environment.NewLine}:-{frame.GetFileName()}:{frame.GetMethod()?.Module.Name}:{frame.GetMethod()?.Name} (Line {frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()})";
        }
        var sqlException = ex as SqlException;
        if (sqlException != null)
        {
            str += $"{Environment.NewLine}:SqlException Data";
            str += $"{Environment.NewLine}:Line = {sqlException.LineNumber}";
            str += $"{Environment.NewLine}:Number = {sqlException.Number}";
            str += $"{Environment.NewLine}:Procedure = {sqlException.Procedure}";
            str += $"{Environment.NewLine}:Server = {sqlException.Server}";
            str += $"{Environment.NewLine}:State = {sqlException.SqlState}";
        }
        if (ex.InnerException != null)
        {
            var ieData = ex.InnerException.GetFullMessage();
            str += $"{Environment.NewLine}|Inner Exception:";
            str += Environment.NewLine + string.Join(Environment.NewLine, ieData.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(l => $"|{l}"));
        }
        return str;
    }
}