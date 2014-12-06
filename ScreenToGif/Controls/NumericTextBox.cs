﻿using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenToGif.Controls
{
    /// <summary>
    /// Numeric only TextBox.
    /// </summary>
    [Description("Numeric only TextBox")]
    public class NumericTextBox : TextBox
    {
        #region Variables

        private TextBox _textBox;

        public readonly static DependencyProperty MinValueProperty;
        public readonly static DependencyProperty ValueProperty;
        public readonly static DependencyProperty MaxValueProperty;

        #endregion

        #region Properties

        /// <summary>
        /// The minimum value of the numeric text box.
        /// </summary>
        [Description("The minimum value of the numeric text box.")]
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetCurrentValue(MinValueProperty, value); }
        }

        /// <summary>
        /// The actual value of the numeric text box.
        /// </summary>
        [Description("The actual value of the numeric text box.")]
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetCurrentValue(ValueProperty, value);
                RaiseValueChangedEvent();
            }
        }

        /// <summary>
        /// The maximum value of the numeric text box.
        /// </summary>
        [Description("The maximum value of the numeric text box.")]
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetCurrentValue(MaxValueProperty, value); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Create a custom routed event by first registering a RoutedEventID, this event uses the bubbling routing strategy.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent;

        /// <summary>
        /// Event raised when the numeric value is changed.
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }  //Provide CLR accessors for the event 
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        void RaiseValueChangedEvent()
        {
            var newEventArgs = new RoutedEventArgs(NumericTextBox.ValueChangedEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        static NumericTextBox()
        {
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(1));
            ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(250));
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(2000));

            ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericTextBox));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PreviewTextInput += TextBox_PreviewTextInput;
            this.ValueChanged += NumericTextBox_ValueChanged;

            //this.TextChanged += TextBox_TextChanged;
            //this.MouseWheel += TextBox_MouseWheel;

            this.AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(PastingEvent));
        }

        #region Events

        private void NumericTextBox_ValueChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == null) return;

            this.ValueChanged -= NumericTextBox_ValueChanged;

            if (Value > MaxValue)
                Value = MaxValue;

            else if (Value < MinValue)
                Value = MinValue;

            this.ValueChanged += NumericTextBox_ValueChanged;

            textBox.Text = Value.ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            #region Changes the value of the Numeric Textbox

            var textBox = sender as TextBox;

            if (textBox == null) return;

            int newValue = Convert.ToInt32(textBox.Text);

            if (newValue > MaxValue)
                Value = MaxValue;
            else if (newValue < MinValue)
                Value = MinValue;
            else
            {
                Value = newValue;
            }

            #endregion
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (Value < MaxValue)
                    Value++;
            }
            else
            {
                if (Value > MinValue)
                    Value -= 1;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Text))
            {
                e.Handled = true;
                return;
            }

            if (IsTextDisallowed(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private void PastingEvent(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (String)e.DataObject.GetData(typeof(String));

                if (IsTextDisallowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        #endregion

        private bool IsTextDisallowed(string text)
        {
            var regex = new Regex("[^0-9]+");
            return regex.IsMatch(text);
        }
    }
}
