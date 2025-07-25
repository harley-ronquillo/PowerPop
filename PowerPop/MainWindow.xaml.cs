﻿using System;
using System.Windows;

namespace PowerPop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void enter_btn(object sender, RoutedEventArgs e)
        {
            string enteredCode = code.Text.Trim();
            string correctCode = "12345"; 

            if (enteredCode == correctCode)
            {
                MainFrame1 mainFrame = new MainFrame1();
                mainFrame.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect code. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                code.Clear();
                code.Focus();
            }
        }
    }
}
