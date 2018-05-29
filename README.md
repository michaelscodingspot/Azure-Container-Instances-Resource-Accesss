# Azure-Container-Instances-Resource-Accesss
A class library to easily create, query and delete containers with Azure Container Instances.

This library allows to:
- Create container instances with most initialization options
- Query container instances - Get IP, container status, etc.
- Delete container instances
- Supports authentication with Managed Service Identity (MSI)
- Supports authentication with Azure Active Directory application registration.

Read [this article](http://michaelscodingspot.com) to better understand Azure Container Instances and this library support.

Example of use:

~~~~
using ACI = AciResourceAccess;
...
 
var configuration = ACI.ConfigurationFactory.CreateConfigWithActiveDirectoryAppAuth(
    azureSubscriptionId: "a187256a-2ebe-4a23-8bfc-8e194d8eagh7",
    resourceGroup: "MyResourceGroup",
    clientId: "n21d9e2f-1f1c-45cf-q7r7-79c363e5c740",
    clientSecret: "zaTD123lof6JUiiMTUb+bGGldmA8NpIvTEht1w7rylA=",
    tenantId: "32dd593d-c0bb-48e8-8cd1-8521ab9e3b5e",
    imageRegistryServer: "mycontainerregistry.azurecr.io",
    imageRegistryUsername: "RegistryUserName",
    imageRegistryPassword: "/bf54vMBFEjOXQgTh/PzTwWj9fhcydn3"
);
 
var containerCreationConfig = new ACI.ContainerCreationConfiguration()
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
    var resourceAccess = new ACI.AciResourceAccess(configuration);
    var container = await resourceAccess.CreateContainer(containerCreationConfig);
    Console.WriteLine("Container created successfully on ip " 
                      + container.properties.ipAddress.ip);
}
catch (Exception e)
{
    Console.WriteLine($"Error creating container: {e}");
}
~~~~


