

from front.test_py.client import ClientWindow
ClientWindow.Instance = ClientWindow()
ClientWindow.Instance.config_connection()
ClientWindow.Instance.set_screen_size(1200, 800)
# ClientWindow.Instance.go_fullscreen()
ClientWindow.Instance.run()