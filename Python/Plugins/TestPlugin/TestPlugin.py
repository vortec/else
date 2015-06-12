# import sys
# sys.path.append("c:\\users\\james\\repos\\else\\Python")
import Else


class TestPlugin(Else.Plugin):
    def Setup(self):
        self.add_command(Else.Command('testplugin', 'Test Plugin', 'Test {arguments}', requires_arguments=True))



Else.register_plugin(TestPlugin())