rm -rf BuildOutput &&
mkdir BuildOutput &&
mkdir BuildOutput/BepInEx/ &&
mkdir BuildOutput/BepInEx/plugins/ &&
mkdir BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/ &&
mkdir BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/MoreCompanyCosmetics/ &&
cp -f "$CURRENT_PROJECT"/bin/Debug/netstandard2.1/TestAccount666."$CURRENT_PROJECT".dll BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/"$CURRENT_PROJECT".dll &&
cp -f "$CURRENT_PROJECT"/README.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/CHANGELOG.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/icon.png BuildOutput/ &&
cp -f LICENSE BuildOutput/ &&
cp -f Assets/TestAccountVariety BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/ &&
cp -f TestAccountVariety1.cosmetics BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/MoreCompanyCosmetics/ &&
cp -f TestAccountVariety2.cosmetics BuildOutput/BepInEx/plugins/TestAccount666-"$CURRENT_PROJECT"/MoreCompanyCosmetics/ &&
./generate_manifest.sh &&
./generate_zipfile.sh &&
dolphin "./BuildOutput"
