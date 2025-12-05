# Codecov Setup Guide

To enable code coverage reporting with Codecov, follow these steps:

## 1. Sign up for Codecov

1. Go to [codecov.io](https://codecov.io)
2. Sign in with your GitHub account
3. Authorize Codecov to access your repositories

## 2. Add Repository

1. Find `Kydoimos97/FileCombiner` in your repository list
2. Click "Setup repo" or enable it

## 3. Get Upload Token

1. Go to repository settings in Codecov
2. Copy the upload token (it will look like: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)

## 4. Add Token to GitHub Secrets

1. Go to your GitHub repository: `https://github.com/Kydoimos97/FileCombiner`
2. Click on **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `CODECOV_TOKEN`
5. Value: Paste the token from Codecov
6. Click **Add secret**

## 5. Verify Setup

Once the token is added:
1. Push a commit or create a PR
2. The CI workflow will run automatically
3. Coverage reports will be uploaded to Codecov
4. The coverage badge in README.md will update

## Optional: Configure Codecov

Create a `codecov.yml` file in the repository root to customize coverage settings:

```yaml
coverage:
  status:
    project:
      default:
        target: 80%
        threshold: 1%
    patch:
      default:
        target: 80%

comment:
  layout: "reach,diff,flags,tree"
  behavior: default
  require_changes: false
```

## Troubleshooting

- **Badge shows "unknown"**: Token not set or first workflow hasn't completed yet
- **Coverage not uploading**: Check GitHub Actions logs for errors
- **Token invalid**: Regenerate token in Codecov and update GitHub secret

## Resources

- [Codecov Documentation](https://docs.codecov.com/)
- [GitHub Actions Integration](https://docs.codecov.com/docs/github-actions-integration)
