# Repository-Local Deviations — Azure DevOps Pipeline Standards

**Overrides:** [`global-azure-devops-pipeline-standards.md`](../../GlobalStandards/docs/standards/global-azure-devops-pipeline-standards.md)
**Repository:** HttpClientManager
**Owner:** Troy Crowe

This file documents every approved deviation from the global Azure DevOps pipeline standards
that applies to this repository. Each deviation must include all fields required by the
exception process defined in
[`global-governance-standards.md` Section 7](../../GlobalStandards/docs/standards/global-governance-standards.md#7-exception-process).

---

## DEV-001 — Azure Artifacts publish pipeline on merge to main

| Field | Value |
|---|---|
| **Rule** | `global-azure-devops-pipeline-standards.md` Step 7 states that `[Library]` repositories skip the release pipeline entirely. `global-nuget-library-standards.md` Section 8.1 states that class libraries do not require a production rollout pipeline. |
| **Deviation** | This repository uses a single authoritative dev pipeline definition, `HttpClientManager-Dev` (`.azure-pipelines/workflows/approve.yml`), for both approval-triggered Feature→Dev validation and post-merge `dev` continuity work. In PR context it performs ChangeDetection, conditional Build, conditional Test, pipeline-owned merge completion, and failure handling. On the subsequent `dev` branch CI run after merge it uses the same variable-gated stages to conditionally republish the durable promoted package artifact and create the next `dev → main` PR. `HttpClientManager - Main Release` (`.azure-pipelines/workflows/publish.yml`) triggers on CI push to `main`, performs change detection, explicitly evaluates promoted artifact availability, and delivers the promoted NuGet package to the `CaptiveInnovations` Azure Artifacts internal feed only when the merged change set contains code-significant changes. |
| **Reason** | The library must be installable by consuming repositories through standard `<PackageReference>` without requiring manual feed upload, and the repository needs a single dev-path pipeline where variables determine whether the run is validating a PR, handling a post-merge `dev` update, or skipping code-specific work. Using one authoritative dev pipeline preserves the approved variable-gated design while still producing a durable artifact source on `dev` that Main Release can download by branch. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether GlobalStandards has introduced a canonical library publish pipeline template that can absorb this deviation. |

---

## DEV-002 — Approval-gated PR validation remains a single registered pipeline

| Field | Value |
|---|---|
| **Rule** | `global-azure-devops-pipeline-standards.md` Step 6 separates PR validation from downstream promotion automation and expects Azure DevOps branch policies to control PR completion after validation succeeds. |
| **Deviation** | This repository uses a single registered dev pipeline (`HttpClientManager-Dev`, `.azure-pipelines/workflows/approve.yml`) for the Feature→Dev path and the subsequent `dev` continuity path. PR creation does nothing; human approval enables the required validation path, and the same pipeline definition later runs on `dev` branch updates to handle carry-forward work. The flow is variable-gated through `CodeChanges`, `BuildSuccess`, and `TestSuccess` so Build, Test, Merge, and CarryForward only run when their prerequisites are satisfied; CarryForward itself is deferred to the real `dev` branch run after merge rather than the PR validation run. |
| **Reason** | For a small single-library repository with a straightforward `feature/* → dev → main` chain, keeping the entire dev path in one variable-gated pipeline preserves traceability and minimizes operational overhead. The pipeline-owned merge keeps approval as the single decision point for Feature→Dev, while the same pipeline definition can process the merged `dev` branch state to publish durable artifacts and create the next `dev → main` PR without requiring a second dev-specific pipeline definition. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether repository complexity has grown to the point where separating these back into two pipelines improves clarity. |

### Azure DevOps live alignment requirements

- Register `HttpClientManager-Dev` against `.azure-pipelines/workflows/approve.yml` as the single authoritative dev pipeline for approval-triggered Feature→Dev validation and `dev` branch continuity runs.
- Disable or unregister `HttpClientManager - Dev Promotion` from `.azure-pipelines/workflows/carry-forward.yml`; it remains only as a compatibility placeholder and should not be used for active flow execution.
- Register `HttpClientManager - Main Release` against `.azure-pipelines/workflows/publish.yml` as the CI pipeline on `main` that downloads and publishes the promoted package.
- Keep `.azure-pipelines/variables/common.yml` `devPromotionPipelineId` aligned with the actual definition id of `HttpClientManager-Dev` so Main Release can download the correct durable artifact source from `dev` branch runs.
- Enable PR auto-complete by default, grant `HttpClientManager Build Service (TCrowe0170)` the `Bypass policies when completing pull requests` permission, and use **CI-DevOps-UMI** for the related Azure/connection configuration.
