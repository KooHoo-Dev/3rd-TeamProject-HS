# Project coding guidelines

- Do not add excessive exception handling or defensive guards. In ordinary Unity gameplay/editor code, let invalid assumptions fail visibly so Unity's error log and stack trace identify the issue.
- Do not use broad `try`/`catch`, silent catches, or fallback behavior merely to suppress errors or `NullReferenceException`s.
- Add explicit validation or exception handling only when the code can recover meaningfully, needs a user-facing error, protects persistent/external data, or the requirement specifically calls for it. Keep that handling narrow and observable (log or rethrow as appropriate).
- For a required user-facing flow, fail fast when its assumptions are broken. Do not conceal the failure with retries, default values, partial execution, or a workaround that leaves the game running incorrectly; let the error and stack trace reach the Unity Console.
- Prefer straightforward code whose normal path and failures are easy to read at a glance.
