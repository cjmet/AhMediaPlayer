
Get-childItem "." | Rename-Item -NewName {$_.Basename.tostring().tolower() + $_.extension}