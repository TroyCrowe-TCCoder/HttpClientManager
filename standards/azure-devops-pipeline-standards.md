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
| **Deviation** | This repository adds a second registered downstream pipeline (`HttpClientManager - Main Release`, `.azure-pipelines/workflows/publish.yml`) that triggers on CI push to `dev` and `main`. On `dev`, it performs change detection, conditional build/test, and CarryForward creation of the `dev → main` PR. On `main`, it performs change detection, conditional build/test, and publishes the NuGet package to the `CaptiveInnovations` Azure Artifacts internal feed from the current run artifact. |
| **Reason** | The library must be installable by consuming repositories through standard `<PackageReference>` without requiring manual feed upload, and the repository also needs a downstream promotion step after `dev` merges so the `dev → main` PR is created automatically. Using one downstream pipeline for both `dev` and `main` preserves the one-run-per-environment model while satisfying the internal package publication rule in `global-nuget-library-standards.md` Section 8.3. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether GlobalStandards has introduced a canonical library publish pipeline template that can absorb this deviation. |

---

## DEV-002 — Approval-gated PR validation remains a single registered pipeline

| Field | Value |
|---|---|
| **Rule** | `global-azure-devops-pipeline-standards.md` Step 6 separates PR validation from downstream promotion automation and expects Azure DevOps branch policies to control PR completion after validation succeeds. |
| **Deviation** | This repository uses a single approval-triggered registered validation pipeline (`HttpClientManager-Dev`, `.azure-pipelines/workflows/approve.yml`) for the Feature→Dev PR path. PR creation does nothing; human approval enables the required validation path, and the pipeline performs ChangeDetection, conditional Build, conditional Test, and fail-fast Failure handling while Azure DevOps branch policy and auto-complete own the merge completion. |
| **Reason** | For a small single-library repository with a straightforward `feature/* → dev → main` chain, keeping validation in a single approval-gated run preserves traceability and minimizes operational overhead without duplicating merge responsibility inside YAML. Downstream Dev→Main promotion can then be positioned against the post-merge dev-side flow. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether repository complexity has grown to the point where separating these back into two pipelines improves clarity. |
