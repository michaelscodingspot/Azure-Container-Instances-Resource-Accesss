using System.Collections.Generic;

namespace AciResourceAccess
{
    public class ContainerCreationConfiguration
    {
        public enum OsType {Windows, Linux };
        public enum RestartPolicyType { Always, OnFailure, Never };

        public string ContainerName { get; set; }
        public string ImageName { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public string Location { get; set; } = "east us";
        public double CpuCore { get; set; } = 1;
        public double MemoryInGB { get; set; } = 4;
        public int Port { get; set; } = 80;
        public OsType OS { get; set; } = OsType.Linux;
        public RestartPolicyType RestartPolicy { get; set; }

    }
}