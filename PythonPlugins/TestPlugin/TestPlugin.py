import time
import clr
clr.AddReferenceToFileAndPath("c:\\users\\james\\repos\\else\\build\\debug\\Else.Extensibility.dll")
from Else.Extensibility import Plugin, Query

# simple python plugin that provides 1 command, the command prints "GO PYTHON GO!"
class TestPlugin(Plugin):
	def Setup(self):
		self.AddCommand("testpython2").Title("Test Python Command").Launch(self.launchCommand)
		
		
	def launchCommand(self, query):
		print "GO PYTHON GO!"