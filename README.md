# MuxServerHelper
 
MuxServerHelper is a command line tool to invoke BD Mux Server or UHD Mux Server with a project XML file (encrypted or unencrypted), without using the Authoring Application. 

## Usage

Download the release executable, copy `MonteCarlo.External.MuxRemoting.dll` and `MuxCommon.DLL` into the same folder as `MuxServerHelper.exe`, then run it in CMD or PowerShell to see its usage. The two files are not included because they are parts of a commercial software. 

To run the program, you need to supply the path to the project XML file, and the path to the Mux Server minimally. Clip number should be adjusted according to your project. The default port for UHD Mux Server is 9930, so you need to manually pass it in arguments when using a UHD Mux Server. 

## Return Code

 - 0: Mux completed successfully if wait is true; or the muxing task is enqueued otherwise.
 - 1: Mux aborted because of an error.
 - -1: Incorrect command line arguments.
 - -2: Mux Server cannot be started, or the Mux Server executable cannot be found.

## Development

The project was developed using Visual Studio 2019 with .NET Framework 4.5.1, with `CommandLineParser` 2.8.0 from NuGet. You also need to place the two aforementioned DLL files in the project directory to compile.

## License and Copyright Notice

This project includes parts of code from [CsmStudio](https://github.com/subelf/CsmStudio), therefore it's released under GPLv3. For more information about the original project and the author please visit the link to the repo.
