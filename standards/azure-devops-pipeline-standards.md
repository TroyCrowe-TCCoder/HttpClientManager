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
| **Deviation** | This repository adds a second registered pipeline (`HttpClientManager - Main Release`, `.azure-pipelines/workflows/main-release.yml`) that triggers on CI push to `main` and publishes the NuGet package to the `CaptiveInnovations` Azure Artifacts internal feed. |
| **Reason** | The library must be installable by consuming repositories through standard `<PackageReference>` without requiring manual feed upload. An automated push to the internal feed on merge to `main` is the minimum automation required to satisfy the package distribution rule in `global-nuget-library-standards.md` Section 8.3, which mandates that internal shared library packages are published to the organization's Azure Artifacts feed. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether GlobalStandards has introduced a canonical library publish pipeline template that can absorb this deviation. |

---

## DEV-002 — PR gate and post-merge carry-forward combined in one registered pipeline

| Field | Value |
|---|---|
| **Rule** | `global-azure-devops-pipeline-standards.md` Step 6 instructs that the dev pipeline file owns one responsibility (the PR gate), and Section 3.3 instructs that the carry-forward automation is driven by a separate pipeline that triggers on CI push to `dev`. The canonical implementation keeps these as two registered pipelines. |
| **Deviation** | This repository combines both behaviors into a single registered pipeline (`HttpClientManager-Dev`, `.azure-pipelines/workflows/httpclientmanager-dev.yml`). The file carries both a `pr:` trigger and a `trigger:` (CI push). Condition guards ensure that the build and test stages run only during a PR (`Build.Reason == 'PullRequest'`) and that the carry-forward stage runs only on a post-merge CI push (`Build.Reason != 'PullRequest'`). |
| **Reason** | For a small single-library repository with a straightforward `feature/* → dev → main` chain, maintaining two separately registered pipelines for the dev path adds operational overhead (two pipeline registrations, two pipeline-policy associations, two places to check run history) without providing meaningful isolation benefit. The condition guards preserve the behavioral separation that the global standard intends. The carry-forward template (`auto-pr-chain.yml`) remains unchanged and reusable. |
| **Requester** | Troy Crowe |
| **Approver** | Troy Crowe |
| **Effective date** | 2026-07-23 |
| **Planned review date** | 2027-01-01 — review whether repository complexity has grown to the point where separating these back into two pipelines improves clarity. |
