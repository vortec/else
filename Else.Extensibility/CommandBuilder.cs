using System;
using System.Windows.Media.Imaging;

namespace Else.Extensibility
{
    /// <summary>
    /// Builds a simple Keyword command (with optional arguments)
    /// </summary>
    public class CommandBuilder : CommandProvider
    {
        /// <param name="appCommands"></param>
        public CommandBuilder(IAppCommands appCommands) : base(appCommands)
        {
            
        }

        /// <summary>
        /// The result title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public CommandBuilder Title(string title)
        {
            _title = title;
            return this;
        }
        /// <summary>
        /// The result subtitle
        /// </summary>
        /// <param name="subTitle"></param>
        /// <returns></returns>
        public CommandBuilder Subtitle(string subTitle)
        {
            _subTitle = subTitle;
            return this;
        }

        /// <summary>
        /// The result action
        /// </summary>
        /// <param name="launch"></param>
        /// <returns></returns>
        public CommandBuilder Launch(Action<Query> launch)
        {
            _launch = launch;
            return this;
        }

        /// <summary>
        /// The result icon
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public CommandBuilder Icon(Lazy<BitmapSource> icon)
        {
            _icon = icon;
            return this;
        }

        /// <summary>
        /// The keyword required to trigger this command
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public CommandBuilder Keyword(string keyword)
        {
            _keyword = keyword;
            return this;
        }

        /// <summary>
        /// This command is a fallback command (provides a result when no other plugin has results).
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public CommandBuilder Fallback(bool fallback = true)
        {
            _fallback = fallback;
            return this;
        }

        /// <summary>
        /// This command requires arguments.
        /// </summary>
        /// <param name="requiresArguments"></param>
        /// <returns></returns>
        public CommandBuilder RequiresArguments(bool requiresArguments = true)
        {
            _requiresArguments = requiresArguments;
            return this;
        }
    }
}