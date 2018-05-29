using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AciResourceAccess
{
    public class AciResourceAccess
    {
        private readonly Configuration _configuration;
        private readonly RestService _restService;
        private readonly ILogger<AciResourceAccess> _logger;
        //Access token when using Active Directory application registration
        private AzureAccessToken _accessToken;
        private readonly SemaphoreSlim _accessTokenGetterSemaphore = new SemaphoreSlim(1, 1);

        public AciResourceAccess(Configuration configuration, ILogger<AciResourceAccess> logger = null)
        {
            _configuration = configuration;
            _restService = new RestService();
            _logger = logger;
        }

        

        public async Task<AciContainer> CreateContainer(ContainerCreationConfiguration creationConfig)
        {
            var subscriptionId = _configuration.AzureSubscriptionId;
            string imageRegistryServer = _configuration.ImageRegistryServer;
            string imageRegistryUsername = _configuration.ImageRegistryUsername;
            string imageRegistryPassword = _configuration.ImageRegistryPassword;
            string resourceGroup = _configuration.ResourceGroup;

            var environmentVariables = creationConfig.EnvironmentVariables ?? new Dictionary<string, string>();
            var containerRequest = new AciContainer()
            {
                id = "/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.ContainerInstance/containerGroups/" + creationConfig.ContainerName,
                location = "east us",
                name = creationConfig.ContainerName,
                properties = new Properties()
                {
                    containers = new Container[1]
                        {
                            new Container()
                            {
                                name= creationConfig.ContainerName,
                                properties = new PropertiesOfContainer()
                                {
                                    command = new string[0],
                                    environmentVariables = environmentVariables.Select(pair =>
                                        new EnvironmentVariable() {name = pair.Key, value = pair.Value }).ToArray(),
                                    image = creationConfig.ImageName,//"ozcodecontainer.azurecr.io/ozcode",
                                    ports = new Port[1]
                                    {
                                        new Port()
                                        {
                                            port = creationConfig.Port,
                                        }
                                    },
                                    resources = new Resources()
                                    {
                                        requests = new Requests()
                                        {
                                            cpu = creationConfig.CpuCore,
                                            memoryInGB = creationConfig.MemoryInGB
                                        }
                                    },
                                },
                            },
                        },
                    imageRegistryCredentials = new ImageRegistryCredential[1]{
                        new ImageRegistryCredential()
                        {
                            password = imageRegistryPassword,
                            username = imageRegistryUsername,
                            server = imageRegistryServer,//"ozcodecontainer.azurecr.io"
                        }
                    },
                    ipAddress = new IpAddress()
                    {
                        ports = new Port2[1]
                        {
                            new Port2()
                            {
                                protocol = "TCP",
                                port = creationConfig.Port
                            }
                        },
                        type = "Public",
                        dnsNameLabel = null,
                    },
                    restartPolicy = creationConfig.RestartPolicy.ToString(),
                    osType = creationConfig.OS.ToString()
                },
                type = "Microsoft.ContainerInstance/containerGroups"
            };

            string url = GetAzureManagementUrl(creationConfig.ContainerName);

            var container = await _restService.SendHttpPutRequest<AciContainer, AciContainer>(url, containerRequest, await GetAccessToken());

            return container;
        }

        public async Task<AciContainer> GetContainer(string containerGroupName)
        {
            var url = GetAzureManagementUrl(containerGroupName);
            try
            {
                return await _restService.SendHttpGetRequest<AciContainer>(url, await GetAccessToken());
            }
            catch (Exception ex)
            {
                _logger?.LogError("GetContainerGroupStatus: Error while sending Get request: {0}", ex);
                throw;
            }


        }

        public async Task<ContainerStatus> GetContainerGroupStatus(string containerGroupName)
        {
            var url = GetAzureManagementUrl(containerGroupName);
            AciContainer container;
            try
            {
                container = await _restService.SendHttpGetRequest<AciContainer>(url, await GetAccessToken());
            }
            catch (Exception ex)
            {
                _logger?.LogError("GetContainerGroupStatus: Error while sending Get request: {0}", ex);
                return ContainerStatus.DELETED;
            }

            if (container.error?.code == "ResourceNotFound")
            {
                return ContainerStatus.DELETED;
            }

            string containerState = container.State?.ToLower();
            switch (containerState)
            {
                case "running":
                    return ContainerStatus.RUNNING;
                case "terminated":
                    return ContainerStatus.TERMINATED;
                default:
                    return ContainerStatus.WAITING;
            }

        }

        private async Task<string> GetAccessToken()
        {
            await _accessTokenGetterSemaphore.WaitAsync();
            try
            {
                if (_configuration.AuthorizationType == AuthorizationType.ActiveDirectoryRegisteredApp)
                    return await GetNewAccessTokenWithActivDeirectoryApp();
                else
                    return await GetNewAccessTokenWithMSI();
            }
            finally
            {
                _accessTokenGetterSemaphore.Release();
            }

        }

        //Documentation in https://docs.microsoft.com/en-gb/azure/active-directory/develop/active-directory-protocols-oauth-service-to-service#request-an-access-token
        public async Task<string> GetNewAccessTokenWithActivDeirectoryApp()
        {
            if (_accessToken == null || _accessToken.IsExpired)
            {
                string clientId = _configuration.ClientId;
                string clientSecret = _configuration.ClientSecret;
                string tenantId = _configuration.TenantId;

                string url = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

                Dictionary<string, string> payload = new Dictionary<string, string>();
                payload["grant_type"] = "client_credentials";
                payload["client_id"] = clientId;
                payload["client_secret"] = clientSecret;
                payload["resource"] = "https://management.azure.com/";


                _accessToken = await _restService.SendHttpPostRequestWithXWWWFormEncoding<AzureAccessToken>(url, payload);
            }

            return _accessToken.AccessTokenStr;
        }

        private async Task<string> GetNewAccessTokenWithMSI()
        {
            try
            {
                _logger?.LogInformation("log-Attempting to get access token with MSI");
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var accessTokenStr = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/");
                return accessTokenStr;
            }
            catch (Exception e)
            {
                _logger?.LogError("Error during 'GetNewAccessTokenWithMSI'");
                _logger?.LogError(e.ToString());
                throw;
            }

        }

        public async Task DeleteContainer(string containerGroupName)
        {
            string url = GetAzureManagementUrl(containerGroupName);
            var resp = await _restService.SendHttpDeleteRequest(url, await GetAccessToken());
        }

        private string GetAzureManagementUrl(string containerGroupName)
        {
            var subscriptionId = _configuration.AzureSubscriptionId;
            string resourceGroup = _configuration.ResourceGroup;
            string apiVersion = _configuration.ApiVersion;
            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ContainerInstance/containerGroups/{containerGroupName}?api-version={apiVersion}";
            return url;
        }
    }
}


