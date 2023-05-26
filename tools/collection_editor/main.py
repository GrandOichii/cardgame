import sys
from PyQt5.QtWidgets import QApplication

from window import CollectionEditor

if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = CollectionEditor()
    ex.show()
    sys.exit(app.exec_())