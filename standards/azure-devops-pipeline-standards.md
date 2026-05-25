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
| **Deviation** | This repository uses two downstream registered pipelines. `HttpClientManager - Dev Promotion` (`.azure-pipelines/workflows/carry-forward.yml`) triggers on CI push to `dev`, performs change detection, conditional build/test, and republishes the durable promoted package artifact consumed later by Main Release. `HttpClientManager - Main Release` (`.azure-pipelines/workflows/publish.yml`) triggers on CI push to `main`, performs change detection, explicitly evaluates promoted artifact availability, and delivers the promoted NuGet package to the `CaptiveInnovations` Azure Artifacts internal feed only when the merged change set contains code-significant changes. |
| **Reason** | The library must be installable by consuming repositories through standard `<PackageReference>` without requiring manual feed upload, and the repository also needs a durable artifact source on `dev` that Main Release can download by branch. Splitting dev artifact promotion from main delivery keeps the Main Release pipeline limited to change detection plus artifact delivery while allowing no-code merges to bypass artifact work cleanly and still satisfying the internal package publication rule in `global-nuget-library-standards.md` Section 8.3. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether GlobalStandards has introduced a canonical library publish pipeline template that can absorb this deviation. |

---

## DEV-002 — Approval-gated PR validation remains a single registered pipeline

| Field | Value |
|---|---|
| **Rule** | `global-azure-devops-pipeline-standards.md` Step 6 separates PR validation from downstream promotion automation and expects Azure DevOps branch policies to control PR completion after validation succeeds. |
| **Deviation** | This repository uses a single approval-triggered registered validation pipeline (`HttpClientManager-Dev`, `.azure-pipelines/workflows/approve.yml`) for the Feature→Dev PR path. PR creation does nothing; human approval enables the required validation path, and the pipeline performs ChangeDetection, conditional Build, conditional Test, pipeline-owned merge completion, and post-merge creation of the next `dev → main` PR. |
| **Reason** | For a small single-library repository with a straightforward `feature/* → dev → main` chain, keeping validation in a single approval-gated run preserves traceability and minimizes operational overhead. The pipeline-owned merge keeps the approval gate as the single decision point, allows no-code PRs to merge without artifact work, and positions `dev → main` PR creation immediately after successful merge so the next promotion step is always created from the merged dev state while the downstream dev pipeline remains the durable artifact source. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether repository complexity has grown to the point where separating these back into two pipelines improves clarity. |

### Azure DevOps live alignment requirements

- Register `HttpClientManager-Dev` against `.azure-pipelines/workflows/approve.yml` as the approval-triggered Feature→Dev validation pipeline.
- Register `HttpClientManager - Dev Promotion` against `.azure-pipelines/workflows/carry-forward.yml` as the CI pipeline on `dev` that republishes the durable promoted artifact.
- Register `HttpClientManager - Main Release` against `.azure-pipelines/workflows/publish.yml` as the CI pipeline on `main` that downloads and publishes the promoted package.
- Keep `.azure-pipelines/variables/common.yml` `devPromotionPipelineId` aligned with the actual definition id of `HttpClientManager - Dev Promotion` so Main Release can download the correct artifact source.
- Enable PR auto-complete by default, grant `HttpClientManager Build Service (TCrowe0170)` the `Bypass policies when completing pull requests` permission, and use **CI-DevOps-UMI** for the related Azure/connection configuration.
