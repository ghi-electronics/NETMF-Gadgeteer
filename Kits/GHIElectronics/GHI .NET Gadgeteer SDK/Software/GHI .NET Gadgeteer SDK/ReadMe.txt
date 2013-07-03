This is a Microsoft .NET Gadgeteer kit solution created using the Kit Template.
This template was most recently changed on 2012-04-18.

This template provides a simple way to build a custom kit installer for Microsoft .NET Gadgeteer incorporating
mainboard(s) and module(s) as well as custom Getting Started Guide documentation for this kit. 

Using this template auto-generates an installer (MSI) that can easily be distributed to end users.

We recommend that you read the Mainboard and Module Builder's Guides at http://gadgeteer.codeplex.com/ for full details.

==============================================

SYSTEM REQUIREMENT

To build the installer MSI automatically, this template requires WiX 3.5 to be installed:
http://wix.codeplex.com/releases/view/60102 (Wix35.msi)

==============================================

KIT TEMPLATE USE INSTRUCTIONS 

For the Getting Started Guide (optional)
	1) Write an HTML getting started guide and place it at [Solution]\Getting Started Guide\GettingStarted.htm, with images in the same directory.
	2) For your convenience, you may want to add the Getting Started Guide as a Solution Folder to the solution, and add all the files underneath that folder.

For the kit's installer (the project that this readme.txt is in)
	1) Edit en-us.wxl to specify the DistributorFull company name.  You can also change the kit name in this file.
	2) Edit msi.wxs, the installer WiX file:
		- Under "<!-- List all images used in the getting started guide here -->" list the getting started guide image files
		  If you don't have a getting started guide, remove the WiX elements relating to the Getting Started Guide

		- Under "<!-- List merge modules for mainboard(s) and module(s) here" follow the instructions to reference merge modules (msm files)
		  for the mainboard(s) and module(s) you are including in this kit. 
		
		- Under "<!-- List all merge modules above here" follow the instructions to reference merge modules as well

==============================================

BUILDING THE KIT

The msi.wxs file relies on merge module (MSM) files being present in the right locations for it to pick up. 
These are built by the respective mainboard and module projects, created using the respective .NET Gadgeteer templates.
These templates only build the MSM file in Release configuration since it is a time-consuming operation.

So, one way to build the Kit Installer is to open up each of the individual mainboard/module solutions 
and build under Release configuration to make the MSMs, before building the kit.

Another more convenient way to rebuild the kit is to add the mainboard/module projects (csprojs) to the Kit solution (sln).
To do this, right click on the solution in Solution Explorer and use the "Add->Existing Project" to link to each of the projects of each module/mainboard
(normally 3 projects, one for NETMF 4.1, one for NETMF 4.2, and one for the installer merge module).  

  If you are using a full version of Visual Studio and not the Express edition, it helps to use the Solution Folders feature to organise the projects - e.g. by 
  making three solution folders under your kit solution (by right clicking on the solution and choosing "Add solution folder"),
  named NETMF41, NETMF42, MergeModules. Then, for each of your modules/mainboards, put its three projects into the relevant folders.

Then, make sure the project dependencies are set up right (Menu->Project->Project Dependencies).  Ensure that each module/mainboard installer project relies on 
its NETMF41 and NETMF42 projects, and ensure that the Kit WiX project relies on all the module/mainboard merge module installer projects.

The kit installer will build in Debug or Release configuration, so one way to ensure that you don't accidentally rebuild all your MSMs is to stay in Debug configuration. 
If you rebuild in Release configuration, then every module/mainboard will be rebuilt and then the kit installer will be rebuilt.

NB If when building the installer you see build errors of the form: 
"Cannot open the merge module 'id_ModuleNamemsm' from file "<long path name>.msm" from msi.wxs
then ensure that the kit installer project has a dependency on all the other WiX projects using the Project->Project Dependencies menu. 

==============================================

MANAGING VERSION NUMBERS

This template provides TWO ways of managing version numbers in your module(s)/mainboard(s)/kit, SEPARATE and SYNCHRONIZED

SEPARATE, which is used by default, means the kit and every module and mainboard to have separately managed version numbers.
	Each module or mainboard actually has MULTIPLE version numbers.
	- One for each version of NETMF supported (e.g. one for NETMF 4.1 and one for NETMF 4.2)
	- One in the installer project's common.wxi file for the installer MSM/MSI files
	Each kit has one version number, in version.wxi.  

	Whenever you update the module/mainboard, you MUST update the affected NETMF version number(s) and the installer version number for that module.
	Whenever you release a new kit build, you MUST update the kit version number in version.wxi.
	Otherwise, the auto-generated installer will not be able to upgrade the older version correctly (an error message will result).

SYNCHRONIZED involves one-off manual edits to each module/mainboard to cause them to always synchronize with a Kit's version number.
	The kit's version number is stored in TWO places: AssemblyInfoGlobal.cs and version.wxi file.    
	For every module/mainboard, to add these files:
	- For the NETMF projects (two if the module/mainboard supports both NETMF 4.1 and NETMF 4.2),
	  using Menu->Project->Add an Existing Item, find the kit's AssemblyInfoGlobal.cs file, but make sure to ""Add as Link" using the dropdown arrow 
	  on the Add button, and not just press the Add button.  The added file will appear with a little shortcut arrow so you know it is a link.
	- In each NETMF project's Properties\AssemblyInfo.cs file, comment out the AssemblyInformationalVersion and AssemblyFileVersion
	- In the installer project's common.wxi file, follow the instructions to include the kit's version.wxi file and use that instead.

	NB you now have to update the kit's version numbers (both AssemblyInfoGlobal.cs and version.wxi) whenever ANY module/mainboard has changed,
	but now all of module/mainboard version numbers will track the kit version numbers, and stay synchronized more easily.

	Suggestion: use 


Notes on versioning:

AssemblyVersion strangely SHOULD NOT be updated with each build, only AssemblyInformationalVersion and AssemblyFileVersion.  
The AssemblyVersion is used for linking, so changing it means that any existing binaries relying on the changed version binary will now fail to link, 
which is often not the intention.  Note that NETMF and Gadgeteer maintain AssemblyVersion numbers static throughout a whole NETMF version, even when 
assemblies are updated to add new functionality.  Thus, in this template AssemblyVersion is left in the individual modules/mainboards' AssemblyInfo.cs files, though 
of course you can change that if desired. 

In contrast, AssemblyFileVersion is used to detect whether the file is newer for installer purposes (so if it is not
updated then the installer will not update the file - bad).  AssemblyInformationalVersion is used to show the user the version.  These should be updated every time.

Installer versions only use the top three numbers out of the four - i.e. in the version number 1.2.3.4, the "4" is ignored by the installer and 1.2.3.0 is the same as 1.2.3.4.

Thus, the suggested numbering scheme to keep numbers in sync is:

X.Y.0.0 for AssemblyVersion
X.Y.Z.0 for AssemblyInformationalVersion and AssemblyFileVersion
X.Y.Z for the installer version
		 

==============================================
