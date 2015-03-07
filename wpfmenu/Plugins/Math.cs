using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using wpfmenu.Model;
using Jace;

namespace wpfmenu.Plugins
{
    class Math : Plugin
    {
        private Regex _isNotMathExpressionRegex = new Regex(@"[^0-9\(\)\^\.\+\*\/\-%<>!= ]", RegexOptions.Compiled);
        private CalculationEngine _calculationEngine = new CalculationEngine();
        private BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/calculator.png"));

        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup() { }

        /// <summary>
        /// Queries the plugin for results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of <see cref="Model.Result" /> to be displayed on the launcher</returns>
        public override List<Result> Query(QueryInfo query)
        {
            Result result;
            // try and execute the query using Jace math library
            try {
                double mathResult = _calculationEngine.Calculate(query.Raw);
                var strMathResult = mathResult.ToString("F99").TrimEnd("0".ToCharArray()).TrimEnd(".".ToCharArray());
                result = new Result{
                    Title = strMathResult,
                    SubTitle = "Action this item to copy this number to the clipboard",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Clipboard.SetText(strMathResult);
                    }
                };
            }
            catch (Jace.ParseException) {
                result = new Result{
                    Title = "...",
                    SubTitle = "Please enter a valid expression"
                };
            }
            result.Icon = _icon;
            return new List<Result>{result};
        }

        /// <summary>
        /// Determines if the query is a math expression.
        /// </summary>
        /// <param name="query">The query information.</param>
        /// <returns></returns>
        public override PluginInterest IsPluginInterested(QueryInfo query)
        {
            if (!_isNotMathExpressionRegex.IsMatch(query.Raw)) {
                return PluginInterest.Exclusive;
            }
            return PluginInterest.None;
        }
    }
}