from front.test_py.client import ClientWindow

import sys

port = 8080
if len(sys.argv) == 2:
    port = int(sys.argv[1])

w = ClientWindow('localhost', port)
w.config_connection()
w.set_screen_size(1200, 800)
w.go_fullscreen()
w.run()