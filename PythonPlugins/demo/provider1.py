
class Result(object):
    # public Object Icon;
    # public Action<Query> Launch
    # public string Title;
    # public string SubTitle;
    pass

"""
Add 2 simple search providers, google and yahoo
"""

class GoogleProvider(BaseProvider):
    def __init__(self):
        pass
    def query(self, query):
        return Result(
            Title="Search google for {}".format(query),
            Launch=self.make_launch(query),
            Icon=some_icon,
            SubTitle="google search"
        )
    def make_launch(self, originalQuery):
        def launch(currentQuery):
            # open web browser
            openBrowser("http://google.co.uk/search?q=" + originalQuery.raw)
        return launch

        
class YahooProvider(BaseProvider):
    def __init__(self):
        pass
    def query(self, query):
        return Result(Title="Search yahoo for {}".format(query), Launch=self.make_launch(query), Icon=some_icon, SubTitle="yahoo search")

    def make_launch(self, originalQuery):
        def launch(currentQuery):
            # open web browser
            openBrowser("http://yahoo.co.uk/search?q=" + originalQuery.raw)
        return launch



class SearchPlugin(Plugin):
    def __init__(self):
        self.AddProvider(GoogleProvider())
        self.AddProvider(YahooProvider())
