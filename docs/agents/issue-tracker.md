# Issue tracker: Gitea (self-hosted)

Issues and specs for this repo live as **Gitea issues** on the self-hosted instance at
`http://vault.local:3000`, repo **`jmarsch/Covfefe`**. Use the [`tea`](https://gitea.com/gitea/tea)
CLI for all operations; drop to the REST API (`/api/v1`) only for what `tea` doesn't cover.

| Thing      | Value                                                         |
| ---------- | ------------------------------------------------------------- |
| Instance   | `http://vault.local:3000` (Gitea 1.27.1)                      |
| Repo       | `jmarsch/Covfefe`                                             |
| Git remote | `nas` — **`origin` is a GitHub mirror, not the tracker**      |
| CLI        | `tea` 0.11.1, login `jmarsch` (authenticated, set as default) |

## Always pass `--remote nas`

This clone has two remotes. `tea`'s auto-detection lands on `origin` (GitHub) and either fails with
`Remote repository required` or silently falls back to a cross-repo search that returns nothing. Run
without an explicit login and it **hangs on an interactive prompt**
(`no gitea login detected, whether falling back to login 'jmarsch'?`).

So: **every `tea` invocation in this repo takes `--remote nas`.** Long form if you need it:
`--login jmarsch --repo jmarsch/Covfefe`.

## Conventions

- **Create an issue**: `tea issues create --remote nas --title "..." --description "..."`.
  `--description` takes a single string — build multi-line bodies in a PowerShell here-string first.
- **Read an issue**: `tea issues <index> --remote nas --comments`
- **List issues**: `tea issues ls --remote nas --state open --output json --fields index,title,state,labels,body,author,comments`.
  Filter with `--labels "needs-triage"`.
- **Comment**: `tea comment <index> "..." --remote nas`
- **Apply / remove labels**: `tea issues edit <index> --remote nas --add-labels "..."` /
  `--remove-labels "..."`. `--add-labels` takes precedence in the same call — use two calls to do both.
- **Close**: `tea issues close <index> --remote nas`. Unlike `gh`, there is no `--comment` flag;
  post the closing note with a separate `tea comment`.
- **Manage labels**: `tea labels ls|create|update|rm --remote nas`

## Pull requests as a triage surface

**PRs as a request surface: no.** _(Set to `yes` if this repo treats incoming PRs as feature
requests; `/triage` reads this flag.)_

When set to `yes`, PRs run through the same labels and states as issues:

- **Read a PR**: `tea pulls <index> --remote nas --comments`; diff via
  `tea pulls <index> --remote nas --fields diff`.
- **List open PRs**: `tea pulls ls --remote nas --state open --output json`
- **Comment / label / close**: `tea comment <index> "..."`, `tea issues edit <index> --add-labels`,
  `tea pulls close <index>`.

Gitea shares one number space across issues and PRs, so a bare `#42` may be either: resolve with
`tea pulls 42 --remote nas` and fall back to `tea issues 42 --remote nas`.

## When a skill says "publish to the issue tracker"

Create a Gitea issue with `tea issues create --remote nas`.

## When a skill says "fetch the relevant ticket"

Run `tea issues <index> --remote nas --comments`.

## Multi-line bodies and UTF-8

**On PowerShell 7 this is a non-issue** — verified 2026-09-20 publishing seven ticket bodies through
the REST API with no special handling beyond reading the file as UTF-8. The rest of this section
applies to **Windows PowerShell 5.1**, which is still what `powershell.exe` launches. The safe
pattern below costs nothing on either version, so keep using it.

`--description` takes one string, and Windows PowerShell 5.1 corrupts it in two separate ways if the
text is read with `Get-Content`. Both fail silently — the issue is created, with broken content.

- **Encoding.** `Get-Content -Raw` defaults to ANSI, so a UTF-8 file with no BOM arrives mangled
  (an em dash becomes `â€”`).
- **Note properties.** `Get-Content` decorates the returned string with `PSPath` / `PSProvider`
  note properties. `ConvertTo-Json` then emits `{"value":…,"Length":…}` instead of a string, and
  `JavaScriptSerializer` throws a circular-reference error on `RuntimeModule`.

Read through .NET instead, which avoids both:

```powershell
$body = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
```

When posting through the REST API, serialize and send as explicit UTF-8 bytes:

```powershell
$payload = @{ body = $body } | ConvertTo-Json -Compress
$bytes   = [System.Text.Encoding]::UTF8.GetBytes($payload)
Invoke-RestMethod -Method Patch -Uri "$API/issues/<n>" -Headers $H `
  -ContentType "application/json; charset=utf-8" -Body $bytes
```

Verify afterwards by reading the issue back and counting em dashes (`[char]0x2014`) against the
source file. A count of zero where the source has many means the body was mangled.

## REST API fallback

`tea` does not expose issue dependencies. Call the API directly; read the token from `tea`'s config
at call time and never print it:

```powershell
$token = [regex]::Match((Get-Content "$env:LOCALAPPDATA\tea\config.yml" -Raw),
                        '(?m)^\s*token:\s*(\S+)').Groups[1].Value
$H   = @{ Authorization = "token $token" }
$API = "http://vault.local:3000/api/v1/repos/jmarsch/Covfefe"
```

## Wayfinding operations

Used by `/wayfinder`. The **map** is a single issue; **tickets** are child issues.

- **Map**: an issue labelled `wayfinder:map` holding the Notes / Decisions-so-far / Fog body.
  `tea issues create --remote nas --labels wayfinder:map`.
- **Child ticket**: Gitea 1.27 has **no sub-issues API** (verified against this instance's swagger),
  so parent/child is conventional: add the child to a markdown task list in the map body and put
  `Part of #<map>` at the top of the child body. Labels: `wayfinder:<type>`
  (`research`/`prototype`/`grilling`/`task`). Once claimed, assign the ticket to the driving dev.
- **Blocking**: use Gitea's **native issue dependencies** — UI-visible and canonical.

  ```powershell
  Invoke-RestMethod -Method Post "$API/issues/<child>/dependencies" -Headers $H `
    -ContentType application/json `
    -Body (@{ index = <blocker>; owner = 'jmarsch'; repo = 'Covfefe' } | ConvertTo-Json)
  ```

  The payload takes the blocker's **issue index** (its `#number`), not a database id. Read blockers
  back with `GET $API/issues/<n>/dependencies`, which returns the issues that block `<n>`; `DELETE`
  removes one edge. If dependencies are disabled in the repo settings, fall back to a
  `Blocked by: #<n>, #<n>` line at the top of the child body. A ticket is unblocked when every
  blocker is closed.
- **Frontier query**: list the map's open children (`tea issues ls --remote nas --state open`, scoped
  to the map's task list), then drop any with an assignee or an open blocker. Gitea's issue payload
  carries **no** dependency summary, so checking blockers costs one
  `GET $API/issues/<n>/dependencies` per candidate — check only the unassigned ones. First in map
  order wins.
- **Claim**: `tea issues edit <n> --remote nas --add-assignees jmarsch`, the session's first write.
- **Resolve**: `tea comment <n> "<answer>" --remote nas`, then `tea issues close <n> --remote nas`,
  then append a context pointer to the map's Decisions-so-far.
