import os
import shutil

source = "PythonPlugins"
output = "Build\Debug\Plugins"

# Scans for python plugin directories in $source and copies them to $output
def copy_plugins():
    if not os.path.exists(source):
        print "Error: source directory {} does not exist".format(source)
        return
    if not os.path.exists(output):
        print "Error: output directory {} does not exist".format(output)
        return


    # iterate directories in Source
    for dirname in os.listdir(source):
        pluginDir = os.path.join(source, dirname)
        script = os.path.join(pluginDir, dirname + ".py")
        # we are only interested in plugins that contain a matching .py file (e.g. if plugin directory is  "GitCommands", then "GitCommands.py" is expected
        if os.path.exists(script):
            outputDir = os.path.join(output, dirname)
            # recursively remove existing directory
            shutil.rmtree(outputDir, True)
            # recursively copy new directory
            shutil.copytree(pluginDir, outputDir)
            print "Copied plugin to {}".format(outputDir)


if __name__ == "__main__":
    copy_plugins()