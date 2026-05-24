# Copilot Instructions

## General Guidelines
- This repository follows the class-library non-deployed-products model.
- Keep repository-local deviations only in the root `standards/` folder when approved for this repository.
- Add and maintain a repository-local validation entry point at `scripts/validate.ps1`.
- Proceed through execution without pausing for step-by-step approval unless a real blocking question remains. Concise step updates are preferred, and approval should not be requested for each step as long as the work follows the agreed plan exactly.
- When asked to complete rollout work in this repository, check in changes, push the branch, and create the PR according to the repository standards rather than stopping at local edits.
- Ensure PRs created by me have auto-complete selected.

## Repository Initialization Rules
- The `feature/* -> dev` path must create the traceable package output promoted later through the `dev -> main` flow. For this repository's variation, the Build step creates the NuGet package artifact, and the Dev->Main flow delivers it only when ChangeDetection is true. Library deliverables are compiled into NuGet packages, as in this repository.
- Handle the feature->dev commit, push, PR creation, and select auto-merge when preparing the PR. The commit must use a descriptive title and description; the PR description must include a summary and a list of changes. Create the PR with Auto-Merge selected; nothing runs until approval.
- Design the Feature->Dev and Dev->Main flow files as reusable templates with placeholders for required variables, so the flow can be generalized across repositories and app types. Feature->Dev remains the standard reusable flow, while database and WebApp/WebAPI repositories vary slightly in their app-specific delivery stages. Prefer app-type-specific pipeline/template variants rather than baking multiple app-type branches into shared files for a cleaner and lower-risk design.
- The repository setting 'Set PRs to auto-complete on creation by default' is enabled.

## Azure DevOps Flow Rules
- Build runs only when ChangeDetection is true.
- Test runs only when ChangeDetection is true and Build succeeded.
- Azure DevOps branch policy and auto-complete own PR merge completion after required validation succeeds.
- PR approval should start the Azure DevOps flow; ChangeDetection returns false for documentation-only changes and true for code changes.
- PR creation must not start the validation pipeline; the pipeline must wait until PR approval.
- CarryForward belongs to the downstream dev-side promotion flow, not the approval-gated PR validation run.
- Complete the repository and Azure DevOps configuration end-to-end when Azure DevOps pipeline work is requested for this repository, rather than stopping at YAML-only changes.
- For Azure DevOps rollout work in this repository, prefer a full reset first: clear all current pipeline and repository delivery configuration and start from scratch. The delivery model must use one run per environment, with logic inside that single run rather than multiple runs.
- Remove or replace outdated DevOps configuration first when it conflicts.
- Use action-based pipeline file names as firm rules, not variants or optional alternatives, to enforce consistency. Standard names in this repository are `approve.yml`, `change-detection.yml`, `build.yml`, `test.yml`, `merge.yml`, `publish.yml`, and `carry-forward.yml`. Handle environment interchangeability through variables rather than environment-specific file names.
- Apply the same code-check/change-detection pattern to the Main Release pipeline so it does not depend on a missing feature-to-dev build artifact.
- Use single-word action-based pipeline file names where possible, and two-word action-based names only when necessary.
- Build/Test/Merge/CarryForward each stop the pipeline and notify on failure.
- Implement fail-fast behavior in the single autonomous pipeline run: if any section fails, downstream work must be skipped immediately, and the flow should go straight to `failure.yml`; the pipeline must not require a second run to complete the process. Approval should gate the single pipeline start, while PR auto-merge can remain selected, and the same run should complete the process.
- Document the specific CI identity used for pipelines as **CI-DevOps-UMI** in [GlobalStandards/global-azure-devops-pipeline-standards.md](../../GlobalStandards/global-azure-devops-pipeline-standards.md).
- The UMI team-member role is expected to provide the standard repo PR permissions, while 'Bypass policies when completing pull requests' must be explicitly granted to HttpClientManager Build Service (TCrowe0170).

## Database Delivery Variation Rules
- In addition to general ChangeDetection, database flows must check for destructive schema changes such as column renames, dropped columns, dropped tables, data type changes, and data length changes. If destructive changes are detected, a pre-deploy script is required; otherwise, the dacpac alone is sufficient. If the pre-deploy script is missing, fail fast and notify that the pre-deploy script is required. After the pre-deploy script is created/run, a new artifact build must occur so the destructive change is no longer present in the generated deployment. The dacpac breaking-change check remains enabled as a failsafe.

## Standards Placement
- Azure DevOps process and pipeline rules should live in [GlobalStandards/global-azure-devops-pipeline-standards.md](../../GlobalStandards/global-azure-devops-pipeline-standards.md); repository-specific wording should live in [GlobalStandards/global-repository-standards.md](../../GlobalStandards/global-repository-standards.md).
- The `azure-devops-pipeline-standards.md` should reference and link `global-repository-standards.md`.
- Keep the temporary rules in this repository's `copilot-instructions.md` only until the build-out is complete.
