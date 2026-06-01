# OpenSpec Git Workflow

Use this for all `/opsx` commands and matching `openspec-*` skills when the repo is a git repository.

## Branch naming

- Feature branch: `openspec/<change-name>` (same kebab-case as `openspec/changes/<change-name>/`)

## Base branch

- Prefer `main`; if only `master` exists (or `origin/HEAD` → `origin/master`), use `master`
- When `origin` exists: `git fetch origin` before creating a branch

## Create or checkout the feature branch

Run after the change name is known (propose: before `openspec new change`; apply: before implementation).

1. Resolve base branch (`main` or `master` as above).
2. If already on `openspec/<change-name>`, continue.
3. Else if `openspec/<change-name>` exists locally: `git checkout openspec/<change-name>`
4. Else create from base:
   - `git checkout <base>`
   - `git pull origin <base>` when `origin` exists and pull succeeds (non-fatal if offline)
   - `git checkout -b openspec/<change-name>`
5. If the working tree has unrelated uncommitted changes, warn the user and ask whether to continue, stash, or commit before proceeding.

## Workflow commits

These commits are part of the OpenSpec workflow (not optional drive-by commits).

| Phase | When | Message pattern |
|-------|------|-----------------|
| Propose | After all apply-required artifacts exist | `openspec: propose <change-name>` |
| Apply | After all tasks are complete (before PR) | `openspec: implement <change-name>` |
| Archive | After moving change to archive (and spec sync if any) | `openspec: archive <change-name>` |

Stage only files relevant to the change (OpenSpec artifacts, implementation, synced specs). Do not commit secrets (`.env`, credentials, etc.).

## Open a pull request (apply complete)

When **all tasks** are done on a feature branch:

1. Run in parallel: `git status`, `git diff`, `git log -5 --oneline`, and confirm tracking branch / `git diff <base>...HEAD` scope.
2. Create the workflow commit if there are uncommitted changes (`openspec: implement <change-name>`).
3. Push: `git push -u origin HEAD` (requires network; request permissions if blocked).
4. Create PR with `gh pr create`:
   - **Title**: short summary from `proposal.md` (or change name if no proposal)
   - **Body** (HEREDOC): Summary bullets from proposal/design; test plan checklist from `tasks.md`; link `openspec/changes/<name>/`
5. Return the PR URL to the user.
6. Do **not** merge unless the user asks.

If `gh` or `origin` is unavailable, report what was committed and pushed locally and give manual PR steps.

## Archive and git

- Archive runs on the current branch (usually the feature branch before merge).
- After archive (and optional spec sync), commit with `openspec: archive <change-name>` and **push** so the PR includes the archive move.
- If a PR already exists, push updates it; do not open a second PR for the same branch unless the user asks.

## Guardrails

- Never change git config, force-push, or skip hooks unless the user explicitly requests it.
- Never amend commits unless the user explicitly requests it and amend rules from project instructions apply.
- If a PR already exists for `openspec/<change-name>`, push new commits instead of creating another PR.
