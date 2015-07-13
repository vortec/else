
class Result():
    """Represents a result displayed in the launcher.

    :param title: title
    :param subtitle: (optional) subtitle
    :param icon: (optional) icon
    :param launch: (optional) launch
    """
    def __init__(self, title, subtitle=None, icon=None, launch=None):
        self.title = title
        self.subtitle = subtitle
        self.icon = icon
        self.launch = launch

