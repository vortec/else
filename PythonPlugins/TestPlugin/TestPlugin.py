import time
from Else.Extensibility import Plugin, Query

class TestPlugin(Plugin):
    def Setup(self):
        self.AddCommand("testpython2").Title("Test Python Command").Launch(self.launchCommand)
    def launchCommand(self, query):
        #self.Logger.Error("Error, something went wrong, omg.")
        #self.Logger.Warn("this is just a warning")
        #self.Logger.Debug("Standard Debug message..")
        #self.Logger.Info("this is informational message")
        pass