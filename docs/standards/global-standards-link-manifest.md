# Global Standards Link Manifest

## Purpose
Define how this repository references canonical standards from a dedicated global standards repository while preserving repository-local deviations.

## Canonical Source Repository
- Repository name: `GlobalStandards` (suggested)
- Canonical path root: `docs/standards/`
- Azure DevOps remote URL: `https://dev.azure.com/tcrowe0170/DocumentManagerAPI/_git/GlobalStandards`

## Canonical Documents to Reference
- `docs/standards/global-engineering-standards.md`
- `docs/standards/global-engineering-governance.md`
- `docs/standards/global-model-execution-playbook.md`
- `docs/standards/copilot-global-user-instructions-template.md`
- `docs/standards/copilot-global-user-instructions-ready-to-paste.md`

## Repository-Local Documents (Keep Local)
- `.github/copilot-instructions.md` (project-specific deviations only)
- `docs/operations/*` handoff and operational notes
- repo-specific pipeline files and environment settings

## Sync Strategy
1. Treat the global repository as the authoritative source for canonical standards documents.
2. Pull updates from the global repository into local `docs/standards/` on a controlled cadence.
3. Keep local edits out of canonical files unless the global repository is also updated.
4. Record approved local deviations only in `.github/copilot-instructions.md`.

## Verification Checklist
- [ ] Shared standards references in `.github/copilot-instructions.md` point to the expected canonical document names.
- [ ] Local repo contains latest canonical standards files from the global repository.
- [ ] Local `.github/copilot-instructions.md` contains only repository-specific deviations.
- [ ] Pipeline and branch governance behavior matches `global-engineering-governance.md`.

## Bootstrap Commands (Example)
```powershell
# initialize canonical global standards repository
.\scripts\standards\Initialize-GlobalStandardsRepository.ps1 -GlobalRepositoryRoot 'C:\Repos\GlobalStandards'

# apply shared standards from canonical source repo to this repo (when run from source repo)
.\scripts\standards\Apply-SharedStandards.ps1 -TargetPaths 'C:\Repos\DocumentManagerAPI'
```

## Master/Main Note
All branching and promotion guidance must explicitly account for repositories that use either `master` or `main` as the root branch by referencing root branch behavior as `Master/Main`.
