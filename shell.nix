{ pkgs ? import <nixpkgs> {} }:
pkgs.mkShell {
  shellHook = ''
    export LD_LIBRARY_PATH="${pkgs.lib.makeLibraryPath [ pkgs.fontconfig pkgs.freetype pkgs.libGL pkgs.libxkbcommon pkgs.xorg.libX11 pkgs.xorg.libICE pkgs.xorg.libSM pkgs.xorg.libXcursor pkgs.xorg.libXext pkgs.xorg.libXi pkgs.xorg.libXrandr ]}:$LD_LIBRARY_PATH"
  '';
}
