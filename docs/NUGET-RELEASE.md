# NuGet release process

## Current package version

`1.0.0-beta.3`

Packages:

- `Clientus.ApiClient`
- `Clientus.Core`

## Release gates

Before any public NuGet publication:

1. canonical `main` must be clean and aligned with `origin/main`;
2. Release build must pass;
3. full test suite must pass;
4. both packages must pack successfully;
5. package contents must be inspected;
6. clean-machine installation test must pass;
7. no credentials, secrets, service-role keys, local paths or temporary artifacts may be included.

## GitHub Actions

`.github/workflows/nuget-release.yml` supports:

- manual validation through `workflow_dispatch`;
- package publication only when `confirm_publish=PUBLISH`;
- publication on version tags such as `v1.0.0-beta.3`;
- `NUGET_API_KEY` stored only as a GitHub Actions environment secret.

The workflow must never contain the API key in source control.

## Initial private-beta release

Recommended tag:

`v1.0.0-beta.3`

Before creating the tag, verify that the package has completed local clean-machine certification.

## Clean-machine certification

The release package must be copied to a temporary empty directory, installed from the local package source, restored, built and executed without using project references back to the SDK repository.

The clean-machine sample must demonstrate:

- package restore;
- SDK construction with `DeveloperCredential`;
- sandbox environment configuration;
- compilation against the public package surface.

No live credential is required for the offline certification.