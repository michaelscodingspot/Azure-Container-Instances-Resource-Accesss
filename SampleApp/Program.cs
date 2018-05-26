using System;
using System.Linq;
using System.Threading.Tasks;
using AciResourceAccess;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var configuration = ConfigurationFactory.CreateConfigWithActiveDirectoryAppAuth(
                azureSubscriptionId: "a187256a-2ebe-4a23-8bfc-8e194d8eagh7",
                resourceGroup: "MyResourceGroup",
                clientId: "n21d9e2f-1f1c-45cf-q7r7-79c363e5c740",
                clientSecret: "zaTD123lof6JUiiMTUb+bGGldmA8NpIvTEht1w7rylA=",
                tenantId: "32dd593d-c0bb-48e8-8cd1-8521ab9e3b5e",
                imageRegistryServer: "mycontainerregistry.azurecr.io",
                imageRegistryUsername: "RegistryUserName",
                imageRegistryPassword: "/bf54vMBFEjOXQgTh/PzTwWj9fhcydn3"
            );

            var resourceAccess = new AciResourceAccess.AciResourceAccess(configuration);
            var containerCreationConfig = new ContainerCreationConfiguration()
            {
                ContainerName = "my-container",//For some reason, container name can't be in Pascal case. Kebab case works.
                CpuCore = 2,
                MemoryInGB = 4,
                ImageName = "mycontainerregistry.azurecr.io/myimage",
                Port = 12345,
                Location = "east us",
                OS = ContainerCreationConfiguration.OsType.Windows
            };

            try
            {
                var container = resourceAccess.CreateContainer(containerCreationConfig).GetAwaiter().GetResult();
                Console.WriteLine("Container created successfully on ip " + container.properties.ipAddress.ip);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating container: {e}");
            }

            Console.ReadLine();




        }
    }
}
