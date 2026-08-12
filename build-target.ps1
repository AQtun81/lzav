param($target)

New-Item -Force -ItemType directory -Path src/runtimes/$target/native
New-Item -Force -ItemType directory -Path src/static/$target

# Install compiler if not already present
switch ($target)
{
  "linux-arm"   { sudo apt-get install -y gcc-arm-linux-gnueabihf }
  "linux-arm64" {  }
  "linux-x64"   {  }
  "win-arm64"   {  }
  "win-x64"     {  }
  "win-x86"     {  }
  "osx-arm64"   {  }
  "osx-x64"     {  }
}

# Build
switch ($target)
{
  "linux-arm"
  {
    arm-linux-gnueabihf-gcc -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    arm-linux-gnueabihf-ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    arm-linux-gnueabihf-gcc -shared -o src/runtimes/$target/native/liblzav.so temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  "linux-arm64"
  {
    aarch64-linux-gnu-gcc -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    aarch64-linux-gnu-ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    aarch64-linux-gnu-gcc -shared -o src/runtimes/$target/native/liblzav.so temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  "linux-x64"
  {
    x86_64-linux-gnu-gcc -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    x86_64-linux-gnu-ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    x86_64-linux-gnu-gcc -shared -o src/runtimes/$target/native/liblzav.so temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  "osx-arm64"
  {
    clang -target arm64-apple-darwin -c temp/lzav.c -o src/static/$target/liblzav.o -O2 
    ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    clang -target arm64-apple-darwin -shared -o src/runtimes/$target/native/liblzav.dylib temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  "osx-x64"
  {
    clang -target x86_64-apple-darwin -c temp/lzav.c -o src/static/$target/liblzav.o -O2 
    ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    clang -target x86_64-apple-darwin -shared -o src/runtimes/$target/native/liblzav.dylib temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  {$_ -like "win-*"}
  {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    $vsPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    $vcVarsPath = "$vsPath\VC\Auxiliary\Build\vcvarsall.bat"
    $arch = $target -replace 'win-', ''
    
    $cmd =
      ("`"$vcVarsPath`" $arch && ") +
      ("cl /c /O2 temp\lzav.c /Fo:src\static\$target\liblzav.obj && ") +
      ("lib src\static\$target\liblzav.obj /OUT:src\static\$target\liblzav.lib && ") +
      ("cl /LD /O2 temp\lzav.c /link /OUT:src\runtimes\$target\native\liblzav.dll")

    cmd.exe /c $cmd
    Remove-Item "src\static\$target\liblzav.obj"
  }
}