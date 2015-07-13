import Else
import bitly_api
import logging
import os
import ctypes
GlobalLock = ctypes.windll.kernel32.GlobalLock
GlobalAlloc = ctypes.windll.kernel32.GlobalAlloc
memcpy = ctypes.cdll.msvcrt.memcpy
memmove = ctypes.cdll.msvcrt.memmove
memmove.argtypes = [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_int64]
GlobalAlloc.restype = ctypes.c_void_p
GlobalLock.restype = ctypes.c_void_p
GlobalLock.argtypes = [ctypes.c_void_p]
GlobalLock.argtypes = [ctypes.c_void_p]

logging.basicConfig(filename=os.path.join(os.path.dirname(__file__), "urlshortener.log"), level=logging.DEBUG)


PLUGIN_NAME = "URLShortener"

import sys
logging.debug(sys.version)
def setup():
    # do plugin setup here.
    Else.add_command(Else.Command('shorten', 'Shorten URL', 'Shorten {arguments}', requires_arguments=True, launch=shorten_url))

def shorten_url(query):
    # check arguments is valid url
    if query['Arguments']:
        # hide the launcher window
        Else.app_commands.HideWindow()
        # query bitly api
        result = shorten(query['Arguments'])
        # copy shortened url to clipboard
        clipboard_set(result['url'])

# use bitly url shortening service..
def shorten(url):
    bitly = bitly_api.Connection(access_token="0a862e797f9cd03f5d2ba1a9a6a85a3d691e23d1")
    logging.debug(bitly.shorten(url))
    return bitly.shorten(url)

def clipboard_set(text):
    CF_UNICODETEXT = 13
    GHND = 0x0042

    if not text:
        return

    bufferSize = (len(text)+1)*2
    # allocate memory to store our text
    hGlobalMem = GlobalAlloc(ctypes.c_int(GHND), ctypes.c_int(bufferSize))
    # lock memory
    lpGlobalMem = GlobalLock(hGlobalMem)
    # copy our text over the memory
    memmove(lpGlobalMem, text, bufferSize)  # (destination, source, len)
    # unlock memory
    ctypes.windll.kernel32.GlobalUnlock(ctypes.c_int(hGlobalMem))
    # set clipboard
    if ctypes.windll.user32.OpenClipboard(0):
        ctypes.windll.user32.EmptyClipboard()
        ctypes.windll.user32.SetClipboardData(ctypes.c_int(CF_UNICODETEXT), ctypes.c_void_p(hGlobalMem))
        ctypes.windll.user32.CloseClipboard()