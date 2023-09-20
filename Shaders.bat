@echo off
rem ** Shader packager
rem The tool uses "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\fxc.exe"
rem Arguments: <include path (use "." if none)> <source file> <destination file without extension> <shader model> <output type: h (c++) or cs (c#)> {<shader type: vs, hs, ds, gs, ps or cs> <namespace of entry; "::main" will be added>}

set EFPAQ="E:\Wew\Demo\bin\x64\Release\net6.0-windows8.0\EfPaq.exe"

%EFPAQ% ..\Media Shaders2D.hlsl Shaders2D 5_0 c# vs VS2DVertex ps PS2DTile
%EFPAQ% ..\Media Shaders3D.hlsl Shaders3D 5_0 c# vs VSInstance ps PSFire vs Sky::VS ps Sky::PS vs Terrain::VS hs Terrain::HS ds Terrain::DS ps Terrain::PS vs Reflect::VS ps Reflect::PS gs Rain::GSGenerate vs Rain::VS gs Rain::GS ps Rain::PS cs CSBitmap cs CSCalculation