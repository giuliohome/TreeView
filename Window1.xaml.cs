/*
 * Created by SharpDevelop.
 * User: sysman
 * Date: 07/01/2017
 * Time: 22.27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TreeVisitor
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		TreeViewModel tvModel = new TreeViewModel();
		public Window1()
		{
			InitializeComponent();
			DataContext = tvModel;
		}
		
		
		void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
//			string text = hidden.Text;
//			if (text.Equals("Choices")) {
//				var choices = ((TreeItem)tree.Items[0]).TreeItems[9];
//				choices.Selected = true;
//				hidden.Focus();
//				hidden.Text = "";
//			}
		}
		
		void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
//			string text = hidden.Text;
//			if (text.Equals("Choices")) {
//				var choices = ((TreeItem)tree.Items[0]).TreeItems[9];
//				choices.Selected = true;
//				hidden.Focus();
//				hidden.Text = "";
//			}
			if (hidden.Text.Equals("Choices")) {
				opBox.Focus();
				hidden.Text = "";
			}
		}
	}
}