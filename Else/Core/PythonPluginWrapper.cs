using System;
using System.IO;
using System.Linq;
using Autofac.Extras.NLog;
using Else.Extensibility;
using Else.Services;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace Else.Core
{
    public class PythonPluginWrapper : BasePluginWrapper
    {
        private readonly ILogger _logger;
        private readonly Paths _paths;
        private readonly IAppCommands _appCommands;

        public PythonPluginWrapper(ILogger logger, Paths paths, IAppCommands appCommands)
        {
            _logger = logger;
            _paths = paths;
            _appCommands = appCommands;
        }

        public override void Load(string path)
        {
            // create python engine
            var engine = Python.CreateEngine();

            // setup paths
            var paths = engine.GetSearchPaths();
            paths.Add(Path.GetDirectoryName(path));
            // todo: we need to distribute this directory with the app
            paths.Add(@"C:\Program Files (x86)\IronPython 2.7\Lib");
            engine.SetSearchPaths(paths);

            var scope = engine.CreateScope();
            // import clr module automatically
            scope.ImportModule("clr");
            // import Else.Extensibility.dll automatically, it's expected to be in the app path.
            var dllPath = _paths.GetAppPath("Else.Extensibility.dll");
            engine.Execute(string.Format(@"clr.AddReferenceToFileAndPath('{0}')", dllPath), scope);
            

            // execute the plugin py script
            
            var source = engine.CreateScriptSourceFromFile(path);
            var compiled = source.Compile();
            try {
                compiled.Execute(scope);
            }
            catch (Exception e) {
                var pytrace = engine.GetService<ExceptionOperations>().FormatException(e);
                _logger.Error("Failed to load plugin '{0}'", path);
                _logger.Error(pytrace);
            }

            // check for any python types that derive from Plugin..
            foreach (var obj in scope.GetItems().Where(pair => pair.Value is PythonType)) {
                var value = obj.Value;

                if (obj.Key != typeof (Plugin).Name && PythonOps.IsSubClass(value, DynamicHelpers.GetPythonTypeFromType(typeof (Plugin)))) {
                    // create instance of Plugin
                    Plugin instance = value();
                    instance.Name = obj.Key;
                    instance.PluginLanguage = "IronPython";
                    Loaded.Add(instance);
                }
            }
        }
    }
}