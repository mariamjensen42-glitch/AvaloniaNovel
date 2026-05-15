using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaNovel.Services;

public interface ILLMService
{
    Task<string> InvokeAsync(string userPrompt, string? systemPrompt = null);
    IAsyncEnumerable<string> InvokeStreamAsync(
        string userPrompt,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);
}
