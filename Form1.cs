using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace Programmer
{
	public partial class FrmMain : Form
	{
		internal static FrmMain _main;
		Mutex _mutex;
		const string _mutexName = "{8C7D6D9F-577A-4313-B8EB-1C9058435DCE}";
		const string _usbDeviceID = @"\\?\USB#VID_6022&PID_5000";

		Encoding _enc = Encoding.GetEncoding( "iso-8859-1" );

		internal SynchronizationContext _sc;

		SafeFileHandle _USB_Handle = null;
		SafeFileHandle[] _USB_Handles = new SafeFileHandle[4];


		public FrmMain()
		{
			_main = this;
			_sc = SynchronizationContext.Current;

			// Проверяем, не запущена ли другая копия этой программы
			//
			try
			{
				_mutex = Mutex.OpenExisting( _mutexName );

				ExitDelay( 5000 );
				MessageBox.Show(
					"Другая копия программы уже работает на этом компьютере !",
					"Programmer",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information );

				Environment.Exit( 0 );
			}
			catch( WaitHandleCannotBeOpenedException )
			{
				_mutex = new Mutex( true, _mutexName );
			}

			this.SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true );

			InitializeComponent();

			Task.Factory.StartNew( (Action)USB_PortOpen_ );

			//BackgroundWorker bw = new BackgroundWorker();
			//bw.DoWork += USB_ReadPipe3;
			//bw.RunWorkerAsync();

			//dINFOToolStripMenuItem_Click( null, null );
		}

		void ExitDelay( int msDel )
		{
			Task.Factory.StartNew( () =>
			{
				Thread.Sleep( msDel );// Task.Delay( msDel );
				Environment.Exit( 0 );
			} );
		}


		/// <summary>
		/// Override called when the window handle has been created.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnHandleCreated( EventArgs e )
		{
			// Подписываем наш обработчик оконных сообщений на получение сообщений
			// DBT_DEVICEARRIVAL и DBT_DEVICEREMOVECOMPLETE
			//
			Win32.DEV_BROADCAST_DEVICEINTERFACE oInterfaceIn = new Win32.DEV_BROADCAST_DEVICEINTERFACE();
			oInterfaceIn.dbcc_size = Marshal.SizeOf( oInterfaceIn );
			oInterfaceIn.dbcc_classguid = Win32.GUID_DEVINTERFACE_USB_DEVICE;
			oInterfaceIn.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
			Win32.RegisterDeviceNotification( this.Handle, oInterfaceIn, Win32.DEVICE_NOTIFY_WINDOW_HANDLE );

			base.OnHandleCreated( e );
		}

		// [System.Security.Permissions.PermissionSet( System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust" )]
		protected override void WndProc( ref Message m )
		{
			base.WndProc( ref m );

			int wParam = (int)m.WParam;

			// Обрабатываем событие при изменении статуса USB устройств.
			//
			if( m.Msg == Win32.WM_DEVICECHANGE )
			{
				if( ( wParam == Win32.DBT_DEVICEARRIVAL ) || ( wParam == Win32.DBT_DEVICEREMOVECOMPLETE ) )
				{
					// Read the dhdr.dbcc_devicetype
					//
					Win32.DEV_BROADCAST_HDR devPtr = (Win32.DEV_BROADCAST_HDR)Marshal.PtrToStructure( m.LParam, typeof( Win32.DEV_BROADCAST_HDR ) );
					if( devPtr.dbcc_devicetype == Win32.DBT_DEVTYP_DEVICEINTERFACE )
					{
						Win32.DEV_BROADCAST_DEVICEINTERFACE dip = (Win32.DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure( m.LParam, typeof( Win32.DEV_BROADCAST_DEVICEINTERFACE ) );
						string sDevName = dip.dbcc_name;
						if( sDevName.ToUpper().StartsWith( _usbDeviceID ) ) // Если это SuperPro 6100.
						{
							if( wParam == Win32.DBT_DEVICEARRIVAL )
							{
								USB_PortOpen_();
							}
							else if( wParam == Win32.DBT_DEVICEREMOVECOMPLETE )
							{
								USB_CloseFiles();
								USB_InfoToggle( false );
							}
						}
					}
				}
			}
		}

		private void txtBox_KeyPress( object sender, KeyPressEventArgs e )
		{
			if( e.KeyChar == '\r' )
			{
				var cmd = txtBox.Lines[txtBox.Lines.Length - 2].Trim();
				if( cmd == "" )
					return;

				
			}
		}

		void USB_InfoToggle( bool isConnected )
		{
			_sc.Post( delegate
			{
				progBarOne.Value = 0;
				progBarOne.Step = 1;
				progBarOne.Maximum = 1024;
				//progBar.Value = 200;

				menuStrip1.Enabled = isConnected;

				if( isConnected )
				{
					txtInfo.BackColor = Color.Moccasin;
					txtInfo.Text = "USB is Connected";
				}
				else
				{
					txtInfo.BackColor = Color.Pink;
					txtInfo.Text = "USB is NOT Connected";
				}
			},
			null );
		}

		private void Form1_Shown( object sender, EventArgs e )
		{
		}

		private void testToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// Проверяем тип адаптера
			//
			if( !CheckAdaptor( "DX1011" ) )
				return;

			// В зависимости от выбранной микросхемы, загружаем соответствующий алгоритм и прошивку для XILINX.
			//
			/*
			if( !SendAlgo( @"c:\Program Files (x86)\SP6100\algo1\Sram_t_1_dec.usb" ) )
			{
			}
			*/

			// В зависимости от выбранной микросхемы, загружаем заполненную структуру.
			//
			if( !SendProgStruc( ref _pinsSDED5 ) )
				return;
			/*
			Task.Factory.StartNew( () =>
			{
				while( true )
				{
					Thread.Sleep( 200 );
					CheckInsertIC();
				}
			} );
			*/

			if( !CheckInsertIC( ref _pinsSDED5 ) )
				return;

			IC_Read();
		}


		bool IC_Read()
		{
			bool bRes = false;

			progBarOne.Value = 0;
			progBarOne.Step = 1;

			progBarAll.Value = 0;
			progBarAll.Step = 1;

			var cmd = new CMD_PROG()
			{
				Cmd = 1,
				ProgProcNum = 0
			};

			Log.L.Out( "Отправляем в программатор команду на чтение." );
			bRes = USB_Write( 2, cmd );
			if( !bRes )
				return false;

			int bufPtr = 0;
			uint blockSize = 0;
			ushort blocks = 0;
			ushort blockNum = 0;

			do
			{
				progBarOne.Value = 0;

				bRes = USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
				if( !bRes )
				{
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
					return false;
				}

				if( !(_buf_USB_ReadPipe[0] == 5 && _buf_USB_ReadPipe[1] == 0) )
				{
					Log.L.InError( "Программатор прислал неизвестный запрос \"" + _buf_USB_ReadPipe[0] + "\".\n" );
					return false;
				}

				blockNum = (ushort)(_buf_USB_ReadPipe[2] + ((ushort)_buf_USB_ReadPipe[3] << 8));
				blocks = (ushort)(_buf_USB_ReadPipe[4] + ((ushort)_buf_USB_ReadPipe[5] << 8));
				blockSize = (uint)(_buf_USB_ReadPipe[8] + ((uint)_buf_USB_ReadPipe[9] << 8) + ((uint)_buf_USB_ReadPipe[10] << 16) + ((uint)_buf_USB_ReadPipe[11] << 24));
				bufPtr = 0;

				progBarAll.Maximum = blocks;
				progBarOne.Maximum = (int)blockSize;

				using( BinaryWriter bw = new BinaryWriter( File.Open( @"d:\9\SDED5_" + blockNum + ".bin", FileMode.Create, FileAccess.ReadWrite, FileShare.Read ) ) )
				{
					do
					{
						bRes = USB_ReadPipe( 1 );
						if( !bRes )
						{
							Log.L.OutError( "Программатор не прислал очередной блок данных.\n" );
							return false;
						}

						progBarOne.PerformStep();

						Application.DoEvents();

						bw.Write( _buf_USB_ReadPipe );
						bw.Flush();

						bufPtr++;
					}
					while( bufPtr < blockSize );
				}

				progBarAll.PerformStep();
			}
			while( ( blockNum + 1 ) < blocks );

			bRes = USB_ReadPipe( 3 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не подтвердил передачу всего буфера чтения.\n" );
				return false;
			}

			Log.L.Out( "Микросхема успешно прочитана.\n" );

			return bRes;
		}


		internal class PIN
		{
			internal string Name;
			internal byte PinType;
		}

		Dictionary<int, PIN> _pinsROM32 = new Dictionary<int, PIN>()
		{
			{   8, new PIN() { Name = "1", PinType = 1 } },
			{   9, new PIN() { Name = "2", PinType = 1 } },
			{  10, new PIN() { Name = "3", PinType = 1 } },
			{  11, new PIN() { Name = "4", PinType = 1 } },
			{  12, new PIN() { Name = "5", PinType = 1 } },
			{  13, new PIN() { Name = "6", PinType = 1 } },
			{  14, new PIN() { Name = "7", PinType = 1 } },
			{  15, new PIN() { Name = "8", PinType = 1 } },
			{  16, new PIN() { Name = "9", PinType = 1 } },
			{  17, new PIN() { Name = "10", PinType = 1 } },
			{  18, new PIN() { Name = "11", PinType = 1 } },
			{  19, new PIN() { Name = "12", PinType = 1 } },
			{  20, new PIN() { Name = "13", PinType = 1 } },
			{  21, new PIN() { Name = "14", PinType = 1 } },
			{  22, new PIN() { Name = "15", PinType = 1 } },
			{  23, new PIN() { Name = "16", PinType = 2 } },
			{  24, new PIN() { Name = "17", PinType = 1 } },
			{  25, new PIN() { Name = "18", PinType = 1 } },
			{  26, new PIN() { Name = "19", PinType = 1 } },
			{  27, new PIN() { Name = "20", PinType = 1 } },
			{  28, new PIN() { Name = "21", PinType = 1 } },
			{  29, new PIN() { Name = "22", PinType = 1 } },
			{  30, new PIN() { Name = "23", PinType = 1 } },
			{  31, new PIN() { Name = "24", PinType = 1 } },
			{  32, new PIN() { Name = "25", PinType = 1 } },
			{  33, new PIN() { Name = "26", PinType = 1 } },
			{  34, new PIN() { Name = "27", PinType = 1 } },
			{  35, new PIN() { Name = "28", PinType = 1 } },
			{  36, new PIN() { Name = "29", PinType = 1 } },
			{  37, new PIN() { Name = "30", PinType = 1 } },
			{  38, new PIN() { Name = "31", PinType = 1 } },
			{  39, new PIN() { Name = "32", PinType = 1 } },
		};

		Dictionary<int, PIN> _pinsSDED5 = new Dictionary<int, PIN>()
		{
			{   4, new PIN() { Name = "E1", PinType = 1 } },
			{  18, new PIN() { Name = "E2", PinType = 1 } },
			{  19, new PIN() { Name = "F2", PinType = 1 } },
			{  20, new PIN() { Name = "G2", PinType = 1 } },
			{  21, new PIN() { Name = "H2", PinType = 1 } },
			{  22, new PIN() { Name = "J2", PinType = 1 } },
			{  31, new PIN() { Name = "D3", PinType = 1 } },
			{  32, new PIN() { Name = "E3", PinType = 1 } },
			{  33, new PIN() { Name = "F3", PinType = 1 } },
			{  34, new PIN() { Name = "G3", PinType = 1 } },
			{  35, new PIN() { Name = "H3", PinType = 2 } },	// GND
			{  36, new PIN() { Name = "J3", PinType = 1 } },
			{  37, new PIN() { Name = "K3", PinType = 1 } },
			{  38, new PIN() { Name = "L3", PinType = 1 } },
			{  45, new PIN() { Name = "D4", PinType = 2 } },	// GND
			{  46, new PIN() { Name = "E4", PinType = 1 } },
			{  47, new PIN() { Name = "F4", PinType = 1 } },
			{  48, new PIN() { Name = "G4", PinType = 1 } },
			{  49, new PIN() { Name = "H4", PinType = 1 } },
			{  50, new PIN() { Name = "J4", PinType = 1 } },
			{  51, new PIN() { Name = "K4", PinType = 1 } },
			{  52, new PIN() { Name = "L4", PinType = 1 } },
			{  59, new PIN() { Name = "D5", PinType = 1 } },
			{  60, new PIN() { Name = "E5", PinType = 1 } },
			{  61, new PIN() { Name = "F5", PinType = 1 } },
			{  64, new PIN() { Name = "J5", PinType = 1 } },
			{  65, new PIN() { Name = "K5", PinType = 1 } },
			{  66, new PIN() { Name = "L5", PinType = 1 } },
			{  73, new PIN() { Name = "L6", PinType = 1 } },
			{  74, new PIN() { Name = "E6", PinType = 1 } },
			{  75, new PIN() { Name = "F6", PinType = 1 } },
			{  78, new PIN() { Name = "J6", PinType = 1 } },
			{  79, new PIN() { Name = "K6", PinType = 1 } },
			{  80, new PIN() { Name = "L6", PinType = 1 } },
			{  87, new PIN() { Name = "D7", PinType = 1 } },
			{  88, new PIN() { Name = "E7", PinType = 1 } },
			{  89, new PIN() { Name = "F7", PinType = 1 } },
			{  90, new PIN() { Name = "G7", PinType = 1 } },
			{  91, new PIN() { Name = "H7", PinType = 1 } },
			{  92, new PIN() { Name = "J7", PinType = 1 } },
			{  93, new PIN() { Name = "K7", PinType = 1 } },
			{  94, new PIN() { Name = "L7", PinType = 1 } },
			{ 101, new PIN() { Name = "D8", PinType = 1 } },
			{ 102, new PIN() { Name = "E8", PinType = 1 } },
			{ 103, new PIN() { Name = "F8", PinType = 1 } },
			{ 104, new PIN() { Name = "G8", PinType = 1 } },
			{ 105, new PIN() { Name = "H8", PinType = 1 } },
			{ 106, new PIN() { Name = "J8", PinType = 1 } },
			{ 107, new PIN() { Name = "K8", PinType = 1 } },
			{ 108, new PIN() { Name = "L8", PinType = 1 } },
			{ 118, new PIN() { Name = "G9", PinType = 1 } },
			{ 119, new PIN() { Name = "H9", PinType = 2 } },	// GND
			{ 120, new PIN() { Name = "J9", PinType = 1 } },
			{ 121, new PIN() { Name = "K9", PinType = 2 } },	// GND
			{ 130, new PIN() { Name = "E10", PinType = 1 } },
			{ 131, new PIN() { Name = "F10", PinType = 1 } },
			{ 132, new PIN() { Name = "G10", PinType = 1 } },
			{ 133, new PIN() { Name = "H10", PinType = 1 } },
			{ 134, new PIN() { Name = "J10", PinType = 1 } },
			{ 135, new PIN() { Name = "K10", PinType = 1 } }
		};

		bool SendProgStruc( ref Dictionary<int, PIN> pinsIC )
		{
			bool bRes = false;

			var progStruc = new PROG_STRUC();
			progStruc.NumOfPins = 140;

			for( int i=0; i<140; i++ )
			{
				if( pinsIC.ContainsKey( i ) )
					progStruc.Pins[i] = pinsIC[i].PinType;
			}

			var cmd = new CMD_PROG()
			{
				Cmd = 11,
				Size = progStruc.StrucSize
			};

			Log.L.Out( "Отправляем в программатор команду на прием от нас структуры PROG_STRUC." );
			bRes = USB_Write( 2, cmd );
			if( !bRes )
				return bRes;

			Log.L.Out( "Отправляем в программатор структуру PROG_STRUC." );
			bRes = USB_Write( 0, progStruc.Buf, progStruc.StrucSize );
			if( !bRes )
				return bRes;

			bRes = USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не ответил на запрос.\n" );
				return bRes;
			}

			if( !( _buf_USB_ReadPipe[0] == 3 && _buf_USB_ReadPipe[1] == 0 ) )
			{
				Log.L.OutError( "Программатор сообщил об ошибке \"" + _buf_USB_ReadPipe[0] + "\" при приеме структуры PROG_STRUC.\n" );
				return false;
			}

			if( bRes )
				Log.L.In( "Программатор подтвердил получение структуры PROG_STRUC.\n" );

			return bRes;
		}


		bool CheckInsertIC( ref Dictionary<int, PIN> pinsIC )
		{
			bool bRes = false;

			var cmd = new ushort[1] { 8 };

			Log.L.Out( "Отправляем в программатор команду на проверку контакта ножек микросхемы с ножками сокеты." );
			bRes = USB_Write( 2, cmd, 2 );
			if( !bRes )
				return false;

			int i = 0;
			while( i++ < 3 )
			{
				bRes = USB_ReadPipe( 3 ); //	_ev_USB_ReadPipe3.WaitOne( 5000 );
				if( !bRes )
				{
					Log.L.OutError( "Программатор не запросил прошивку XILINX #" + (i+1) + ".\n" );
					return bRes;
				}

				if( _buf_USB_ReadPipe[1] == 0 && _buf_USB_ReadPipe[2] == 0 && _buf_USB_ReadPipe[3] == 0 )
				{
					switch( _buf_USB_ReadPipe[0] )
					{
						// Программатор запрашивает прошивку XILINX "GENERAL_.BIN" с pull-down
						//
						case 17:
							{
								Log.L.In( "Программатор запрашивает прошивку XILINX \"GENERAL_.BIN\" с pull-down." );
								SendLibFile( @"c:\Program Files (x86)\SP6100\lib\GENERAL_.BIN" );
							}
							break;

						// Программатор запрашивает прошивку XILINX "GENERAL~.BIN" с pull-up
						//
						case 18:
							{
								Log.L.In( "Программатор запрашивает прошивку XILINX \"GENERAL~.BIN\" с pull-up." );
								SendLibFile( @"c:\Program Files (x86)\SP6100\lib\GENERAL~.BIN" );
							}
							break;

						// Программатор запрашивает прошивку XILINX для конкретной микросхемы.
						// В некоторых простых случаях это "GENERAL~.BIN" с pull-up
						//
						case 19:
							{
								Log.L.In( "Программатор запрашивает прошивку XILINX для конкретной микросхемы." );
								//SendLibFile( @"c:\Program Files (x86)\SP6100\lib\DOC_H3.BIN" );
								SendLibFile( @"d:\xeltek.bin" );
						}
						break;

						default:
							{
								bRes = false;
							}
							break;
					}
				}
				else
				{
					bRes = false;
				}

				if( !bRes )
				{
					Log.L.OutError( "Программатор прислал неизвестный запрос \"" + _buf_USB_ReadPipe[0] + "\" на прошивку XILINX.\n" );
					return false;
				}
			}

			bRes = USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не cообщил о результате проверки.\n" );
				return bRes;
			}

			if( _buf_USB_ReadPipe[1] == 0 && _buf_USB_ReadPipe[2] == 0 && _buf_USB_ReadPipe[3] == 0 )
			{
				if( _buf_USB_ReadPipe[0] == 20 )
				{
					Log.L.In( "Программатор подтвердил контакт всех ножек микросхемы.\n" );
				}
				else if( _buf_USB_ReadPipe[0] == 21 )
				{
					byte[] bufPins = new byte[144];

					for( i=0; i<3; i++ )
					{
						bRes = USB_ReadPipe( 3 ); //	_ev_USB_ReadPipe3.WaitOne( 5000 );
						if( !bRes )
						{
							Log.L.OutError( "Программатор не прислал буфер #" + (i+1) + " с результатом проверки.\n" );
							return bRes;
						}

						Array.Copy( _buf_USB_ReadPipe, 0, bufPins, i*64, ((i<2) ? (64) : (16)) );
					}

					bool isEmpty = true;
					for( i=0; i<bufPins.Length; i++ )
					{
						if( pinsIC.ContainsKey( i ) && bufPins[i] == 1 )
						{
							isEmpty = false;
							break;
						}
					}

					if( isEmpty )
					{
						Log.L.In( "Программатор сообщает об отсутствии микросхемы в сокете.\n" );
						return false;
					}

					Log.L.In( "Программатор сообщает об отсутствии контакта следующих ножек микросхемы.\n" );

					var sb = new StringBuilder();

					for( i=0; i<bufPins.Length; i++ )
					{
						if( pinsIC.ContainsKey( i ) && bufPins[i] == 0 )
						{
							sb.Append( pinsIC[i].Name + ", " );
						}
					}

					Log.L.In( sb.ToString() + "\n" );

					bRes = false;
				}
			}
			else
			{
				Log.L.OutError( "Программатор прислал неизвестный ответ \"" + _buf_USB_ReadPipe[0] + "\" по результатам проверки.\n" );
				bRes = false;
			}

			return bRes;
		}


		bool CheckAdaptor( string adaptorName )
		{
			if( adaptorName.Length != 6 )
				return false;

			bool bRes = false;

			var cmd = new ushort[1] { 20 };

			Log.L.Out( "Запрашиваем у программатора тип установленного адаптера." );
			bRes = USB_Write( 2, cmd, 2 );
			if( !bRes )
				return bRes;

			bRes = USB_ReadPipe( 3 ); // _ev_USB_ReadPipe3.WaitOne( 5000 );
			if( !bRes )
			{
				Log.L.OutError( "Программатор не ответил на запрос.\n" );
				return bRes;
			}

			var sb = new StringBuilder();
			for( int i = 0; i < 6; i++ )
			{
				sb.Append( (char)_buf_USB_ReadPipe[i] );
			}
			Log.L.In( "Программатор сообщил тип адаптера - \"" + sb.ToString() + "\"\n" );

			for( int i = 0; i < 6; i++ )
			{
				if( adaptorName[i] != _buf_USB_ReadPipe[i] )
				{
					bRes = false;
					break;
				}
			}

			return bRes;
		}

		bool SendAlgo( string algoFile )
		{
			bool bRes = false;

			bRes= SendLoaderFile();
			if( !bRes )
				return false;

			bRes= SendAlgoFile( algoFile );
			if( !bRes )
				return false;

			return true;
		}

		bool SendAlgoFile( string algoFile )
		{
			bool bRes = false;

			var cmd = new CMD_PROG();
			var buf = new byte[64];
			var algoBuf = new byte[0xFE00];

			// SRAM_T_1

			var algo = File.ReadAllBytes( algoFile );
			int lenAlgo = algo.Length;

			cmd.Cmd = 13;
			cmd.ProgProcNum = 0;
			cmd.BaseAddr = 0x20000000;
			cmd.Size = 0x2FA00;

			bRes = USB_Write( 2, cmd );

			for( int i = 0; i<3; i++ )
			{
				if( lenAlgo > 0 )
				{
					int len = ( ( lenAlgo >= algoBuf.Length ) ? ( algoBuf.Length ) : ( lenAlgo ) );
					Array.Copy( algo, i*0xFE00, algoBuf, 0, len );
					lenAlgo -= len;
				}

				bRes = USB_Write( 0, algoBuf, 0xFE00 );
				if( !bRes )
					return false;
			}

			if( bRes )
				bRes = USB_Read( 3, buf );

			if( bRes && buf[0] != 3 )
					bRes = false;

			return bRes;
		}

		bool SendLibFile( string fName )
		{
			bool bRes = false;

			var libFile = File.ReadAllBytes( fName );

			Log.L.Out( "Отправляем в программатор файл \"" + fName + "\"." );
			bRes = USB_Write( 0, libFile, libFile.Length );

			return bRes;
		}

		bool SendLoaderFile()
		{
			bool bRes = false;

			var cmd = new CMD_PROG();
			var buf = new byte[64];
			var loaderBuf = new byte[0xF800];

			// Сначала загружаем в программатор файл "\bin\loader.bin"
			//
			cmd.Cmd = 13;
			cmd.ProgProcNum = 0;
			cmd.BaseAddr = 0x20200000;
			cmd.Size = 0xF800;

			bRes = USB_Write( 2, cmd );
			if( !bRes )
				return bRes;

			var loader = File.ReadAllBytes( @"c:\Program Files (x86)\SP6100\bin\loader.bin" );
			Array.Copy( loader, loaderBuf, loader.Length );

			bRes = USB_Write( 0, loaderBuf, 0xF800 );
			if( bRes )
				bRes = USB_Read( 3, buf );

			if( bRes && buf[0] != 3 )
				bRes = false;

			return bRes;
		}

		private void dINFOToolStripMenuItem_Click( object sender, EventArgs e )
		{
			bool bRes = false;

			do
			{
				// Проверяем тип адаптера
				//
				if( !CheckAdaptor( "DX1011" ) )
					break;

				// В зависимости от выбранной микросхемы, загружаем соответствующий алгоритм и прошивку для XILINX.
				//
				//if( !SendAlgo( @"c:\Program Files (x86)\SP6100\algo1\Sram_t_1_dec.usb" ) )
				//{
				//}

				// В зависимости от выбранной микросхемы, загружаем заполненную структуру.
				//
				if( !SendProgStruc( ref _pinsSDED5 ) )
					break;
				//Task.Factory.StartNew( () =>
				//{
				//	while( true )
				//	{
				//		Thread.Sleep( 200 );
				//		CheckInsertIC();
				//	}
				//} );

				if( !CheckInsertIC( ref _pinsSDED5 ) )
					break;

				Log.L.Out( "Отправляем в программатор команду на подачу питания на микросхему." );
				var cmd = new CMD_PROG() { Cmd = 1, ProgProcNum = 14 };
				if( !FrmMain._main.USB_Write( 2, cmd ) )
					break;

				if( !FrmMain._main.USB_ReadPipe( 3 ) )
				{
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
					break;
				}

				var start = txtBox.TextLength;
				txtBox.Select( start, 0 );
				txtBox.Focus();

				//DOCSHELL.Cmd( "DIMAGE /WINSRC:00000 /FILETRG:sded5.img" );
				DOCSHELL.Cmd( "DIMAGE /FILESRC:sded5_0_.img /WINTRG:0" );
				//DOCSHELL.Cmd( "DFORMAT /WIN:00000 /BDKL0:512K /BDTLL0:1M /BDTLL1:1M /BDTLL2:1M /BDTLL3:1M" );
				//DOCSHELL.Cmd( "DFORMAT /WIN:00000 /UNFORMAT" );
				//DOCSHELL.Cmd( "DINFO /WIN:00000 /IPL" );
				//DOCSHELL.Cmd( "DINFO /WIN:00000 /BDTL" );
				//mainEntry( "" );

				bRes = true;
			}
			while( false );

			if( !bRes )
			{
				var start = txtBox.TextLength;
				txtBox.Select( start, 0 );
				txtBox.Focus();

				Log.L.Out( "Отправляем в программатор команду на снятие питания с микросхемы." );
				var cmd = new CMD_PROG() { Cmd = 1, ProgProcNum = 15 };
				bRes = FrmMain._main.USB_Write( 2, cmd );

				if( !FrmMain._main.USB_ReadPipe( 3 ) )
				{
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
				}
			}
		}

		private void txtBox2_KeyPress( object sender, KeyPressEventArgs e )
		{
			if( e.KeyChar != '\r' )
				return;

			int line = txtBox2.GetLineFromCharIndex( txtBox2.SelectionStart );
			var cmd = txtBox2.Lines[line-1];
		}

		private void txtBox2_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			int line = txtBox2.GetLineFromCharIndex( txtBox2.SelectionStart );
			var cmdLine = txtBox2.Lines[line];

			bool bRes = false;

			do
			{
				// Проверяем тип адаптера
				//
				if( !CheckAdaptor( "DX1011" ) )
					break;

				// В зависимости от выбранной микросхемы, загружаем соответствующий алгоритм и прошивку для XILINX.
				//
				//if( !SendAlgo( @"c:\Program Files (x86)\SP6100\algo1\Sram_t_1_dec.usb" ) )
				//{
				//}

				// В зависимости от выбранной микросхемы, загружаем заполненную структуру.
				//
				if( !SendProgStruc( ref _pinsSDED5 ) )
					break;
				//Task.Factory.StartNew( () =>
				//{
				//	while( true )
				//	{
				//		Thread.Sleep( 200 );
				//		CheckInsertIC();
				//	}
				//} );

				if( !CheckInsertIC( ref _pinsSDED5 ) )
					break;

				Log.L.Out( "Отправляем в программатор команду на подачу питания на микросхему." );
				var cmd = new CMD_PROG() { Cmd = 1, ProgProcNum = 14 };
				if( !FrmMain._main.USB_Write( 2, cmd ) )
					break;

				if( !FrmMain._main.USB_ReadPipe( 3 ) )
				{
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
					break;
				}

				var start = txtBox.TextLength;
				txtBox.Select( start, 0 );
				txtBox.Focus();

				DOCSHELL.Cmd( cmdLine );

				bRes = true;
			}
			while( false );

			if( !bRes )
			{
				var start = txtBox.TextLength;
				txtBox.Select( start, 0 );
				txtBox.Focus();

				Log.L.Out( "Отправляем в программатор команду на снятие питания с микросхемы." );
				var cmd = new CMD_PROG() { Cmd = 1, ProgProcNum = 15 };
				bRes = FrmMain._main.USB_Write( 2, cmd );

				if( !FrmMain._main.USB_ReadPipe( 3 ) )
				{
					Log.L.OutError( "Программатор не ответил на запрос.\n" );
				}
			}
		}

		private void toolStripMenuItem6_Click( object sender, EventArgs e )
		{
			if( openFileDialog.ShowDialog() != DialogResult.OK )
				return;

			var fName = openFileDialog.FileName;
			var bufIn = File.ReadAllBytes( fName );
			byte[] bufOut = new byte[bufIn.Length];
			int i = 0;
			foreach( var bt in bufIn )
			{
				byte b = 0;
				b |= (byte)( (bt>>7) & 0x01 );
				b |= (byte)( (bt>>5) & 0x02 );
				b |= (byte)( (bt>>3) & 0x04 );
				b |= (byte)( (bt>>1) & 0x08 );
				b |= (byte)( (bt<<1) & 0x10 );
				b |= (byte)( (bt<<3) & 0x20 );
				b |= (byte)( (bt<<5) & 0x40 );
				b |= (byte)( (bt<<7) & 0x80 );

				bufOut[i++] = b;
			}

			var path = Path.GetDirectoryName( fName ) + "\\" + Path.GetFileNameWithoutExtension( fName ) + "_" + Path.GetExtension( fName );
			File.WriteAllBytes( path, bufOut );
		}

	}
}
