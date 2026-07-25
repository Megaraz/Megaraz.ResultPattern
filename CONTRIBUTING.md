# Contributing to Megaraz.ResultPattern

Thank you for your interest in contributing. Megaraz.ResultPattern is a focused
.NET library for representing successful operations, failures, and validation
errors.

## Development setup

Install the .NET 10 SDK, then clone the repository and run the test suite:

```bash
git clone https://github.com/Megaraz/Megaraz.ResultPattern.git
cd Megaraz.ResultPattern
dotnet test Megaraz.ResultPattern.slnx --configuration Release
```

The solution targets .NET 8, .NET 9, and .NET 10. Tests run for each target
framework. The same restore, build, and test steps are also run by GitHub
Actions for pushes and pull requests targeting `main`.

## Making changes

- Keep changes focused on the behavior being addressed.
- Preserve nullable reference type safety and existing public API behavior
  unless the change intentionally updates that contract.
- Add or update tests for new or changed public behavior.
- Keep test names focused on observable behavior rather than implementation
  details.
- Update the README or relevant documentation when public behavior changes.

Before opening a pull request, run:

```bash
dotnet test Megaraz.ResultPattern.slnx --configuration Release
```

## Issues and pull requests

For bugs, include the expected behavior, actual behavior, relevant code, and a
minimal reproduction when possible. For feature requests, explain the use case
and proposed public API rather than only the implementation.

Pull requests should describe:

- What changed and why.
- How the change was tested.
- Any compatibility or public API considerations.

Keep pull requests small and focused. All CI checks should pass before merging.

## License

By contributing, you agree that your contributions will be licensed under the
MIT License included in this repository.
