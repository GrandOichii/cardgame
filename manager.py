import sys
from PyQt5.QtWidgets import QApplication
from front.test_py.client import ClientWindow

from tools.manager.window import ManagerEditor, StartMatchInterface

# TODO issues: front freezes when closing, can't open multiple windows at the same time

class GUIMatchStarter(StartMatchInterface):
    def start_match(self, host, port):
        w = ClientWindow(host, int(port))
        w.config_connection()
        w.set_screen_size(1200, 800)
        # w.go_fullscreen()
        w.run()

if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = ManagerEditor()
    ex.match_processor = GUIMatchStarter()
    ex.show()
    sys.exit(app.exec_())