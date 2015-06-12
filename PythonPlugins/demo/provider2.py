
class Result(object):
    # public Object Icon;
    # public Action<Query> Launch
    # public string Title;
    # public string SubTitle;
    pass

"""
Add 2 simple search providers, google and yahoo
"""

class SearchPlugin(Plugin):
    def __init__(self):
        self.AddProvider().Keyword("yahoo").Query(self.yahoo_query)
        self.AddProvider().Keyword("google").Query(self.google_query)

    def yahoo_query(self, query):
        return Result(Title="Search yahoo for {}".format(query), Launch=self.yahoo_launch(query), Icon=some_icon, SubTitle="yahoo search")

    def yahoo_launch(self, originalQuery):
        def launch(currentQuery):
            # open web browser
            openBrowser("http://yahoo.co.uk/search?q=" + originalQuery.raw)
        return launch
    
    def google_query(self, query):
        return Result(Title="Search google for {}".format(query), Launch=self.yahoo_launch(query), Icon=some_icon, SubTitle="google search")

    def google_launch(self, originalQuery):
        def launch(currentQuery):
            # open web browser
            openBrowser("http://google.co.uk/search?q=" + originalQuery.raw)
        return launch
    
        
