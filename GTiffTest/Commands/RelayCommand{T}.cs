using System;
using System.Windows.Input;

namespace GTiffTest.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;

        public RelayCommand(Action<T> executeMethod)
            : this(executeMethod, parameter => true)
        { }

        public RelayCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod));
            _canExecuteMethod = canExecuteMethod ?? throw new ArgumentNullException(nameof(canExecuteMethod));
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod((T) parameter);
        }

        public void Execute(object parameter)
        {
            _executeMethod((T) parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }
}