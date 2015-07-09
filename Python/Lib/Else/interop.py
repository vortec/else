try:
    # attempt to import embedded C module
    from _else import app_commands
except ImportError:
    # import failed
    from Else import mock_app_commands as app_commands

providers = []

def add_command(command):
    """Registers a :class:`Command <Command>`.

    :param command: The :class:`Command <Command>` object.
    """
    providers.append(command)

def add_provider(provider):
    """Registers a :class:`ResultProvider <ResultProvider>`.

    :param provider: The :class:`ResultProvider<ResultProvider>` object.
    """
    providers.append(provider)