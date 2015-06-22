using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac.Extras.NLog;
using Else.Extensibility;
using Else.Services;
using IronPython.Hosting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace Else.Core
{
    public class PythonPluginWrapper : PluginWrapper
    {
        private readonly IAppCommands _appCommands;
        private readonly ILogger _logger;
        private readonly Paths _paths;

        public PythonPluginWrapper(ILogger logger, Paths paths, IAppCommands appCommands)
        {
            _logger = logger;
            _paths = paths;
            _appCommands = appCommands;
        }

        public override void Load(string path)
        {
            // create python engine
            var options = new Dictionary<string, object>();
            options["Frames"] = ScriptingRuntimeHelpers.True;

            var engine = Python.CreateEngine(options);

            // setup paths
            var paths = engine.GetSearchPaths();
            paths.Add(Path.GetDirectoryName(path));

            // determine path to IronPython library

            // in the same directory as our executable
            var local = Path.Combine(Directory.GetCurrentDirectory(), "PythonLib");

            // in the parent directory
            var parent = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "PythonLib");

            if (Directory.Exists(local)) {
                paths.Add(local);
            }
            else if (Directory.Exists(parent)) {
                paths.Add(parent);
            }
            else {
                _logger.Error("Python library not found. Checked {0};{1};", local, parent);
            }
            engine.SetSearchPaths(paths);

            var scope = engine.CreateScope();
            // import clr module automatically
            scope.ImportModule("clr");
            // import Else.Extensibility.dll automatically, it's expected to be in the app path.
            var dllPath = _paths.GetAppPath("Else.Extensibility.dll");
            engine.Execute(string.Format(@"clr.AddReferenceToFileAndPath(r'{0}')", dllPath), scope);


            // execute the plugin py script
            var source = engine.CreateScriptSourceFromFile(path);
            var compiled = source.Compile();
            try {
                compiled.Execute(scope);
            }
            catch (Exception e) {
                var pytrace = engine.GetService<ExceptionOperations>().FormatException(e);
                throw new Exception($"Failed to load plugin '{path}' {pytrace}");
            }

            // check for any python types that derive from Plugin..
            foreach (var obj in scope.GetItems().Where(pair => pair.Value is PythonType)) {
                var value = obj.Value;

                if (obj.Key != typeof (Plugin).Name && PythonOps.IsSubClass(value, DynamicHelpers.GetPythonTypeFromType(typeof (Plugin)))) {
                    // create instance of Plugin
                    Plugin instance = value();
                    instance.Name = obj.Key;
                    //instance._pluginLanguage = "IronPython";
                    Loaded.Add(instance);
                }
            }
        }
    }
}