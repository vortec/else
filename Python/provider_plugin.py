# implementation of ResultProvider
from Else import ResultProvider, Result

class TwitterProvider(ResultProvider):
    twitter_icon = "icons/twitter.png"
    def __init__(self):
        super().__init__(keyword="twitter")

    def query(self, query, cancel_token):
        if not query['HasArguments']:
            return

        results = [
            Result(title="Open twitter user '{arguments}'", icon=self.twitter_icon, launch=self.open_twitter_user(query)),
            Result(title="Search twitter for '{arguments}'", icon=self.twitter_icon, launch=self.search_twitter(query))
        ]
        return results

    def open_twitter_user(self, original_query):
        def func(currentQuery):
            openBrowser("http://twitter.com")
        return func
    
    def search_twitter(self, original_query):
        def func(currentQuery):
            openBrowser("http://twitter.com")
        return func


# example of query from c#
demoquery = {
    'Raw': "twitter mrweiss",
    'Arguments': " mrweiss",
    'Empty': False,
    'HasArguments': True,
    'Keyword': "twitter",
    'KeywordComplete': "true"
}

# test
test = TwitterProvider()
results = test.query(demoquery, None)
print(results)