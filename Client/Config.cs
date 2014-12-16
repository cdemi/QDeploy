using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Config
    {
        public List<Deployment> Deployments { get; set; }
        public List<string> Exclusions { get; set; } 
    }

    public class Deployment
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string Path { get; set; }
        public List<string> Exclusions { get; set; }
    }
}
