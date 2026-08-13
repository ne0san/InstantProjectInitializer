{
  pkgs,
  ...
}:
{
  # F# has no dedicated devenv language option.
  packages = [
    pkgs.fsharp
  ];
  # https://devenv.sh/languages/
  languages = {
    dotnet.enable = true;
    dotnet.package = pkgs.dotnet-sdk_10;
  };
  # See full reference at https://devenv.sh/reference/options/
}

