using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Programmer
{
	class Log
	{
		internal DateTime _dateNow = DateTime.Now;
		DateTime _datePrev = DateTime.MinValue;
		BinaryWriter _logFile;

		Dictionary<string, string> _lstQueue = new Dictionary<string, string>();
		//string _msgPrev = "";
		Encoding _enc = Encoding.GetEncoding( "iso-8859-1" );

		internal static Log L;


		static Log()
		{
			L = new Log(); ;
		}

		bool _bAdapter = true;

		internal static string StrToHex( char ch )
		{
			return string.Format( "{0:X2}", (byte)ch );
		}

		internal static string BufDecToStr( byte[] buf, int start )
		{
			var sb = new StringBuilder();

			for( int i=start; i<buf.Length; i++ )
			{
				if( buf[i] == 0 )
					break;

				sb.Append( (char)buf[i] );
			}

			return sb.ToString();
		}

		internal static string StrToHex( string str )
		{
			if( String.IsNullOrEmpty( str ) )
				return String.Empty;

			StringBuilder sb = new StringBuilder( 3*str.Length );
			for( int i=0; i<str.Length; i++ )
			{
				sb.Append( String.Format( "{0:X2} ", (byte)str[i] ) );
			}
			sb.Remove( sb.Length-1, 1 );

			return sb.ToString();
		}

		StreamWriter _sw = File.CreateText( "log.txt" );

		internal void LogInternal( RichTextBox box, string msg, Color color )
		{
			bool scroll = false;

			_sw.Write( msg );
			_sw.Flush();

			try
			{
				int curPos = box.SelectionStart;
				if( curPos == box.TextLength )
					scroll = true;

				//_DocumentText += Regex.Replace( msg, @"[\r\n]", "" ) + "<br/>";
				//FrmMain._main.html.DocumentText = "<html>" + _DocumentText + "</html>";
				//FrmMain._main.html.Document.BackColor = Color.Black;
				//FrmMain._main.html.Document.ForeColor = Color.White;
				//FrmMain._main.html.Document.Body.Style = "";

				//this.Focus();

				//StackTrace st = new StackTrace( 1 );
				//string methodName = st.GetFrame( 0 ).GetMethod().Name;	// имя метода из которого вызывается данный метод

				//			Console.WriteLine( DateTime.Now.TimeOfDay.ToString() );
				int start = box.TextLength;
				box.SelectionColor = color;
				box.AppendText( msg );

				box.Select( start, msg.Length );
				box.SelectionColor = color;

				start = box.TextLength;
				box.Select( start, 0 );

				if( !scroll )
					box.SelectionStart = curPos;

				//txtBox.Focus();
			}
			catch
			{
			}
		}

		void LogString( string msg )
		{
			LogString( msg, Color.Yellow );
		}

		void LogString( string msg, Color color )
		{
			//if( !FrmMain._main._cbLogEnable.Checked )
			//	return;
			FrmMain._main._sc.Post( delegate
			{
				LogInternal( FrmMain._main.txtBox, msg, color );
			},
			null );
		}


		void LogLine( string msg )
		{
			LogLine( msg, Color.Yellow );
		}

		void LogLine( string msg, Color color )
		{
			//if( !FrmMain._main._cbLogEnable.Checked )
			//	return;

			msg += "\n";

			if( _bAdapter )
			{
				FrmMain._main._sc.Post( delegate
				{
					LogInternal( FrmMain._main.txtBox, msg, color );
				},
				null );
			}
			else
			{
				FrmMain._main._sc.Post( delegate
				{
					LogInternal( FrmMain._main.txtBox2, msg, color );
				},
				null );
			}
		}

		internal void Out( string msg )
		{
			ToScreen( msg, Color.Yellow, true );
		}

		internal void Out( string msg, Color color )
		{
			ToScreen( msg, color, true );
		}

		internal void OutError( string msg )
		{
			ToScreen( msg, Color.Salmon, true );
		}
		internal void InError( string msg )
		{
			ToScreen( msg, Color.Salmon, true );
		}

		internal void In( string msg )
		{
			ToScreen( msg, Color.Cyan, false );
		}

		internal void ToScreen( string msg, Color color, bool isOut )
		{
			//if( !FrmMain._main._cbLogEnable.Checked )
			//	return;

			var newMsg = new StringBuilder();
			var parts = msg.Split( new char[] { '\n' } );//, StringSplitOptions.RemoveEmptyEntries );
			int i = 0;
			foreach( var line in parts )
			{
				if( i++ == 0 )
					newMsg.Append( DateTime.Now.ToString( "HH:mm:ss.fff" ) + " " + ((isOut) ? (">") : ("<")) + " " + line );
				else
					newMsg.Append( "\n" + ( (line == "")?(""):("               " + line ) ) );
			}

			msg = newMsg.ToString() + "\n";

			if( FrmMain._main.txtBox.InvokeRequired )
			{
				FrmMain._main._sc.Send( delegate
				{
				//					int pos = FrmMain._main.txtBox.SelectionStart;
				//					FrmMain._main.txtBox.Select( pos, msg.Length );
				//					FrmMain._main.txtBox.SelectionColor = Color.Yellow;
				LogInternal( FrmMain._main.txtBox, msg, color );
				},
				null );
			}
			else
			{
				LogInternal( FrmMain._main.txtBox, msg, color );
				//Task.Factory.StartNew( () => { LogInternal( FrmMain._main.txtBox, msg, color ); }, CancellationToken.None, TaskCreationOptions.None, FrmMain._main._scScheduler );
			}
		}

	}
}
