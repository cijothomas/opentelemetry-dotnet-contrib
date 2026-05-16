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
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

app.MapGet("/", () => "ok");
app.MapGet("/users/{id:int}", (int id) => Results.Ok(new { id, name = $"user-{id}" }));
app.MapPost("/items", () => Results.Created("/items/1", new { id = 1 }));
app.MapPut("/items/{id:int}", (int id) => Results.Ok(new { id, updated = true }));
app.MapGet("/error", () => Results.Problem("intentional", statusCode: 500));

app.Run();
