using System.Globalization;
using System.Text;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Diagnostics;

/// <summary>
/// Provides helpers for exporting authorization diagnostics.
/// </summary>
public static class AuthorizationDiagnosticExporter
{
    /// <summary>
    /// Exports the specified authorization result tree to Graphviz DOT format.
    /// </summary>
    /// <param name="root">The root authorization result.</param>
    /// <returns>
    /// A string containing a Graphviz DOT representation of the authorization result tree.
    /// </returns>
    public static string ToGraphvizDot(RequestAuthorizationResult root)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph Authorization {");
        sb.AppendLine("  node [shape=box];");

        var num = 0;
        EmitNode(sb, root, ref num);

        sb.Append("}");
        return sb.ToString();
    }

    private static string EmitNode(StringBuilder sb, RequestAuthorizationResult node, ref int num)
    {
        var color = node.IsAuthorized ? "green" : "red";

        var nodeId = GetNodeId(ref num);
        sb.AppendLine($"  {nodeId} [label=\"{EscapeGraphvizLabel(node.Requirement.ToString())}\", color={color}];");

        var diagnostic = node.Diagnostic;
        if (diagnostic == null)
        {
            return nodeId;
        }

        foreach (var evaluated in diagnostic.EvaluatedChildren ?? [])
        {
            var nextNodeId = EmitNode(sb, evaluated, ref num);
            sb.AppendLine($"  {nodeId} -> {nextNodeId}");
        }

        foreach (var skipped in diagnostic.SkippedChildren ?? [])
        {
            var nextNodeId = GetNodeId(ref num);
            sb.AppendLine($"  {nextNodeId} [label=\"{EscapeGraphvizLabel(skipped.ToString())}\", color=orange];");
            sb.AppendLine($"  {nodeId} -> {nextNodeId}");
        }

        return nodeId;
    }

    private static string GetNodeId(ref int num)
    {
        var id = "n" + num.ToString(CultureInfo.InvariantCulture);
        num++;
        return id;
    }

    private static string? EscapeGraphvizLabel(string? toEscape)
    {
        return toEscape?
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }
}
