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
	$(CP) Lycus.Satori.ESim/bin/$(MODE)/e-sim.exe bin
	$(SED) s/__MONO_OPTIONS__/$(mono_opt)/ e-sim.in > bin/e-sim
	$(CHMOD) +x bin/e-sim

clean:
	$(XBUILD) $(XBUILD_FLAGS) /target:Clean
	$(RM) -r bin

check: all
