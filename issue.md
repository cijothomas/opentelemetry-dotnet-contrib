# [Feature Request] Geneva ETW Exporter: Opt-in to surface ETW write failures via ExportResult

## Problem

When the Geneva Exporter uses ETW transport on Windows, the exporter returns
`ExportResult.Success` even when the kernel dropped events due to buffer
exhaustion (e.g., the Geneva agent can't drain events fast enough). The ETW
consumer can detect drops via its `EventsLost` counter, but the producer
application has no visibility into these failures.

This is the correct default, but some users need producer-side detection of data
loss.

## Proposal

Add an opt-in option (e.g., `GenevaExporterOptions.ReportETWWriteFailures`) that
uses `EventSourceSettings.ThrowOnEventWriteErrors` on the internal
`EtwEventSource`. The existing export pipeline already catches exceptions from
the transport and returns `ExportResult.Failure`, so the plumbing is in place.

**What would be surfaced:** ETW buffer-full errors → `ExportResult.Failure`.

**What would NOT be surfaced:** No ETW session listening — the kernel reports
success in this case. Detecting this would require a separate `IsEnabled()`
check.

**Tradeoff:** Each failed write throws/catches an `EventSourceException`
internally, adding overhead under sustained failures. The .NET `EventSource` API
does not offer a lightweight error-code path — a separate issue in
[dotnet/runtime](https://github.com/dotnet/runtime) could request one.

## Motivation

A team was found using reflection hacks to modify the internal `EventSource`
configuration at runtime to detect these failures. A proper opt-in option would
eliminate such fragile workarounds.
