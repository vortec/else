import Else
from Else import add_command, Result, ResultProvider
import socket
import urllib.request

PLUGIN_NAME = "IP Address"
URL = "https://otp.me.uk/~james/ip.php"

def setup():
    add_command(ResultProvider('ip', query_func=query))

def query(query, cancelToken):
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.connect(('8.8.8.8', 80))
    local = Result(s.getsockname()[0])
    s.close()
    
    remote = Result(urllib.request.urlopen(URL).read().decode("utf-8").strip())




    return [remote,local]