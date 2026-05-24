# Repository Addendum

- **Global baseline:** [Global Governance Standards](../../GlobalStandards/docs/standards/global-governance-standards.md)

## Repository Initialization Rules
- This repository follows the class-library non-deployed-products model.
- Keep repository-local deviations only in the root `standards/` folder when approved for this repository.
- Add and maintain a repository-local validation entry point at `scripts/validate.ps1`.
- The `feature/* -> dev` path must create the traceable package output promoted later through the `dev -> main` flow.
- Handle the feature->dev commit, push, PR creation, and select auto-merge when preparing the PR.

## Azure DevOps Flow Rules
- Build runs only when ChangeDetection is true.
- Test runs only when ChangeDetection is true and Build succeeded.
- Merge requires Test success when ChangeDetection is true but still runs when ChangeDetection is false.
- PR approval should start the Azure DevOps flow; ChangeDetection returns false for documentation-only changes and true for code changes.
- CarryForward creates a fresh dev->main PR and carries forward only the title and description.
- Complete the repository and Azure DevOps configuration end-to-end when Azure DevOps pipeline work is requested for this repository, rather than stopping at YAML-only changes.
