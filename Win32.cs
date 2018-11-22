using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;


namespace Programmer
{
	class Win32
	{
		#region Constants

		[Flags]
		public enum EFileAccess : uint
		{
			//
			// Standart Section
			//

			AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
			MaximumAllowed = 0x2000000,     // MaximumAllowed access type

			Delete = 0x10000,
			ReadControl = 0x20000,
			WriteDAC = 0x40000,
			WriteOwner = 0x80000,
			Synchronize = 0x100000,

			StandardRightsRequired = 0xF0000,
			StandardRightsRead = ReadControl,
			StandardRightsWrite = ReadControl,
			StandardRightsExecute = ReadControl,
			StandardRightsAll = 0x1F0000,
			SpecificRightsAll = 0xFFFF,

			FILE_READ_DATA = 0x0001,        // file & pipe
			FILE_LIST_DIRECTORY = 0x0001,       // directory
			FILE_WRITE_DATA = 0x0002,       // file & pipe
			FILE_ADD_FILE = 0x0002,         // directory
			FILE_APPEND_DATA = 0x0004,      // file
			FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
			FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
			FILE_READ_EA = 0x0008,          // file & directory
			FILE_WRITE_EA = 0x0010,         // file & directory
			FILE_EXECUTE = 0x0020,          // file
			FILE_TRAVERSE = 0x0020,         // directory
			FILE_DELETE_CHILD = 0x0040,     // directory
			FILE_READ_ATTRIBUTES = 0x0080,      // all
			FILE_WRITE_ATTRIBUTES = 0x0100,     // all

			//
			// Generic Section
			//

			GenericRead = 0x80000000,
			GenericWrite = 0x40000000,
			GenericExecute = 0x20000000,
			GenericAll = 0x10000000,

			SPECIFIC_RIGHTS_ALL = 0x00FFFF,
			FILE_ALL_ACCESS =
			StandardRightsRequired |
			Synchronize |
			0x1FF,

			FILE_GENERIC_READ =
			StandardRightsRead |
			FILE_READ_DATA |
			FILE_READ_ATTRIBUTES |
			FILE_READ_EA |
			Synchronize,

			FILE_GENERIC_WRITE =
			StandardRightsWrite |
			FILE_WRITE_DATA |
			FILE_WRITE_ATTRIBUTES |
			FILE_WRITE_EA |
			FILE_APPEND_DATA |
			Synchronize,

			FILE_GENERIC_EXECUTE =
			StandardRightsExecute |
				FILE_READ_ATTRIBUTES |
				FILE_EXECUTE |
				Synchronize
		}

		[Flags]
		public enum EFileShare : uint
		{
			/// <summary>
			///
			/// </summary>
			None = 0x00000000,
			/// <summary>
			/// Enables subsequent open operations on an object to request read access.
			/// Otherwise, other processes cannot open the object if they request read access.
			/// If this flag is not specified, but the object has been opened for read access, the function fails.
			/// </summary>
			Read = 0x00000001,
			/// <summary>
			/// Enables subsequent open operations on an object to request write access.
			/// Otherwise, other processes cannot open the object if they request write access.
			/// If this flag is not specified, but the object has been opened for write access, the function fails.
			/// </summary>
			Write = 0x00000002,
			/// <summary>
			/// Enables subsequent open operations on an object to request delete access.
			/// Otherwise, other processes cannot open the object if they request delete access.
			/// If this flag is not specified, but the object has been opened for delete access, the function fails.
			/// </summary>
			Delete = 0x00000004
		}

		public enum ECreationDisposition : uint
		{
			/// <summary>
			/// Creates a new file. The function fails if a specified file exists.
			/// </summary>
			New = 1,
			/// <summary>
			/// Creates a new file, always.
			/// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
			/// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
			/// </summary>
			CreateAlways = 2,
			/// <summary>
			/// Opens a file. The function fails if the file does not exist.
			/// </summary>
			OpenExisting = 3,
			/// <summary>
			/// Opens a file, always.
			/// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
			/// </summary>
			OpenAlways = 4,
			/// <summary>
			/// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
			/// The calling process must open the file with the GENERIC_WRITE access right.
			/// </summary>
			TruncateExisting = 5
		}

		[Flags]
		public enum EFileAttributes : uint
		{
			Readonly = 0x00000001,
			Hidden = 0x00000002,
			System = 0x00000004,
			Directory = 0x00000010,
			Archive = 0x00000020,
			Device = 0x00000040,
			Normal = 0x00000080,
			Temporary = 0x00000100,
			SparseFile = 0x00000200,
			ReparsePoint = 0x00000400,
			Compressed = 0x00000800,
			Offline = 0x00001000,
			NotContentIndexed = 0x00002000,
			Encrypted = 0x00004000,
			Write_Through = 0x80000000,
			Overlapped = 0x40000000,
			NoBuffering = 0x20000000,
			RandomAccess = 0x10000000,
			SequentialScan = 0x08000000,
			DeleteOnClose = 0x04000000,
			BackupSemantics = 0x02000000,
			PosixSemantics = 0x01000000,
			OpenReparsePoint = 0x00200000,
			OpenNoRecall = 0x00100000,
			FirstPipeInstance = 0x00080000
		}

		public const uint WM_KEYDOWN				= 0x0100;
		public const uint WM_KEYUP					= 0x0101;
		public const uint WM_CLOSE					= 0x0010;
		public const uint WM_SYSCOMMAND			= 0x0112;
		public const uint WM_MOUSEMOVE			= 0x0200;
		public const uint WM_POWERBROADCAST	= 0x0218;

		public const int WH_MOUSE_LL = 14;

		public static readonly IntPtr HWND_TOPMOST		= new IntPtr( -1 );
		public static readonly IntPtr HWND_NOTOPMOST	= new IntPtr( -2 );
		public static readonly IntPtr HWND_TOP				= new IntPtr( 0 );
		public static readonly IntPtr HWND_BOTTOM		= new IntPtr( 1 );

		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;
		public const uint SWP_NOZORDER = 0x0004;
		public const uint SWP_NOREDRAW = 0x0008;
		public const uint SWP_NOACTIVATE = 0x0010;
		public const uint SWP_FRAMECHANGED = 0x0020;		// The frame changed: send WM_NCCALCSIZE
		public const uint SWP_SHOWWINDOW = 0x0040;
		public const uint SWP_HIDEWINDOW = 0x0080;
		public const uint SWP_NOCOPYBITS = 0x0100;
		public const uint SWP_NOOWNERZORDER = 0x0200;		// Don't do owner Z ordering
		public const uint SWP_NOSENDCHANGING = 0x0400;  // Don't send WM_WINDOWPOSCHANGING

		/// <summary>Windows message sent when a device is inserted or removed</summary>
		public const int WM_DEVICECHANGE = 0x0219;
		/// <summary>WParam for above : A device was inserted</summary>
		public const int DBT_DEVICEARRIVAL = 0x8000;
		/// <summary>WParam for above : A device was removed</summary>
		public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
		/// <summary>Used in SetupDiClassDevs to get devices present in the system</summary>
		public const int DIGCF_PRESENT = 0x02;
		public const int DIGCF_ALLCLASSES = 0x04;
		/// <summary>Used in SetupDiClassDevs to get device interface details</summary>
		public const int DIGCF_DEVICEINTERFACE = 0x10;
		/// <summary>Used when registering for device insert/remove messages : specifies the type of device</summary>
		public const int DBT_DEVTYP_DEVICEINTERFACE = 0x05;
		/// <summary>Used when registering for device insert/remove messages : we're giving the API call a window handle</summary>
		public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
		/// <summary>Purges Win32 transmit buffer by aborting the current transmission.</summary>
		public const uint PURGE_TXABORT = 0x01;
		/// <summary>Purges Win32 receive buffer by aborting the current receive.</summary>
		public const uint PURGE_RXABORT = 0x02;
		/// <summary>Purges Win32 transmit buffer by clearing it.</summary>
		public const uint PURGE_TXCLEAR = 0x04;
		/// <summary>Purges Win32 receive buffer by clearing it.</summary>
		public const uint PURGE_RXCLEAR = 0x08;
		/// <summary>CreateFile : Open file for read</summary>
		public const uint GENERIC_READ = 0x80000000;
		/// <summary>CreateFile : Open file for write</summary>
		public const uint GENERIC_WRITE = 0x40000000;
		/// <summary>CreateFile : Open handle for overlapped operations</summary>
		public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
		/// <summary>CreateFile : Resource to be "created" must exist</summary>
		public const uint OPEN_EXISTING = 3;
		/// <summary>ReadFile/WriteFile : Overlapped operation is incomplete.</summary>
		public const uint ERROR_IO_PENDING = 997;
		/// <summary>Infinite timeout</summary>
		public const uint INFINITE = 0xFFFFFFFF;
		/// <summary>Simple representation of a null handle : a closed stream will get this handle. Note it is public for comparison by higher level classes.</summary>
		public static IntPtr NullHandle = IntPtr.Zero;
		/// <summary>Simple representation of the handle returned when CreateFile fails.</summary>
		public static IntPtr InvalidHandleValue = new IntPtr( -1 );

		public static Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid( "A5DCBF10-6530-11D2-901F-00C04FB951ED" );
		public static Guid GUID_DEVINTERFACE_MODEM = new Guid( "2C7089AA-2E0E-11D1-B114-00C04FC2AAE4" );
		public static Guid GUID_DEVINTERFACE_COMPORT = new Guid( "86E0D1E0-8089-11D0-9CE4-08003E301F73" );
		public static Guid GUID_DEVINTERFACE_SERENUM_BUS_ENUMERATOR = new Guid( "4D36E978-E325-11CE-BFC1-08002BE10318" );


		public static int SPINT_ACTIVE = 0x00000001;
		public static int SPINT_DEFAULT = 0x00000002;
		public static int SPINT_REMOVED = 0x00000004;

		/// <summary> make change in all hardware profiles </summary>
		public static int DICS_FLAG_GLOBAL = 0x00000001;
		/// <summary> make change in specified profile only </summary>
		public static int DICS_FLAG_CONFIGSPECIFIC = 0x00000002;
		/// <summary> 1 or more hardware profile-specific </summary>
		public static int DICS_FLAG_CONFIGGENERAL = 0x00000004;

		/// <summary> Open/Create/Delete device key </summary>
		public static int DIREG_DEV = 0x00000001;
		/// <summary> Open/Create/Delete driver key </summary>
		public static int DIREG_DRV = 0x00000002;
		/// <summary> Delete both driver and Device key </summary>
		public static int DIREG_BOTH = 0x00000004;

		[Flags]
		public enum RegSAM
		{
			QueryValue=0x0001,
			SetValue=0x0002,
			CreateSubKey=0x0004,
			EnumerateSubKeys=0x0008,
			Notify=0x0010,
			CreateLink=0x0020,
			WOW64_32Key=0x0200,
			WOW64_64Key=0x0100,
			WOW64_Res=0x0300,
			Read=0x00020019,
			Write=0x00020006,
			Execute=0x00020019,
			AllAccess=0x000f003f
		}

		public static IntPtr INVALID_HANDLE_VALUE = new IntPtr( -1 );
	
		public enum PowerBroadcast : int
		{
			PBT_APMRESUMECRITICAL=0x6,
			PBT_APMRESUMESUSPEND=0x7,
			PBT_APMRESUMESTANDBY=0x8,
			PBT_APMRESUMEAUTOMATIC=0x12,
		}

		public const int SC_CLOSE	= 0xF060;
		public enum InputType : int
		{
			INPUT_MOUSE=0,
			INPUT_KEYBOARD=1,
			INPUT_HARDWARE=2,
		}

		public enum GetWindow_Cmd : uint
		{
			GW_HWNDFIRST=0,
			GW_HWNDLAST=1,
			GW_HWNDNEXT=2,
			GW_HWNDPREV=3,
			GW_OWNER=4,
			GW_CHILD=5,
			GW_ENABLEDPOPUP=6
		}

		public enum ShowWindow_Cmd : int
		{
			/// <summary
			/// Hides the window and activates another window.
			/// </summary>
			Hide=0,
			/// <summary>
			/// Activates and displays a window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position.
			/// An application should specify this flag when displaying the window 
			/// for the first time.
			/// </summary>
			Normal=1,
			/// <summary>
			/// Activates the window and displays it as a minimized window.
			/// </summary>
			ShowMinimized=2,
			/// <summary>
			/// Maximizes the specified window.
			/// </summary>
			Maximize=3, // is this the right value?
			/// <summary>
			/// Activates the window and displays it as a maximized window.
			/// </summary>       
			ShowMaximized=3,
			/// <summary>
			/// Displays a window in its most recent size and position. This value 
			/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
			/// the window is not actived.
			/// </summary>
			ShowNoActivate=4,
			/// <summary>
			/// Activates the window and displays it in its current size and position. 
			/// </summary>
			Show=5,
			/// <summary>
			/// Minimizes the specified window and activates the next top-level 
			/// window in the Z order.
			/// </summary>
			Minimize=6,
			/// <summary>
			/// Displays the window as a minimized window. This value is similar to
			/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowMinNoActive=7,
			/// <summary>
			/// Displays the window in its current size and position. This value is 
			/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowNA=8,
			/// <summary>
			/// Activates and displays the window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position. 
			/// An application should specify this flag when restoring a minimized window.
			/// </summary>
			Restore=9,
			/// <summary>
			/// Sets the show state based on the SW_* value specified in the 
			/// STARTUPINFO structure passed to the CreateProcess function by the 
			/// program that started the application.
			/// </summary>
			ShowDefault=10,
			/// <summary>
			///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
			/// that owns the window is not responding. This flag should only be 
			/// used when minimizing windows from a different thread.
			/// </summary>
			ForceMinimize=11
		}

		#endregion

		[StructLayout( LayoutKind.Sequential, Pack = 8 )]
		public struct NativeOverlapped
		{
			private IntPtr InternalLow;
			private IntPtr InternalHigh;
			public long Offset;
			public IntPtr EventHandle;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct MOUSEINPUT
		{
			public int dx;
			public int dy;
			public uint mouseData;
			public uint dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct KEYBDINPUT
		{
			public ushort wVk;
			public ushort wScan;
			public uint dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct HARDWAREINPUT
		{
			public uint uMsg;
			public ushort wParamL;
			public ushort wParamH;
		}

		[StructLayout( LayoutKind.Explicit )]
		public struct INPUT
		{
			[FieldOffset( 0 )]
			public InputType type;
			[FieldOffset( 4 )]
			public MOUSEINPUT mi;
			[FieldOffset( 4 )]
			public KEYBDINPUT ki;
			[FieldOffset( 4 )]
			public HARDWAREINPUT hi;

			public static int cbSize = Marshal.SizeOf( typeof( INPUT ) );
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public ShowWindow_Cmd showCmd;
			public System.Drawing.Point ptMinPosition;
			public System.Drawing.Point ptMaxPosition;
			public System.Drawing.Rectangle rcNormalPosition;

			static int _cbSize = Marshal.SizeOf( typeof( WINDOWPLACEMENT ) );
			public int cbSize
			{
				get
				{
					return _cbSize;
				}
			}
		}

		// Structure with information for RegisterDeviceNotification.
		[StructLayout( LayoutKind.Sequential )]
		public struct DEV_BROADCAST_HANDLE
		{
			public int dbch_size;
			public int dbch_devicetype;
			public int dbch_reserved;
			public IntPtr dbch_handle;
			public IntPtr dbch_hdevnotify;
			public Guid dbch_eventguid;
			public long dbch_nameoffset;
			//public byte[] dbch_data[1]; // = new byte[1];
			public byte dbch_data;
			public byte dbch_data1;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct DEV_BROADCAST_HDR
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
		}
		/*
		[StructLayout( LayoutKind.Sequential )]
		public struct DEV_BROADCAST_DEVICEINTERFACE
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
			public Guid dbcc_classguid;
			public short dbcc_name;
		}
		*/
		/// <summary>
		/// Used when registering a window to receive messages about devices added or removed from the system.
		/// </summary>
		[StructLayout( LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=1 )]
		public class DEV_BROADCAST_DEVICEINTERFACE
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
			public Guid dbcc_classguid;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst=256 )]
			public string dbcc_name;
		}

		/// <summary>
		/// Provides details about a single USB device
		/// </summary>
		[StructLayout( LayoutKind.Sequential, Pack=1 )]
		public struct SP_DEVINFO_DATA
		{
			public int cbSize;
			public Guid InterfaceClassGuid;
			public int Flags;
			public int Reserved;
		}

		/// <summary>
		/// Access to the path for a device
		/// </summary>
		[StructLayout( LayoutKind.Sequential, Pack=1 )]
		public struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public int cbSize;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst=256 )]
			public string DevicePath;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct FILETIME
		{
			public uint dwLowDateTime;
			public uint dwHighDateTime;
		}

		[DllImport( "kernel32.dll", SetLastError = true )]
		public static extern bool GetOverlappedResult(
			SafeFileHandle hFile,
			[In] ref System.Threading.NativeOverlapped lpOverlapped,
			out uint lpNumberOfBytesTransferred,
			bool bWait );

		[DllImport( @"kernel32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool ReadFile(
			SafeFileHandle hFile,					// handle to file
			[Out] byte[] lpBuffer,				// data buffer, should be fixed
			uint NumberOfBytesToRead,			// number of bytes to read
			out uint pNumberOfBytesRead,		// number of bytes read, provide IntPtr.Zero here
			ref System.Threading.NativeOverlapped lpOverlapped ); // should be fixed, if not IntPtr.Zero

		[DllImport( "kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi, SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool WriteFile(
			SafeFileHandle hFile,
			byte[] lpBuffer,
			uint nNumberOfBytesToWrite,
			out uint lpNumberOfBytesWritten,
			[In] ref System.Threading.NativeOverlapped lpOverlapped );

		[DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
		public static extern SafeFileHandle CreateFile(
			 string lpFileName,
			 EFileAccess dwDesiredAccess,
			 EFileShare dwShareMode,
			 IntPtr lpSecurityAttributes,
			 ECreationDisposition dwCreationDisposition,
			 EFileAttributes dwFlagsAndAttributes,
			 IntPtr hTemplateFile );

		[DllImport( "kernel32.dll" )]
		public static extern IntPtr GetModuleHandle( string moduleName );

		public delegate IntPtr HookProc( int nCode, uint wParam, IntPtr lParam );
		[DllImport( "user32.dll" )]
		public static extern IntPtr SetWindowsHookEx( int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId );

		[DllImport( "user32.dll" )]
		public static extern int UnhookWindowsHookEx( IntPtr hhook );

		[DllImport( "user32.dll" )]
		public static extern IntPtr CallNextHookEx( IntPtr hhk, int nCode, uint wParam, IntPtr lParam );

		[DllImport( "kernel32.dll", SetLastError=true )]
		public static extern bool GetSystemTimes(
			out FILETIME lpIdleTime,
			out FILETIME lpKernelTime,
			out FILETIME lpUserTime );
	
		[DllImport( "user32.dll" )]
		public static extern IntPtr GetMessageExtraInfo();

		[DllImport( "user32.dll", SetLastError=true )]
		//public static extern uint SendInput( uint nInputs, INPUT[] pInputs, int cbSize );
		public static extern uint SendInput( uint nInputs, ref INPUT pInputs, int cbSize );

		[DllImport( "user32.dll" )]
		public static extern bool GetAsyncKeyState( System.Windows.Forms.Keys vKey );

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool SetForegroundWindow( IntPtr hWnd );

		[DllImport( "user32.dll" )]
		public static extern IntPtr SetFocus( IntPtr hWnd );

		[DllImport( "user32.dll", SetLastError=true )]
		public static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

		// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
		// Also consider whether you're being lazy or not.
		[DllImport( "user32.dll", EntryPoint="FindWindow", SetLastError=true )]
		public static extern IntPtr FindWindowByCaption( IntPtr ZeroOnly, string lpWindowName );

		[DllImport( "user32.dll", SetLastError=true )]
		public static extern IntPtr GetWindow( IntPtr hWnd, uint uCmd );

		[DllImport( "user32.dll" )]
		public static extern IntPtr GetDlgItem( IntPtr hDlg, int nIDDlgItem );

		[DllImport( "user32.dll" )]
		public static extern IntPtr GetActiveWindow();

		[DllImport( "user32.dll" )]
		public static extern IntPtr GetParent( IntPtr hWnd );

		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32.dll" )]
		public static extern bool EnableWindow( IntPtr hWnd, bool bEnable );

		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32.dll", SetLastError=true )]
		public static extern bool BringWindowToTop( IntPtr hWnd );

		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32.dll", SetLastError=true )]
		public static extern bool PostMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

		[DllImport( "user32.dll", CharSet=CharSet.Auto, SetLastError=false )]
		public static extern IntPtr SendMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

		[DllImport( "user32.dll", SetLastError=true, CharSet=CharSet.Auto )]
		public static extern bool SendNotifyMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

		//[DllImport( "user32.dll" )]
		//public static extern uint GetWindowThreadProcessId( IntPtr hWnd, IntPtr ProcessId );

		[DllImport( "user32.dll" )]
		public static extern uint GetWindowThreadProcessId( IntPtr hWnd, out uint lpdwProcessId );

		[DllImport( "user32.dll" )]
		public static extern bool AttachThreadInput( uint idAttach, uint idAttachTo, bool fAttach );

		[DllImport( "kernel32.dll" )]
		public static extern uint GetCurrentThreadId();

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool ShowWindow( IntPtr hWnd, ShowWindow_Cmd nCmdShow );

		[DllImport( "user32.dll" )]
		public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags );

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool ShowWindowAsync( IntPtr hWnd, ShowWindow_Cmd nCmdShow );

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowPlacement( IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl );

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern int ShowCursor( bool bShow );


		/// <summary>
		/// Allocates an InfoSet memory block within Windows that contains details of devices.
		/// </summary>
		/// <param name="gClass">Class guid (e.g. HID guid)</param>
		/// <param name="strEnumerator">Not used</param>
		/// <param name="hParent">Not used</param>
		/// <param name="nFlags">Type of device details required (DIGCF_ constants)</param>
		/// <returns>A reference to the InfoSet</returns>
		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern IntPtr SetupDiGetClassDevs(
			ref Guid gClass,
			[MarshalAs( UnmanagedType.LPStr )]
			string strEnumerator,
			IntPtr hParent,
			uint nFlags );

		/// <summary>
		/// Registers a window for device insert/remove messages
		/// </summary>
		/// <param name="hwnd">Handle to the window that will receive the messages</param>
		/// <param name="lpInterface">DeviceBroadcastInterrface structure</param>
		/// <param name="nFlags">set to DEVICE_NOTIFY_WINDOW_HANDLE</param>
		/// <returns>A handle used when unregistering</returns>
		[DllImport( "user32.dll", SetLastError=true )]
		public static extern IntPtr RegisterDeviceNotification( IntPtr hwnd, DEV_BROADCAST_DEVICEINTERFACE oInterface, uint nFlags );

		/// <summary>
		/// Unregister from above.
		/// </summary>
		/// <param name="hHandle">Handle returned in call to RegisterDeviceNotification</param>
		/// <returns>True if success</returns>
		[DllImport( "user32.dll", SetLastError=true )]
		public static extern bool UnregisterDeviceNotification( IntPtr hHandle );

		/// <summary>
		/// Frees InfoSet allocated in call to above.
		/// </summary>
		/// <param name="lpInfoSet">Reference to InfoSet</param>
		/// <returns>true if successful</returns>
		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern int SetupDiDestroyDeviceInfoList( IntPtr lpInfoSet );

		/// <summary>
		/// Gets the DeviceInterfaceData for a device from an InfoSet.
		/// </summary>
		/// <param name="lpDeviceInfoSet">InfoSet to access</param>
		/// <param name="nDeviceInfoData">Not used</param>
		/// <param name="gClass">Device class guid</param>
		/// <param name="nIndex">Index into InfoSet for device</param>
		/// <param name="oInterfaceData">DeviceInterfaceData to fill with data</param>
		/// <returns>True if successful, false if not (e.g. when index is passed end of InfoSet)</returns>
		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr lpDeviceInfoSet,
			uint nDeviceInfoData,
			ref Guid gClass,
			uint nIndex,
			ref SP_DEVINFO_DATA oInterfaceData );
		/*
		[DllImport( "setupapi.dll", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern Boolean SetupDiEnumDeviceInterfaces(
			 IntPtr hDevInfo,
			 ref SP_DEVINFO_DATA devInfo,
			 ref Guid interfaceClassGuid,
			 UInt32 memberIndex,
			 ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
		);
		*/
		/// <summary>
		/// SetupDiGetDeviceInterfaceDetail - two of these, overloaded because they are used together in slightly different
		/// ways and the parameters have different meanings.
		/// Gets the interface detail from a DeviceInterfaceData. This is pretty much the device path.
		/// You call this twice, once to get the size of the struct you need to send (nDeviceInterfaceDetailDataSize=0)
		/// and once again when you've allocated the required space.
		/// </summary>
		/// <param name="lpDeviceInfoSet">InfoSet to access</param>
		/// <param name="oInterfaceData">DeviceInterfaceData to use</param>
		/// <param name="lpDeviceInterfaceDetailData">DeviceInterfaceDetailData to fill with data</param>
		/// <param name="nDeviceInterfaceDetailDataSize">The size of the above</param>
		/// <param name="nRequiredSize">The required size of the above when above is set as zero</param>
		/// <param name="lpDeviceInfoData">Not used</param>
		/// <returns></returns>
		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr lpDeviceInfoSet,
			ref SP_DEVINFO_DATA oInterfaceData,
			IntPtr lpDeviceInterfaceDetailData,
			uint nDeviceInterfaceDetailDataSize,
			ref uint nRequiredSize,
			IntPtr lpDeviceInfoData );
		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr lpDeviceInfoSet,
			ref SP_DEVINFO_DATA oInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA oDetailData,
			uint nDeviceInterfaceDetailDataSize,
			ref uint nRequiredSize,
			ref SP_DEVINFO_DATA deviceInfoData );

		[DllImport( "setupapi.dll", SetLastError=true )]
		public static extern bool SetupDiClassGuidsFromName(
			string ClassName,
			IntPtr ClassGuidList,
			UInt32 ClassGuidListSize,
			out UInt32 RequiredSize );

		[DllImport( "setupapi.dll", CharSet=CharSet.Auto, SetLastError=true )]
		public static extern bool SetupDiClassNameFromGuid(
			Guid ClassGuid,
			StringBuilder ClassName,
			UInt32 ClassNameSize,
			out UInt32 RequiredSize );

		[DllImport( "setupapi.dll", SetLastError=true, CharSet=CharSet.Auto )]
		public static extern bool SetupDiGetDeviceInstanceId(
			 IntPtr DeviceInfoSet,
			 ref SP_DEVINFO_DATA DeviceInfoData,
			 StringBuilder DeviceInstanceId,
			 int DeviceInstanceIdSize,
			 out int RequiredSize );

		[DllImport( "Setupapi", CharSet=CharSet.Auto, SetLastError=true )]
		public static extern IntPtr SetupDiOpenDevRegKey(
				IntPtr hDeviceInfoSet,
				ref SP_DEVINFO_DATA deviceInfoData,
				int scope,
				int hwProfile,
				int parameterRegistryValueKind,
				int samDesired );

		[DllImport( "advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueExW", SetLastError=true )]
		public static extern int RegQueryValueEx(
				IntPtr hKey,
				string lpValueName,
				IntPtr lpReserved,
				out uint lpType,
				System.Text.StringBuilder lpData,
				ref uint lpcbData );

		[DllImport( "advapi32.dll", SetLastError=true )]
		public static extern int RegCloseKey(
				IntPtr hKey );


		public static IntPtr FindModalWindow( string lpszWindow )
		{
			return FindWindow( "#32770", lpszWindow );
		}

		public static void CloseWindow( IntPtr hWnd )
		{
			//_SendMessage( hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero );
			SendMessage( hWnd, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero );
		}

	}
}
