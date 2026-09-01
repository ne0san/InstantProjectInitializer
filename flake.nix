{
  description = "InstantProjectInitializer build output";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      systems = [ "x86_64-linux" "aarch64-linux" "aarch64-darwin" "x86_64-darwin" ];
      forAllSystems = nixpkgs.lib.genAttrs systems;
    in {
      packages = forAllSystems (system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          ipi = pkgs.buildDotnetModule {
            pname = "ipi";
            version = "0.1.0";
            src = ./.;
            projectFile = "src/InstantProjectInitializer/InstantProjectInitializer.fsproj";
            nugetDeps = ./deps.json;
            dotnet-sdk = pkgs.dotnet-sdk_10;
            dotnet-runtime = pkgs.dotnet-runtime_10;
          };
        in {
          inherit ipi;
          default = ipi;
        }
      );
    };
}
