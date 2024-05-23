namespace transaction
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components!= null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonStartSimulation = new System.Windows.Forms.Button();
            this.comboBoxIsolationLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nudTypeAUsers = new System.Windows.Forms.NumericUpDown();
            this.nudTypeBUsers = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudTypeAUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTypeBUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStartSimulation
            // 
            this.buttonStartSimulation.Location = new System.Drawing.Point(18, 160);
            this.buttonStartSimulation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonStartSimulation.Name = "buttonStartSimulation";
            this.buttonStartSimulation.Size = new System.Drawing.Size(180, 35);
            this.buttonStartSimulation.TabIndex = 0;
            this.buttonStartSimulation.Text = "Start Simulation";
            this.buttonStartSimulation.UseVisualStyleBackColor = true;
            this.buttonStartSimulation.Click += new System.EventHandler(this.buttonStartSimulation_Click);
            // 
            // comboBoxIsolationLevel
            // 
            this.comboBoxIsolationLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIsolationLevel.FormattingEnabled = true;
            this.comboBoxIsolationLevel.Items.AddRange(new object[] {
            "ReadUncommitted",
            "ReadCommitted",
            "RepeatableRead",
            "Serializable"});
            this.comboBoxIsolationLevel.Location = new System.Drawing.Point(18, 118);
            this.comboBoxIsolationLevel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxIsolationLevel.Name = "comboBoxIsolationLevel";
            this.comboBoxIsolationLevel.Size = new System.Drawing.Size(178, 28);
            this.comboBoxIsolationLevel.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(207, 49);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Type A Users:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(207, 89);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Type B Users:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(207, 123);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Isolation Level:";
            // 
            // nudTypeAUsers
            // 
            this.nudTypeAUsers.Location = new System.Drawing.Point(18, 43);
            this.nudTypeAUsers.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudTypeAUsers.Name = "nudTypeAUsers";
            this.nudTypeAUsers.Size = new System.Drawing.Size(178, 26);
            this.nudTypeAUsers.TabIndex = 7;
            // 
            // nudTypeBUsers
            // 
            this.nudTypeBUsers.Location = new System.Drawing.Point(18, 82);
            this.nudTypeBUsers.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudTypeBUsers.Name = "nudTypeBUsers";
            this.nudTypeBUsers.Size = new System.Drawing.Size(178, 26);
            this.nudTypeBUsers.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 592);
            this.Controls.Add(this.nudTypeBUsers);
            this.Controls.Add(this.nudTypeAUsers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxIsolationLevel);
            this.Controls.Add(this.buttonStartSimulation);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Transaction Simulation";
            ((System.ComponentModel.ISupportInitialize)(this.nudTypeAUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTypeBUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartSimulation;
        private System.Windows.Forms.ComboBox comboBoxIsolationLevel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudTypeAUsers;
        private System.Windows.Forms.NumericUpDown nudTypeBUsers;
    }
}