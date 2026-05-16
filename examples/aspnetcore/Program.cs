// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "aspnetcore-example";

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o =>
        {
            o.RecordException = true;
        })
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

// Happy-path GET.
app.MapGet("/", () => "ok");

// Templated GET with route parameter (validates http.route resolution).
app.MapGet("/users/{id:int}", (int id) => Results.Ok(new { id, name = $"user-{id}" }));

// 201 Created.
app.MapPost("/items", () => Results.Created("/items/1", new { id = 1 }));

// PUT (idempotent update).
app.MapPut("/items/{id:int}", (int id) => Results.Ok(new { id, updated = true }));

// PATCH (partial update).
app.MapPatch("/items/{id:int}", (int id) => Results.Ok(new { id, patched = true }));

// DELETE -> 204 No Content.
app.MapDelete("/items/{id:int}", (int id) => Results.NoContent());

// HEAD via MapMethods (no body returned).
app.MapMethods("/items/{id:int}", HttpMethodLists.HttpHead, (int id) => Results.Ok());

// OPTIONS via MapMethods.
app.MapMethods("/items", HttpMethodLists.HttpOptions, () => Results.Ok());

// Catch-all wildcard route.
app.MapGet("/files/{*path}", (string path) => Results.Ok(new { path }));

// Explicit 400 (validates client-error status_code attribute).
app.MapGet("/bad-request", () => Results.BadRequest(new { error = "bad input" }));

// Explicit 401.
app.MapGet("/unauthorized", () => Results.Unauthorized());

// Explicit 500 via Results.Problem (handled error).
app.MapGet("/error", () => Results.Problem("intentional", statusCode: 500));

// Unhandled exception path (validates error.type attribute on spans/metrics
// when RecordException is enabled and middleware does not catch it).
app.MapGet("/throw", () => { throw new InvalidOperationException("test"); });

app.Run();

internal static class HttpMethodLists
{
    public static readonly string[] HttpHead = { "HEAD" };
    public static readonly string[] HttpOptions = { "OPTIONS" };
}
