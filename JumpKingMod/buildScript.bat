:: For some reason Release versions flag as viruses on some machines, we don't need the perf
:: so we'll just build in debug =)

"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe" JumpKingRavensMod.sln /build Debug

mkdir "Jump King Mod Release"
cd "Jump King Mod Release"
mkdir "Installer"
mkdir "Mod Files"
cd ..

robocopy "JumpKingMod.Install.UI/bin/Debug/" "Jump King Mod Release/Installer" /COPYALL /E
robocopy "JumpKingModifiersMod/bin/Debug/" "Jump King Mod Release/Mod Files" /COPYALL /E
robocopy "JumpKingRavensMod/bin/Debug/" "Jump King Mod Release/Mod Files" /COPYALL /E
robocopy "JumpKingPunishmentMod/bin/Debug/" "Jump King Mod Release/Mod Files" /COPYALL /E
robocopy "JumpKingModLoader/bin/Debug/" "Jump King Mod Release/Mod Files" /COPYALL /E
