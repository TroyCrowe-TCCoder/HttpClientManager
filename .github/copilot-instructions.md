# Repository Addendum

- **Global baseline:** [`docs/standards/global-governance-standards.md`](../docs/standards/global-governance-standards.md)

## Repository Initialization Rules
- This repository follows the class-library non-deployed-products model.
- Keep repository-local deviations only in the root `standards/` folder when approved for this repository.
- Add and maintain a repository-local validation entry point at `scripts/validate.ps1`.
- The `feature/* -> dev` path must create the traceable package output promoted later through the `dev -> main` flow.
