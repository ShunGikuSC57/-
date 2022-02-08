using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace EntityMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constant
        /// <summary>
        /// Defined array index
        /// </summary>
        private enum DefIndex : int
        {
            LogicalName,
            PhysicsName,
            DataType,
            Required
        }

        /// <summary>
        /// Define Excel Index
        /// </summary>
        /// <remarks>
        /// Customize here for each project.
        /// </remarks>
        private enum DefExcelIndex : int
        {
            LogicalName = 1,
            PhysicsName,
            DataType,
            Required
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow() => InitializeComponent();
        #endregion

        #region Event Handler
        /// <summary>
        /// Initialize window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) => Clear();

        /// <summary>
        /// TextBox check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsTextBox_Checked(object sender, RoutedEventArgs e)
        {
            FileOpen.IsEnabled = false;
            DefineExcel.IsEnabled = false;
            IsNameOnly.IsEnabled = true;
            IsDefinition.IsEnabled = true;
            ConvertSource.IsReadOnly = false;
            ConvertSource.Background = Brushes.White;

            IsNameOnly.IsChecked = true;
            FilePath.Text = string.Empty;
        }

        /// <summary>
        /// Excel check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsExcel_Checked(object sender, RoutedEventArgs e)
        {
            FileOpen.IsEnabled = true;
            DefineExcel.IsEnabled = true;
            IsNameOnly.IsEnabled = false;
            IsDefinition.IsEnabled = false;
            ConvertSource.IsReadOnly = true;
            ConvertSource.Background = Brushes.Silver;

            IsDefinition.IsChecked = true;
        }

        /// <summary>
        /// Select file click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excelブック (*.xlsx)|*.xlsx";
            if (dialog.ShowDialog().GetValue())
                FilePath.Text = dialog.FileName;
        }

        /// <summary>
        /// View Definition Excel click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefineExcel_Click(object sender, RoutedEventArgs e)
        {
            using (var book = new XLWorkbook())
            {
                var sheet = book.AddWorksheet(Properties.Resources.DEF);
                sheet.Cell(1, 1).SetValue(Properties.Resources.LOGICAL_NAME);
                sheet.Cell(1, 2).SetValue(Properties.Resources.PHYSICAL_NAME);
                sheet.Cell(1, 3).SetValue(Properties.Resources.DATA_TYPE);
                sheet.Cell(1, 4).SetValue(Properties.Resources.REQUIRED);

                var dialog = new SaveFileDialog();
                dialog.FileName = $"{Properties.Resources.DEF}.xlsx";
                if (dialog.ShowDialog().GetValue())
                    book.SaveAs(dialog.FileName);
            }
                
        }

        /// <summary>
        /// Name Only check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsNameOnly_Checked(object sender, RoutedEventArgs e)
        {
            IsPascal.IsEnabled = true;
            IsCamel.IsEnabled = true;
            IsLower.IsEnabled = true;
            IsUpper.IsEnabled = true;
            IsViewModel.IsEnabled = false;
            IsSnakeToPascal.IsEnabled = false;
            IsUseAsIs.IsEnabled = false;

            IsPascal.IsChecked = true;
            IsSnakeToPascal.IsChecked = false;
            IsUseAsIs.IsChecked = false;

            StickHere.ToolTip = Properties.Resources.EXAMPLE_NAME_ONLY;
        }

        /// <summary>
        /// Definition click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsDefinition_Checked(object sender, RoutedEventArgs e)
        {
            IsPascal.IsEnabled = false;
            IsCamel.IsEnabled = false;
            IsLower.IsEnabled = false;
            IsUpper.IsEnabled = false;
            IsViewModel.IsEnabled = true;
            IsSnakeToPascal.IsEnabled = true;
            IsUseAsIs.IsEnabled = true;

            IsProperty.IsChecked = true;
            IsSnakeToPascal.IsChecked = true;

            StickHere.ToolTip = Properties.Resources.EXAMPLE_DEFINITION;
        }

        /// <summary>
        /// Single pattern click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsSinglePattern_Checked(object sender, RoutedEventArgs e)
        {
            if (IsSnakeToPascal.IsEnabled)
            {
                IsSnakeToPascal.IsEnabled = false;
                IsUseAsIs.IsEnabled = false;

                IsSnakeToPascal.IsChecked = false;
                IsUseAsIs.IsChecked = false;
            }
        }

        /// <summary>
        /// Property and ViewModel click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsProperty_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsSnakeToPascal.IsEnabled)
            {
                IsSnakeToPascal.IsEnabled = true;
                IsUseAsIs.IsEnabled = true;

                IsSnakeToPascal.IsChecked = true;
            }
        }

        /// <summary>
        /// Make button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Make_Click(object sender, RoutedEventArgs e)
        {
            var itemsSource = new ObservableCollection<ConvertGridRow>();
            if (IsTextBox.IsChecked.GetValue())
            {
                if (!ConvertSource.Text.HasValue()) return;
                foreach (var row in ConvertSource.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
                {
                    if (!row.HasValue())
                        continue;
                    var sourceCode = string.Empty;
                    if (IsNameOnly.IsChecked.GetValue())
                    {
                        if (IsPascal.IsChecked.GetValue())
                            sourceCode = ConvertSnakeToPascal(row.ToLower());
                        else if (IsCamel.IsChecked.GetValue())
                            sourceCode = ConvertPascalToCamel(ConvertSnakeToPascal(row.ToLower()));
                        else if (IsLower.IsChecked.GetValue())
                            sourceCode = row.ToLower();
                        else if (IsUpper.IsChecked.GetValue())
                            sourceCode = row.ToUpper();
                        else if (IsProperty.IsChecked.GetValue())
                            sourceCode = ConvertProperty(IsSnakeToPascal.IsChecked.GetValue() ? ConvertSnakeToPascal(row.ToLower()) : row);

                        itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                    }
                    else if (IsDefinition.IsChecked.GetValue())
                    {
                        // Customize here for each project.
                        var values = row.Split(',');
                        if (IsProperty.IsChecked.GetValue())
                            sourceCode = ConvertProperty(values, IsSnakeToPascal.IsChecked.GetValue());
                        else if (IsViewModel.IsChecked.GetValue())
                            sourceCode = ConvertProperty(values, IsSnakeToPascal.IsChecked.GetValue(), IsViewModel.IsChecked.GetValue());
                    }

                    if (sourceCode.HasValue())
                        itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                }
            }
            else if (IsExcel.IsChecked.GetValue())
            {
                if (!FilePath.Text.HasValue()) return;
                if (!System.IO.File.Exists(FilePath.Text)) return;
                using (var book = new XLWorkbook(FilePath.Text))
                {
                    var sheet = book.Worksheet(Properties.Resources.DEF);
                    if (sheet.IsNull()) return;
                    var last = sheet.LastRowUsed().RowNumber();
                    for (int i = 2; i <= last; i++)
                    {
                        var logicalName = sheet.Cell(i, (int)DefExcelIndex.LogicalName).GetValue<string>();
                        var physicsName = sheet.Cell(i, (int)DefExcelIndex.PhysicsName).GetValue<string>();
                        var dataType = sheet.Cell(i, (int)DefExcelIndex.DataType).GetValue<string>();
                        var required = sheet.Cell(i, (int)DefExcelIndex.Required).GetValue<string>();
                        if (!logicalName.HasValue() || !physicsName.HasValue() || !dataType.HasValue()) return;
                        var sourceCode = ConvertProperty(new string[] { logicalName, physicsName, dataType, required }, IsSnakeToPascal.IsChecked.GetValue(), IsViewModel.IsChecked.GetValue());

                        if (sourceCode.HasValue())
                            itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                    }
                }
            }

            if (itemsSource.Count > 0) AfterConvert.ItemsSource = itemsSource;
        }

        /// <summary>
        /// Converts a string to a property.
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <returns>Property</returns>
        /// <remarks>
        /// Create everything as a string except from the definition.
        /// </remarks>
        private string ConvertProperty(string propertyName)
        {
            var prop = $"/// <summary>{Environment.NewLine}/// {Environment.NewLine}/// </summary>{Environment.NewLine}";
            prop += $"public string {propertyName} {{ get; set; }} = string.Empty;{Environment.NewLine}";
            return prop;
        }

        /// <summary>
        /// Converts a define to a property.
        /// </summary>
        /// <param name="values">Row values</param>
        /// <param name="isSnakeToPascal">Convert from snake case to pascal case?</param>
        /// <param name="isViewModel">ViewModel?</param>
        /// <returns>Property</returns>
        private string ConvertProperty(string[] values, bool isSnakeToPascal, bool isViewModel = false)
        {
            if (values.IsNull() || values.Length < 4)
                return string.Empty;
            var logicalName = values[(int)DefIndex.LogicalName].Trim();
            var prop = $"/// <summary>{Environment.NewLine}";
            prop += $"/// {string.Format(Properties.Resources.GET_OR_SET, logicalName)}{Environment.NewLine}";
            prop += $"/// </summary>{Environment.NewLine}";
            if (isViewModel)
            {
                // Edit this section for each project or framework.
            }
            var physicsName = isSnakeToPascal ? ConvertSnakeToPascal(values[(int)DefIndex.PhysicsName].Trim().ToLower()) : values[(int)DefIndex.PhysicsName].Trim();
            var dataType = values[(int)DefIndex.DataType].Trim().ToLower();
            var required = values[(int)DefIndex.Required].Trim().ToLower().Contains('y');
            var nullable = required ? string.Empty : "?";
            var initialize = string.Empty;
            if (dataType.Contains("char") || dataType.Contains("text"))
            {
                dataType = "string";
                nullable = string.Empty;
                initialize = "string.Empty";
            }
            else if (nameof(SqlDbType.Int).ToLower() == dataType)
            {
                dataType = nameof(SqlDbType.Int).ToLower();
                initialize = required ? "0" : "null";
            }
            else if (nameof(SqlDbType.Date).ToLower() == dataType || nameof(SqlDbType.DateTime).ToLower() == dataType)
            {
                dataType = nameof(SqlDbType.DateTime);
                initialize = required ? "DateTime.MinValue" : "null";
            }
            else
            {
                dataType = string.Empty;
            }

            if (dataType.HasValue())
                prop += $"public {dataType}{nullable} {physicsName} {{ get; set; }} = {initialize};{Environment.NewLine}";
            else
                prop = string.Empty;

            return prop;
        }

        /// <summary>
        /// Clear button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearValue_Click(object sender, RoutedEventArgs e) => Clear();
        #endregion

        #region Generic method
        /// <summary>
        /// 画面の値をクリアします。
        /// </summary>
        private void Clear()
        {
            IsViewModel.IsEnabled = false;
            IsSnakeToPascal.IsEnabled = false;
            IsUseAsIs.IsEnabled = false;

            IsTextBox.IsChecked = true;
            IsNameOnly.IsChecked = true;
            IsPascal.IsChecked = true;
            IsSnakeToPascal.IsChecked = false;
            IsUseAsIs.IsChecked = false;
            ConvertSource.Text = string.Empty;
            AfterConvert.ItemsSource = null;
        }

        /// <summary>
        /// Convert snake case to pascal case.
        /// </summary>
        /// <param name="str">変換対象文字列</param>
        /// <returns>パスカルケース文字列</returns>
        private string ConvertSnakeToPascal(string str)
        {
            if (!str.HasValue()) return string.Empty;
            return str
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        /// <summary>
        /// Convert pascal case to camel case.
        /// </summary>
        /// <param name="str">変換対象文字列</param>
        /// <returns>キャメルケース文字列</returns>
        private string ConvertPascalToCamel(string str)
        {
            if (!str.HasValue()) return string.Empty;
            return char.ToLower(str[0]) + str.Substring(1);
        }
        #endregion

        #region Inner Class
        /// <summary>
        /// Converted grid row class
        /// </summary>
        private class ConvertGridRow
        {
            /// <summary>source code</summary>
            public string SourceCode { get; set; } = string.Empty;
        }
        #endregion
    }
}
