using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACI = AciResourceAccess;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RunSample().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task RunSample()
        {
            var azureSubscriptionConfiugration = ACI.ConfigurationFactory.CreateConfigWithActiveDirectoryAppAuth(
                azureSubscriptionId: "a187256a-2ebe-4a23-8bfc-8e194d8eagh7",
                resourceGroup: "MyResourceGroup",
                clientId: "n21d9e2f-1f1c-45cf-q7r7-79c363e5c740",
                clientSecret: "zaTD123lof6JUiiMTUb+bGGldmA8NpIvTEht1w7rylA=",
                tenantId: "32dd593d-c0bb-48e8-8cd1-8521ab9e3b5e",
                imageRegistryServer: "mycontainerregistry.azurecr.io",
                imageRegistryUsername: "RegistryUserName",
                imageRegistryPassword: "/bf54vMBFEjOXQgTh/PzTwWj9fhcydn3"
            );

            var containerCreationConfiguration = new ACI.ContainerCreationConfiguration()
            {
                //For some reason, container name can't be in Pascal case. Kebab case works.
                ContainerName = "my-container",
                CpuCore = 2,
                MemoryInGB = 4,
                ImageName = "mycontainerregistry.azurecr.io/myimage",
                Port = 12345,
                Location = "east us",
                OS = ACI.ContainerCreationConfiguration.OsType.Windows
            };

            try
            {
                var resourceAccess = new ACI.AciResourceAccess(azureSubscriptionConfiugration);
                var container = resourceAccess.CreateContainer(containerCreationConfiguration).GetAwaiter().GetResult();
                Console.WriteLine("Container created successfully on ip "
                                  + container.properties.ipAddress.ip);

                //Get container object
                Thread.Sleep(1000);
                container = await resourceAccess.GetContainer("my-container");
                Console.WriteLine("Container status now is: " + container.properties.containers.First()
                                      .properties.instanceView.currentState.state); //'Waiting' because it's still pulling image

                //Alternatively, we can use 'GetContainerGroupStatus' to get status
                await Task.Delay(TimeSpan.FromMinutes(7));//Wait for image to finish pulling
                var containerStatus = await resourceAccess.GetContainerGroupStatus("my-container");
                Console.WriteLine("Container status now is: " + containerStatus);//ContainerStatus.RUNNING

                //Delete container
                await resourceAccess.DeleteContainer("my-container");
                containerStatus = await resourceAccess.GetContainerGroupStatus("my-container");
                Console.WriteLine("Container status now is: " + containerStatus);//ContainerStatus.DELETED

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured: {e}");
            }
        }
    }
}
