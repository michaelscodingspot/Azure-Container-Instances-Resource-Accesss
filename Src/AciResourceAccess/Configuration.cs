using System;
using System.Collections.Generic;
using System.Text;

namespace AciResourceAccess
{
    public enum AuthorizationType { MSI, ActiveDirectoryRegisteredApp};

    public class Configuration
    {

        public string AzureSubscriptionId { get; internal set; }

        public string ImageRegistryServer { get; set; }
        public string ImageRegistryUsername { get; internal set; }
        public string ImageRegistryPassword { get; internal set; }

        public string ResourceGroup { get; internal set; }

        // Active directory registration
        public AuthorizationType AuthorizationType { get; internal set; }
        public string ClientId { get; internal set; }
        public string ClientSecret { get; internal set; }
        public string TenantId { get; internal set; }

        public string ApiVersion { get; private set; } = "2018-04-01";
    }

    public static class ConfigurationFactory
    {

        public static Configuration CreateConfigWithMsiAuth(string azureSubscriptionId, string imageRegistryServer, string imageRegistryUsername,
            string imageRegistryPassword, string resourceGroup)
        {
            Configuration config = CreateBasicConfig(azureSubscriptionId, imageRegistryServer, imageRegistryUsername, imageRegistryPassword, resourceGroup);
            return config;
        }

        public static Configuration CreateConfigWithActiveDirectoryAppAuth(string azureSubscriptionId, string imageRegistryServer, string imageRegistryUsername,
            string imageRegistryPassword, string resourceGroup,
            string clientId, string clientSecret, string tenantId)
        {
            Configuration config = CreateBasicConfig(azureSubscriptionId, imageRegistryServer, imageRegistryUsername, imageRegistryPassword, resourceGroup);
            config.ClientId = clientId;
            config.ClientSecret = clientSecret;
            config.TenantId = tenantId;
            return config;
        }

        private static Configuration CreateBasicConfig(string azureSubscriptionId, string imageRegistryServer, string imageRegistryUsername, string imageRegistryPassword, string resourceGroup)
        {
            return new Configuration()
            {
                AzureSubscriptionId = azureSubscriptionId,
                ImageRegistryServer = imageRegistryServer,
                ImageRegistryUsername = imageRegistryUsername,
                ImageRegistryPassword = imageRegistryPassword,
                ResourceGroup = resourceGroup,
                AuthorizationType = AuthorizationType.MSI
            };
        }

        
    }
}
