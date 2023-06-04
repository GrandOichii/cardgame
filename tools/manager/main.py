import sys
from PyQt5.QtWidgets import QApplication

from window import ManagerEditor

if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = ManagerEditor()
    ex.show()
    sys.exit(app.exec_())