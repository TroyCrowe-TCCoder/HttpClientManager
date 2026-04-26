# Global Engineering Governance

## Purpose and Scope
This document is the global source of truth for repository governance covering check-in rules, merge controls, and deployment behavior. It is intended for reuse across repositories and applications.

Repository-local instruction files must contain only project-specific deviations, environment details, and approved exceptions.

## Rule Precedence
1. Global governance documents in `docs/standards/`
2. Repository-local instruction addendum (`.github/copilot-instructions.md`)
3. Repository operational notes and handoff documents

When a repository rule conflicts with this global baseline, treat the conflict as drift unless the repository file explicitly records an approved deviation.

## Branching and Check-In Governance
### Root branch naming
- Repositories may use either `master` or `main` as the root branch.
- Governance references must use `Master/Main` to account for both.

### Required branch model
- Long-lived branches:
  - `dev`
  - `Master/Main`
- Delivery branch types:
  - `feature/*`
  - `hotfix/*` (when needed)

### Check-in rules
- No direct check-ins/commits to `dev`.
- No direct check-ins/commits to `Master/Main`.
- All changes must flow through PRs.
- Work branch source:
  - Create `feature/*` from refreshed local `dev`.
- Minimum local sync before branch creation:
  1. Fetch remote updates.
  2. Sync local `dev` with remote `dev`.
  3. Branch from the refreshed local `dev`.

## Pull Request and Merge Governance
### Required PR flow
1. `feature/*` -> `dev`
2. `dev` -> `Master/Main`

### Merge controls
- Require manual user approval before merge.
- Auto-complete may be enabled only after required approvals and required checks pass.
- Branch policies must enforce PR-only merges for `dev` and `Master/Main`.

### Required validation checks
- Build validation pipeline must pass for `feature/*` -> `dev` PRs.
- Release/deploy pipeline validation must pass for `dev` -> `Master/Main` PRs.
- Repository-required automated tests must pass for each protected branch PR.

## Deployment Governance
### Required pipeline split
1. Validation pipeline (feature/dev integration)
   - No deployment stage.
2. Release pipeline (dev/Master/Main promotion)
   - Deployment-enabled.

### Production deployment window policy (CST)
- At/after 8:00 PM CST:
  - Deployment may auto-run.
- Before 8:00 PM CST for CI-triggered runs:
  - Deployment stage must skip.
  - Do not hold the run waiting for manual approval.
- Before 8:00 PM CST for manual runs:
  - Deployment is allowed to run immediately.

### Manual approval gate policy
- Do not use long-wait manual-validation gates for daytime CI runs.
- Prefer explicit manual run initiation when daytime deployment is needed.

## Environment Governance
### Current environment reality
- No remote `Dev` environment is currently provisioned.
- No remote `QA` environment is currently provisioned.
- Current deploy path remains pipeline-based promotion toward production from `dev` through `Master/Main` governance.
- Environment-chain progression requirements apply when additional remote environments are introduced.

### Future environment model (when available)
- Standard chain: Dev -> QA -> Prod.
- Environment approvals and deployment controls should be layered at environment boundaries.

## Pipeline Implementation Requirements
### Minimum release pipeline behavior
- Build/publish artifacts.
- Apply deployment configuration.
- Execute deployment.
- Validate service health/readiness.
- Enforce time-window deployment condition behavior.

### Guardrails
- Avoid hardcoding secrets.
- Use secure variable groups/Key Vault-backed configuration.
- Ensure deployment conditions are deterministic and observable in run logs.

## Compliance and Drift Verification
Use this checklist to verify a repository against the global standard:
- [ ] Uses `feature/* -> dev -> Master/Main` PR flow.
- [ ] Blocks direct check-ins to `dev` and `Master/Main`.
- [ ] Has two-pipeline split (validation + release).
- [ ] Implements before/after 8 PM CST deploy behavior correctly.
- [ ] Does not wait on daytime CI approval gates.
- [ ] Documents approved repository-specific deviations.

## Exception Process
- Any deviation from this global governance must be explicitly documented in the repository addendum.
- Each deviation must include:
  - reason
  - owner
  - effective date
  - planned review date

## Reusable Implementation Template (Quick Copy)
- Branch policy language:
  - No direct commits to `dev` or `Master/Main`; PR-only merges required.
- Pipeline policy language:
  - Validation pipeline for feature/dev integration with no deploy.
  - Release pipeline for dev/Master/Main with time-window deploy behavior.
- Deployment policy language:
  - Before 8 PM CST CI: skip deploy stage.
  - Before 8 PM CST manual run: allow deploy.
  - At/after 8 PM CST: allow automatic deploy.
