from Else.baseprovider import BaseProvider
import sys
from Else.result import Result
from Else.interop import app_commands
import traceback

class Command(BaseProvider):
    """A simple keyword command (with optional arguments)

    :param keyword: keyword required to trigger displaying of this command.
    :param title: result title.
    :param subtitle: (optional) result subtitle.
    :param icon: (optional) result icon.
    :param launch: (optional) method called when the result is launched.
    :param requires_arguments: (optional) if True, only display this command if arguments have been provided.
    :param fallback: (optonal) if True, the command will be displayed when no other plugins provide results.

    """
    def __init__(self, keyword, title, subtitle=None, launch=None, icon=None, requires_arguments=False, fallback=False):
        self.keyword = keyword
        self.title = title
        self.subtitle = subtitle
        self.icon = icon
        self.launch = launch
        self.requires_arguments = requires_arguments
        self.fallback = fallback

    def is_interested(self, query):
        if query.get('Keyword') and self.keyword:
            if query.get('KeywordComplete') and query.get('Keyword') == self.keyword:
                return self.Interest.Exclusive
            if not query.get('KeywordComplete') and self.keyword.startswith(query.get('Keyword')):
                return self.Interest.Shared

        if self.fallback:
            return self.Interest.Fallback

        return self.Interest.Nil

    def handle_launch(self, query):
        if self.requires_arguments and (not query.get('KeywordComplete') or not query.get('Arguments')):
            # auto complete the query
            app_commands.RewriteQuery(self.keyword + " ")
        else:
            self.launch(query)
    
    def query(self, query, cancel_token):
        result = Result(title=self.title, subtitle=self.subtitle, icon=self.icon, launch=self.handle_launch)
        # attempt to replace {arguments} token in the title string
        if self.requires_arguments:
            arguments = ""
            # check if our keyword was matched
            if self.keyword.startswith(query.get('Keyword')):
                # use arguments then
                arguments = query.get('Arguments')
            elif self.fallback:
                # otherwise use entire query (e.g. fallback=True commands)
                arguments = query.get('Raw')

            # use nice ellipsis if no arguments

            arguments = arguments or "..."

            if result.title:
                result.title = result.title.replace("{arguments}", arguments)
            if self.subtitle:
                result.subtitle = result.subtitle.replace("{arguments}", arguments)
        
        return [result]

