# Two distribution tracks: a per-machine installer and a portable binary, as peers

Covfefe ships two ways: a per-machine installer that requires administrator rights and installs into Program Files, and a portable standalone binary that requires none. Neither is the primary channel and neither is a fallback for the other — each serves its own audience.

## Context

Covfefe is used in environments where many users have no local administrator authority. It is sanctioned by IT in those environments, but IT deployment turnaround takes weeks, so a route must exist that a user can take themselves. Central IT deployment (Intune / SCCM / GPO) is explicitly out of scope and is not a distribution channel.

## Considered Options

A single per-user install into `%APPDATA%` was the earlier decision. It satisfied the no-admin requirement for every user at once, but installing executables into `%APPDATA%` is a strong antivirus and SmartScreen heuristic and is the likeliest cause of the false positives users reported. Splitting the tracks lets the installer be an ordinary, well-behaved per-machine install while the portable binary carries the no-admin case alone.

## Consequences

The portable build must be self-contained: a framework-dependent build would need the .NET runtime installed, which itself requires admin. `%APPDATA%` is abandoned as an install location on both tracks. Code signing becomes more important rather than less, because a loose unsigned `.exe` has no installer reputation to lean on. The portable track does not defeat AppLocker or WDAC where those are enforced.
