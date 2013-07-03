This is a Microsoft .NET Gadgeteer module project created using the Module template in .NET Gadgeteer Builder Templates.
This template was most recently changed on 2012-04-30.

This template provides a simple way to build software for a custom Microsoft .NET Gadgeteer hardware module 
that is compliant with the Microsoft .NET Gadgeteer Module Builder's Guide specifications found at http://gadgeteer.codeplex.com/ 
Using this template auto-generates an installer (MSI) that can easily be distributed to end users, and an 
installation merge module (MSM) that can be used to make kit installers including many modules/mainboards/templates/etc.

We recommend that you read the Module Builder's Guide at http://gadgeteer.codeplex.com/ for full details.

==============================================

SYSTEM REQUIREMENT

To build the installer MSI automatically, this template requires WiX 3.5 to be installed:
http://wix.codeplex.com/releases/view/60102 (Wix35.msi)

==============================================

BUILD NOTES

Building with the Release configuration generates an MSI installer, which includes your code in the output directory 
of the project (bin/Release/Installer).  This takes a little time, and Visual Studio/C# Express may be unresponsive during the build.  
To avoid this delay, build with the Debug configuration. 

Visual C# Express always builds in Release configuration. In order to turn off the installer build to speed up the build process,
you can go to Menu->Project->[MainboardName] Properties->Build tab and tick the "Define DEBUG constant" box.

If you see the error "The system cannot find the file..." try "Rebuild" rather than "Build"

==============================================

MODULE TEMPLATE DETAILS

This template supports both NETMF 4.1 and NETMF 4.2 - so modules can provide a driver for both versions of NETMF.

The template is split into three projects each starting with the name of the module:
 [ModuleName]_41 : the NETMF 4.1 driver for the module
 [ModuleName]_42 : the NETMF 4.2 driver for the module
 [ModuleName] : the installer for the module

For the NETMF projects, the files are similar:
1) [ModuleName]_4x.cs - software implementation of the module's "device driver".
3) Properties\AssemblyInfo.cs - a cs file including the version number of the module driver, see MAKING CHANGES for when to change this.

In the installer project (with no suffix), the files are:
1) ReadMe.txt - this file
2) GadgeteerHardware.xml - defines some parameters about your module.
3) Image.jpg - placeholder for an image representing the module.
4) common.wxi - WiX (installer) configuration file that specifies parameters for the installer, including the version number. 
5) en-us.wxl - WiX (installer) localization file that specifies text strings that are displayed during installation.
6) msm.wxs - WiX (installer) script that generates an installation "merge module".
7) msi.wxs - WiX (installer) script that generates an installer (msi file) using the merge module.
8) G.ico - G graphic used by the installer.

==============================================

MODULE TEMPLATE USE INSTRUCTIONS 

1)  Edit the [ModuleName].cs files to implement software for your module.
    If you only want to support one version of NETMF then you only need to edit the relevant CS file, nonetheless, it is recommended
    not to delete the other project in case you wish to add support for the other version of NETMF in future. 
    See steps 5 and 6 for other actions necessary to support only one NETMF version.
    There are comments and examples in the cs files to assist you with this process.

2)  Test your module. Modules cannot be run directly, since they are class libraries (dll) not executables (exe).   
    Testing is most easily accomplished by adding a new Gadgeteer project to the same Visual Studio/Visual C# Express solution. 
    You will need to create two test projects if you want to test both NETMF41 and NETMF42 versions.
    With the new Program.cs file open, use the menu item Project->Add Reference, and, in the Projects tab, choose your module. 
    You should be able to instantiate the module using "new GTM.[Manufacturer Name].[Module Name]" - the designer will not work yet. 
    (Note - to run your test program, using the keyboard shortcut "F5" sometimes deploys the wrong app if there are >1 NETMF apps in a solution,
    right-clicking on the project name in Solution Explorer and choosing Debug->Start New Instance should always work.)

3)  Edit the GadgeteerHardware.xml file to specify information about your module, as described in that file.

4)  Optionally, change Image.jpg to a good quality top-down image of the module with the socket side facing up,
    cropped tight (no margin), in the same orientation as the width and height specified in GadgeteerHardware.xml (not rotated).   

5)  Build in Release configuration to build the module installer in the installer project's bin\Release\Installer directory!
    Install this and check that it appears correctly in the Control Panel's Add/Remove Programs window.

6)  Now open a new Gadgeteer App (for the right version(s) of NETMF) and your module should appear in the designer.
    It is recommended to re-run tests using the designer, as well as checking the appearance of the module in the designer.

==============================================

RELEASING THE MODULE SOFTWARE, INDIVIDUALLY OR IN A KIT

The MSI installer generated in the installer project's bin\Release\Installer directory can be distributed to end users.
The MSM merge module in the installer project's bin\Release\Installer directory can be used to build other installers such as "kit" installers 
that incorporate multiple module(s)/mainboard(s).  This will install/remove correctly - e.g. if two kits including a Foo Module 
are both installed, there will be one copy of the Foo module (the most up-to-date version) and if either kit is removed, 
the Foo module will remain installed, because the other kit requires it.

==============================================

MAKING CHANGES

If you make want to release a new version of your module, make sure to change the version number in common.wxi. 
Otherwise, the auto-generated installer will not be able to upgrade the older version correctly (an error message will result).
It is also necessary to change the versions in BOTH Properties\AssemblyInfo.cs files and often a good idea to 
keep these synchronized with your common.wxi version.  

If you have are using the kit template to make an installer, then we you may wish to synchronize the version numbers with the kit.
This process is documented in the Kit readme.txt file; it requires one-off changes to the AssemblyInfo.cs and common.wxi files.

If you want to change the long or short name of your module, or the long or short manufacturer name, it can be tricky to
find all references to those names.  Do a global search/replace on the whole solution, but in addition, look in the two 
NETMF project's project properties (search/replace doesn't find this!) under Assembly Name and Default Namespace.

==============================================

UPGRADING FROM AN OLDER TEMPLATE

The only (2.41.500 and before) template was a single project targeting only NETMF 4.1. 
To upgrade an old module to this template (supporting NETMF4.2 as well):

1) Create a new module template with the same name as the old one.  
2) Follow this ReadMe steps above based on your old code, GadgeteerHardware.xml file, and Properties\AssemblyInfo.cs files
   - fix any bugs in the [ModuleName]_42.cs file that might occur due to SDK changes
   - copy over GadgeteerHardware.xml but read "NEW DESIGNER BEHAVIOR" below to see how to augment it to support NETMF 4.2
   - remember to copy over the image
   - copy over the details above the line in the common.wxi file in the WiX directory 
3) Before building the installer, you need to copy over TWO GUIDs in order to make the upgrade work.  
   In common.wxi, just copy over the GUIDs for guid_msi_upgrade_code and guid_msi_package_id from the old version.
4) Make sure to use a higher version than the old one in THREE places - two Properties\AssemblyInfo.cs files and common.wxi 

If you follow these steps, the new installer should upgrade correctly over the old installer.

==============================================

NEW DESIGNER BEHAVIOR

As of 2.42.XXX, the designer supports a number of constraints/user messages based on GadgeteerHardware.xml.
(NB this support is present for both NETMF 4.1 and NETMF 4.2)
With point (2) particularly, this will cause some modules ported over from NETMF 4.1 to NOT WORK with NETMF 4.2 unless changes are made.

1) Only hardware supporting the current project's framework version (i.e. having least one Assembly for that version) will be shown.
   When porting from 4.1, you must add <Assembly MFVersion="4.2" Name="drivername"/> in the <Assemblies> element.

2) A few components of Gadgeteer have been split into separate assemblies to make it possible to exclude them when not required.
   This saves RAM and flash space when deployed. The designer can automatically include such core "fragments" when including a module
   that relies on them or is commonly used with them.  The fragment libraries should be listed in the Assemblies section of the module
   alongside that module's driver, so that the designer automatically includes a reference to them when that module is included.
   The fragments are: Gadgeteer.SPI, Gadgeteer.Serial, Gadgeteer.DaisyLink (inc. SoftwareI2C), Gadgeteer.WebClient and Gadgeteer.WebServer
   If you use any of these in your module, then it will NOT work in 4.2 unless you add an Assemblies entry in GadgeteerHardware.xml

3) A mainboard with built-in modules should nonetheless provide separate module classes for each type of functionality.  
   These modules should be named in a way that makes it clear that they only apply to one mainboard, e.g. ModulenameForMainboardname.
   The mainboard should list its driver under LibrariesProvided; the module should list the mainboard driver in ExtraLibrariesRequired.
   The ErrorMessage attribute of the module will be shown if the module is used with any other mainboard, and should make it 
   clear why that does not work, e.g.: "This module is built-in to [MainboardName] and cannot be used with any other mainboard."

4) A mainboard with custom firmware support libraries that modules might rely directly (so that the modules do not work with all mainboards)
   should list the assembly names in LibrariesProvided.  A module requiring a custom mainboard firmware library should list that under 

5) The MinimumGadgeteerCoreVersion attribute is supported, and the designer will present the user with an error message if the installed
   version of GadgeteerCore is not high enough, pointing them at http://gadgeteer.codeplex.com/ to get a newer version.

Examples are provided in comments in GadgeteerHardware.xml. 


