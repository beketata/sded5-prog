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


namespace Programmer
{
	public partial class FrmMain
	{
		StringBuilder _sbRs = new StringBuilder();
		object _locParse = new object();


		void serialPort_DataReceived( object sender, SerialDataReceivedEventArgs e )
		{
			if( !_serialPort.IsOpen || _serialPort.BytesToRead == 0 )
				return;

			try
			{
				string sRead = _serialPort.ReadExisting();
				_sbRs.Append( sRead );

				byte[] btRead = _serialPort.Encoding.GetBytes( sRead );

				// Разбираем принятые данные
				//
				while( _sbRs.Length > 0 )
				{
					// Проверяем, содержится ли в принятой строке символ начала сообщения '(' ?
					//
					int i = 0;
					if( ( i = _sbRs.ToString().IndexOf( '(' ) ) == -1 )
					{
						// Если символа '(' в строке нет, удаляем всю принятую строку.
						//
						_sbRs.Remove( 0, _sbRs.Length );

						return;
					}
					//
					// Если '(' обнаружен в середине строки, удаляем начало строки до '('
					//
					else if( i != 0 )
					{
						_sbRs.Remove( 0, i );
					}

					// Проверяем, содержится ли в буфере одна целая строка ?
					//
					int end = _sbRs.ToString().IndexOf( ")" );		// символ конца строки
					if( end == -1 )
						return;

					// В приемном буфере целая строка, разбираем ее.
					//
					//Log.L._dateNow = DateTime.Now;	// Фиксируем текущее время.

					string str = _sbRs.ToString().Substring( 1, end - 1 );
					_sbRs.Remove( 0, end+1 );

					// Отправляем принятую строку на обработку.
					//
					if( !string.IsNullOrEmpty( str ) )
					{
						Log.L.Parse( str );
					}
				}
			}
			catch
			{
				//LogErr( "Exception в SocketReadCallback(): " + exc );
			}
		}

		async void ComPortOpen()
		{
			await Task.Run( (Action) ComPortOpen_ );
		}

		void ComPortOpen_()
		{
			bool bRes = false;
			string sPortName = "";

			_serialPort.DataReceived -= serialPort_DataReceived;

			try
			{
				// USB
				//
				// Gets a list of all HID devices currently connected to the computer (InfoSet)
				//
				IntPtr hInfoSet = Win32.SetupDiGetClassDevs( ref Win32.GUID_DEVINTERFACE_MODEM, null, IntPtr.Zero, Win32.DIGCF_DEVICEINTERFACE | Win32.DIGCF_PRESENT );
				if( hInfoSet != Win32.INVALID_HANDLE_VALUE )
				{
					int nIndex = 0;

					try
					{
						var devInfoData = new Win32.SP_DEVINFO_DATA();	// build up a device interface data block
						devInfoData.cbSize = Marshal.SizeOf( devInfoData );

						// Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
						// to get device details for each device connected
						while( Win32.SetupDiEnumDeviceInterfaces( hInfoSet, 0, ref Win32.GUID_DEVINTERFACE_MODEM, (uint)nIndex, ref devInfoData ) )	// this gets the device interface information for a device at index 'nIndex' in the memory block
						{
							uint nRequiredSize = 0;

							// Get the device interface details
							if( !Win32.SetupDiGetDeviceInterfaceDetail( hInfoSet, ref devInfoData, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero ) )
							{
								// build a DevInfo Data structure
								var da = new Win32.SP_DEVINFO_DATA();
								da.cbSize = Marshal.SizeOf( da );

								var oDetail = new Win32.SP_DEVICE_INTERFACE_DETAIL_DATA();
								oDetail.cbSize = ( (IntPtr.Size == 4)?( 5 ):( 8 ) );

								if( Win32.SetupDiGetDeviceInterfaceDetail( hInfoSet, ref devInfoData, ref oDetail, nRequiredSize, ref nRequiredSize, ref da ) )
								{
									if( oDetail.DevicePath.ToUpper().Contains( _usbDeviceID ) )	// do a string search, if we find the VID/PID string then we found our device!
									{
										IntPtr hKey = Win32.SetupDiOpenDevRegKey( hInfoSet, ref da, Win32.DICS_FLAG_GLOBAL, 0, Win32.DIREG_DEV, (int)Win32.RegSAM.AllAccess );
										if( hKey != Win32.INVALID_HANDLE_VALUE )
										{
											// читаем имя порта (COMx, LPTx)
											//
											uint size = 256;
											uint type;
											StringBuilder keyBuffer = new StringBuilder();
											if( Win32.RegQueryValueEx( hKey, "PortName", IntPtr.Zero, out type, keyBuffer, ref size ) == 0 )
											{
												sPortName = keyBuffer.ToString();

												bRes = true;

												break;
											}

											Win32.RegCloseKey( hKey );
										}
									}
								}
							}

							nIndex++;	// if we get here, we didn't find our device. So move on to the next one.
						}
					}
					finally
					{
						// Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
						//
						Win32.SetupDiDestroyDeviceInfoList( hInfoSet );
					}
				}

				if( bRes )
				{
					try
					{
						_serialPort.Close();
					}
					catch {}

					_serialPort.PortName = sPortName;
					_serialPort.Encoding = Encoding.GetEncoding( "iso-8859-1" );
					_serialPort.BaudRate = 115200;
					_serialPort.Parity = Parity.None;
					_serialPort.StopBits = StopBits.One;

					_serialPort.Open();

					_serialPort.DataReceived += serialPort_DataReceived;
					_serialPort.ErrorReceived += _serialPort_ErrorReceived;

					_sc.Post( delegate
					{
						USBInfoToggle( true );
					},
					null );
				}
				else
				{
					/*
					throw new ApplicationException();

					_modeRs = MODE_RS.NO_REMOTE;

					MessageBox.Show(
						"В системе отсутствует COM порт (" +
							( ( Settings.Default.Config_iIRinterface == 0 )?( "USB" ):( "RS232" ) ) + ") !\r\n\r\n" +
								"Программа будет работать без пульта ДУ.",
							"Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning );
					*/
				}
			}
			catch( ApplicationException )
			{
			}
			catch
			{
			}
		}

		void _serialPort_ErrorReceived( object sender, SerialErrorReceivedEventArgs e )
		{
			_sc.Post( delegate
			{
				//Log.L.LogInternal( FrmMain._main.txtBox, "!!! ERROR !!! on USB receiving: " + e.EventType.ToString() + "\r\n" );
			},
			null );
		}

		object _lockSerialPortWriteString = new object();
		internal bool SerialPort_Write( string sData )
		{
			bool bRes = false;

			if( !_serialPort.IsOpen )
				return bRes;

			lock( _lockSerialPortWriteString )
			{
				Debug.WriteLine( "Отправляем в IR модуль: " + sData );

				try
				{
					_serialPort.Write( sData );
					_serialPort.DiscardOutBuffer();

					bRes = true;
				}
				catch
				{
					try
					{
						_serialPort.Close();
					}
					catch { }
				}
			}

			return bRes;
		}

		object _lockSerialPortWriteArray = new object();
		internal bool SerialPort_Write( byte[] buf )
		{
			bool bRes = false;

			if( !_serialPort.IsOpen )
				return bRes;

			lock( _lockSerialPortWriteArray )
			{
				try
				{
					_serialPort.Write( buf, 0, buf.Length );
					_serialPort.DiscardOutBuffer();

					bRes = true;
				}
				catch
				{
					try
					{
						_serialPort.Close();
					}
					catch { }
				}
			}

			return bRes;
		}

	}
}
