﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace pdfforge.PDFCreator.Utilities
{
    public interface IOsHelper
    {
        /// <summary>
        ///     Detect if the current process is running in 64-Bit mode
        /// </summary>
        bool Is64BitProcess { get; }

        /// <summary>
        ///     Detect if the application is run on a 64-Bit Windows edition
        /// </summary>
        bool Is64BitOperatingSystem { get; }

        bool UserIsAdministrator();

        string WindowsFontsFolder { get; }
    }

    public class OsHelper : IOsHelper
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
            );

        /// <summary>
        ///     Detect if the current process is running in 64-Bit mode
        /// </summary>
        public bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

        /// <summary>
        ///     Detect if the application is run on a 64-Bit Windows edition
        /// </summary>
        public bool Is64BitOperatingSystem
        {
            get { return Is64BitProcess || InternalCheckIsWow64(); }
        }

        private bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            return false;
        }

        public string GetWindowsVersion()
        {
            string windowsVersion = Environment.OSVersion.ToString();

            try
            {
                RegistryKey myKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (myKey != null)
                    windowsVersion = (string)myKey.GetValue("ProductName") + " (" + windowsVersion + ")";
            }
            catch
            {
            }

            return windowsVersion;
        }

        public bool UserIsAdministrator()
        {
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                if (user == null)
                    return false;

                WindowsPrincipal principal = new WindowsPrincipal(user);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string WindowsFontsFolder
        {
            get
            {
                string windir = Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows";

                return Path.Combine(windir, "Fonts");
            }
        }
    }
}