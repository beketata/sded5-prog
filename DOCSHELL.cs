using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Programmer
{
	unsafe class DOCSHELL
	{
		enum DOCH_Command : byte
		{
			// I/O
			DOCH_VSCMD_READ_PARTITION = 0x82,
			DOCH_VSCMD_WRITE_PARTITION = 0x83,
			DOCH_VSCMD_WRITE_FLEXI = 0x84,

			// Sectors Operations
			DOCH_VSCMD_OPTIMIZE_PARTITION_SECTORS = 0x86,
			DOCH_VSCMD_ERASE_PARTITION_SECTORS = 0x88,

			// Hash
			DOCH_VSCMD_READ_CALCULATED_HASH = 0x8A,
			DOCH_VSCMD_WRITE_CALCULATED_HASH = 0x8B,
			DOCH_VSCMD_READ_ORIGINAL_HASH = 0x8C,
			DOCH_VSCMD_WRITE_GIVEN_HASH = 0x8D,

			// Sub-commanded commands
			DOCH_VSCMD_PARTITION_MANAGEMENT = 0xFA,
			DOCH_VSCMD_ACCESS_CONTROL = 0xFB,
			DOCH_VSCMD_EXT_DEVICE_CTRL = 0xFC,
			DOCH_VSCMD_EXT_SECURITY_CTRL = 0xFD,

			// Supported standard ATA commands
			DOCH_CMD_IDENTIFY_DEV = 0xEC,
			DOCH_DOWNLOAD_MICROCODE = 0x92
		};

		/* set of operation codes for DOCH_VSCMD_PARTITION_MANAGEMENT command */
		enum DOCH_PartitionManageOp : byte
		{
			DOCH_GET_PARTITION_INFO = 0x00,
			DOCH_SET_DEFAULT_PARTITION = 0x11,
			DOCH_SET_PARTITION_PROTECTION = 0x18,
			DOCH_GET_PARTITION_USER_ATTR = 0x70,
			DOCH_SET_PARTITION_USER_ATTR = 0x71,
			DOCH_DELETE_PARTITIONS = 0xB0,
			DOCH_ADD_PARTITION = 0xB4,
			DOCH_SECURE_ERASE = 0xB8
		};

		/* set of operation codes for DOCH_VSCMD_EXT_DEVICE_CTRL command */
		enum DOCH_DeviceCtrlOp : byte
		{
			DOCH_IDENTIFY_DISKONCHIP_DEVICE = 0x00,
			DOCH_GET_EXTENDED_DEVICE_INFO = 0x01,
			DOCH_SET_DATA_XFER_MODE = 0x10,
			DOCH_ATOMIC_WRITE_SEQUENCE = 0x20,
			DOCH_OPTIMIZE_MEDIA = 0x30,
			DOCH_GET_CUSTOM_PARAM = 0x40,
			DOCH_SET_CUSTOM_PARAM = 0x41,
			DOCH_CALIBRATE_CLOCK = 0x50,
			DOCH_GET_POWER_MODE = 0x60,
			DOCH_SET_POWER_MODE = 0x61,
			DOCH_GET_DISK_USER_ATTR = 0x70,
			DOCH_SET_DISK_USER_ATTR = 0x71,
			DOCH_GET_CONFIGURATION_DATA = 0x72,
			DOCH_SET_CONFIGURATION_DATA = 0x73,
			DOCH_ACTIVATE_DEBUG_MODE = 0x7C,
			DOCH_RETRIEVE_DBG_MSG = 0x7E,
			DOCH_SET_ALERT_LEVEL = 0x7F,
			DOCH_GET_RESET_STATUS = 0x80,
			DOCH_NOTIFY_PLATFORM_RESUMED = 0x8E,
			DOCH_NOTIFY_RESET = 0x8F
		};

		/* set of operation codes for DOCH_VSCMD_ACCESS_CONTROL command */
		enum DOCH_AccessCtrlOp : byte
		{
			DOCH_EN_ACCESS_WPWD = 0x30,
			DOCH_DISABLE_ACCESS = 0x31,

			/*Enable access with challange/response protocol*/
			DOCH_TX_HOST_PUBLICKEY = 0x32,
			DOCH_RX_DOCH_PUBLICKEY = 0x33,
      DOCH_VERIFY_HOST_KEY = 0x34,
			/*SLPP Specific*/
			DOCH_SLPP_UNLOCK_RANGE = 0x40,
			DOCH_SLPP_UNLOCK_ENTIRE_PARTITION = 0x41,
			DOCH_SLPP_LOCK_RANGE = 0x42,
			DOCH_SLPP_STICKY_LOCK_RANGE = 0x43,
			DOCH_SLPP_REPORT_LOCKED_RANGES = 0x44
		};

		static uint _currentAddrRd = 0;
		static uint _currentAddrWr = 0;
		static ushort _currentDataRd = 0;
		static ushort _currentDataWr = 0;
		//static Dictionary<uint, ushort> _currenDataRd = new Dictionary<uint, ushort>();
		//static Dictionary<uint, ushort> _currenDataWr = new Dictionary<uint, ushort>();

		static readonly bool _isDebug = false;

		static void DOCSHELL_ExPrint( string msg )
		{
			Log.L.Out( msg );

			if( msg == "\n\n" )
			{
				_currentAddrRd = 0;
				_currentAddrWr = 0;
				_currentDataRd = 0;
				_currentDataWr = 0;
				//_currenDataRd.Clear();
				//_currenDataWr.Clear();

				Log.L.Out( "Отправляем в программатор команду на снятие питания с микросхемы." );
				var cmd = new CMD_PROG() { Cmd = 1, ProgProcNum = 15 };
				bool bRes = FrmMain._main.USB_Write( 2, cmd );
				if( !bRes )
					return;

				bRes = FrmMain._main.USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
				if( !bRes )
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
				else
					Log.L.Out( "--------------- END ---------------" );
			}
		}


		static bool _isNeedToPrint = true;
		//static int _numAtaCmd = 0;
		static Dictionary<ushort, byte> _lstAtaCmd = new Dictionary<ushort, byte>();

		static void DOCSHELL_FLWRITE_IO_WORD( ushort wrd, uint addr )
		{
			bool bRes;

			if( addr == 0x080C && wrd == 0x00C0 )
			{

			}

			string sAddr = addr.ToString( "X04" );
			string sWrd = wrd.ToString( "X04" );

			// [080C] <- 0000
			// [080E] -> 0050
			//
			// [0804] <- 0000
			// [0806] <- 0000
			// [0808] <- 0000
			// [080A] <- 0000
			// [0802] <- DOCH_ADD_PARTITION
			// [080E] <- DOCH_VSCMD_PARTITION_MANAGEMENT

			if( addr == 0x80C )
				Log.L.Out( "" );

			if( _isDebug )
			{
				switch( addr )
				{
					case 0x802:
					case 0x804:
					case 0x806:
					case 0x808:
					case 0x80A:
					{
						_lstAtaCmd.Add( (ushort)addr, (byte)wrd );
						_isNeedToPrint = false;
					}
					break;

					case 0x80E:
					{
						_lstAtaCmd.Add( (ushort)addr, (byte)wrd );
						_isNeedToPrint = true;
					}
					break;

					default:
					{
						if( _lstAtaCmd.Count > 0 )
							_isNeedToPrint = true;
					}
					break;
				}

				if( _lstAtaCmd.Count == 0 )
				{
					if( (_currentAddrWr != addr) || (_currentDataWr != (ushort)wrd) )
						Log.L.Out( "[" + sAddr + "] <- " + sWrd );
				}
				else if( _isNeedToPrint )
				{
					var sb = new StringBuilder();

					foreach( var pair in _lstAtaCmd )
					{
						sb.Append( "[" + pair.Key.ToString( "X04" ) + "] <- " );

						if( pair.Key == 0x802 )
						{
							sWrd = pair.Value.ToString( "X02" );

							switch( _lstAtaCmd[0x80E] )
							{
								// I/O

								// DOCH_VSCMD_READ_PARTITION = 0x82,
								case 0x82:
								{
								}
								break;

								// DOCH_VSCMD_WRITE_PARTITION = 0x83,
								case 0x83:
								{
								}
								break;

								// DOCH_VSCMD_WRITE_FLEXI = 0x84,
								case 0x84:
								{
								}
								break;

								// Sectors Operations

								// DOCH_VSCMD_OPTIMIZE_PARTITION_SECTORS = 0x86,
								case 0x86:
								{
								}
								break;

								// DOCH_VSCMD_ERASE_PARTITION_SECTORS = 0x88,
								case 0x88:
								{
								}
								break;

								// Hash

								// DOCH_VSCMD_READ_CALCULATED_HASH = 0x8A,
								case 0x8A:
								{
								}
								break;

								// DOCH_VSCMD_WRITE_CALCULATED_HASH = 0x8B,
								case 0x8B:
								{
								}
								break;

								// DOCH_VSCMD_READ_ORIGINAL_HASH = 0x8C,
								case 0x8C:
								{
								}
								break;

								// DOCH_VSCMD_WRITE_GIVEN_HASH = 0x8D,
								case 0x8D:
								{
								}
								break;

								// Sub-commanded commands

								// DOCH_VSCMD_PARTITION_MANAGEMENT = 0xFA,
								case 0xFA:
								{
									sWrd = Enum.GetName( typeof( DOCH_PartitionManageOp ), pair.Value );
								}
								break;

								// DOCH_VSCMD_ACCESS_CONTROL = 0xFB,
								case 0xFB:
								{
									sWrd = Enum.GetName( typeof( DOCH_AccessCtrlOp ), pair.Value );
								}
								break;

								// DOCH_VSCMD_EXT_DEVICE_CTRL = 0xFC,
								case 0xFC:
								{
									sWrd = Enum.GetName( typeof( DOCH_DeviceCtrlOp ), pair.Value );
								}
								break;

								// DOCH_VSCMD_EXT_SECURITY_CTRL = 0xFD,
								case 0xFD:
								{
								}
								break;

								// Supported standard ATA commands

								//DOCH_CMD_IDENTIFY_DEV = 0xEC,
								case 0xEC:
								{
								}
								break;

								//DOCH_DOWNLOAD_MICROCODE = 0x92
								case 0x92:
								{
								}
								break;
							}

							sb.Append( sWrd + "\n" );
						}
						else if( pair.Key == 0x80E )
						{
							sb.Append( Enum.GetName( typeof( DOCH_Command ), pair.Value ) );
						}
						else
						{
							sb.Append( pair.Value.ToString( "X02" ) + "\n" );
						}
					}

					_lstAtaCmd.Clear();

					Log.L.Out( sb.ToString() );

					_isNeedToPrint = false;
				}

				_currentAddrWr = addr;
				_currentDataWr = (ushort)wrd;

				_currentAddrRd = 0;
			}

			var cmd = new CMD_PROG()
			{
				Cmd = 1,
				ProgProcNum = 1,
				BaseAddr = addr,
				Size = wrd
			};

			//Log.L.Out( "Отправляем в программатор команду на запись одного слова." );
			bRes = FrmMain._main.USB_Write( 2, cmd );
			if( !bRes )
				return;

			bRes = FrmMain._main.USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не ответил на запрос.\n" );
			}
		}

		static uint DOCSHELL_FLREAD_IO_WORD( uint addr )
		{
			bool bRes = false;

			var cmd = new CMD_PROG()
			{
				Cmd = 1,
				ProgProcNum = 2,
				BaseAddr = addr
			};

			//Log.L.Out( "Отправляем в программатор команду на чтение одного слова." );
			bRes = FrmMain._main.USB_Write( 2, cmd );
			if( !bRes )
				return 0;

			bRes = FrmMain._main.USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не ответил на запрос.\n" );
				return 0;
			}

			uint wrd = (uint)( FrmMain._main._buf_USB_ReadPipe[2] + ( (ushort)FrmMain._main._buf_USB_ReadPipe[3] << 8 ) );

			if( _isDebug )
			{
				if( (_currentAddrRd != addr) || (_currentDataRd != (ushort)wrd) )
					Log.L.In( "[" + addr.ToString( "X04" ) + "] -> " + wrd.ToString( "X04" ) );

				_currentAddrRd = addr;
				_currentDataRd = (ushort)wrd;

				_currentAddrWr = 0;
			}

			return wrd;
		}

		static string DOCSHELL_BUFFER_ID()
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_BUFFER_ID()" );

			return "sded5_formatted.img";//	"PG4UWBfr1_4DIMG_WTRG0.img";
		}

		static BinaryReader _br = null;
		//static int _iGetDeviceAccess = 0;
		static uint DOCSHELL_Get_DeviceAccess()
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_Get_DeviceAccess()" );

			/*
			if( _iGetDeviceAccess++ == 0 )
			{
				string fName = DOCSHELL_Get_PathFromLoadedFile() + "\\" + DOCSHELL_BUFFER_ID();
				_br = new BinaryReader( File.Open( fName, FileMode.Open ) );
			}

			return 1;
			*/

			return 0;
		}

		static string DOCSHELL_Get_PathFromLoadedFile()
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_Get_PathFromLoadedFile()" );

			return @"D:\9";
		}

		static uint DOCSHELL_Get_TotalCapacityDeviceFromRT()
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_Get_TotalCapacityDeviceFromRT()" );

			return 0x00100000;	// SDED5-512M
		}

		static Int64 DOCSHELL_Get_file_size()
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_Get_file_size()" );

			string fName = DOCSHELL_Get_PathFromLoadedFile() + "\\" + DOCSHELL_BUFFER_ID();
			return ( new FileInfo( fName ) ).Length;
		}

		static void DOCSHELL_PrgBarDevice( string msg, int a2, int a3, int a4, int a5 )
		{
			Log.L.In( msg );
		}

		static int DOCSHELL_Read_Block_From_Buffer( int a1, int offset, int a3, IntPtr pBuf, int size )
		{
			if( _isDebug )
				Log.L.In( "DOCSHELL_Read_Block_From_Buffer()" );

			if( _br == null )
				return 0;

			if( a3 != 0 )
			{
			}

			_br.BaseStream.Position = offset;
			var bufRead = _br.ReadBytes( size );

			fixed ( byte* buf = bufRead )
			{
				System.Buffer.MemoryCopy( buf, pBuf.ToPointer(), size, size );
			}

			return 1;
		}

		static void DOCSHELL_Write_Block_To_Buffer()
		{
		}


		static char GetPrintable( byte bt )
		{
			if( bt >= 0x20 && bt <= 127 )
				return (char)bt;

			return '.';
		}

		static string GetHex( byte[] buf )
		{
			var sb = new StringBuilder();
			var sbText = new StringBuilder();

			for( int i=0; i<buf.Length; i++ )
			{
				sbText.Append( GetPrintable( buf[i] ) );

				if( i%16 == 0 )
					sb.Append( i.ToString( "X8" ) + "  " );

				sb.Append( buf[i].ToString( "X2" ) + " " );

				if( i%16 == 15 )
				{
					sb.Append( " " + sbText.ToString() + " \n" );
					sbText.Clear();
				}
			}

			return sb.ToString();
		}

		static uint DOCSHELL_hal_blk_read_nor( uint addr, uint a2, uint a3, IntPtr pBuf, uint blocks, uint a6, uint a7 )
		{
			bool bRes;

			_currentAddrRd = 0;
			_currentAddrWr = 0;

			byte[] bufRead = new byte[512*blocks];

			if( addr != 0x0800 || a2 != 0 || a3 != 0x1000 || a6 != 1 || a7 != 7 )
				addr = 0x0800;

			if( _isDebug )
			{
				Log.L.In( "DOCSHELL_hal_blk_read_nor( " +
				addr.ToString( "X8" ) + ", " +
				a2.ToString( "X8" ) + ", " +
				a3.ToString( "X8" ) + ", " +
				pBuf.ToInt32().ToString( "X8" ) + ", " +
				blocks.ToString( "X8" ) + ", " +
				a6.ToString( "X8" ) + ", " +
				a7.ToString( "X8" ) +
				" )" );
			}
			
			var cmd = new CMD_PROG()
			{
				Cmd = 1,
				ProgProcNum = 4,
				BaseAddr = addr,
				Size = (int)blocks
			};

			bRes = FrmMain._main.USB_Write( 2, cmd );
			if( !bRes )
				return 0;

			for( uint i = 0; i<blocks; i++ )
			{
				bRes = FrmMain._main.USB_ReadPipe( 1 );
				if( !bRes )
				{
					Log.L.OutError( "Программатор не прислал очередной блок данных.\n" );
					return 0;
				}

				Array.Copy( FrmMain._main._buf_USB_ReadPipe, 0, bufRead, 512*i, 512 );
			}

			bRes = FrmMain._main.USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не подтвердил передачу сектора.\n" );
				return 0;
			}

			fixed ( byte* buf = bufRead )
			{
				IntPtr ptr = (IntPtr)buf;
				System.Buffer.MemoryCopy( ptr.ToPointer(), pBuf.ToPointer(), 512*blocks, 512*blocks );
			}

			if( _isDebug )
				Log.L.In( GetHex( bufRead ) );

			return 0;
		}

		static uint DOCSHELL_hal_blk_write_nor( uint addr, uint a2, uint a3, IntPtr pBuf, uint blocks, uint a6, uint a7 )
		{
			bool bRes;

			_currentAddrRd = 0;
			_currentAddrWr = 0;

			byte[] buf = new byte[512];
			byte[] bufWrite = new byte[512*blocks];

			fixed ( byte* pBufWrite = bufWrite )
			{
				System.Buffer.MemoryCopy( pBuf.ToPointer(), pBufWrite, 512*blocks, 512*blocks );
			}

			if( addr != 0x0800 || a2 != 0 || a3 != 0x1000 || a6 != 1 || a7 != 7 )
				addr = 0x0800;

			if( _isDebug )
			{
				Log.L.Out( "DOCSHELL_hal_blk_write_nor( " +
				addr.ToString( "X8" ) + ", " +
				a2.ToString( "X8" ) + ", " +
				a3.ToString( "X8" ) + ", " +
				pBuf.ToInt32().ToString( "X8" ) + ", " +
				blocks.ToString( "X8" ) + ", " +
				a6.ToString( "X8" ) + ", " +
				a7.ToString( "X8" ) +
				" )" );

				Log.L.Out( GetHex( bufWrite ) );
			}

			var cmd = new CMD_PROG()
			{
				Cmd = 1,
				ProgProcNum = 3,
				BaseAddr = addr,
				Size = (int)blocks
			};

			bRes = FrmMain._main.USB_Write( 2, cmd );
			if( !bRes )
				return 0;

			for( uint i=0; i<blocks; i++ )
			{
				Array.Copy( bufWrite, 512*i, buf, 0, 512 );

				bRes = FrmMain._main.USB_Write( 0, buf, 512 );
				if( !bRes )
				{
					Log.L.OutError( "Не удалось отправить в программатор очередной сектор.\n" );
					return 0;
				}
			}

			bRes = FrmMain._main.USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не подтвердил прием сектора.\n" );
				return 0;
			}

			return 0;
		}


		static internal bool Setup()
		{
			bool bRes = false;

			do
			{
				//if( ExSetAccessRapperFileName( "" ) != 0 )
				//	break;

				//if( ExSetOSRapperFileName( "" ) != 0 )
				//	break;

				dynamic fp = new _DOCSHELL_ExPrint( DOCSHELL_ExPrint );
				GCHandle gch = GCHandle.Alloc( fp );
				IntPtr ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				if( !Set_ELNEC_ExPrint( ip ) )
					break;

				fp = new _DOCSHELL_FLWRITE_IO_WORD( DOCSHELL_FLWRITE_IO_WORD );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				if( !Set_ELNEC_FLWRITE_IO_WORD( ip ) )
					break;

				fp = new _DOCSHELL_FLREAD_IO_WORD( DOCSHELL_FLREAD_IO_WORD );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				if( !Set_ELNEC_FLREAD_IO_WORD( ip ) )
					break;

				fp = new _DOCSHELL_Get_DOCSHELL_BUFFER_ID( DOCSHELL_BUFFER_ID );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Get_DOCSHELL_BUFFER_ID( ip );

				fp = new _DOCSHELL_Get_DeviceAccess( DOCSHELL_Get_DeviceAccess );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Get_DeviceAccess( ip );

				fp = new _DOCSHELL_Get_PathFromLoadedFile( DOCSHELL_Get_PathFromLoadedFile );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Get_PathFromLoadedFile( ip );

				fp = new _DOCSHELL_Get_TotalCapacityDeviceFromRT( DOCSHELL_Get_TotalCapacityDeviceFromRT );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Get_TotalCapacityDeviceFromRT( ip );

				fp = new _DOCSHELL_Get_file_size( DOCSHELL_Get_file_size );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Get_file_size( ip );

				fp = new _DOCSHELL_PrgBarDevice( DOCSHELL_PrgBarDevice );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_PrgBarDevice( ip );

				fp = new _DOCSHELL_Read_Block_From_Buffer( DOCSHELL_Read_Block_From_Buffer );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Read_Block_From_Buffer( ip );

				fp = new _DOCSHELL_Write_Block_To_Buffer( DOCSHELL_Write_Block_To_Buffer );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_Write_Block_To_Buffer( ip );

				fp = new _DOCSHELL_hal_blk_read_nor( DOCSHELL_hal_blk_read_nor );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_hal_blk_read_nor( ip );

				fp = new _DOCSHELL_hal_blk_write_nor( DOCSHELL_hal_blk_write_nor );
				gch = GCHandle.Alloc( fp );
				ip = Marshal.GetFunctionPointerForDelegate( fp );
				GC.Collect();                      // force garbage collection cycle to prove that the delegate doesn't get disposed  
				Set_ELNEC_hal_blk_write_nor( ip );


				bRes = true;
			}
			while( false );

			if( !bRes )
			{

			}

			return bRes;
		}


		static bool _isSetup = false;
		static public void Cmd( string cmd )
		{
			if( !_isSetup )
			{
				_isSetup = true;
				Setup();
			}

			Log.L.Out( cmd );

			mainEntry( cmd );
		}

		// ExSetAccessRapperFileName
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		static extern int ExSetAccessRapperFileName( [MarshalAs( UnmanagedType.LPStr )] string fname );

		// ExSetOSRapperFileName
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		static extern int ExSetOSRapperFileName( [MarshalAs( UnmanagedType.LPStr )] string fname );

		//  Set_ELNEC_ExPrint
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_ExPrint( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate void _DOCSHELL_ExPrint( string cmd );

		//  Set_ELNEC_FLREAD_IO_WORD
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_FLREAD_IO_WORD( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		//[return: MarshalAs( UnmanagedType.U2 )]
		delegate uint _DOCSHELL_FLREAD_IO_WORD( uint addr );

		// Set_ELNEC_FLWRITE_IO_WORD
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_FLWRITE_IO_WORD( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate void _DOCSHELL_FLWRITE_IO_WORD( ushort wrd, uint addr );

		// Set_ELNEC_Get_DOCSHELL_BUFFER_ID
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Get_DOCSHELL_BUFFER_ID( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		[return: MarshalAs( UnmanagedType.LPStr )]
		delegate string _DOCSHELL_Get_DOCSHELL_BUFFER_ID();

		// Set_ELNEC_Get_DeviceAccess
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Get_DeviceAccess( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate uint _DOCSHELL_Get_DeviceAccess();

		// Set_ELNEC_Get_PathFromLoadedFile
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Get_PathFromLoadedFile( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		[return: MarshalAs( UnmanagedType.LPStr )]
		delegate string _DOCSHELL_Get_PathFromLoadedFile();

		// Set_ELNEC_Get_TotalCapacityDeviceFromRT
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Get_TotalCapacityDeviceFromRT( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate uint _DOCSHELL_Get_TotalCapacityDeviceFromRT();

		// Set_ELNEC_Get_file_size
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Get_file_size( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate Int64 _DOCSHELL_Get_file_size();

		// Set_ELNEC_PrgBarDevice
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_PrgBarDevice( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate void _DOCSHELL_PrgBarDevice( [MarshalAs( UnmanagedType.LPStr )] string msg, int a2, int a3, int a4, int a5 );

		// Set_ELNEC_Read_Block_From_Buffer
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Read_Block_From_Buffer( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate int _DOCSHELL_Read_Block_From_Buffer( int a1, int a2, int a3, IntPtr pBuf, int size );

		// Set_ELNEC_Write_Block_To_Buffer
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_Write_Block_To_Buffer( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate void _DOCSHELL_Write_Block_To_Buffer();

		// Set_ELNEC_hal_blk_read_nor
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_hal_blk_read_nor( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate uint _DOCSHELL_hal_blk_read_nor( uint a1, uint a2, uint a3, IntPtr pBuf, uint a5, uint a6, uint a7 );

		// Set_ELNEC_hal_blk_write_nor
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool Set_ELNEC_hal_blk_write_nor( IntPtr pFn );

		[UnmanagedFunctionPointer( CallingConvention.StdCall )]
		delegate uint _DOCSHELL_hal_blk_write_nor( uint a1, uint a2, uint a3, IntPtr pBuf, uint a5, uint a6, uint a7 );

		// mainEntry
		//
		[DllImport( "DOCSHELL.dll", CallingConvention = CallingConvention.Cdecl )]
		[return: MarshalAs( UnmanagedType.I4 )]
		static extern int mainEntry( [MarshalAs(UnmanagedType.LPStr )] string cmd );
	}
}
