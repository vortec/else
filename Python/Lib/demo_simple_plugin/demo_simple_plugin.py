import Else
import webbrowser
import urllib.parse

PLUGIN_NAME = "GoogleSearch"

def setup():
    Else.add_command(Else.Command('google', 'Search Google', 'Search google for {arguments}', requires_arguments=True, launch=open_browser))

def open_browser(query):
    if query['Arguments']:
        args = urllib.parse.quote_plus(query['Arguments'])
        url = "https://www.google.com/search?q={}".format(args)
        webbrowser.open(url)
        Else.app_commands.HideWindow()
