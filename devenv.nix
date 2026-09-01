{
  pkgs,
  ...
}:
{
  packages = [
    pkgs.fsharp
  ];

  languages.dotnet = {
    enable = true;
    package = pkgs.dotnet-sdk_10;
  };
  scripts = {
    test-unit.exec = ''
      dotnet run --project tests/InstantProjectInitializer.UnitTests
    '';
    test-integration.exec = ''
      dotnet run --project tests/InstantProjectInitializer.IntegrationTests
    '';
    test-all.exec = ''
      test-unit
      test-integration
    '';
    update-deps.exec = ''
      tmp=$(mktemp -d)
      trap 'rm -rf "$tmp"' EXIT
      NUGET_PACKAGES="$tmp" dotnet restore src/InstantProjectInitializer/InstantProjectInitializer.fsproj
      NUGET_PACKAGES="$tmp" dotnet tool restore
      cp -r ~/.nuget/packages/microsoft.playwright.cli "$tmp/"
      nix run nixpkgs#nuget-to-json -- "$tmp" 2>/dev/null > deps.json
      echo "deps.json updated"
    '';
  };
}

