try:
    # attempt to import embedded C module
    from _else import app_commands
except ImportError:
    # import failed
    print("c extension not available")
    
    # define dummy objects (so our plugins can be executed without the c module)
    class AppCommands():
        def __getattr__(self, attr):
            return lambda: print("AppCommand '{0}' is not available".format(attr))
    
    app_commands = AppCommands()

providers = []

def add_command(command):
    providers.append(command)

def add_provider(provider):
    providers.append(provider)