from .interop import app_commands

class Plugin():
    def __init__(self):
        self.name = "MyPlugin"
        self.providers = []
        self.root_dir = "%appdata%\\else\\plugins\\MyPlugin"
        self.logger = None
        self.app_commands = app_commands

    def add_command(self, command):
        self.providers.append(command)
    
    def add_provider(self, provider):
        self.providers.append(provider)
    
