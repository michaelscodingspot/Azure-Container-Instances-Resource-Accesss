using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AciResourceAccess
{
    public class EnvironmentVariable
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Port
    {
        public int port { get; set; }
    }

    public class Requests
    {
        public double cpu { get; set; }
        public double memoryInGB { get; set; }
    }

    public class Resources
    {
        public Requests requests { get; set; }
    }

    public class CurrentState
    {
        public string state { get; set; }
    }

    public class InstanceView
    {
        public CurrentState currentState { get; set; }
    }

    public class PropertiesOfContainer
    {
        public string[] command { get; set; }
        public EnvironmentVariable[] environmentVariables { get; set; }
        public string image { get; set; }
        public Port[] ports { get; set; }
        public Resources resources { get; set; }
        public InstanceView instanceView { get; set; }
    }

    public class Container
    {
        public string name { get; set; }
        public PropertiesOfContainer properties { get; set; }
    }

    public class ImageRegistryCredential
    {
        public string password { get; set; }
        public string server { get; set; }
        public string username { get; set; }
    }

    public class Port2
    {
        public string protocol { get; set; }
        public int port { get; set; }
    }

    public class IpAddress
    {
        public Port2[] ports { get; set; }
        public string type { get; set; }
        public object dnsNameLabel { get; set; }
        public string ip { get; set; }
    }

    public class Properties
    {
        public Container[] containers { get; set; }
        public ImageRegistryCredential[] imageRegistryCredentials { get; set; }
        public IpAddress ipAddress { get; set; }
        public string osType { get; set; }
        public string restartPolicy { get; set; }
    }


    public class Error
    {
        public string code { get; set; }
    }

    public class AciContainer
    {
        public string id { get; set; }
        public string location { get; set; }
        public string name { get; set; }
        public Properties properties { get; set; }
        public string type { get; set; }
        public Error error { get; set; }

        public string State
        {
            get
            {
                return properties?.containers?.FirstOrDefault()?.properties?.instanceView?.currentState?.state;
            }
        }
    }
}
