# import sys
# sys.path.append("c:\\users\\james\\repos\\else\\Python")
import Else
import bitly_api


class URLShortener(Else.Plugin):
    def Setup(self):
        self.add_command(Else.Command('shorten', 'Shorten URL', 'Shorten {arguments}', requires_arguments=True, launch=self.shortenUrl))
        
        
    def shortenUrl(self, query):
        # query bitly api
        short = self.shorten(query.Arguments)
        # hide the launcher window
        self.AppCommands.HideWindow()
        # copy shortened url to clipboard
        Clipboard.SetText(short["url"])

    # use bitly url shortening service..
    def shorten(self, url):
        bitly = bitly_api.Connection(access_token="0a862e797f9cd03f5d2ba1a9a6a85a3d691e23d1")
        return bitly.shorten(url)



Else.register_plugin(URLShortener())
