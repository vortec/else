import time
import clr
import bitly_api

#clr.AddReferenceToFileAndPath("c:\\users\\james\\repos\\else\\build\\debug\\Else.Extensibility.dll")  # yes i will fix this nasty shit
from Else.Extensibility import Plugin, Query

clr.AddReference('System.Windows.Forms')
from System.Windows.Forms import Clipboard

class URLShortener(Plugin):
    def Setup(self):
        self.AddCommand("shorten").RequiresArguments().Title("Shorten URL").Subtitle("Shorten {arguments}").Launch(self.launchCommand)
        
    def launchCommand(self, query):
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
        