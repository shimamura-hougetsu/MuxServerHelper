using System;
using System.IO;
using MonteCarlo.External.MuxRemoting;
using CommandLine;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.Net.Sockets;

namespace MuxServerHelper
{
    class Program
    {
        class CliArgs
        {
            [Option("project", Required = true, HelpText = "Path to project xml file.")]
            public string ProjectFile { get; set; }

            [Option("clip", Default = 1, HelpText = "Clip Number for the project.")]
            public int ClipCount { get; set; }

            [Option("server", Required = true, HelpText = "Path to the Mux Server executable.")]
            public string MuxServerExecutable { get; set; }

            [Option("port", Default = "9920", HelpText = "Port of the Mux Server.")]
            public string Port { get; set; }

            [Option("wait", Default = true, HelpText = "Whether program waits and exits on muxing complete or cancelled.")]
            public bool WaitFinish { get; set; }

        }

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<CliArgs>(args).MapResult(
                    opts => InvokeMuxServer(opts), _ => -1);
        }

        static IMuxRemotingService ConnectMuxService(string Port)
        {
            IMuxRemotingService muxService = (IMuxRemotingService)Activator.GetObject(typeof(IMuxRemotingService), $"tcp://localhost:{Port}/MuxRemotingService");
            muxService.GetServiceStatus();
            return muxService;
        }

        static void DisposeProcess(ref Process Proc)
        {
            if (Proc != null)
            {
                Proc.Dispose();
                Proc = null;
            }
        }

        static IMuxRemotingService StartMuxServerAndService(string MuxServerExecutable, string Port)
        {
            IMuxRemotingService muxService;
            FileInfo MuxServiceExecutableFile = new FileInfo(MuxServerExecutable);
            Process muxProc = null;
            try
            {
                muxService = ConnectMuxService(Port);  //assume muxer already started.
                if (muxProc == null || muxProc.HasExited)
                {
                    DisposeProcess(ref muxProc);
                    var tProcesses = Process.GetProcessesByName(MuxServiceExecutableFile.Name);
                    if (tProcesses.Length > 0)
                    {
                        muxProc = tProcesses[0];
                    }
                }
                return muxService;
            }
            catch (RemotingException) { }
            catch (SocketException) { }

            try
            {
                DisposeProcess(ref muxProc);
                ProcessStartInfo tStartInfo = new ProcessStartInfo(MuxServiceExecutableFile.FullName);
                tStartInfo.CreateNoWindow = true;
                muxProc = Process.Start(tStartInfo);
                muxService = ConnectMuxService(Port);  //retry.
                return muxService;
            }
            catch (InvalidOperationException) { }
            catch (FileNotFoundException) { }
            catch (RemotingException) { }
            catch (Win32Exception) { }

            return null;
        }
        static int InvokeMuxServer(CliArgs opts)
        {
            var muxService = StartMuxServerAndService(opts.MuxServerExecutable, opts.Port);
            if (muxService.Equals(null))
            {
                return -2;
            }
            MuxEnqueueStruct tMuxTaskDef = new MuxEnqueueStruct(opts.ProjectFile, opts.ClipCount);
            var muxTaskId = muxService.Enqueue(tMuxTaskDef);
            if (opts.WaitFinish)
            {
                for (; ; )
                {
                    var tInfo = muxService.GetRequestInfo(muxTaskId);
                    var tStatus = tInfo.Status;

                    if (tStatus.HasFlag(MuxRequestStatus.EndFlag))
                    {
                        var tIsOk = tStatus.HasFlag(MuxRequestStatus.Processed);
                        tIsOk &= tInfo.LastMuxStatusCode == MuxCommon.MuxStatusCode.MUX_SN_S_DONE;
                        muxService.Confirm(muxTaskId);
                        if (tIsOk) { return 0; } else { return 1; }
                    }
                    Thread.Sleep(1000);
                }
            }
            return 0;
        }
    }
}
