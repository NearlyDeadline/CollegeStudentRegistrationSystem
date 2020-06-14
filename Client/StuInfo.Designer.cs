using System;
using System.Text.RegularExpressions;

namespace Client
{
    partial class StuInfo
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
            if (disposing && (components != null))
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
            this.label提供信息提示 = new System.Windows.Forms.Label();
            this.button取消 = new System.Windows.Forms.Button();
            this.comboBox毕业年份 = new System.Windows.Forms.ComboBox();
            this.comboBox学历 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button添加 = new System.Windows.Forms.Button();
            this.button重置 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.textBox身份证号 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox学生姓名 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label提供信息提示
            // 
            this.label提供信息提示.AutoSize = true;
            this.label提供信息提示.Location = new System.Drawing.Point(34, 20);
            this.label提供信息提示.Name = "label提供信息提示";
            this.label提供信息提示.Size = new System.Drawing.Size(157, 15);
            this.label提供信息提示.TabIndex = 0;
            this.label提供信息提示.Text = "请提供学生以下信息：";
            // 
            // button取消
            // 
            this.button取消.Location = new System.Drawing.Point(258, 324);
            this.button取消.Name = "button取消";
            this.button取消.Size = new System.Drawing.Size(86, 46);
            this.button取消.TabIndex = 28;
            this.button取消.Text = "取消";
            this.button取消.UseVisualStyleBackColor = true;
            this.button取消.Click += new System.EventHandler(this.button取消_Click);
            // 
            // comboBox毕业年份
            // 
            this.comboBox毕业年份.FormattingEnabled = true;
            this.comboBox毕业年份.Items.AddRange(new object[] {
            "2020",
            "2021",
            "2022",
            "2023",
            "2024"});
            this.comboBox毕业年份.Location = new System.Drawing.Point(126, 270);
            this.comboBox毕业年份.Name = "comboBox毕业年份";
            this.comboBox毕业年份.Size = new System.Drawing.Size(176, 23);
            this.comboBox毕业年份.TabIndex = 27;
            this.comboBox毕业年份.Text = "<请选择>";
            this.comboBox毕业年份.SelectedIndexChanged += new System.EventHandler(this.comboBox毕业年份_SelectedIndexChanged);
            // 
            // comboBox学历
            // 
            this.comboBox学历.FormattingEnabled = true;
            this.comboBox学历.Items.AddRange(new object[] {
            "本科生",
            "硕士研究生",
            "博士研究生"});
            this.comboBox学历.Location = new System.Drawing.Point(126, 218);
            this.comboBox学历.Name = "comboBox学历";
            this.comboBox学历.Size = new System.Drawing.Size(176, 23);
            this.comboBox学历.TabIndex = 26;
            this.comboBox学历.Text = "<请选择>";
            this.comboBox学历.SelectedIndexChanged += new System.EventHandler(this.comboBox学历_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(48, 270);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 25;
            this.label6.Text = "毕业年份";
            // 
            // button添加
            // 
            this.button添加.Location = new System.Drawing.Point(33, 324);
            this.button添加.Name = "button添加";
            this.button添加.Size = new System.Drawing.Size(82, 46);
            this.button添加.TabIndex = 24;
            this.button添加.Text = "添加";
            this.button添加.UseVisualStyleBackColor = true;
            this.button添加.Click += new System.EventHandler(this.button添加_Click);
            // 
            // button重置
            // 
            this.button重置.Location = new System.Drawing.Point(145, 324);
            this.button重置.Name = "button重置";
            this.button重置.Size = new System.Drawing.Size(86, 46);
            this.button重置.TabIndex = 23;
            this.button重置.Text = "重置";
            this.button重置.UseVisualStyleBackColor = true;
            this.button重置.Click += new System.EventHandler(this.button重置_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(63, 221);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 15);
            this.label4.TabIndex = 22;
            this.label4.Text = "学历";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(126, 112);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(176, 25);
            this.dateTimePicker1.TabIndex = 21;
            // 
            // textBox身份证号
            // 
            this.textBox身份证号.Location = new System.Drawing.Point(126, 166);
            this.textBox身份证号.Name = "textBox身份证号";
            this.textBox身份证号.Size = new System.Drawing.Size(176, 25);
            this.textBox身份证号.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(48, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 15);
            this.label3.TabIndex = 19;
            this.label3.Text = "身份证号";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "出生日期";
            // 
            // textBox学生姓名
            // 
            this.textBox学生姓名.Location = new System.Drawing.Point(126, 60);
            this.textBox学生姓名.Name = "textBox学生姓名";
            this.textBox学生姓名.Size = new System.Drawing.Size(176, 25);
            this.textBox学生姓名.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "学生姓名";
            // 
            // StuInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 441);
            this.Controls.Add(this.button取消);
            this.Controls.Add(this.comboBox毕业年份);
            this.Controls.Add(this.comboBox学历);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button添加);
            this.Controls.Add(this.button重置);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.textBox身份证号);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox学生姓名);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label提供信息提示);
            this.Name = "StuInfo";
            this.Text = "添加学生";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StuInfo_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label提供信息提示;
        private System.Windows.Forms.Button button取消;
        private System.Windows.Forms.ComboBox comboBox毕业年份;
        private System.Windows.Forms.ComboBox comboBox学历;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button添加;
        private System.Windows.Forms.Button button重置;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.TextBox textBox身份证号;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox学生姓名;
        private System.Windows.Forms.Label label1;
    }
}
#endregion