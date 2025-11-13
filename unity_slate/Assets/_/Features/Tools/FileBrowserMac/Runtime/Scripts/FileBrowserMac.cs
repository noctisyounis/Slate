using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FileBrowserMac.Runtime
{
    public class FileBrowserMac : MonoBehaviour
    {
    
        #if UNITY_STANDALONE_OSX
                [DllImport("file_browser_mac")]
                private static extern IntPtr OpenFileDialogMac(string title, string directory, string extensions);

                [DllImport("file_browser_mac")]
                private static extern IntPtr OpenFolderDialogMac(string title, string directory);
        #endif

        public static string OpenFileDialog(string title = "Choisir un fichier", string directory = "", string extensions = "")
        {
                #if UNITY_STANDALONE_OSX
                            IntPtr ptr = OpenFileDialogMac(title, directory, extensions);
                            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
                #else
                        Debug.LogWarning("OpenFileDialogMac est uniquement disponible en build macOS.");
                        return null;
                #endif
        }

        public static string OpenFolderDialog(string title = "Choisir un dossier", string directory = "")
        {
                #if UNITY_STANDALONE_OSX
                            IntPtr ptr = OpenFolderDialogMac(title, directory);
                            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
                #else
                        Debug.LogWarning("OpenFolderDialogMac est uniquement disponible en build macOS.");
                        return null;
                #endif
        }
    }
}
