import Else
import bitly_api


PLUGIN_NAME = "URLShortener"

def setup():
    # do plugin setup here.
    Else.add_command(Else.Command('shorten', 'Shorten URL', 'Shorten {arguments}', requires_arguments=True, launch=shortenUrl))

def shortenUrl(query):
    # check arguments is valid url
    if query['Arguments']:
        # query bitly api
        short = shorten(query['Arguments'])
        # hide the launcher window
        Else.app_commands.HideWindow()
        print("shortened: {}", short)
        # copy shortened url to clipboard
        #Clipboard.SetText(short["url"])

# use bitly url shortening service..
def shorten(url):
    bitly = bitly_api.Connection(access_token="0a862e797f9cd03f5d2ba1a9a6a85a3d691e23d1")
    print(bitly.shorten(url))
    return bitly.shorten(url)


