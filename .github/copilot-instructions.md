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

## Change Detection Rules
- ChangeDetection should default to true and only switch to false when no code changes are found; Build and Test must then skip when false, while merge/completion should still proceed either way.
- For this repository's ChangeDetection, prioritize C#, CSS-family files, JavaScript/TypeScript variants (including ReactJS and `.rxjs`), and optionally Python; exclude VB, J#, C, C++, and PHP-related file types. Workflow/docs/instruction changes should not count as code changes.

## Repository Initialization Rules
- The `feature/* -> dev` path must create the traceable package output promoted later through the `dev -> main` flow. For this repository's variation, `approve.yml` is the single authoritative dev pipeline: in PR context it validates and merges, and on post-merge `dev` branch runs it conditionally republishes the durable promoted artifact and creates the next `dev -> main` PR. The Main Release flow delivers that promoted artifact only when ChangeDetection is true. Library deliverables are compiled into NuGet packages, as in this repository.
- Handle the feature->dev commit, push, PR creation, and select auto-merge when preparing the PR. The commit must use a descriptive title and description; the PR description must include a summary and a list of changes. Create the PR with Auto-Merge selected; nothing runs until approval.
- Design the Feature->Dev and Dev->Main flow files as reusable templates with placeholders for required variables, using `<NextBranchName>` for the branch promoted from dev rather than a repo-specific variable name. Feature->Dev remains the standard reusable flow, while database and WebApp/WebAPI repositories vary slightly in their app-specific delivery stages. Prefer app-type-specific pipeline/template variants rather than baking multiple app-type branches into shared files for a cleaner and lower-risk design.
- The repository setting 'Set PRs to auto-complete on creation by default' is enabled.
- For Class Library pipeline naming, keep orchestration files separate from action-step files: `dev.yml` and `main.yml` are orchestration files; `change-detection.yml`, `build.yml`, `test.yml`, `publish.yml`, and `carry-forward.yml` are action-step files; the shared support file should be named `variables.yml`.
- Use Dev, Main, and Variables folders, with `Variables/variables.yml` as the shared support file location. Use a distributed structure instead of a flat structure for the pipeline template layout: separate files organized under top-level Dev, Main, and Variables folders, not nested under templates/variables subfolders. This structure allows teams to copy those folders into a repo, replace placeholders, and update ADO YAML entry paths with minimal setup.
- Before replacing live pipeline files, preserve the current files in a backup directory for easy rollback, and perform a thorough equivalence review of new files for logic, order, and orchestration before switching and testing them.
- When removing legacy pipeline assets from `.azure-pipelines`, move them out of scope for rollback rather than deleting them.

## Azure DevOps Flow Rules
- Build runs only when ChangeDetection is true; if ChangeDetection is false, Build and Test must not run. Treat any run that builds/tests after non-code changes as a defect to investigate immediately.
- Test runs only when ChangeDetection is true and Build succeeded.
- The approval-triggered validation pipeline may complete the PR after required validation succeeds.
- PR approval should start the Azure DevOps flow; ChangeDetection returns false for documentation-only changes and true for code changes.
- PR creation must not start the validation pipeline; the pipeline must wait until PR approval.
- CarryForward must run only after merge completion is successful; this is normally enforced because all pipeline errors are hard failures that stop execution.
- CarryForward is not delivery; it creates the Main PR by carrying the Dev PR information forward into the next PR. CarryForward belongs only in the dev pipeline flow because main does not perform the carry-forward process.
- CarryForward must remain part of the single authoritative dev pipeline definition, but it should execute only on the real post-merge `dev` branch run. On PR runs Merge is the terminal dev action; on `refs/heads/dev` runs the same pipeline definition performs durable artifact republish and `dev -> main` PR creation.
- Complete the repository and Azure DevOps configuration end-to-end when Azure DevOps pipeline work is requested for this repository, rather than stopping at YAML-only changes.
- For Azure DevOps rollout work in this repository, prefer a full reset first: clear all current pipeline and repository delivery configuration and start from scratch. The delivery model must use one run per environment, with logic inside that single run rather than multiple runs.
- Remove or replace outdated DevOps configuration first when it conflicts.
- Use action-based pipeline file names as firm rules, not variants or optional alternatives, to enforce consistency. Standard names in this repository are `approve.yml`, `change-detection.yml`, `build.yml`, `test.yml`, `merge.yml`, `publish.yml`, and `carry-forward.yml`. Handle environment interchangeability through variables rather than environment-specific file names.
- Apply the same code-check/change-detection pattern to the Main Release pipeline so it does not depend on a missing feature-to-dev build artifact.
- The Main Release pipeline must only perform ChangeDetection and deliver artifacts when ChangeDetection is true; it must not run Build, Test, or CarryForward.
- Use single-word action-based pipeline file names where possible, and two-word action-based names only when necessary.
- Build/Test/Merge each stop the pipeline and notify on failure.
- Implement fail-fast behavior in the single autonomous pipeline run: if any section fails, downstream work must be skipped immediately, and the flow should go straight to `failure.yml`; the pipeline must not require a second run to complete the process. Approval should gate the single pipeline start, while PR auto-merge can remain selected, and the same run should complete the process.
- Document the specific CI identity used for pipelines as **CI-DevOps-UMI** in [GlobalStandards/global-azure-devops-pipeline-standards.md](../../GlobalStandards/global-azure-devops-pipeline-standards.md).
- The UMI team-member role is expected to provide the standard repo PR permissions, while 'Bypass policies when completing pull requests' must be explicitly granted to HttpClientManager Build Service (TCrowe0170).

## Database Delivery Variation Rules
- In addition to general ChangeDetection, database flows must check for destructive schema changes such as column renames, dropped columns, dropped tables, data type changes, and data length changes. If destructive changes are detected, a pre-deploy script is required; otherwise, the dacpac alone is sufficient. If the pre-deploy script is missing, fail fast and notify that the pre-deploy script is required. After the pre-deploy script is created/run, a new artifact build must occur so the destructive change is no longer present in the generated deployment. The dacpac breaking-change check remains enabled.

## Standards Placement
- Azure DevOps process and pipeline rules should live in [GlobalStandards/global-azure-devops-pipeline-standards.md](../../GlobalStandards/global-azure-devops-pipeline-standards.md); repository-specific wording should live in [GlobalStandards/global-repository-standards.md](../../GlobalStandards/global-repository-standards.md).
- The `azure-devops-pipeline-standards.md` should reference and link `global-repository-standards.md`.
- Keep the temporary rules in this repository's `copilot-instructions.md` only until the build-out is complete.

## Pipeline Control Variables
- The Dev pipeline must remain the authoritative pipeline using three gating variables for flow control: `CodeChanges`, `BuildSuccess`, and `TestSuccess`. Validate to ensure CarryForward and all stage conditions use those variables correctly.
- Build checks `CodeChanges`, Test checks `CodeChanges` and `BuildSuccess`, Merge follows successful validation or skipped code path. If `CodeChanges` is false, merge proceeds; if `CodeChanges` is true, both `BuildSuccess` and `TestSuccess` must be true or the merge must fail. CarryForward runs only on the successful post-merge `dev` branch run, and Failure is the terminal failure path.
- The Main pipeline workflow has three parts only—CodeChanges check, deliver only when CodeChanges is true, and complete the merge; merge completion is treated as the final outcome after successful delivery.

## Library Builds
- When referring to 'library builds' in this standards work, treat that as Class Library repositories specifically.

## Repository Status Inquiry
- When asked what is live on dev or main, interpret that as the active repository's pipelines unless they explicitly say GlobalStandards.

## Pipeline Refactoring
- Create separate `Dev` and `Main` folders in the working directory for pipeline refactoring.
- The `Dev` folder should only validate, while the `Main` folder should only deliver.
- Both folders should evaluate whether code changes are present before proceeding.
- Preserve the existing logic and change only the assembly/structure and file naming to match the user's workflow model.
- For the staged pipeline refactor, each pipeline piece should be independently created as a reusable template because the files will be templatized and reused elsewhere; preserve the current working logic while restructuring for reuse.

## Simplified Pipeline Experiment
- Remove conditions and carry-forward from the pipeline.
- Use normal merge plus delivery.
- Move superseded files out of scope instead of deleting them.

## Merge Behavior
- Prefer non-squash behavior when validating branch mirroring/deletion semantics to avoid issues caused by squash merges that remove change history. For this repository, squash merge was the cause of the branch cleanup and promotion issue; use basic merge for this validation/promotion flow.
- After cleanup is complete, disable squash merge behavior for these promotion/cleanup PRs to preserve branch history and expected deletion semantics.

## Branch Repair Plan
- First clean main to the intended final pipeline state, then recreate dev from main, then test the implementation again without squash merges.
