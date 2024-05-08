
$gpus = Get-WmiObject Win32_VideoController | Select-Object -Property Name,__PATH | Where-Object { $_.Name -match '.*NVIDIA.*' }
if ($null -ne $gpus)
{
    if ($gpus[0] -match '"VideoController([0-9]+)')
    {
        Write-Host "NVIDIA Card $($gpus[0].Name) is in position $($Matches[1])" -ForegroundColor Green;
    }
}
else 
{
    Write-Host "No NVIDIA cards located." -ForegroundColor Magenta;
}