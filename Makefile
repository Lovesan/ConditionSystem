CSC=csc.exe /nologo

CONDITION_SYSTEM_NAME=ConditionSystem
CONDITION_SYSTEM_SRC=Conditions.cs \
										 HandlerBindCallback.cs \
										 HandlerBody.cs \
										 HandlerCaseCallback.cs \
										 RestartBindCallback.cs \
										 RestartCaseCallback.cs \
										 RestartNotFoundException.cs \
										 ConditionSystemAssemblyInfo.cs
CONDITION_SYSTEM_OUT=$(CONDITION_SYSTEM_NAME).dll
CONDITION_SYSTEM_SNK=$(CONDITION_SYSTEM_NAME).snk
CONDITION_SYSTEM_PDB=$(CONDITION_SYSTEM_NAME).pdb

CONDITION_SYSTEM_EXAMPLE_NAME=ConditionSystemExample
CONDITION_SYSTEM_EXAMPLE_SRC=ConditionSystemExample.cs \
														 ConditionSystemExampleAssemblyInfo.cs
CONDITION_SYSTEM_EXAMPLE_OUT=$(CONDITION_SYSTEM_EXAMPLE_NAME).exe
CONDITION_SYSTEM_EXAMPLE_PDB=$(CONDITION_SYSTEM_EXAMPLE_NAME).pdb

all: $(CONDITION_SYSTEM_OUT) $(CONDITION_SYSTEM_EXAMPLE_OUT)

$(CONDITION_SYSTEM_OUT): $(CONDITION_SYSTEM_SRC) $(CONDITION_SYSTEM_SNK)
	$(CSC) /out:$(CONDITION_SYSTEM_OUT) \
		     /target:library \
				 /keyfile:$(CONDITION_SYSTEM_SNK) \
				 /optimize \
	 			$(CONDITION_SYSTEM_SRC)

$(CONDITION_SYSTEM_EXAMPLE_OUT): $(CONDITION_SYSTEM_OUT) \
 						                     $(CONDITION_SYSTEM_EXAMPLE_SRC)
	$(CSC) /out:$(CONDITION_SYSTEM_EXAMPLE_OUT) \
				 /target:exe \
				 /optimize \
         /r:$(CONDITION_SYSTEM_OUT) \
				 $(CONDITION_SYSTEM_EXAMPLE_SRC)

clean:
	del /F /Q \
 		$(CONDITION_SYSTEM_OUT) \
		$(CONDITION_SYSTEM_PDB) \
		$(CONDITION_SYSTEM_EXAMPLE_OUT) \
		$(CONDITION_SYSTEM_EXAMPLE_PDB)

# vim: et!
