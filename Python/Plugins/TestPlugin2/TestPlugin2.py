import sys
sys.path.append("c:\\users\\james\\repos\\else\\python\\lib")
import Else
import logging
logging.basicConfig(filename="c:\\users\\james\\repos\\else\\python\\plugins\\testplugin2\\testplugin2.log", level=logging.DEBUG)

PLUGIN_NAME = "TestPlugin"
def setup():
    Else.add_command(Else.Command('testplugin2', 'Test Plugin2', 'Test2 {arguments}', requires_arguments=True))

def hello():
    print("hello")


from threading import Event, Thread

class TimerThread(Thread):
    def __init__(self, label, repeat_delay=3):
        Thread.__init__(self)
        self.stop = Event()
        self.label = label
        self.repeat_delay = repeat_delay
        self.counter = 0
        
    def run(self):
        while not self.stop.wait(self.repeat_delay):
            logging.debug("{} ~ {} seconds [{}]".format(self.label, self.repeat_delay, self.counter))
            self.counter += 1


t = TimerThread("thread1", 1)
t2 = TimerThread("thread2", 1)
t.start()
t2.start()