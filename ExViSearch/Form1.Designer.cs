namespace ExViSearch {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.watchTextbox = new System.Windows.Forms.TextBox();
			this.debugTextbox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// watchTextbox
			// 
			this.watchTextbox.Location = new System.Drawing.Point(12, 28);
			this.watchTextbox.Multiline = true;
			this.watchTextbox.Name = "watchTextbox";
			this.watchTextbox.Size = new System.Drawing.Size(330, 139);
			this.watchTextbox.TabIndex = 0;
			// 
			// debugTextbox
			// 
			this.debugTextbox.Location = new System.Drawing.Point(381, 28);
			this.debugTextbox.Multiline = true;
			this.debugTextbox.Name = "debugTextbox";
			this.debugTextbox.Size = new System.Drawing.Size(299, 139);
			this.debugTextbox.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(692, 195);
			this.Controls.Add(this.debugTextbox);
			this.Controls.Add(this.watchTextbox);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox watchTextbox;
		private System.Windows.Forms.TextBox debugTextbox;
	}
}

