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
        public VerificationCodeWindow()
        {
            InitializeComponent();
        }

       

        private void OnVerifyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string code = txtVerificationCode.Text.Trim();

                // 1️⃣ Validación: campo vacío
                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_EmptyVerificationCode"),
                        "warning"
                    );
                    return;
                }

         
                bool isValid = code == "1234";

                if (!isValid)
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_InvalidVerificationCode"),
                        "error"
                    );
                    return;
                }

       
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_VerificationSuccess"),
                    "success"
                );

          
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VerificationCodeWindow.OnVerifyClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

  

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            // Indica que NO se verificó
            this.DialogResult = false;
            this.Close();
        }
    }
}

