
# simple plugin that provides multiple commands

class ShutdownCommand(Command):
    def __init__(self):
        self.keyword = "shutdown"
        self.title = "Shutdown"
    def Launch(self):
        pass

class RestartCommand(Command):
    def __init__(self):
        self.keyword = "restart"
        self.title = "Restart"
    def Launch(self):
        pass

class SleepCommand(Command):
    def __init__(self):
        self.keyword = "sleep"
        self.title = "Sleep"
    def Launch(self):
        pass

class HibernateCommand(Command):
    def __init__(self):
        self.keyword = "hibernate"
        self.title = "Hibernate"
    def Launch(self):
        pass

class LockCommand(Command):
    def __init__(self):
        self.keyword = "lock"
        self.title = "Lock"
    def Launch(self):
        pass

class RecycleBinCommand(Command):
    def __init__(self):
        self.keyword = "recyclebin"
        self.title = "Recycle Bin"
    def Launch(self):
        pass

class LogoffCommand(Command):
    def __init__(self):
        self.keyword = "logoff"
        self.title = "Logoff"
    def Launch(self):
        pass

# simple plugin that provides multiple commands
class SystemCommands(Plugin):
    def Setup(self):
        self.AddCommand(ShutdownCommand)
        self.AddCommand(RestartCommand)
        self.AddCommand(SleepCommand)
        self.AddCommand(HibernateCommand)
        self.AddCommand(LockCommand)
        self.AddCommand(RecycleBinCommand)
        self.AddCommand(ShutdownCommand)
        self.AddCommand(LogoffCommand)