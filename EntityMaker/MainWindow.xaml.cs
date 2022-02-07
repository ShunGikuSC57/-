using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using EntityMaker.Properties;

namespace EntityMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constant
        /// <summary>
        /// Definition column index
        /// </summary>
        private enum DefIndex : int
        {
            LogicalName,
            PhysicsName,
            DataType,
            NotNull
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

            StickHere.ToolTip = MainWindowResources.EXAMPLE_NAME_ONLY;
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

            StickHere.ToolTip = MainWindowResources.EXAMPLE_DEFINITION;
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
            if (!ConvertSource.Text.HasValue()) return;
            var itemsSource = new ObservableCollection<ConvertGridRow>();
            foreach (var row in ConvertSource.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            {
                if (!row.HasValue()) continue;
                if (IsNameOnly.IsChecked.GetValue())
                {
                    var sourceCode = string.Empty;
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
                else if(IsDefinition.IsChecked.GetValue())
                {
                    var sourceCode = string.Empty;
                    if (IsProperty.IsChecked.GetValue()) sourceCode = ConvertProperty(row.Split(','), IsSnakeToPascal.IsChecked.GetValue());
                    // Customize here for each project.
                    if (sourceCode.HasValue()) itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                    continue;
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
        /// <returns>Property</returns>
        private string ConvertProperty(string[] values, bool isSnakeToPascal)
        {
            if (values.IsNull() || values.Length < 4) return string.Empty;
            var logicalName = values[(int)DefIndex.LogicalName].Trim();
            var prop = $"/// <summary>{Environment.NewLine}";
            prop += $"/// {string.Format(MainWindowResources.GET_OR_SET, logicalName)}{Environment.NewLine}";
            prop += $"/// </summary>{Environment.NewLine}";
            var physicsName = isSnakeToPascal ? ConvertSnakeToPascal(values[(int)DefIndex.PhysicsName].Trim().ToLower()) : values[(int)DefIndex.PhysicsName].Trim();
            var dataType = values[(int)DefIndex.DataType].Trim().ToLower();
            var isNotNull = values[(int)DefIndex.NotNull].Trim().ToLower().Contains('y');
            var nullable = isNotNull ? string.Empty : "?";
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
                initialize = isNotNull ? "0" : "null";
            }
            else if (nameof(SqlDbType.Date).ToLower() == dataType || nameof(SqlDbType.DateTime).ToLower() == dataType)
            {
                dataType = nameof(SqlDbType.DateTime);
                initialize = isNotNull ? "DateTime.MinValue" : "null";
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
            IsNameOnly.IsChecked = true;
            IsPascal.IsChecked = true;
            IsSnakeToPascal.IsChecked = false;
            IsUseAsIs.IsChecked = false;

            IsViewModel.IsEnabled = false;
            IsSnakeToPascal.IsEnabled = false;
            IsUseAsIs.IsEnabled = false;

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
