using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
    public class Icarus : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Icarus", // WindowsGSM.XXXX
            author = "Addicted Games - Necrodemus",
            description = "Icarus Dedicated Server",
            version = "1.0",
            url = "https://github.com/Necrodemus/WindowsGSM_Plugins", // Github repository link (Best practice)
            color = "#ffffff" // Color Hex
        };


        // - Standard Constructor and properties
        public Icarus(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;


        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "2089300";


        // - Game server Fixed variables
        public override string StartPath => @"Icarus/Binaries/Win64/IcarusServer-Win64-Shipping.exe";
        public string FullName = "Icarus: First Cohort";
        public bool AllowsEmbedConsole = false;
        public int PortIncrements = 2;
        public object QueryMethod = new A2S();


        // - Game server default values
        public string Port = "17777";
        public string QueryPort = "27015";
        public string DefaultMap = "empty";
        public string Maxplayers = "8";
        public string Additional = " -UserDir=./SaveData";


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {

        }


        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string shipExePath = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }

            string param = string.IsNullOrWhiteSpace(_serverData.ServerName) ? string.Empty : $"-SteamServerName=\"{_serverData.ServerName}\"";
            param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" -MULTIHOME={_serverData.ServerIP}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -PORT={_serverData.ServerPort}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $"?QueryPort={_serverData.ServerQueryPort}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerMaxPlayer) ? string.Empty : $"?MaxPlayers={_serverData.ServerMaxPlayer}";

            Process p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false,
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath),
                    Arguments = param.ToString()
                },
                EnableRaisingEvents = true
            };
            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                base.Error = e.Message;
                return null; // return null if fail to start
            }
        }


        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                p.Kill();
            });
        }

        public async Task<Process> Install()
        {
            var steamCMD = new WindowsGSM.Installer.SteamCMD();
            Process p = await steamCMD.Install(_serverData.ServerID, string.Empty, AppId);
            Error = steamCMD.Error;

            return p;
        }

        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await WindowsGSM.Installer.SteamCMD.UpdateEx(_serverData.ServerID, AppId, validate, custom: custom);
            Error = error;
            return p;
        }

        public bool IsInstallValid()
        {
            return File.Exists(ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath));
        }

        public bool IsImportValid(string path)
        {
            string exePath = Path.Combine(path, "PackageInfo.bin");
            Error = $"Invalid Path! Fail to find {Path.GetFileName(exePath)}";
            return File.Exists(exePath);
        }

        public string GetLocalBuild()
        {
            var steamCMD = new WindowsGSM.Installer.SteamCMD();
            return steamCMD.GetLocalBuild(_serverData.ServerID, AppId);
        }

        public async Task<string> GetRemoteBuild()
        {
            var steamCMD = new WindowsGSM.Installer.SteamCMD();
            return await steamCMD.GetRemoteBuild(AppId);
        }
    }
}
