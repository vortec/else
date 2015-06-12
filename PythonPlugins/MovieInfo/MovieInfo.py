
# simple plugin that provides multiple commands
class SystemCommands(Plugin):
    def Setup(self):
        self.AddCommand("shutdown").Title("Shutdown").Launch(self.Shutdown)
        self.AddCommand("restart").Title("Restart").Launch(self.Restart)
        self.AddCommand("sleep").Title("Sleep").Launch(self.Sleep)
        self.AddCommand("hibernate").Title("Hibernate").Launch(self.Hibernate)
        self.AddCommand("lock").Title("Lock").Launch(self.Lock)
        self.AddCommand("recyclebin").Title("Recycle Bin").Launch(self.RecycleBin)
        self.AddCommand("logoff").Title("Log off").Launch(self.Logoff)
    
    def Shutdown(self):
        pass
    def Restart(self):
        pass
    def Sleep(self):
        pass
    def Hibernate(self):
        pass
    def Lock(self):
        pass
    def RecycleBin(self):
        pass
    def Logoff(self):
        pass