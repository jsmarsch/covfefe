# Covfefe v2 is archived by tag, not kept in the tree or split into a second repo

Covfefe v2 — the .NET Framework application and its WiX v3 installer — has been deleted from the repository. It is preserved at the annotated tag `2.3.41-final`, present on both remotes. `git checkout 2.3.41-final` restores it in full.

## Context

The repository carried the same application twice: `src/Covfefe/` (v2, .NET Framework) and `src/Covfefe-core/` (the .NET rewrite). Both used the `Covfefe` root namespace with identical file names — `Program.cs`, `TrayForm.cs`, `SettingsFacade.cs` and the rest — so every search and every open-by-name returned two candidates, and every agent working the repo had to be told which tree to ignore. `CLAUDE.md` carried a section whose only job was to explain the duplication.

v2 was already frozen by decision (`modernizeplan.md`, Constraint 4): no further work would happen there. What remained was not an active second codebase but a copy being kept in case someone needed to read it.

## Considered Options

**Keep both trees.** Rejected. The cost is paid continuously, on every search and every task, to preserve something nobody is allowed to change.

**Move v2 to a second repository.** Rejected. A second repo is strictly worse than a tag for this purpose: it duplicates the remote pair (this repo already mirrors to two — a self-hosted Gitea and GitHub, so it would mean four), the issue tracker and any future CI, all to hold a codebase that is finished. A second repository earns its keep when two lines must ship simultaneously to different audiences, or when the new thing is a different product. Neither applies: same name, same icon, one release pipeline, and v2 is not shipping again.

**Keep a long-lived `v2` maintenance branch.** Rejected for now, but cheap to create later from the tag if a v2 patch ever becomes necessary. Creating it up front asserts an intention to patch that does not exist.

**Archive by tag.** Chosen. A tag is immutable, named, present on both remotes, and costs nothing to carry.

## Consequences

The recovery path is a single command, but it only helps someone who knows the tag exists — which is why it is recorded here and in `CLAUDE.md` rather than left to repository archaeology.

`2.3.41-final` points at the last *released* v2 commit. One unreleased commit on the `dev` branch bumps v2 from .NET Framework 4.6 to 4.8 and is not covered by the tag; it is a six-line change across four files and was never shipped. If `dev` is ever pruned, that bump is lost. This is accepted, and noted so that it is a decision rather than a discovery.

Deleting the WiX v3 installer with v2 leaves the project with no installer at all until the replacement specced in `modernizeplan.md` §10 is built. That installer was tied to both a binary that no longer exists and an install location (`%APPDATA%`) that ADR-0002 abandons, so nothing recoverable was lost — but two facts had to survive the deletion: the `UpgradeCode` `0a24d660-a0cd-483f-9f71-6b33d7f0eed1` must remain stable forever or upgrades break for existing users, and the old component GUID must **not** be carried forward, because MSI component rules bind a GUID to a resource at a path, and the path is changing.

The `Covfefe-core` project keeps its name for now. The `-core` suffix distinguished it from a tree that no longer exists, so it has become meaningless, but renaming the folder, the namespace and the solution would touch every open ticket. That rename is deferred to a point where it is not competing with in-flight work.

Removing the second solution file had one unlooked-for benefit: a bare `dotnet test` at the repository root now works, where it previously failed with _"Found more than one solution file"_.
