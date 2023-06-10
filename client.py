from front.test_py.client import ClientWindow

import sys

port = 8080
if len(sys.argv) == 2:
    port = int(sys.argv[1])

ClientWindow.Instance = ClientWindow('localhost', port)
ClientWindow.Instance.config_connection()
ClientWindow.Instance.set_screen_size(1200, 800)
ClientWindow.Instance.go_fullscreen()
ClientWindow.Instance.run()