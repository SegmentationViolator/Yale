{
    inputs = {
        nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
        systems.url = "github:nix-systems/default";
    };

    outputs =
        {
            self,
            nixpkgs,
            systems,
            ...
        }:
            let
                aliases = eachSystem (system: {
                    pkgs = import nixpkgs {
                        inherit system;
                    };
                });

                eachSystem = nixpkgs.lib.genAttrs (import systems);
            in
            {
                devShell = eachSystem (system: aliases.${system}.pkgs.mkShell {
                    packages = with aliases.${system}.pkgs; [ dotnet-sdk_10 ];
                });
            };
}
