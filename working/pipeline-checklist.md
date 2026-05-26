# Active Pipeline Checklist

## Target Model
- [ ] Use a 2-flow design: `dev.yml` and `publish.yml`.
- [ ] Use Azure DevOps branch policies to require PR approval and build validation before merge.
- [ ] Do not code PR merge inside YAML.
- [ ] Do not override repo auto-complete behavior in YAML if Azure DevOps configuration already handles it.
- [ ] Keep real build and test steps in the validation pipeline so branch policy validation is meaningful.
- [ ] Keep CarryForward inside `dev.yml`, but run it only in the post-merge `dev` context.
- [ ] Keep Publish in the main-only pipeline, not the dev flow.
- [ ] Remove reliance on `merge.yml` and `failure.yml` from the target pipeline design.

## Execution Checklist
- [x] Re-baseline the workspace from a clean `origin/dev` state.
- [x] Reset Azure DevOps pipeline definitions and lingering test PRs.
- [x] Remove all existing YAML pipeline files from the repository.
- [x] Rebuild `dev.yml` as the single dev flow for PR validation plus post-merge CarryForward.
- [x] Recreate `change-detection.yml` for validation skip logic.
- [x] Recreate `build.yml` with real build and artifact steps.
- [x] Recreate `test.yml` with real test steps.
- [x] Fold CarryForward logic into `dev.yml` so it runs only on `refs/heads/dev` after merge.
- [x] Recreate `publish.yml` as the main-only publish flow.
- [x] Remove the standalone `carry-forward.yml` workflow from the rebuilt design.
- [ ] Configure Azure DevOps branch policy for `dev` to require approval and validation.
- [ ] Verify auto-complete/default PR behavior is configuration-driven, not YAML-driven.
- [x] Validate the workspace.
- [ ] Commit and create the validation PR.
- [ ] Recreate Azure DevOps pipeline definitions from the new YAML files.
- [ ] Monitor PR validation behavior.
- [ ] Monitor post-merge CarryForward behavior.
- [ ] Monitor main publish behavior.

## Notes
- PR approval and merge gating belong to Azure DevOps configuration.
- Validation pipeline code belongs in `dev.yml` plus `change-detection.yml`, `build.yml`, and `test.yml`.
- CarryForward must run only after the merge into `dev`, not inside the pre-merge validation run.
- Publish belongs only to the main flow.
- Move this checklist into the standards document only after the pipeline is confirmed working.
