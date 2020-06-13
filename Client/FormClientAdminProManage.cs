using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
        private void AddProfessor(object sender, EventArgs e)
        {
            //新增教授子流程,单击教授管理中的添加按钮，弹出添加窗口
            ProInfo tempform = new ProInfo();
            tempform.ShowDialog();
            string[] pro_info = Regex.Split(tempform.Text, " ", RegexOptions.IgnoreCase); //通过分隔符空格拆开必要信息

        }
    }
}
