{
  description = "Minecraft Console Client development environment";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs =
    { nixpkgs, ... }:
    let
      forAllSystems = nixpkgs.lib.genAttrs [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
    in
    {
      devShells = forAllSystems (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
          dotnet = pkgs.dotnetCorePackages.sdk_10_0;
        in
        {
          default = pkgs.mkShell {
            packages = [
              dotnet
              pkgs.jdk25
              pkgs.python3
              pkgs.tmux
              pkgs.nodejs_22
              pkgs.curl
              pkgs.unzip
            ];
            DOTNET_ROOT = "${dotnet}/share/dotnet";
            # Keep mcc-debug session files stable across nix develop invocations
            shellHook = ''
              export TMPDIR=/tmp
            '';
          };
        }
      );

      formatter = forAllSystems (system: nixpkgs.legacyPackages.${system}.nixfmt);
    };
}
