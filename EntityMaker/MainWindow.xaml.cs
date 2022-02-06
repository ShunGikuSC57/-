using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace EntityMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        /// Snake Case check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsSnake_Checked(object sender, RoutedEventArgs e)
        {
            IsPascal.IsEnabled = true;
            IsCamel.IsEnabled = true;
            IsLower.IsEnabled = true;
            IsUpper.IsEnabled = true;
            IsViewModel.IsEnabled = false;

            IsPascal.IsChecked = true;
        }

        /// <summary>
        /// Use as-is check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsUseAsIs_Checked(object sender, RoutedEventArgs e)
        {
            IsPascal.IsEnabled = false;
            IsCamel.IsEnabled = false;
            IsLower.IsEnabled = true;
            IsUpper.IsEnabled = true;
            IsViewModel.IsEnabled = false;

            IsLower.IsChecked = true;
        }

        /// <summary>
        /// By definition check event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsDef_Checked(object sender, RoutedEventArgs e)
        {
            IsPascal.IsEnabled = false;
            IsCamel.IsEnabled = false;
            IsLower.IsEnabled = false;
            IsUpper.IsEnabled = false;
            IsViewModel.IsEnabled = true;

            IsProperty.IsChecked = true;
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
            foreach (var value in ConvertSource.Text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            {
                if (!value.HasValue()) continue;
                if (IsSnake.IsChecked.GetValue())
                {
                    var sourceCode = string.Empty;
                    if (IsPascal.IsChecked.GetValue())
                        sourceCode = ConvertSnakeToPascal(value.ToLower());
                    else if (IsCamel.IsChecked.GetValue())
                        sourceCode = ConvertPascalToCamel(ConvertSnakeToPascal(value.ToLower()));
                    else if (IsLower.IsChecked.GetValue())
                        sourceCode = value.ToLower();
                    else if (IsUpper.IsChecked.GetValue())
                        sourceCode = value.ToUpper();
                    else if (IsProperty.IsChecked.GetValue())
                        sourceCode = ConvertProperty(ConvertSnakeToPascal(value.ToLower()));

                    itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                }
                else if(IsUseAsIs.IsChecked.GetValue())
                {
                    var sourceCode = string.Empty;
                    if (IsLower.IsChecked.GetValue())
                        sourceCode = value.ToLower();
                    else if (IsUpper.IsChecked.GetValue())
                        sourceCode = value.ToUpper();
                    else if (IsProperty.IsChecked.GetValue())
                        sourceCode = ConvertProperty(value);

                    itemsSource.Add(new ConvertGridRow { SourceCode = sourceCode });
                }
                else if(IsDef.IsChecked.GetValue())
                {
                    // Customize here for each project.
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
            IsSnake.IsChecked = true;
            IsPascal.IsChecked = true;
            IsViewModel.IsEnabled = false;
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
