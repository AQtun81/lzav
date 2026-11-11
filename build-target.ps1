param($target)

New-Item -Force -ItemType directory -Path src/runtimes/$target/native
New-Item -Force -ItemType directory -Path src/static/$target

# Compiler setup
switch ($target)
{
  "linux-arm"   { sudo apt-get install -y gcc-arm-linux-gnueabihf }
  "linux-arm64" {  }
  "linux-x64"   {  }
  "linux-x86"   { sudo apt-get install -y gcc-i686-linux-gnu }
  "win-arm64"   {  }
  "win-x64"     {  }
  "win-x86"     {  }
  "osx-arm64"   {  }
  "osx-x64"     {  }
  {$_ -like "android-*"}
  {
    if (-not $env:ANDROID_NDK_HOME)
    {
      $ndkDir = Get-ChildItem "/usr/local/lib/android/sdk/ndk/*" -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending | Select-Object -First 1
      if ($ndkDir) { $env:ANDROID_NDK_HOME = $ndkDir.FullName }
    }
  }
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
  "linux-x86"
  {
    i686-linux-gnu-gcc -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    i686-linux-gnu-ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    i686-linux-gnu-gcc -shared -o src/runtimes/$target/native/liblzav.so temp/lzav.c -O2
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
  {$_ -like "android-*"}
  {
    $hostTag = if ($IsLinux) { "linux-x86_64" } elseif ($IsMacOS) { "darwin-x86_64" } else { "windows-x86_64" }
    $cc = "$env:ANDROID_NDK_HOME/toolchains/llvm/prebuilt/$hostTag/bin/clang"
    $ar = "$env:ANDROID_NDK_HOME/toolchains/llvm/prebuilt/$hostTag/bin/llvm-ar"

    $androidTarget = switch ($target)
    {
      "android-arm"   { "armv7a-linux-androideabi21" }
      "android-arm64" { "aarch64-linux-android21" }
      "android-x86"   { "i686-linux-android21" }
      "android-x64"   { "x86_64-linux-android21" }
    }

    & $cc --target=$androidTarget -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    & $ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    & $cc --target=$androidTarget -shared -o src/runtimes/$target/native/liblzav.so temp/lzav.c -O2
    Remove-Item "src/static/$target/liblzav.o"
  }
  {$_ -like "ios*"}
  {
    $sdkName = if ($target -like "iossimulator-*") { "iphonesimulator" } else { "iphoneos" }

    $iosTarget = switch ($target)
    {
      "ios-arm64"          { "arm64-apple-ios" }
      "iossimulator-arm64" { "arm64-apple-ios-simulator" }
      "iossimulator-x64"   { "x86_64-apple-ios-simulator" }
    }

    & xcrun --sdk $sdkName clang -target $iosTarget -c temp/lzav.c -o src/static/$target/liblzav.o -O2 -fPIC
    & xcrun --sdk $sdkName ar rcs src/static/$target/liblzav.a src/static/$target/liblzav.o
    Remove-Item "src/static/$target/liblzav.o"
  }
}