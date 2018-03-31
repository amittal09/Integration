using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dummy Check in
            var application = ApplicationContext.Create(_application => _application.
            Database(database => database.IntegrationDb(ConnectionString.FromName("default"))));
            
        }
    }
}
