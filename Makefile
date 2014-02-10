CP ?= cp
CHMOD ?= chmod
MKDIR ?= mkdir
MODE ?= Debug
SED ?= sed
XBUILD ?= xbuild

XBUILD_FLAGS += /nologo /property:Configuration=$(MODE) /verbosity:quiet

ifeq ($(MODE), Debug)
	override mono_opt = --debug
else
	override mono_opt =
endif

.PHONY: all clean check

all:
	$(XBUILD) $(XBUILD_FLAGS)
	$(MKDIR) -p bin
	$(CP) Lycus.Satori/bin/$(MODE)/Lycus.Satori.dll bin
ifeq ($(MODE), Debug)
	$(CP) Lycus.Satori/bin/$(MODE)/Lycus.Satori.dll.mdb bin
endif
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe bin
ifeq ($(MODE), Debug)
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe.mdb bin
endif
	$(CP) Lycus.Satori.EExec/bin/$(MODE)/e-exec.exe.config bin
	$(SED) s/__MONO_OPTIONS__/$(mono_opt)/ e-exec.in > bin/e-exec
	$(CHMOD) +x bin/e-exec

clean:
	$(XBUILD) $(XBUILD_FLAGS) /target:Clean
	$(RM) -r bin

check: all
