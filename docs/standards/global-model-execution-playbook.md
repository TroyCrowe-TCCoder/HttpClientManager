# Global Model Execution Playbook

## Purpose
Define a reusable execution pattern any active model can follow to read governance rules, verify implementation, identify drift, and apply missing check-in/merge/deploy pieces consistently.

## Required Read Order (Bootstrapping)
Before proposing or implementing repository governance changes, the active model must read in this order:
1. `docs/standards/global-engineering-governance.md`
2. `docs/standards/global-engineering-standards.md`
3. `docs/standards/copilot-global-user-instructions-template.md`
4. `.github/copilot-instructions.md`
5. Active handoff and operational plan documents in `docs/operations/`

## Scope and Plan Acknowledgement Requirement
Before implementation, the active model must:
- Read planned work artifacts relevant to the requested slice.
- Explicitly acknowledge understanding of plan scope and boundaries.
- Confirm whether any requested change crosses repository or environment boundaries.

## Standard Execution Workflow
1. Identify current implementation state.
2. Compare state against global governance checklist.
3. Identify missing or inconsistent implementation pieces.
4. Apply required updates with minimal targeted edits.
5. Validate consistency and summarize actionable outcomes.

## Required Output Contract
Each governance execution response must include:
1. Current state vs expected state summary.
2. Files created/updated.
3. Assumptions and risks.
4. Next actions checklist (ordered).

### Next actions checklist format (required)
Use markdown checkboxes in ordered sequence:
- [ ] Action 1
- [ ] Action 2
- [ ] Action 3

## Pipeline Auto-Generation and Repair Rules
When asked to verify or implement governance:
- If validation pipeline is missing, create it.
- If release pipeline is missing, create it.
- If deploy-window behavior is incorrect, repair conditions to enforce:
  - before 8 PM CST CI run: skip deploy stage (no wait gate)
  - before 8 PM CST manual run: allow deployment
  - at/after 8 PM CST: allow automatic deployment
- Preserve approved repository-specific deviations.

## Branch and PR Enforcement Rules
- Treat root branch as `Master/Main` to support repositories using either `master` or `main`.
- Enforce no direct check-ins to `dev`.
- Enforce no direct check-ins to `Master/Main`.
- Enforce PR flow:
  - `feature/*` -> `dev`
  - `dev` -> `Master/Main`

## Environment and Deployment Notes
- Current baseline reality for this rollout set:
  - no remote `Dev` deployment environment
  - no remote `QA` deployment environment
- Apply environment-chain logic when remote environments are introduced.
- Until then, enforce pipeline-governed promotion behavior and deployment timing policy.

## Documentation and Handoff Rules
For any meaningful governance, pipeline, or delivery-flow change:
- Update related documentation in the same work slice.
- Update handoff docs with current state and next starting point.
- Record DB status when database work is active or ownership is transitioning.
- Keep handoff language executable and explicit so a new operator can resume quickly.

## Drift Verification Checklist
Use this checklist during verification:
- [ ] Repository uses `feature/* -> dev -> Master/Main` PR flow.
- [ ] Direct commits to `dev` and `Master/Main` are blocked by policy.
- [ ] Validation pipeline exists and does not deploy.
- [ ] Release pipeline exists and applies deploy-window rules.
- [ ] Daytime CI runs do not wait for manual approval gates.
- [ ] Documentation reflects actual pipeline behavior.

## Prompt Macros (Reusable)
### Verify governance and report drift
Read global governance and repo addendum, compare current branch/pipeline implementation, and return a drift report with a checked next-actions checklist.

### Implement missing governance pieces
Read global governance and repo addendum, create or fix missing branch/pipeline governance pieces, and update related documentation plus handoff.

### Prepare handoff for operator transition
Read current plan and handoff docs, acknowledge scope boundaries, update implementation status, and provide an ordered checked handoff checklist.
