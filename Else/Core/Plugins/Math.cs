using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using Else.Helpers;
using Else.Model;
using Else.Services;
using Jace;

namespace Else.Core.Plugins
{
    class Math : Plugin
    {
        private Regex _isNotMathExpressionRegex = new Regex(@"[^0-9\(\)\^\.\+\*\/\-%<>!= ]", RegexOptions.Compiled);
        private CalculationEngine _calculationEngine = new CalculationEngine();
        private Lazy<BitmapSource> _icon = UI.LoadImageFromResources("Icons/calculator.png");

        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            Providers.Add(new ResultProvider{
                Fallback = true,
                // we only show interest in queries which are math expressions
                IsInterested = query => {
                    if (!_isNotMathExpressionRegex.IsMatch(query.Raw)) {
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                },
                Query = (query, cancelToken) => {
                    Result result;
                    // try and execute the query using Jace math library
                    try {
                        double mathResult = _calculationEngine.Calculate(query.Raw);
                        // todo: check the string representation is okay for really long numbers
                        // converting from double to string gives us math exponents, so we use this line to provide a simple number string
                        var strMathResult = mathResult.ToString("F99").TrimEnd("0".ToCharArray()).TrimEnd(".".ToCharArray());
                        result = new Result{
                            Title = strMathResult,
                            SubTitle = "Launch this item to copy this number to the clipboard",
                            Launch = info => {
                                PluginCommands.HideWindow();
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
                    var results = new List<Result>{result};
                    return results;
                }
            });
        }
    }
}