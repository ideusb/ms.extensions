﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Provides an optional base class for an <see cref="IChatClient"/> that passes through calls to another instance.
/// </summary>
/// <remarks>
/// This is recommended as a base type when building clients that can be chained around an underlying <see cref="IChatClient"/>.
/// The default implementation simply passes each call to the inner client instance.
/// </remarks>
/// <related type="Article" href="https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai#custom-ichatclient-middleware">Custom IChatClient middleware.</related>
public class DelegatingChatClient : IChatClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingChatClient"/> class.
    /// </summary>
    /// <param name="innerClient">The wrapped client instance.</param>
    protected DelegatingChatClient(IChatClient innerClient)
    {
        InnerClient = Throw.IfNull(innerClient);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an action that is invoked when a function call is made by the chat client.
    /// </summary>
    public Action<IChatClient, string, IDictionary<string, object?>?>? OnFunctionCall { get; set; }

    /// <summary>Gets the inner <see cref="IChatClient" />.</summary>
    protected IChatClient InnerClient { get; }

    /// <inheritdoc />
    public virtual Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        InnerClient.GetResponseAsync(messages, options, cancellationToken);

    /// <inheritdoc />
    public virtual IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        InnerClient.GetStreamingResponseAsync(messages, options, cancellationToken);

    /// <inheritdoc />
    public virtual object? GetService(Type serviceType, object? serviceKey = null)
    {
        _ = Throw.IfNull(serviceType);

        // If the key is non-null, we don't know what it means so pass through to the inner service.
        return
            serviceKey is null && serviceType.IsInstanceOfType(this) ? this :
            InnerClient.GetService(serviceType, serviceKey);
    }

    /// <summary>Provides a mechanism for releasing unmanaged resources.</summary>
    /// <param name="disposing"><see langword="true"/> if being called from <see cref="Dispose()"/>; otherwise, <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerClient.Dispose();
        }
    }
}
