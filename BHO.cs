using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SimpleBHO
{
    [
        ComVisible(true),
        Guid("29E827A9-D957-487C-BB5F-3C93F125DF7D"),
        ClassInterface(ClassInterfaceType.None)
    ]
    public class BHO : IObjectWithSite
    {
        dynamic webBrowser;
        delegate void DWebBrowserEvents2_DocumentCompleteEventHandler(object pDisp, ref object URL);

        public int SetSite(object site)
        {
            if (site != null)
            {
                webBrowser = (dynamic)site;
                webBrowser.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(this.OnDocumentComplete);
            }
            else
            {
                webBrowser.DocumentComplete -= new DWebBrowserEvents2_DocumentCompleteEventHandler(this.OnDocumentComplete);
                webBrowser = null;
            }

            return 0;
        }
        public int GetSite(ref Guid guid, out IntPtr ppvSite)
        {
            IntPtr punk = Marshal.GetIUnknownForObject(webBrowser);
            int hr = Marshal.QueryInterface(punk, ref guid, out ppvSite);
            Marshal.Release(punk);

            return hr;
        }

        public void OnDocumentComplete(object pDisp, ref object URL)
        {
            dynamic document = webBrowser.Document;
            document.Script.setTimeout("javascript: alert('Hello, IE!')");
        }

        public static string BHO_REGISTRY_KEY_NAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";

        [ComRegisterFunction]
        public static void RegisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHO_REGISTRY_KEY_NAME, true);

            if (registryKey == null)
            {
                registryKey = Registry.LocalMachine.CreateSubKey(BHO_REGISTRY_KEY_NAME);
            }

            string guid = type.GUID.ToString("B");
            RegistryKey ourKey = registryKey.OpenSubKey(guid);

            if (ourKey == null)
            {
                ourKey = registryKey.CreateSubKey(guid);
            }

            ourKey.SetValue("NoExplorer", 1, RegistryValueKind.DWord);

            registryKey.Close();
            ourKey.Close();
        }

        [ComUnregisterFunction]
        public static void UnregisterBHO(Type type)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(BHO_REGISTRY_KEY_NAME, true);
            string guid = type.GUID.ToString("B");

            if (registryKey != null)
            {
                registryKey.DeleteSubKey(guid, false);
            }
        }
    }
}