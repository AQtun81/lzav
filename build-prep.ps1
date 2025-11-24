$functionsToExport = @(
    'lzav_compress_bound',
    'lzav_compress_default',
    'lzav_compress_bound_hi',
    'lzav_compress_hi',
    'lzav_decompress'
)
$pattern = '\b(' + ($functionsToExport -join '|') + ')\b'

New-Item -Force -ItemType directory -Path temp | Out-Null

(Get-Content -Path "lzav_macro.h") | Out-File -FilePath "temp/lzav.h" -Encoding UTF8
foreach ($line in Get-Content -Path "lzav_src/lzav.h") {
    if ($line -match $pattern) {
        $line = $line -replace '\bLZAV_INLINE_F\b',  'LZAV_EXPORT'
        $line = $line -replace '\bLZAV_INLINE\b',    'LZAV_EXPORT'
        $line = $line -replace '\bLZAV_NO_INLINE\b', 'LZAV_EXPORT'
    }
    $line | Out-File -FilePath "temp/lzav.h" -Encoding UTF8 -Append
}

("#include `"lzav.h`"") | Out-File -FilePath "temp/lzav.c" -Encoding UTF8