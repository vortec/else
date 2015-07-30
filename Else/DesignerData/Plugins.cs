using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Else.Extensibility;

namespace Else.DesignerData
{
    public class Plugins
    {
        private class MockPlugin : Plugin
        {
            public override void Setup()
            {
                throw new NotImplementedException();
            }
        };

        public BindingList<Plugin> Items => new BindingList<Plugin>()
        {
            new MockPlugin {
                Name = "Test Plugin1"
            },
            new MockPlugin {
                Name = "Test Plugin1"
            },
        };
    }
}
