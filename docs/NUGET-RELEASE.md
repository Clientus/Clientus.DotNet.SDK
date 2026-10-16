# NuGet release process

## Current candidate version

`1.0.0-beta.6`

Packages:

- `Clientus.ApiClient`
- `Clientus.Core`

HOME.LONG.3B prepares and certifies this package line. It does **not** authorize publication to NuGet.org and does not create a release tag.

## Release gates

Before any public NuGet publication:

1. canonical `main` must be clean and aligned with `origin/main`;
2. Release build must pass;
3. full test suite must pass;
4. both packages must pack successfully;
5. package contents must be inspectable and contain no credentials, service-role keys, local paths or temporary artifacts;
6. a clean consumer project must install `Clientus.ApiClient` and `Clientus.Core` from the generated local feed;
7. the consumer must compile and run against the public package surface without project references;
8. publication/tag creation requires a separate explicit release approval.

## GitHub Actions

`.github/workflows/build.yml` validates pull requests and `main` by restoring, building, testing, packing both packages and performing a local-feed clean-install smoke test.

The same workflow has a tag-only publish job. **Do not create `v1.0.0-beta.6` until the explicit NuGet.org release gate is approved.** Tag creation would activate the publication path.

NuGet authentication uses GitHub OIDC through `NuGet/login`; no NuGet API key is committed to source control.

## Clean-install certification

CI creates a fresh .NET 8 console consumer, installs `Clientus.ApiClient 1.0.0-beta.6` from the generated package directory, restores, builds and runs it.

The smoke consumer demonstrates package restore, SDK construction with `DeveloperCredential`, sandbox configuration, availability of `client.Home`, and execution without project references or live credentials.

No public NuGet publication is performed by this certification.
