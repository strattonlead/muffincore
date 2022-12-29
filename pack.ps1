#!/usr/bin/env pwsh

$array = New-Object System.Collections.ArrayList
$array.Add("Muffin.AspNetCore.Authentication")
$array.Add("Muffin.AspNetCore.Authorization.Permissions")
$array.Add("Muffin.AspNetCore.Extensions")
$array.Add("Muffin.AspNetCore.Ngrok")
$array.Add("Muffin.BackgroundServices")
$array.Add("Muffin.Common")
$array.Add("Muffin.ComponentModel.DataAnnotations")
$array.Add("Muffin.Deepl")
$array.Add("Muffin.Deepl.Abstraction")
$array.Add("Muffin.Drawing")
$array.Add("Muffin.EntityFrameworkCore")
$array.Add("Muffin.EntityFrameworkCore.Abstraction")
$array.Add("Muffin.EntityFrameworkCore.AuditTrail")
$array.Add("Muffin.EntityFrameworkCore.Centron")
$array.Add("Muffin.EntityFrameworkCore.DataProtection")
$array.Add("Muffin.EntityFrameworkCore.Entity")
$array.Add("Muffin.EntityFrameworkCore.Entity.Abstraction")
$array.Add("Muffin.EntityFrameworkCore.Globalization")

$array.Add("Muffin.EntityFrameworkCore.Mail")
$array.Add("Muffin.EntityFrameworkCore.Mail.Render")
$array.Add("Muffin.EntityFrameworkCore.Mail.Services")

$array.Add("Muffin.EntityFrameworkCore.Tenancy")
$array.Add("Muffin.EntityFrameworkCore.Tenancy.Abstraction")

$array.Add("Muffin.Mail.Abstraction")
$array.Add("Muffin.Services")
$array.Add("Muffin.Services.Abstraction")

$array.Add("Muffin.StateManagement")
$array.Add("Muffin.StateManagement.Models")
$array.Add("Muffin.Tenancy.Abstraction")
$array.Add("Muffin.Hetzner.Robot.Api")

$array | ForEach-Object {
Write-Output "Clear $_ ..."
$project = $_

dotnet clean $project 

Write-Output "Cleared $_ ..."
}

$array | ForEach-Object {
Write-Output "Compile $_ ..."
$project = $_
$namespace = "CreateIf." + $project
$currentDirectoryPath = pwd
$file = Get-ChildItem -Path $currentDirectoryPath/$project/bin/Release -File
$filename = $file.Name

$version = $file.Name -replace "\.(\d+\.\d+\.\d+)\.nupkg", '$1'
$version = $version -replace $namespace

dotnet pack $project --configuration Release

Write-Output "Compiled $_ ..."
}

$array | ForEach-Object {
Write-Output "Pack $_ ..."
$project = $_
$namespace = "CreateIf." + $project
$currentDirectoryPath = pwd
$file = Get-ChildItem -Path $currentDirectoryPath/$project/bin/Release -File
$filename = $file.Name

$version = $file.Name -replace "\.(\d+\.\d+\.\d+)\.nupkg", '$1'
$version = $version -replace $namespace

$nugetFilePath = Join-Path -Path $pwd -ChildPath "$project/bin/Release/$filename"
Write-Output $nugetFilePath

dotnet nuget push $nugetFilePath --api-key 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 --source "createif-labs" --skip-duplicate
Write-Output "Packed $_ ..."
}