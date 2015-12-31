/*
------------------------------------------------------------------------------
This source file is a part of LinewiseRegExTester.

Copyright (c) 2015 VIS/University of Stuttgart

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LinewiseRegExTester
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		
		void miAddLines_Click(object sender, RoutedEventArgs e)
		{
			MemoWindow.EditStrings(this.strings);
		}
		
		private sealed class RefreshCommand : ICommand
		{
			public RefreshCommand(MainWindow owner)
			{
				if (owner == null) {
					throw new ArgumentNullException("owner");
				}
				
				this.owner = owner;
			}
			
			private readonly MainWindow owner;
			
			public event EventHandler CanExecuteChanged;
			
			public void Execute(object parameter)
			{
				Regex regEx;
				try {
					regEx = new Regex(owner.tbRegEx.Text);
				}
				catch (ArgumentException) {
					MessageBox.Show("The regular expression is invalid. The table will not be updated.");
					return;
				}
				
				while (owner.dg.Columns.Count > 1) {
					owner.dg.Columns.RemoveAt(owner.dg.Columns.Count - 1);
				}
				foreach (int groupNumber in regEx.GetGroupNumbers()) {
					string groupName = regEx.GroupNameFromNumber(groupNumber);
					bool hasName = !string.IsNullOrWhiteSpace(groupName) && (groupName != groupNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
					
					var col = new DataGridTemplateColumn();
					if (groupNumber == 0) {
						col.Header = "(complete match)";
					} else {
						col.Header = hasName ? string.Format(System.Globalization.CultureInfo.InvariantCulture,
						                                     "{0} ({1})",
						                                     groupName, groupNumber) : groupNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
					}
					
					{
						var bg = new FrameworkElementFactory(typeof(Border));
						bg.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
						bg.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Stretch);
						bg.SetValue(Border.PaddingProperty, new Thickness(2));
						bg.SetBinding(Border.BackgroundProperty, new Binding("Background"));
						
						var tb = new FrameworkElementFactory(typeof(TextBlock));
						tb.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
						tb.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
						tb.SetBinding(TextBlock.TextProperty, new Binding("Match.Groups[" + groupNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + "].Value"));
						bg.AppendChild(tb);
						
						var template = new DataTemplate();
						template.VisualTree = bg;
						
						col.CellTemplate = template;
					}
					
					owner.dg.Columns.Add(col);
				}
				
				var matches = owner.strings.Select(s => new MatchedString(s, regEx.Match(s))).ToArray();
				owner.dg.ItemsSource = matches;
			}
			
			public bool CanExecute(object parameter)
			{
				return true;
			}
		}
		
		private ICommand refresh;
		
		public ICommand Refresh {
			get {
				if (refresh == null) {
					refresh = new RefreshCommand(this);
				}
				return refresh;
			}
		}
		
		private readonly List<string> strings = new List<string>();
	}
}