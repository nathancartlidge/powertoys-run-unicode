$ErrorActionPreference = "Stop";

echo "Building...";

dotnet restore;
dotnet test;
dotnet build --configuration RELEASE /p:Platform=x64 /p:EnableWindowsTargeting=true;

if (Test-Path "~\AppData\Local\Microsoft\PowerToys\PowerToys Run") {
    echo "Testing if PowerToys is running...";
    $pt = Get-Process "PowerToys" -ea SilentlyContinue;
    if ($pt) {
        echo "PowerToys is running, killing it";
        sudo pwsh -cwa "Stop-Process -Name PowerToys";
        sleep 2;
    }

    echo "Installing plugin...";
    if (Test-Path "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput") {
        echo "Deleting existing files";
        rm -Recurse "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput";
    }
    mkdir "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput";
    
    cp -Recurse "Community.PowerToys.Run.Plugin.UnicodeInput\bin\x64\Release\Community.*" "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput";
    cp -Recurse "Community.PowerToys.Run.Plugin.UnicodeInput\bin\x64\Release\images"      "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput";
    cp          "Community.PowerToys.Run.Plugin.UnicodeInput\bin\x64\Release\plugin.json" "~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\UnicodeInput";
    
    echo "Install Complete, launching PowerToys";
    ii "~\AppData\Local\PowerToys\PowerToys.exe";
} else {
    echo "Unable to find PowerToys installation - you will need to modify this script";
}
