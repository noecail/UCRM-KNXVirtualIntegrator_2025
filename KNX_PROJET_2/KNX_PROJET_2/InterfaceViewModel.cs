using Knx.Falcon.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNX_PROJET_2
{
    public class InterfaceViewModel
    {
        public ConnectorType Type { get; }
        public string DisplayName { get; }
        public string ConnectionString { get; }

        public InterfaceViewModel(ConnectorType type, string displayName, string connectionString)
        {
            Type = type;
            DisplayName = displayName;
            ConnectionString = connectionString;
        }

        public override string ToString() => DisplayName;
    }

}
