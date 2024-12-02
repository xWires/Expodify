mkdir bin/app/Expodify.app
mkdir bin/app/Expodify.app/Contents
mkdir bin/app/Expodify.app/Contents/MacOS
mkdir bin/app/Expodify.app/Contents/Resources

cat > bin/app/Expodify.app/Contents/Info.plist << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple Computer//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleGetInfoString</key>
  <string>Expodify</string>
  <key>CFBundleExecutable</key>
  <string>Expodify</string>
  <key>CFBundleIdentifier</key>
  <string>xyz.tangledwires.Expodify</string>
  <key>CFBundleName</key>
  <string>Expodify</string>
  <key>CFBundleIconFile</key>
  <string>icon.icns</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>IFMajorVersion</key>
  <integer>0</integer>
  <key>IFMinorVersion</key>
  <integer>1</integer>
</dict>
</plist>
EOF

cp -r bin/osx-arm64/* bin/app/Expodify.app/Contents/MacOS

# To sign, use this command, replacing "TangledWires" with the name of your own certificate
#
# codesign --deep -fs TangledWires Expodify.app
