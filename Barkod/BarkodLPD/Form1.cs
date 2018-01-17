using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace BarkodLPD
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox txtMain;
		private System.Timers.Timer tmr;
		private AxMSWinsockLib.AxWinsock ws;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private byte[]				ByteArray;
		private DirectoryInfo		di;
		private FileInfo			fi;
		private FileStream			fs;
		private string				currentFile;
		int							fileCount;
		bool						firstLog;

		private	ArrayList			dirList;
		private int					dirIndex;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			firstLog = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			this.txtMain = new System.Windows.Forms.TextBox();
			this.tmr = new System.Timers.Timer();
			this.ws = new AxMSWinsockLib.AxWinsock();
			((System.ComponentModel.ISupportInitialize)(this.tmr)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ws)).BeginInit();
			this.SuspendLayout();
			// 
			// txtMain
			// 
			this.txtMain.Location = new System.Drawing.Point(0, 8);
			this.txtMain.Multiline = true;
			this.txtMain.Name = "txtMain";
			this.txtMain.ReadOnly = true;
			this.txtMain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtMain.Size = new System.Drawing.Size(648, 248);
			this.txtMain.TabIndex = 0;
			this.txtMain.Text = "";
			// 
			// tmr
			// 
			this.tmr.Enabled = true;
			this.tmr.Interval = 1000;
			this.tmr.SynchronizingObject = this;
			this.tmr.Elapsed += new System.Timers.ElapsedEventHandler(this.tmr_Elapsed);
			// 
			// ws
			// 
			this.ws.Enabled = true;
			this.ws.Location = new System.Drawing.Point(8, 232);
			this.ws.Name = "ws";
			this.ws.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("ws.OcxState")));
			this.ws.Size = new System.Drawing.Size(28, 28);
			this.ws.TabIndex = 3;
			this.ws.ConnectEvent += new System.EventHandler(this.ws_ConnectEvent);
			this.ws.SendComplete += new System.EventHandler(this.ws_SendComplete);
			this.ws.Error += new AxMSWinsockLib.DMSWinsockControlEvents_ErrorEventHandler(this.ws_Error);
			this.ws.CloseEvent += new System.EventHandler(this.ws_CloseEvent);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(650, 261);
			this.Controls.Add(this.ws);
			this.Controls.Add(this.txtMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "BarkodLPD";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.tmr)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ws)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			appendLog("Program baþlatýldý...", true);
			dirIndex = 0;
		}

		private void appendLog(string Text, bool PutTimeStamp)
		{
			string output	= "";
			
			if (PutTimeStamp)
			{
				if (firstLog)
				{
					firstLog	=	false;
				}
				else
				{
					output		+=	"\r\n";
				}
				output	+=	"[";
				output	+=	System.DateTime.Now.Year.ToString() + ".";
				output	+=	System.DateTime.Now.Month.ToString() + ".";
				output	+=	System.DateTime.Now.Day.ToString() + " ";
				output	+=	System.DateTime.Now.Hour.ToString() + ":";
				output	+=	System.DateTime.Now.Minute.ToString() + ":";
				output	+=	System.DateTime.Now.Second.ToString() + ":";
				output	+=	System.DateTime.Now.Millisecond.ToString() + "] ";
			}
			else
			{
				output	+= " ";
			}
			output					+=	Text;
			
			txtMain.Text			+=	output;

			if (txtMain.Text.Length > 2500)
			{
				txtMain.Text = txtMain.Text.Remove(0, txtMain.Text.Length - 2500);
			}

			txtMain.SelectionStart	=	txtMain.Text.Length;
			txtMain.ScrollToCaret();

			Application.DoEvents();
		}

		private void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			printFiles();
		}

		private void printFiles()
		{
			bool			continueReading = true;
			int				nBytes, nBytesRead;
			string			devIp, devPort;

			tmr.Enabled = false;

			nBytes = 10000;

			while (continueReading)
			{
				try
				{
					// Detect current directory & refresh if neccessary
					if (dirList == null || dirIndex >= dirList.Count)
					{
						di		= new DirectoryInfo("brk");
						dirList	= new ArrayList();
						dirList.Clear();
						foreach (DirectoryInfo dir in di.GetDirectories())
						{
							dirList.Add(dir.FullName);
						}

						dirIndex = -1;
					}
					dirIndex++;

					// Detect the oldest file and assign to fi
					di = new DirectoryInfo(dirList[dirIndex].ToString());
					fileCount = 0;
					foreach(FileInfo myFi in di.GetFiles())
					{
						fileCount++;
						
						if ((fileCount == 1))
						{
							fi = myFi;
						}
					}

					if (fileCount > 0)
					{
						// Open File
						currentFile		= fi.Name;
						appendLog(currentFile + " dosyasý açýlýyor...", true);
						fs				= fi.OpenRead();
						ByteArray		= new byte[nBytes];	
						nBytesRead		= fs.Read(ByteArray, 0, nBytes);
						appendLog(nBytesRead.ToString() + " byte okundu...", false);
						fs.Close();

						// Detect IP & Port info
						devIp	= "";
						devPort = "";
						detectIpAndPort(currentFile, ref devIp, ref devPort);

						// Send data
						appendLog("Yazýcý baðlantýsý açýlýyor...", true);
						ws.Connect(devIp, devPort);
						continueReading = false;
					}
					else
					{
						continueReading = false;
						tmr.Enabled		= true;
					}
				}
				catch
				{
					ws.Close();
					continueReading = false;
					tmr.Enabled		= true;
				}
			}
		}

		private void ws_ConnectEvent(object sender, System.EventArgs e)
		{
			appendLog("Açýldý!", false);
			appendLog("Veri gönderiliyor...", true);
			try
			{
				ws.SendData(ByteArray);
			}
			catch
			{
				tmr.Enabled = true;
			}
		}

		private void ws_SendComplete(object sender, System.EventArgs e)
		{
			appendLog("Gönderildi!", false);
			ws.Close();

			appendLog("Dosya siliniyor...", true);
			fi.Delete();
			appendLog("Silindi!", false);
			tmr.Enabled = true;
		}

		private void detectIpAndPort(string FileName, ref string IP, ref string Port)
		{
			int first_Pos, second_Pos, third_Pos;

			first_Pos	= FileName.IndexOf("_", 0);
			second_Pos	= FileName.IndexOf("_", first_Pos + 1);
			third_Pos	= FileName.IndexOf("_", second_Pos + 1);

			IP			= FileName.Substring(first_Pos + 1, second_Pos - first_Pos - 1);
			Port		= FileName.Substring(second_Pos + 1, third_Pos - second_Pos - 1);
		}

		private void ws_Error(object sender, AxMSWinsockLib.DMSWinsockControlEvents_ErrorEvent e)
		{
			tmr.Enabled = true;
			ws.Close();
		}

		private void ws_CloseEvent(object sender, System.EventArgs e)
		{
			tmr.Enabled = true;
		}


	}
}
