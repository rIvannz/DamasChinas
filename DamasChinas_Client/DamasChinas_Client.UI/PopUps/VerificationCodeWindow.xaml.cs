using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

using DamasChinas_Client.UI.Utilities;




namespace DamasChinas_Client.UI.PopUps
{
    public partial class VerificationCodeWindow : Window
    {
        public string CodeValue { get; private set; }

        public VerificationCodeWindow()
        {
            InitializeComponent();
        }

       

        private void OnVerifyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var code = txtVerificationCode.Text.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_EmptyVerificationCode"),
                        PopupType.Warning
                    );
                    return;
                }

                CodeValue = code;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VerificationCodeWindow.OnVerifyClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

  

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
           
            this.DialogResult = false;
            this.Close();
        }
    }
}

