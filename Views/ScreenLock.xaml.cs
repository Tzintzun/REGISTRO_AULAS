using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AulasSiencb2.ViewModel;
using Serilog;

namespace AulasSiencb2.Views
{
    /// <summary>
    /// Lógica de interacción para ScreenLock.xaml
    /// </summary>
    public partial class ScreenLock : Window
    {
        /*
         =====================================================
         =================Sobreponer Ventana==================
         =====================================================
         */
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        /*
         =====================================================
         ==================Bloquear Teclas====================
         =====================================================
         */
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        private IntPtr ptrHook = IntPtr.Zero;
        private LowLevelKeyboardProc objKeyboardProcess;
        private ProcessModule objCurrentModule;

        public ScreenLock()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;


        }

        public void Configure()
        {
            Debug.WriteLine(this.DataContext.GetType().Name);
            if (this.DataContext is ScreenLockViewModel screenLock)
            {

                Debug.WriteLine("is ScrrenLockViewModel");
                screenLock.CloseWindowEvent += () =>
                {
                    Debug.WriteLine("Cerrando...");
                    Close();
                };
            }
        }

        public void OnClosign(object sender, CancelEventArgs e)
        {
            this.DialogResult = null;
            if (this.DataContext is ScreenLockViewModel screenLock)
            {
                if (screenLock.DialogResult != null)
                {
                    try
                    {
                        Debug.WriteLine("Desbloquenado");
                        Log.Information("Desbloqueando");
                        UnhookWindowsHookEx(ptrHook);
                        var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                        SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                        this.DialogResult = screenLock.DialogResult;
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                        Log.Error($"No se pudo desbloqeuar la ventana {ex.Message} - {ex.StackTrace}");
                    }
                }
                this.DialogResult = null;
#if DEBUG
                e.Cancel = true;
#else
                e.Cancel = true;
#endif
                Debug.WriteLine("No se cierra");
                return;
            }
        }

        /****************************************************/
        /*********** Control de bloque de ventana ***********/
        /****************************************************/

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys 

                if (objKeyInfo.vkCode == 0x5B || objKeyInfo.vkCode == 0x5C || // Teclas Windows
                    (objKeyInfo.vkCode == 0x1B && Keyboard.Modifiers == ModifierKeys.Control) || // Ctrl + Esc
                    (objKeyInfo.vkCode == 0x1B && HasAltModifier((int)objKeyInfo.flags)) || // Alt + Esc
                    (objKeyInfo.vkCode == 0x09 && HasAltModifier((int)objKeyInfo.flags))) // Alt + Tab
                {
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if RELEASE
            objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS); //sobreponer ventana de aplicacion
#endif
        }


    }
}
