using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;


namespace Programmer
{
	public partial class FrmMain
	{
		StringBuilder _sbRs = new StringBuilder();
		object _locParse = new object();

		void USB_PortOpen_()
		{
			USB_InfoToggle( USB_CreateFiles() );
		}

		bool USB_Read( int pipeNum, byte[] buf )
		{
			return USB_ReadWrite( USB_MODE.USB_READ, pipeNum, buf, buf.Length );
		}

		internal bool USB_Write( int pipeNum, CMD_PROG cmd )
		{
			return USB_Write( pipeNum, cmd.Buf, cmd.StrucSize );
		}

		internal bool USB_Write( int pipeNum, object _buf, int len )
		{
			byte[] buf = null;

			if( _buf is ushort[] )
			{
				var ubuf = _buf as ushort[];
				buf = new byte[2 * ubuf.Length];

				Buffer.BlockCopy( ubuf, 0, buf, 0, buf.Length );
			}
			else if( _buf is byte[] )
			{
				buf = _buf as byte[];
			}

			return USB_ReadWrite( USB_MODE.USB_WRITE, pipeNum, buf, len );
		}

		bool USB_ReadWrite( USB_MODE readOrWrite, int pipeNum, byte[] buf, int len )
		{
			bool bRes = false;

			var hFile = _USB_Handles[pipeNum];
			if( hFile == null || hFile.IsInvalid )
				return bRes;

			uint NumberOfBytesRead = 0;
			uint nNumberOfBytesToRead = (uint)len;

			NativeOverlapped Overlapped = new NativeOverlapped();
			var ev = new AutoResetEvent( false );
			Overlapped.EventHandle = ev.SafeWaitHandle.DangerousGetHandle();

			if( readOrWrite == USB_MODE.USB_READ )
				bRes = Win32.ReadFile( hFile, buf, nNumberOfBytesToRead, out NumberOfBytesRead, ref Overlapped );
			else
				bRes = Win32.WriteFile( hFile, buf, nNumberOfBytesToRead, out NumberOfBytesRead, ref Overlapped );

			if( !bRes &&
					Marshal.GetLastWin32Error() == 997 &&              // ERROR_IO_PENDING
					ev.WaitOne( Timeout.Infinite /* 10000 */ ) &&
					Win32.GetOverlappedResult( hFile, ref Overlapped, out NumberOfBytesRead, false ) )
				bRes = true;

			if( len != NumberOfBytesRead )
				bRes = false;

			if( !bRes )
			{
				if( readOrWrite == USB_MODE.USB_WRITE )
					Log.L.Out( "Не удалось отправить сообщение в программатор.", Color.Salmon );
			}

			return bRes;
		}


		//Task _task_USB_ReadPipe3 = null;
		//AutoResetEvent _ev_USB_ReadPipe3 = new AutoResetEvent( false );
		internal byte[] _buf_USB_ReadPipe = new byte[512];

		internal bool USB_ReadPipe( int pipeNum )		// ( object sender, DoWorkEventArgs e )
		{
			bool bRes = false;

			AutoResetEvent ev = new AutoResetEvent( false );

			NativeOverlapped Overlapped = new NativeOverlapped();
			Overlapped.EventHandle = ev.SafeWaitHandle.DangerousGetHandle();

			//while( true )
			{
				try
				{
					if( _USB_Handles[pipeNum] == null || _USB_Handles[pipeNum].IsInvalid )
						return false;

					uint NumberOfBytes = 64;
					uint NumberOfBytesRead = 0;

					if( pipeNum != 3 )
						NumberOfBytes = 512;

					bRes = Win32.ReadFile( _USB_Handles[pipeNum], _buf_USB_ReadPipe, NumberOfBytes, out NumberOfBytesRead, ref Overlapped );

					if( !bRes &&
							Marshal.GetLastWin32Error() == 997 &&              // ERROR_IO_PENDING
							ev.WaitOne( Timeout.Infinite /* 5000 */ ) &&
							Win32.GetOverlappedResult( _USB_Handles[pipeNum], ref Overlapped, out NumberOfBytesRead, false ) )
					{
						bRes = true;
					}
					else
					{
						Log.L.InError( "Произошла ошибка при приеме." );
						return false;
					}

					if( NumberOfBytesRead != NumberOfBytes )
					{
						Log.L.InError( "Получено " + NumberOfBytesRead + " байт от программатора, вместо " + NumberOfBytes + "." );
						return false;
					}

					//if( bRes )
					//{
					//	_ev_USB_ReadPipe3.Set();
					//	//Log.L.ToScreenIn( "Получено 64 байта от программатора. buf[0] = " + _buf_USB_ReadPipe3[0] );
					//}
				}
				catch
				{
					bRes = false;
				}

				return bRes;
			}
		}


		void _serialPort_ErrorReceived( object sender, SerialErrorReceivedEventArgs e )
		{
			_sc.Post( delegate
			{
				//Log.L.LogInternal( FrmMain._main.txtBox, "!!! ERROR !!! on USB receiving: " + e.EventType.ToString() + "\n" );
			},
			null );
		}

		void USB_CloseFiles()
		{
			for( int i = 0; i<3; i++ )
			{
				if( _USB_Handles[i] != null )
				{
					_USB_Handles[i].Close();
					_USB_Handles[i].Dispose();

					_USB_Handles[i] = null;
				}
			}

			if( _USB_Handle != null )
			{
				_USB_Handle.Close();
				_USB_Handle.Dispose();
				_USB_Handle = null;
			}
		}


		bool USB_CreateFiles()
		{
			if( _USB_Handle != null && !_USB_Handle.IsInvalid )
				return true;

			var devPath = GetDevicePath( 0 );

			_USB_Handle = Win32.CreateFile( devPath,
				Win32.EFileAccess.GenericRead | Win32.EFileAccess.GenericWrite,
				Win32.EFileShare.Read | Win32.EFileShare.Write,
				IntPtr.Zero,
				Win32.ECreationDisposition.OpenExisting,
				0,
				IntPtr.Zero );//  0xC0000000, 3u, 0, 3u, 0, 0 ); );

			if( _USB_Handle.IsInvalid )
				return false;

			for( int i=0; i<_USB_Handles.Length; i++ )
			{
				Win32.EFileAttributes fa = Win32.EFileAttributes.Overlapped;

				_USB_Handles[i] = Win32.CreateFile( devPath + "\\PIPE" + i.ToString( "D2" ),
					Win32.EFileAccess.GenericRead | Win32.EFileAccess.GenericWrite,
					Win32.EFileShare.Read | Win32.EFileShare.Write,
					IntPtr.Zero,
					Win32.ECreationDisposition.OpenExisting,
					fa,
					IntPtr.Zero );

				if( _USB_Handles[i].IsInvalid )
				{
					_USB_Handle.Close();
					_USB_Handle.Dispose();
					_USB_Handle = null;

					for( int j=0; j<i; j++ )
					{
						_USB_Handles[j].Close();
						_USB_Handles[j].Dispose();

						_USB_Handles[j] = null;
					}

					return false;
				}
			}

			//if( _task_USB_ReadPipe3 == null || _task_USB_ReadPipe3.IsCompleted )
			//	_task_USB_ReadPipe3 = Task.Factory.StartNew( (Action)USB_ReadPipe3 );

			//if( _bw_USB_ReadPipe3 == null )
			//{
			//	_bw_USB_ReadPipe3 = new BackgroundWorker();
			//	_bw_USB_ReadPipe3.DoWork += USB_ReadPipe3;
			//	_bw_USB_ReadPipe3.RunWorkerAsync();
			//}

			return true;
		}

		string GetDevicePath( uint MemberIndex )
		{
			string sDevPath = "";
			uint RequiredSize = 0;
			Guid USB_GUID = new Guid( "7B68107E-5CED-4A55-BFC3-7ECDBCA3ADF9" );

			// dd 7B68107Eh
			// dw 5CEDh; Data2
			// dw 4A55h; Data3
			// db 0BFh, 0C3h, 7Eh, 0CDh, 0BCh, 0A3h, 0ADh, 0F9h; Data4

			// "7E10687B-ED5C-554A-BFC37ECDBCA3ADF9"

			var DeviceInfoSet = Win32.SetupDiGetClassDevs( ref USB_GUID, null, IntPtr.Zero, Win32.DIGCF_DEVICEINTERFACE | Win32.DIGCF_PRESENT );
			if( DeviceInfoSet == Win32.INVALID_HANDLE_VALUE )
				return sDevPath;

			var DeviceInterfaceData = new Win32.SP_DEVINFO_DATA();
			DeviceInterfaceData.cbSize = Marshal.SizeOf( DeviceInterfaceData );
			if( IntPtr.Size == 8 ) // 64-bit 
				DeviceInterfaceData.cbSize += 4;

			if( Win32.SetupDiEnumDeviceInterfaces( DeviceInfoSet, 0, ref USB_GUID, MemberIndex, ref DeviceInterfaceData ) )
			{
				if( !Win32.SetupDiGetDeviceInterfaceDetail( DeviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0, ref RequiredSize, IntPtr.Zero ) )
				{
					var da = new Win32.SP_DEVINFO_DATA();
					da.cbSize = Marshal.SizeOf( da );
					if( IntPtr.Size == 8 ) // 64-bit 
						da.cbSize += 4;

					var DeviceInterfaceDetailData = new Win32.SP_DEVICE_INTERFACE_DETAIL_DATA();
					DeviceInterfaceDetailData.cbSize = ( ( IntPtr.Size == 4 ) ? ( 5 ) : ( 8 ) );

					if( Win32.SetupDiGetDeviceInterfaceDetail( DeviceInfoSet, ref DeviceInterfaceData, ref DeviceInterfaceDetailData, RequiredSize, ref RequiredSize, ref da ) )
					{
						sDevPath = DeviceInterfaceDetailData.DevicePath;
					}
				}
			}

			// Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
			//
			Win32.SetupDiDestroyDeviceInfoList( DeviceInfoSet );

			return sDevPath;
		}

	}

	enum USB_MODE
	{
		USB_WRITE = 0,
		USB_READ = 1
	};

	[StructLayout( LayoutKind.Sequential, Pack = 1 )]
	internal class CMD_PROG
	{
		internal UInt16 Cmd;
		internal byte ProgProcNum;
		internal byte field_3;
		internal UInt32 BaseAddr;
		internal int Size;


		static int _strucSize;
		static byte[] _buf;

		internal CMD_PROG()
		{
			_strucSize = Marshal.SizeOf( typeof( CMD_PROG ) );
			_buf = new byte[_strucSize];
		}

		internal int StrucSize
		{
			get { return _strucSize; }
		}

		internal byte[] Buf
		{
			get
			{
				IntPtr ptr = Marshal.AllocHGlobal( _strucSize );
				Marshal.StructureToPtr( this, ptr, true );
				Marshal.Copy( ptr, _buf, 0, _strucSize );
				Marshal.FreeHGlobal( ptr );

				return _buf;
			}
		}
	};


	[StructLayout( LayoutKind.Sequential, Pack = 1 )]
	internal class PROG_STRUC
	{
		internal byte field_0;
		internal byte field_1;
		internal byte field_2;
		internal byte field_3;
		internal byte field_4;
		internal byte field_5;
		internal byte field_6;
		internal byte field_7;
		internal byte Package;
		internal byte field_9;
		internal byte field_A;
		internal byte field_B;
		internal byte VerifyingVccCurrent_dev10;
		internal byte field_D;
		internal byte VerifyingVcc_dev10;
		internal byte VerifyingVccLow_dev10;
		internal byte VerifyingVccHight_dev10;
		internal byte field_11;
		internal byte field_12;
		internal byte field_13;
		internal uint field_14;
		internal byte field_18;
		internal byte field_19;
		internal byte VerifingVccMode_IsNot0;
		internal byte field_1B;
		internal byte VCC_XilinxVCCO;
		internal byte field_1D;
		internal byte field_1E;
		internal byte field_1F;
		internal byte field_20;
		internal byte field_21;
		internal byte field_22;
		internal byte field_23;
		internal ushort NumOfPins;
		internal ushort field_26;
		internal ushort field_28;
		internal ushort field_2A;
		internal ushort field_2C;
		internal ushort field_2E;
		internal uint field_30;
		internal uint field_34;
		internal uint field_38;
		internal ushort field_3C;
		internal ushort field_3E;
		internal ushort field_40;
		internal ushort field_42;
		internal uint field_44;
		internal uint field_48;
		internal uint field_4C;
		internal uint field_50;
		internal uint field_54;
		internal uint field_58;
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
		internal char[] ProgProcedures = new char[16];
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
		internal byte[] AlgoName = new byte[16];
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 144 )]
		internal byte[] Pins = new byte[144];
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 256 )]
		internal byte[] field_10C = new byte[256];
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
		internal byte[] LcdLine1 = new byte[16];
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
		internal byte[] LcdLine2 = new byte[16];
		internal uint field_22C;
		internal uint field_230;
		internal uint field_234;
		internal uint field_238;
		internal uint field_23C;
		internal uint field_240;
		internal uint field_244;
		internal uint field_248;
		internal uint field_24C;
		internal uint field_250;
		internal uint field_254;
		internal uint field_258;
		internal uint field_25C;
		internal uint field_260;
		internal uint field_264;
		internal uint field_268;
		internal uint field_26C;
		internal uint field_270;
		internal uint field_274;
		internal uint field_278;
		internal uint field_27C;
		internal uint field_280;
		internal uint field_284;
		internal uint field_288;
		internal uint field_28C;
		internal uint field_290;
		internal uint field_294;
		internal uint field_298;

		static int _strucSize;
		static byte[] _buf;

		internal PROG_STRUC()
		{
			_strucSize = Marshal.SizeOf( typeof( PROG_STRUC ) );
			_buf = new byte[_strucSize];
		}

		internal int StrucSize
		{
			get { return _strucSize; }
		}

		internal byte[] Buf
		{
			get
			{
				IntPtr ptr = Marshal.AllocHGlobal( _strucSize );
				Marshal.StructureToPtr( this, ptr, true );
				Marshal.Copy( ptr, _buf, 0, _strucSize );
				Marshal.FreeHGlobal( ptr );

				return _buf;
			}
		}
	};


}
