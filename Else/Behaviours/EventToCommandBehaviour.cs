﻿using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Else.Behaviours
{
    /// <summary>
    /// Behavior that will connect an UI event to a viewmodel CommandProvider,
    /// allowing the event arguments to be passed as the CommandParameter.
    /// <see cref="http://stackoverflow.com/a/16317999"/>
    /// </summary>
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        private Delegate _handler;
        private EventInfo _oldEvent;
        // Event
        public string Event
        {
            get { return (string) GetValue(EventProperty); }
            set { SetValue(EventProperty, value); }
        }

        // Command
        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // PassArguments (default: false)
        public bool PassArguments
        {
            get { return (bool) GetValue(PassArgumentsProperty); }
            set { SetValue(PassArgumentsProperty, value); }
        }

        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var beh = (EventToCommandBehavior) d;

            if (beh.AssociatedObject != null) // is not yet attached at initial load
                beh.AttachHandler((string) e.NewValue);
        }

        protected override void OnAttached()
        {
            AttachHandler(Event); // initial set
        }

        /// <summary>
        /// Attaches the handler to the event
        /// </summary>
        private void AttachHandler(string eventName)
        {
            // detach old event
            _oldEvent?.RemoveEventHandler(AssociatedObject, _handler);

            // attach new event
            if (!string.IsNullOrEmpty(eventName)) {
                var ei = AssociatedObject.GetType().GetEvent(eventName);
                if (ei != null) {
                    var mi = GetType().GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic);
                    _handler = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                    ei.AddEventHandler(AssociatedObject, _handler);
                    _oldEvent = ei; // store to detach in case the Event property changes
                }
                else
                    throw new ArgumentException(string.Format("The event '{0}' was not found on type '{1}'", eventName,
                        AssociatedObject.GetType().Name));
            }
        }

        /// <summary>
        /// Executes the CommandProvider
        /// </summary>
        private void ExecuteCommand(object sender, EventArgs e)
        {
            object parameter = PassArguments ? e : null;
            if (Command != null) {
                if (Command.CanExecute(parameter))
                    Command.Execute(parameter);
            }
        }

        public static readonly DependencyProperty EventProperty = DependencyProperty.Register("Event", typeof (string),
            typeof (EventToCommandBehavior), new PropertyMetadata(null, OnEventChanged));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof (ICommand), typeof (EventToCommandBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty PassArgumentsProperty = DependencyProperty.Register("PassArguments",
            typeof (bool), typeof (EventToCommandBehavior), new PropertyMetadata(false));
    }
}