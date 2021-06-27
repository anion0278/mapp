using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmap.CommonServices
{
    //public class OperationResult
    //{
    //    private readonly IList<IReportItem> _messages = new List<IReportItem>();
    //    private readonly IList<OperationResult> _mergedResults = new List<OperationResult>();

    //    public static OperationResult Succeeded => new OperationResult("Operation");

    //    public bool Success => _messages.All(p => p.Severity < Severity.Warning) && _mergedResults.All(r => r.Success);

    //    public Severity HighestSeverity => GetHighestSeverity();

    //    public string OperationName { get; }

    //    public OperationResult(string operationName)
    //    {
    //        OperationName = operationName;
    //    }

    //    public override string ToString()
    //    {
    //        var builder = new StringBuilder();
    //        foreach (var mergedResult in _mergedResults)
    //        {
    //            var mergedText = mergedResult.ToString();
    //            if (!string.IsNullOrEmpty(mergedText)) builder.Append(mergedText);
    //        }

    //        foreach (var item in _messages)
    //        {
    //            builder.AppendLine(item.ToString());
    //        }

    //        return builder.ToString();
    //    }

    //    private Severity GetHighestSeverity()
    //    {
    //        var severities = _messages
    //            .Select(m => m.Severity)
    //            .Concat(_mergedResults.Select(r => r.HighestSeverity))
    //            .ToArray();

    //        return severities.Any() ? severities.Max() : Severity.Information;
    //    }

    //    public OperationResult MergeWith(OperationResult partialResult)
    //    {
    //        _mergedResults.Add(partialResult);
    //        return this;
    //    }

    //    public OperationResult AppendInfo(string message)
    //    {
    //        Append(Severity.Information, message);
    //        return this;
    //    }

    //    public OperationResult AppendWarning(string message)
    //    {
    //        Append(Severity.Warning, message);
    //        return this;
    //    }

    //    public OperationResult AppendError(string message)
    //    {
    //        Append(Severity.Error, message);
    //        return this;
    //    }

    //    public OperationResult AppendMessage(IReportItem item)
    //    {
    //        _messages.Add(item);
    //        return this;
    //    }

    //    private OperationResult Append(Severity severity, string message)
    //    {
    //        _messages.Add(new ReportItem(message, severity));
    //        return this;
    //    }
    //}

    //public class OperationResult<T> : OperationResult
    //{
    //    public static OperationResult<T> SucceededWithData(T data)
    //    {
    //        return new OperationResult<T>(data, "Operation");
    //    }

    //    public T ResultData { get; }

    //    public OperationResult(T resultData, string operationName)
    //        : base(operationName)
    //    {
    //        ResultData = resultData;
    //    }
    //}

    //public enum Severity
    //{
    //    Information,
    //    Warning,
    //    Error
    //}
}