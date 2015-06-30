import Else


PLUGIN_NAME = "TestPlugin"
def setup():
    Else.add_command(Else.Command('testplugin', 'Test Plugin', 'Test {arguments}', requires_arguments=True))