# Contributing to SoundHop

Thank you for showing your interest in contributing to **SoundHop**!

You can contribute to **SoundHop** by filing issues (which includes bug reports and feature requests) and making pull requests (including code and docs). Simply filing issues for the problems you encounter is a great way to contribute. Contributing code via PRs is greatly appreciated!

Below is our guidance for how to file bug reports, propose new features, and submit contributions via Pull Requests (PRs).

## Before you start, file an issue

We use GitHub issues to track bugs and features. It helps us to prioritize tasks and make our plan.

Please follow this simple rule to help us eliminate any unnecessary wasted effort & frustration, and ensure an efficient and effective use of everyone's time - yours, ours, and other community members':

> If you have a question, think you've discovered an issue, would like to propose a new feature, etc., then find/file an issue **BEFORE** starting work to fix/implement it.

### Search existing issues first

Before filing a new issue, search existing open and closed issues first: It is likely someone else has found the problem you're seeing, and someone may be working on or have already contributed a fix!

If no existing item describes your issue/feature, great - please file a new issue:

### File a new Issue

- Experienced a bug or crash? [File a bug report](https://github.com/SoundHop/SoundHop/issues/new?labels=bug&template=bug_report.md&title=Bug%3A)
- Got a great idea for a new feature or have a suggestion? [File a feature request](https://github.com/SoundHop/SoundHop/issues/new?labels=enhancement&template=feature_request.md&title=Feature+Request%3A)
- Don't know whether you're reporting a bug or requesting a feature? [File an issue](https://github.com/SoundHop/SoundHop/issues/new)
- Found an existing issue that describes yours? Great - upvote and add additional commentary / info / repro-steps / etc.

## Code Contribution Guidelines

Before contributing any code via PRs to **SoundHop**, please **file a new issue** regarding it and ask for approval.
If you want to implement or fix any existing issues, please leave a comment on the issue notifying us about your intention to contribute code.

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/SoundHop/SoundHop.git
   ```

2. Open the solution in Visual Studio 2022 or later

3. Build and run:
   ```bash
   dotnet build
   dotnet run --project SoundHop.UI
   ```

### Pull Request Process

1. Fork the repository and create your branch from `main`
2. Make your changes
3. Ensure the build passes with no errors
4. Update documentation if needed
5. Submit a pull request with a clear description of your changes

### Copying files from other projects

The following rules must be followed for PRs that include files from another project:

- The license of the file is [permissive](https://en.wikipedia.org/wiki/Permissive_software_license)
- The license of the file is left intact
- The contribution is correctly attributed in the README or relevant documentation

## Questions?

If you have questions that aren't answered here, feel free to open a discussion or issue on our [GitHub repository](https://github.com/SoundHop/SoundHop).

Thank you for helping make SoundHop better! 🎧
