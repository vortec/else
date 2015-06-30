import Else
import bitly_api
import logging
import os

logging.basicConfig(filename=os.path.join(os.path.dirname(__file__), "urlshortener.log"), level=logging.DEBUG)


PLUGIN_NAME = "URLShortener"

def setup():
    # do plugin setup here.
    Else.add_command(Else.Command('shorten', 'Shorten URL', 'Shorten {arguments}', requires_arguments=True, launch=shortenUrl))

def shortenUrl(query):
    # check arguments is valid url
    if query['Arguments']:
        # query bitly api
        short = shorten(query['Arguments'])
        url = short['url']
        # hide the launcher window
        Else.app_commands.HideWindow()
        # copy shortened url to clipboard
        # ...

# use bitly url shortening service..
def shorten(url):
    bitly = bitly_api.Connection(access_token="0a862e797f9cd03f5d2ba1a9a6a85a3d691e23d1")
    logging.debug(bitly.shorten(url))
    return bitly.shorten(url)