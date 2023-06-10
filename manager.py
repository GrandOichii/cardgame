import sys
from PyQt5.QtWidgets import QApplication
from front.test_py.client import ClientWindow

from tools.manager.window import ManagerEditor, StartMatchInterface

class GUIMatchStarter(StartMatchInterface):
    def start_match(self, host, port):
        ClientWindow.Instance = ClientWindow(host, int(port))
        ClientWindow.Instance.config_connection()
        ClientWindow.Instance.set_screen_size(1200, 800)
        # ClientWindow.Instance.go_fullscreen()
        ClientWindow.Instance.run()

if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = ManagerEditor()
    ex.match_processor = GUIMatchStarter()
    ex.show()
    sys.exit(app.exec_())