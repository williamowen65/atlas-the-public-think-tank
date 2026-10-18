using System.Collections;
using Atlas.Application.Storage;

namespace Atlas.ConsoleApp.Diagnostics;

/// <summary>Developer-only terminal formatting; SQL reads stay behind infrastructure.</summary>
internal static class SqlCollectionDisplay
{
    public static string Describe(string collectionKey) => FormatRows(SqlStorage.ReadCollection(collectionKey));
    private static string FormatRows<T>(IEnumerable<T> rows) => string.Join(
        Environment.NewLine + Environment.NewLine,
        rows.Select(row => FormatRecord(row!))
    );
    private static string FormatRecord(object row) => string.Join(
        " | ",
        row.GetType().GetProperties().Select(property =>
    {
        var value = property.GetValue(row);
        var text = value is IEnumerable items && value is not string
            ? "[" + string.Join(
                "; ",
                items.Cast<object>().Select(item => item.GetType().IsValueType ? item.ToString() : FormatRecord(item))
            ) + "]"
            : value?.ToString() ?? "(none)";
        return $"{property.Name}: {text}";
    })
    );

}
