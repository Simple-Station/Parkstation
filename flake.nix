{
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs = { self, nixpkgs, ... }: let
    pkgs = nixpkgs.legacyPackages."x86_64-linux";
  in {
    devShells.x86_64-linux.default = with pkgs; stdenv.mkDerivation rec {
      name = "dev-env";

      nativeBuildInputs = [
        dotnet-sdk_7
        omnisharp-roslyn
        python3
      ];

      buildInputs = [
        freetype
        glfw
        libglvnd
        openal
        fluidsynth
      ];

      LD_LIBRARY_PATH = lib.makeLibraryPath buildInputs;
    };
  };
}
