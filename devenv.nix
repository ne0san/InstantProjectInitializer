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
  };
}

