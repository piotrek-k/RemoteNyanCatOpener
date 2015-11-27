using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.HubModels
{
    public class CreateFile
    {
        public string UserName { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
    }
}