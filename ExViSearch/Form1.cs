using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;

namespace ExViSearch {
	public partial class Form1 : Form {
		FileResults fileResults = new FileResults();
		globalKeyboardHook gkh = new globalKeyboardHook();
		KeysConverter keysConverter = new KeysConverter();
		bool inSearchMode = false;
		bool activelySearching = false;
		bool shiftDown = false;
		Explorer explorer = new Explorer();
		Timer listFilesTimer = new Timer();


		Regex searchQueryRe = new Regex("[a-zA-Z0-9-,\\.';=`~\\[\\]!@#\\$\\%\\^\\&\\*\\(\\)\\+\\s]");

		public Form1() {
			InitializeComponent();

			gkh.KeyDown += gkh_KeyDown;
			gkh.KeyUp += gkh_KeyUp;

			Timer t = new Timer();
			t.Interval = 100;
			t.Tick += t_Tick;
			t.Start();


			listFilesTimer.Interval = 1;
			listFilesTimer.Tick += listFilesTimer_Tick;
			listFilesTimer.Enabled = false;
		}

		void listFilesTimer_Tick(object sender, EventArgs e) {
			listFilesTimer.Enabled = false;
			listFilesTimer.Stop();
			fileResults.Files = explorer.ListFiles(ref fileResults.sh32Window);
			log_debug("Files: " + fileResults.Files.Count.ToString());
		}



		void t_Tick(object sender, EventArgs e) {
			watchTextbox.Clear();
			log_watch("SearchMode: " + inSearchMode.ToString());
			log_watch("activelySearching: " + activelySearching.ToString());
			log_watch("shiftDown: " + shiftDown.ToString());
			log_watch("path: " + explorer.GetPath());
			log_watch("query: " + fileResults.SearchQuery);
			//List<string> files = FilesAndFolders();
		}

		void gkh_KeyDown(object sender, KeyEventArgs e) {
			//log(e.KeyCode + " (" + e.KeyValue + ")");
			switch (e.KeyCode) {
				case Keys.LShiftKey:
				case Keys.RShiftKey:
				case Keys.Shift:
				case Keys.ShiftKey:
					shiftDown = true;
					break;
				case Keys.Return:
					if (inSearchMode) {
						fileResults.NextResult();
						inSearchMode = false; //Return keyboard control
						activelySearching = true; //Listen for 'n' and 'N'; find next/previous result until escape is hit
					}
					break;
				case Keys.OemQuestion: // forward-slash
					if (!shiftDown) {
						inSearchMode = true;

						//this gives us the files in the explorer window 
						listFilesTimer.Enabled = true;
						listFilesTimer.Start();

						e.Handled = true;
					}
					break;
				case Keys.Escape:
					inSearchMode = false;
					activelySearching = false;
					fileResults.Reset();
					break;
				case Keys.N:
				default:

					if (e.KeyCode == Keys.N) {
						if (activelySearching) {
							if (shiftDown) {
								fileResults.PreviousResult();
							}
							else {
								try {
									fileResults.NextResult();
								}
								catch {
									System.Threading.Thread.Sleep(200);
									fileResults.NextResult();
								}
							}
						}

					}


					if (e.KeyCode == Keys.Back && fileResults.SearchQuery.Length > 0) {
						fileResults.SearchQuery = fileResults.SearchQuery.Substring(0, fileResults.SearchQuery.Length - 1);
						e.Handled = true;
						break;
					}

					if (inSearchMode) {

						string pressedChar = keysConverter.ConvertToString(e.KeyCode);
						if (!shiftDown) {
							pressedChar = pressedChar.ToLower();
						}
						else {
							switch (pressedChar) {
								case "1":
									pressedChar = "!";
									break;
								case "2":
									pressedChar = "@";
									break;
								case "3":
									pressedChar = "#";
									break;
								case "4":
									pressedChar = "$";
									break;
								case "5":
									pressedChar = "%";
									break;
								case "6":
									pressedChar = "^";
									break;
								case "7":
									pressedChar = "&";
									break;
								case "8":
									pressedChar = "*";
									break;
								case "9":
									pressedChar = "(";
									break;
								case "0":
									pressedChar = ")";
									break;
								case "-":
									pressedChar = "_";
									break;
								case "=":
									pressedChar = "+";
									break;
							}
						}
						if (pressedChar == "space") {
							pressedChar = " ";
						}
						if (pressedChar == "oemperiod") {
							pressedChar = ".";
						}
						if (pressedChar.Length == 1 && searchQueryRe.IsMatch(pressedChar)) {
							fileResults.SearchQuery += pressedChar;
							e.Handled = true;
						}
					}
					break;
			}
			if (activelySearching) {
				e.Handled = true;
			}
		}

		void gkh_KeyUp(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.LShiftKey:
				case Keys.RShiftKey:
				case Keys.Shift:
				case Keys.ShiftKey:
					shiftDown = false;
					break;
			}
		}

		void log_watch(string text) {
			if (text != null) {
				watchTextbox.AppendText(text);
				watchTextbox.AppendText("\n");
			}
		}

		void log_debug(string text) {
			if (text != null) {
				debugTextbox.AppendText(text);
				debugTextbox.AppendText("\n");
			}
		}

		public void HighlightResult(string result) {
			bool prevInSearch = inSearchMode;
			bool prevActivelySearching = activelySearching;

			inSearchMode = false;
			activelySearching = false;

			//SendKeys.SendWait("{HOME}");

			//System.Threading.Thread.Sleep(10);

			foreach (char letter in result) {
				//SendKeys.Send(letter.ToString());
				//log_debug(letter.ToString());
				//System.Threading.Thread.Sleep(10);
			}

			SendKeys.Send(result);
			inSearchMode = prevInSearch;
			activelySearching = prevActivelySearching;
		}

		class FileResults {
			Explorer explorer = new Explorer();
			public string Path = "";
			public string SearchQuery { get; set; }
			public List<Shell32.FolderItem> Files = new List<Shell32.FolderItem>();
			public string CurrentResult = "";
			public int CurrentIndex = -1;
			public SHDocVw.InternetExplorer sh32Window;

			public void DeselectFiles() {
				Timer deselectTimer = new Timer();
				deselectTimer.Tick += deselectTimer_Tick;
				deselectTimer.Interval = 1;
				deselectTimer.Start();
			}


			public void NextResult() {
				DeselectFiles();
				System.Threading.Thread.Sleep(10); //Give it time to deselect
				Timer nextResultTimer = new Timer();
				nextResultTimer.Tick += nextResultTimer_Tick;
				nextResultTimer.Interval = 1;
				nextResultTimer.Start();
			}

			public void PreviousResult() {
				DeselectFiles();
				System.Threading.Thread.Sleep(10); //Give it time to deselect
				Timer prevResultTimer = new Timer();
				prevResultTimer.Tick += prevResultTimer_Tick;
				prevResultTimer.Interval = 1;
				prevResultTimer.Start();

			}


			public void Reset() {
				this.DeselectFiles();
				this.SearchQuery = "";
				this.CurrentResult = "";
				this.CurrentIndex = -1;
			}
			private bool isMatch(string fileName, string searchQuery) {
				return fileName.ToLower().IndexOf(searchQuery.ToLower()) > -1;
			}

			void nextResultTimer_Tick(object sender, EventArgs e) {
				((Timer)sender).Stop();
				bool foundResult = false;
				//Make sure we have the list of files we're dealing with
				for (int i = CurrentIndex + 1; i < Files.Count; i++) {
					if (isMatch(Files[i].Name, this.SearchQuery)) {
						CurrentIndex = i;
						((Shell32.IShellFolderViewDual2)this.sh32Window.Document).SelectItem(Files[i], 1);
						foundResult = true;
						break;
					}
				}
				//if no result was found, reset CurrentIndex and try again
				if (!foundResult) {
					CurrentIndex = -1;
					for (int i = CurrentIndex + 1; i < Files.Count; i++) {
						if (isMatch(Files[i].Name, this.SearchQuery)) {
							CurrentIndex = i;
							((Shell32.IShellFolderViewDual2)this.sh32Window.Document).SelectItem(Files[i], 1);
							break;
						}
					}
				}
			}

			void prevResultTimer_Tick(object sender, EventArgs e) {
				((Timer)sender).Stop();
				bool foundResult = false;
				//Make sure we have the right path
				if (this.Path != explorer.GetPath()) {
					this.Path = explorer.GetPath();
				}

				//Make sure we have the list of files we're dealing with
				for (int i = CurrentIndex - 1; i >= 0; i--) {
					if (isMatch(Files[i].Name, this.SearchQuery)) {
						CurrentIndex = i;
						((Shell32.IShellFolderViewDual2)this.sh32Window.Document).SelectItem(Files[i], 1);
						foundResult = true;
						break;
					}
				}
				//if no result was found, set CurrentIndex to the end of the list and try again
				if (!foundResult) {
					CurrentIndex = Files.Count;
					for (int i = CurrentIndex - 1; i >= 0; i--) {
						if (isMatch(Files[i].Name, this.SearchQuery)) {
							CurrentIndex = i;
							((Shell32.IShellFolderViewDual2)this.sh32Window.Document).SelectItem(Files[i], 1);
							break;
						}
					}
				}
			}

			void deselectTimer_Tick(object sender, EventArgs e) {
				((Timer)sender).Stop();
				foreach (Shell32.FolderItem file in this.Files) {
					((Shell32.IShellFolderViewDual2)this.sh32Window.Document).SelectItem(file, 0);
				}
			}

		}

		class Explorer {
			const int WM_GETTEXT = 0x000D;
			const int WM_GETTEXTLENGTH = 0x000E;
			[DllImport("user32.dll")]
			static extern IntPtr GetForegroundWindow();
			[DllImport("user32.dll")]
			static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
			[DllImport("user32.dll")]
			static extern int InternalGetWindowText(IntPtr hWnd, StringBuilder text, int count);
			[DllImport("user32.dll", EntryPoint = "FindWindow")]
			private static extern IntPtr FindWindow(string sClass, string sWindow);
			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
			public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);
			[DllImport("user32")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);
			[DllImport("user32.dll", EntryPoint = "SendMessage")]
			public static extern IntPtr SendMessageGetText(IntPtr hWnd, uint msg, UIntPtr wParam, StringBuilder lParam);

			public static string GetWindowText(IntPtr hWnd) {
				const uint WM_GETTEXT = 13;
				const int bufferSize = 1000; // adjust as necessary
				StringBuilder sb = new StringBuilder(bufferSize);
				SendMessageGetText(hWnd, WM_GETTEXT, new UIntPtr(bufferSize), sb);
				string controlText = sb.ToString();
				return controlText;
			}




			private string internalGetWindowText(IntPtr handle) {
				const int nChars = 256;
				StringBuilder Buff = new StringBuilder(nChars);
				if (InternalGetWindowText(handle, Buff, nChars) > 0) {
					return Buff.ToString();
				}
				return null;
			}

			public string GetPath() {
				IntPtr focusedWindow = GetForegroundWindow();
				IntPtr childHandle = IntPtr.Zero;
				string path = "";
				childHandle = FindWindowEx(focusedWindow, IntPtr.Zero, "WorkerW", "");
				if (childHandle != IntPtr.Zero) {
					childHandle = FindWindowEx(childHandle, IntPtr.Zero, "ReBarWindow32", "");
					if (childHandle != IntPtr.Zero) {
						childHandle = FindWindowEx(childHandle, IntPtr.Zero, "Address Band Root", "");
						if (childHandle != IntPtr.Zero) {
							childHandle = FindWindowEx(childHandle, IntPtr.Zero, "msctls_progress32", "");
							if (childHandle != IntPtr.Zero) {
								childHandle = FindWindowEx(childHandle, IntPtr.Zero, "Breadcrumb Parent", "");
								if (childHandle != IntPtr.Zero) {
									List<IntPtr> children = GetChildWindows(childHandle);
									try {
										path = GetWindowText(children[0]);
										path = path.Replace("Address: ", "");
									}
									catch { }
								}
							}
						}
					}
				}
				return path;
			}



			public List<Shell32.FolderItem> ListFiles(ref SHDocVw.InternetExplorer sh32Window) {
				string filename;
				List<Shell32.FolderItem> explorerItems = new List<Shell32.FolderItem>();
				var shell = new Shell32.Shell();
				foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows()) {
					filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
					if (filename.ToLowerInvariant() == "explorer") {
						sh32Window = window;
						Shell32.Folder folder = ((Shell32.IShellFolderViewDual2)window.Document).Folder;
						Shell32.FolderItems items = folder.Items();
						foreach (Shell32.FolderItem item in items) {
							explorerItems.Add(item);
						}
					}
				}
				return explorerItems;
			}



			public static List<IntPtr> GetChildWindows(IntPtr parent) {
				List<IntPtr> result = new List<IntPtr>();
				GCHandle listHandle = GCHandle.Alloc(result);
				try {
					EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
					EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
				}
				finally {
					if (listHandle.IsAllocated)
						listHandle.Free();
				}
				return result;
			}

			private static bool EnumWindow(IntPtr handle, IntPtr pointer) {
				GCHandle gch = GCHandle.FromIntPtr(pointer);
				List<IntPtr> list = gch.Target as List<IntPtr>;
				if (list == null) {
					throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
				}
				list.Add(handle);
				//  You can modify this to check to see if you want to cancel the operation, then return a null here
				return true;
			}

			public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
		}
	}



}
