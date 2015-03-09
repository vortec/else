using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using Jace;
using wpfmenu.Model;

namespace wpfmenu.Core.Plugins
{
    class Math : Plugin
    {
        private Regex _isNotMathExpressionRegex = new Regex(@"[^0-9\(\)\^\.\+\*\/\-%<>!= ]", RegexOptions.Compiled);
        private CalculationEngine _calculationEngine = new CalculationEngine();
        private BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/calculator.png"));

        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            Providers.Add(new ResultProvider{
                Fallback = true,
                IsInterested = query => {
                    if (!_isNotMathExpressionRegex.IsMatch(query.Raw)) {
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                },
                Query = query => {
                    Result result;
                    // try and execute the query using Jace math library
                    try {
                        double mathResult = _calculationEngine.Calculate(query.Raw);
                        // todo: check the string representation is okay for really long numbers
                        var strMathResult = mathResult.ToString("F99").TrimEnd("0".ToCharArray()).TrimEnd(".".ToCharArray());
                        result = new Result{
                            Title = strMathResult,
                            SubTitle = "Launch this item to copy this number to the clipboard",
                            Launch = info => {
                                Engine.LauncherWindow.Hide();
                                Clipboard.SetText(strMathResult);
                            }
                        };
                    }
                    catch (ParseException) {
                        result = new Result{
                            Title = "...",
                            SubTitle = "Please enter a valid expression"
                        };
                    }
                    result.Icon = _icon;
                    return new List<Result>{result};
                }
            });
        }
    }
}