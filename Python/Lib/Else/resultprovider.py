from Else.baseprovider import BaseProvider
from Else.result import Result

class ResultProvider(BaseProvider):
    def __init__(self, keyword, is_fallback=False, matchall=False):
        self.keyword = keyword
        self.is_fallback = is_fallback
        self.matchall = matchall
    
    def is_interested(self, query):
        if query.get('Keyword') and self.keyword:
            if query.get('KeywordComplete') and query.get('Keyword') == self.keyword:
                return self.Interest.Exclusive
            if not query.get('KeywordComplete') and query.get('Keyword').startswith(self.keyword):
                return self.Interest.Shared
        
        if self.is_fallback:
            return self.Interest.Fallback
        
        if self.matchall:
            return self.Interest.Shared

        return self.Interest.Nil

    def query(self, query, cancel_token):
        raise NotImplementedError



