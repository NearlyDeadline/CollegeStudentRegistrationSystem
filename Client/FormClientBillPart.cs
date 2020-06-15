using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class FormClient
    {
        private void buttonGetBill_Click(object sender, EventArgs e)
        {
            GetServerMessage("bill@"+ textBoxLoginName.Text + "," + CurrentYear + "," + CurrentSemester);
            string[] received = ReceiveMessage.Split('@');
            if (received.Length == 2)
            {
                if (received[0].Equals("True"))
                {
                    richTextBoxBill.Text = received[1];
                }
                else if (received[0].Equals("False"))
                {
                    MessageBox.Show(received[1], "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonSaveBill_Click(object sender, EventArgs e)
        {
            saveFileDialogBill.FileName = textBoxLoginName.Text + String.Format("_{0}{1}",CurrentYear,CurrentSemester);
            saveFileDialogBill.DefaultExt = ".txt";
            saveFileDialogBill.Filter = "文本文件 (.txt)|*.txt";
            if (saveFileDialogBill.ShowDialog() == DialogResult.OK)
            {
                string FilePath = saveFileDialogBill.FileName;
                using (StreamWriter writer = new StreamWriter(FilePath, false))
                {
                    writer.Write(richTextBoxBill.Text);
                }
            }
        }
    }
}
