# Copilot Instructions

## General Guidelines
- This repository follows the class-library non-deployed-products model.
- Keep repository-local deviations only in the root `standards/` folder when approved for this repository.
- Add and maintain a repository-local validation entry point at `scripts/validate.ps1`.
- Provide concise progress updates and continue execution through all planned steps without waiting for additional prompts unless there is a real blocker; explicit user permission is already granted for the requested actions.
- When a plan has been flushed out and approved, proceed through all plan steps without waiting for the user to prompt between steps unless a real blocker or question arises.
- When asked to complete rollout work in this repository, commit changes, push the branch, and create the PR according to the repository standards rather than stopping at local edits.
- Ensure PRs created by me have auto-complete selected.
- The AI model and the human developer must use the `working/` directory only for active task context and planning artifacts. Files created there for a task may remain only while that task is in progress and must be removed as soon as that task is complete.
- When updating standards files, write directives for two audiences (the AI model and the human developer) and use mandatory wording rather than optional or suggestive language.
- When an instruction says to build an outline, it means a numbered or bulleted outline, not a plain list of tasks or loose step lines; this likely belongs in `global-file-specification-standards.md`.
- All designs must have a graphical and outlined representation when a workflow process is being defined.
- For standards work, use the standards file's Pre-Commit Checklist as the review lens against the other affected standards files too, not just against the file being edited.
- When the user has already confirmed the relevant repo state, do not re-inspect it unnecessarily; proceed directly to the requested cleanup or change.

## Repository Hosting
- This repository is hosted on GitHub at `https://github.com/TroyCrowe-TCCoder/HttpClientManager`.
- Branch governance follows: `local branch -> remote branch -> PR to dev -> PR to main`.
- Use GitHub Actions (if/when configured) for CI; this class-library repository does not require a standalone delivery pipeline.
- Direct pushes to `dev` and `main` are disallowed for non-owner users; PRs require repository owner approval.

## Library Builds
- When referring to 'library builds' in this standards work, treat that as Class Library repositories specifically.

## Repository Status Inquiry
- When asked what is live on dev or main, interpret that as the active repository's branches unless they explicitly say GlobalStandards.

## Merge Behavior
- Prefer non-squash behavior when validating branch mirroring/deletion semantics to avoid issues caused by squash merges that remove change history. For this repository, squash merge was the cause of the branch cleanup and promotion issue; use basic merge for this validation/promotion flow.
- After cleanup is complete, disable squash merge behavior for these promotion/cleanup PRs to preserve branch history and expected deletion semantics.

## Branch Repair Plan
- First clean main to the intended final pipeline state, then recreate dev from main, then test the implementation again without squash merges.
