from Else.baseprovider import BaseProvider
from Else.result import Result

class ResultProvider(BaseProvider):
    """A flexible result provider.

    :param keyword: (optional) keyword required to trigger querying of this provider
    :param query_func: (optional) override the query method
    :param is_interested_func: (optional) override the is_interested method
    :param is_fallback: (optonal) if True, the provider will be queried when no other plugins provide results.
    :param matchall: (optonal) if True, the provider will always be queried.

    """
    def __init__(self, keyword=None, query_func=None, is_interested_func=None, is_fallback=False, matchall=False):
        self.keyword = keyword
        self.is_fallback = is_fallback
        self.matchall = matchall
        self.query_func = query_func
        self.is_interested_func = is_interested_func

    
    def is_interested(self, query):
        # if an alternative implementation has been provided, use that
        if self.is_interested_func:
            return self.is_interested_func(query)

        # otherwise default behaviour (standard keyword matching)
        if query.get('Keyword') and self.keyword:
            if query.get('KeywordComplete') and query.get('Keyword') == self.keyword:
                return self.Interest.Exclusive
            if not query.get('KeywordComplete') and self.keyword.startswith(query.get('Keyword')):
                return self.Interest.Shared
        
        if self.is_fallback:
            return self.Interest.Fallback
        
        if self.matchall:
            return self.Interest.Shared

        return self.Interest.Nil

    def query(self, query, cancel_token):
        # if an alternative implementation has been provided, use that
        if self.query_func:
            return self.query_func(query, cancel_token)
        # otherwise default behaviour (no results)
        return []



