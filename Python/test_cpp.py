import sys
sys.path.append("c:\\users\\james\\repos\\else\\python\\lib")
from Else import register_plugin

from Else import Plugin
class Test(Plugin):
    pass


register_plugin(Test())