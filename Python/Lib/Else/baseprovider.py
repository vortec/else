from enum import IntEnum
from Else.result import Result


class BaseProvider():
    Interest = IntEnum('Interest', 'Nil Shared Fallback Exclusive')  # C# uses None, we must use Nil because python
    
    def is_interested(self, query):
        raise NotImplementedError

    def query(self, query, cancelToken):
        raise NotImplementedError