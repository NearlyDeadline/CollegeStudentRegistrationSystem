using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    partial class FormClient
    {

        private void buttonStart_Click(object sender, EventArgs e)
        {
            GetServerMessage("start");
            MessageBox.Show(ReceiveMessage);
        }

        private void buttonEnd_Click(object sender, EventArgs e)
        {
            GetServerMessage("end");
            MessageBox.Show(ReceiveMessage);
        }

        private void buttonCloseRegistration_Click(object sender, EventArgs e)
        {
            GetServerMessage("close");
            if (ReceiveMessage != String.Empty)
            {
                string[] message = ReceiveMessage.Split('@');
                if (message.Length == 2)
                {
                    if (message[0].Equals("Success"))
                        MessageBox.Show("操作成功，服务器开始产生课表并生成学生账单", "关闭注册", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(message[1], "关闭注册", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("与服务器通信失败", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
