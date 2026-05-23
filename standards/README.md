# Repository-Local Standards

This folder contains approved deviations from the global governance baseline for this repository.

- **Global baseline:** [Global Governance Standards](../../GlobalStandards/docs/standards/global-governance-standards.md)

## Usage

Each deviation file in this folder must:

1. Be named after the global standards file it overrides, with the `global-` prefix removed.
2. State which specific rule it deviates from.
3. Include all fields required by the exception process in `global-governance-standards.md` Section 7.
4. Be kept to the minimum scope needed — prefer conforming to global standards wherever possible.

## Active Deviation Files

| File | Overrides | Deviations |
|---|---|---|
| [`azure-devops-pipeline-standards.md`](azure-devops-pipeline-standards.md) | `global-azure-devops-pipeline-standards.md` | DEV-001 Azure Artifacts publish pipeline; DEV-002 combined PR gate and carry-forward pipeline |
