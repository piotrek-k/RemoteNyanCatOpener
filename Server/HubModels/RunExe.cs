using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.HubModels
{
    public class RunExe
    {
        public string UserName { get; set; }
        public string Path { get; set; }
        public bool ThenCloseLauncher { get; set; }
        public string Arguments { get; set; }
        public bool HideOpenedFile { get; set; }
    }
}