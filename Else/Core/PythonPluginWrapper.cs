using System.Linq;
using Else.Extensibility;
using IronPython.Hosting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace Else.Core
{
    public class PythonPluginWrapper : BasePluginWrapper
    {
        private readonly IAppCommands _appCommands;

        public PythonPluginWrapper(IAppCommands appCommands)
        {
            _appCommands = appCommands;
        }

        public override void Load(string path)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromFile(path);
            var compiled = source.Compile();
            var result = compiled.Execute(scope);

            foreach (var obj in scope.GetItems().Where(pair => pair.Value is PythonType)) {
                var value = obj.Value;

                if (obj.Key != typeof (Plugin).Name && PythonOps.IsSubClass(value, DynamicHelpers.GetPythonTypeFromType(typeof (Plugin)))) {
                    Plugin instance = value();
                    instance.Name = obj.Key;
                    instance.PluginLanguage = "IronPython";
                    Loaded.Add(instance);
                }
            }
        }
    }
}