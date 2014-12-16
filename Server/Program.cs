using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic;
using System.ServiceModel;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(Deployer)))
            {
                host.Open();
                Console.WriteLine("Service host running......");

                Console.ReadLine();

                host.Close();
            }
        }
    }
}
