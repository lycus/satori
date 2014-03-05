CP ?= cp
CHMOD ?= chmod
INSTALL ?= install
MKDIR ?= mkdir
MODE ?= Debug
PREFIX ?= /usr/local
SED ?= sed
XBUILD ?= xbuild

XBUILD_FLAGS += /nologo /property:Configuration=$(MODE) /verbosity:quiet

ifeq ($(MODE), Debug)
	override mono_opt = --debug
else
	override mono_opt =
endif

.PHONY: all clean check install uninstall

all:
	$(XBUILD) $(XBUILD_FLAGS)
	$(MKDIR) -p bin
	$(MKDIR) -p lib/satori
	$(CP) Lycus.Satori/bin/$(MODE)/Lycus.Satori.dll lib/satori
ifeq ($(MODE), Debug)
	$(CP) Lycus.Satori/bin/$(MODE)/Lycus.Satori.dll.mdb lib/satori
endif
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe lib/satori
ifeq ($(MODE), Debug)
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe.mdb lib/satori
endif
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe.config lib/satori
	$(SED) s/__MONO_OPTIONS__/$(mono_opt)/ e-exec.in > bin/e-exec
	$(CHMOD) +x bin/e-exec

clean:
	$(XBUILD) $(XBUILD_FLAGS) /target:Clean
	$(RM) -r bin
	$(RM) -r lib

check: all

install:
	$(INSTALL) -m755 -d $(PREFIX)
	$(INSTALL) -m755 -d $(PREFIX)/bin
	$(INSTALL) -m755 -d $(PREFIX)/lib
	$(INSTALL) -m755 -d $(PREFIX)/lib/satori
	$(INSTALL) -m755 lib/satori/Lycus.Satori.dll $(PREFIX)/lib/satori
ifeq ($(MODE), Debug)
	$(INSTALL) -m755 lib/satori/Lycus.Satori.dll.mdb $(PREFIX)/lib/satori
endif
	$(INSTALL) -m755 lib/satori/e-exec.exe $(PREFIX)/lib/satori
ifeq ($(MODE), Debug)
	$(INSTALL) -m755 lib/satori/e-exec.exe.mdb $(PREFIX)/lib/satori
endif
	$(INSTALL) -m755 lib/satori/e-exec.exe.config $(PREFIX)/lib/satori
	$(INSTALL) -m755 bin/e-exec $(PREFIX)/bin

uninstall:
	$(RM) $(PREFIX)/lib/satori/Lycus.Satori.dll
ifeq ($(MODE), Debug)
	$(RM) $(PREFIX)/lib/satori/Lycus.Satori.dll.mdb
endif
	$(RM) $(PREFIX)/lib/satori/e-exec.exe
ifeq ($(MODE), Debug)
	$(RM) $(PREFIX)/lib/satori/e-exec.exe.mdb
endif
	$(RM) $(PREFIX)/lib/satori/e-exec.config
	$(RM) $(PREFIX)/bin/e-exec
