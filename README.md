![Build Status](https://github.com/vanheesk/Demo-CancellationToken/actions/workflows/ci/badge.svg)

# Demo on Cancellation of a pending request

A small demo application to demonstrate cancelling a request made from a frontend. 
Since cancelling a request is pretty standard behaviour, it should also not be marked as a failure in Application Insights.
(where it would cause false-positives on your alerting, thus lowering the meaning of alerting in general)

## Requirements

* Azure Application Insights
* .NET 9

## Build & Run

In order to effectively use/run, you'll need the connection-string of your application insight instance.  Please add the following section to the user-secrets of all three startup projects.

```json
{
  "ApplicationInsights": {
    "ConnectionString": "<YOUR-CONNECTION-STRING>"
  }
}
```

Build & Run should just work out of the box, no further changes required.
